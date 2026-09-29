import {AfterViewInit, Component, computed, ElementRef, inject, input, NgZone, OnDestroy, OnInit, output, signal, ViewChild} from '@angular/core';
import {AlbumDto} from '../../models/AlbumDto';
import {PAGE_LAYOUT_PRESETS} from '../../models/ApiEnums';
import {PhotoDto} from '../../models/PhotoDto';
import {Navbar} from '../navbar/navbar';
import {ImageDto} from '../../models/ImageDto';
import {AlbumItemApiService} from '../../api/album-item-api-service';
import {AlbumItemDto, PhotoDisplayCollectionItem, PhotoDisplayDto, PhotoDisplayItem} from '../../models/AlbumItemDto';
import {PortfolioPagePhotoDisplay} from '../portfolio-page-photo-display/portfolio-page-photo-display';
import {
  PortfolioPagePhotoDisplayCollection
} from '../portfolio-page-photo-display-collection/portfolio-page-photo-display-collection';

type SizedPhoto = { item: PhotoDisplayItem; width: number | null };
type RowMetrics = { width: number; rootFont: number; viewportHeight: number };
type PortfolioRow =
  | { kind: 'photos'; key: string; photos: SizedPhoto[]; width: number | null; gap: number }
  | { kind: 'collection'; key: string; item: PhotoDisplayCollectionItem; width: number | null };


@Component({
  selector: 'app-portfolio-page-default',
  imports: [
    Navbar,
    PortfolioPagePhotoDisplay,
    PortfolioPagePhotoDisplayCollection
  ],
  templateUrl: './portfolio-page-default.html',
  styleUrl: './portfolio-page-default.css',
})
export class PortfolioPageDefault implements OnInit, AfterViewInit, OnDestroy {
  private readonly albumItemApi = inject(AlbumItemApiService);
  private readonly zone = inject(NgZone);
  private resizeObserver?: ResizeObserver;
  private resizeFrame: number | null = null;
  private pendingWidth: number | null = null;

  @ViewChild('portfolioItems') private portfolioItems?: ElementRef<HTMLElement>;

  public album = input.required<AlbumDto>();
  protected items = signal<AlbumItemDto[]>([]);
  private readonly rowMetrics = signal<RowMetrics>({ width: 0, rootFont: 16, viewportHeight: 0 });
  protected readonly rows = computed<PortfolioRow[]>(() => this.buildRows(this.items(), this.rowMetrics()));
  // protected photos = signal<PhotoDto[]>([]);

  protected displayIndex = signal<number>(0);

  protected displayedItem = computed<AlbumItemDto | null>(() =>
    this.items().length > 0 ? this.items()[this.displayIndex()] : null);
  protected displayedPhotoDisplay = computed<PhotoDisplayDto | null>(() => this.displayedItem() !== null ? <PhotoDisplayDto>this.displayedItem()!.content : null); // TODO big time

  protected displayedPhoto = computed<PhotoDto | null>(() => this.displayedPhotoDisplay() !== null ? this.displayedPhotoDisplay()!.photo : null);
  protected displayedImage = computed<ImageDto | null>(() => this.displayedPhoto() !== null ? this.displayedPhoto()!.image : null);


  protected loadingItems = signal<boolean>(true);
  protected loadingItemsError = signal<boolean>(false);

  public requestNavToAlbumOfId = output<number>();
  adminModeBlockedNavigation = output<void>();
  public requestNavToEditAlbums = output<number>();


  ngOnInit(){
    this.albumItemApi.fetchAlbumItems(this.album().id).subscribe({
      next: items => {
        this.items.set(items);
        this.loadingItems.set(false);
      },
      error: () => {
        this.loadingItemsError.set(true);
        this.loadingItems.set(false);
      }
    })
  }

  ngAfterViewInit() {
    if (!this.portfolioItems || typeof ResizeObserver === 'undefined') return;
    const element = this.portfolioItems.nativeElement;

    this.zone.runOutsideAngular(() => {
      this.resizeObserver = new ResizeObserver(entries => {
        const width = entries[0]?.contentRect.width;
        if (width === undefined) return;

        this.pendingWidth = width;
        if (this.resizeFrame !== null) return;

        this.resizeFrame = requestAnimationFrame(() => {
          this.resizeFrame = null;
          const latestWidth = this.pendingWidth;
          this.pendingWidth = null;
          if (latestWidth === null) return;

          const rootFont = Number.parseFloat(getComputedStyle(document.documentElement).fontSize) || 16;
          const width = latestWidth;
          const viewportHeight = window.innerHeight;
          const previous = this.rowMetrics();
          if (Math.abs(width - previous.width) < 0.5 &&
            Math.abs(rootFont - previous.rootFont) < 0.01 &&
            Math.abs(viewportHeight - previous.viewportHeight) < 0.5) return;

          this.zone.run(() => this.rowMetrics.set({ width, rootFont, viewportHeight }));
        });
      });
      this.resizeObserver.observe(element);
    });
  }

  ngOnDestroy() {
    this.resizeObserver?.disconnect();
    if (this.resizeFrame !== null) cancelAnimationFrame(this.resizeFrame);
    this.pendingWidth = null;
  }

  private displayedImageWidth(image: ImageDto | null, available: number, maxHeight: number): number {
    if (!image || available <= 0) return 0;
    return image.width > 0 && image.height > 0
      ? Math.min(available, image.width, maxHeight * image.width / image.height)
      : Math.min(available, maxHeight);
  }

  private buildRows(items: AlbumItemDto[], metrics: RowMetrics): PortfolioRow[] {
    const rows: PortfolioRow[] = [];
    const run: PhotoDisplayItem[] = [];
    const available = metrics.width;

    const flushRun = () => {
      const photos: SizedPhoto[] = run.map(item => {
        const image = item.content.photo.image;
        const width = available <= 0 ? null : image
          ? this.displayedImageWidth(image, available, 32 * metrics.rootFont)
          : Math.min(available, 32 * metrics.rootFont);
        return { item, width };
      });

      for (let start = 0; start < photos.length;) {
        let end = start + 1;
        let rowWidth = photos[start].width;
        let rowGap = 0;
        let sum = photos[start].width ?? 0;

        if (available > 0) {
          for (let next = start + 1; next < photos.length; next++) {
            sum += photos[next].width ?? 0;
            const count = next - start + 1;
            const gap = Math.min(available * 0.025, (sum / count) * 0.08);
            const candidateWidth = sum + (count - 1) * gap;
            if (candidateWidth > available) break;
            // Sparse pairs remain separate; larger adjacent groups can form a centered, narrower row.
            if (count >= 3 || candidateWidth >= available * 0.95) {
              end = next + 1;
              rowWidth = candidateWidth;
              rowGap = gap;
            }
          }
        }

        const group = photos.slice(start, end);
        rows.push({ kind: 'photos', key: `photo-${group[0].item.id}`, photos: group, width: rowWidth, gap: rowGap });
        start = end;
      }
      run.length = 0;
    };

    for (const item of items) {
      if (item.kind === 'photoDisplay') {
        run.push(item);
      } else {
        flushRun();
        rows.push({ kind: 'collection', key: `collection-${item.id}`, item, width: null });
      }
    }
    flushRun();

    const captionRowHeight = Math.min(7.5 * metrics.rootFont,
      Math.max(5.5 * metrics.rootFont, metrics.viewportHeight * 0.12));
    for (let index = 0; index < rows.length; index++) {
      const row = rows[index];
      if (row.kind !== 'collection') continue;

      const largestImageWidth = row.item.content.photoDisplays.reduce((largest, display) => {
        const photo = display.content.photo;
        const details = display.content;
        const hasCaption = (details.displaysName && !!photo.name) ||
          (details.displaysDescription && !!photo.description) ||
          (details.displaysYearContentCreated && photo.yearContentCreated !== null);
        // Without a caption, the image also occupies the caption grid row.
        const stageHeight = 32 * metrics.rootFont + (hasCaption ? 0 : captionRowHeight);
        return Math.max(largest, this.displayedImageWidth(photo.image, available, stageHeight));
      }, 0);
      const previous = rows[index - 1];
      const next = rows[index + 1];
      const adjacentWidth = Math.max(
        previous?.kind === 'photos' ? previous.width ?? 0 : 0,
        next?.kind === 'photos' ? next.width ?? 0 : 0
      );
      const width = Math.max(largestImageWidth, adjacentWidth);
      rows[index] = { ...row, width: width > 0 ? Math.min(available, width) : null };
    }
    return rows;
  }

  nextPhoto() {
    if (this.items().length <= 1){
      return;
    }

    if (this.displayIndex() + 1 >= this.items().length){
      this.displayIndex.set(0);
    } else {
      this.displayIndex.set(this.displayIndex() + 1);
    }

  }

  prevPhoto() {
    if (this.items().length <= 1){
      return;
    }

    if (this.displayIndex() -1 < 0){
      this.displayIndex.set(this.items().length - 1);
    } else {
      this.displayIndex.set(this.displayIndex() - 1);
    }

  }

  protected readonly PAGE_LAYOUT_PRESETS = PAGE_LAYOUT_PRESETS;
  protected readonly alert = alert;
}
