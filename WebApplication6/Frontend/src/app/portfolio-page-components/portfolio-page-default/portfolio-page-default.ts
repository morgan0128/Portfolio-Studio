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
type PhotoRowPlan = {
  singletons: number;
  tailSingleton: boolean;
  sparsePairs: number;
  rows: number;
  raggedness: number;
  count: number;
  width: number;
  gap: number;
};


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

  private preferPhotoPlan(candidate: PhotoRowPlan, current: PhotoRowPlan | null): boolean {
    if (current === null) return true;
    if (candidate.singletons !== current.singletons) return candidate.singletons < current.singletons;
    if (candidate.tailSingleton !== current.tailSingleton) return !candidate.tailSingleton;
    if (candidate.sparsePairs !== current.sparsePairs) return candidate.sparsePairs < current.sparsePairs;
    if (candidate.rows !== current.rows) return candidate.rows < current.rows;
    if (Math.abs(candidate.raggedness - current.raggedness) > 1e-9) {
      return candidate.raggedness < current.raggedness;
    }
    return candidate.count > current.count;
  }

  private buildRows(items: AlbumItemDto[], metrics: RowMetrics): PortfolioRow[] {
    const rows: PortfolioRow[] = [];
    const run: PhotoDisplayItem[] = [];
    const available = metrics.width;

    const flushRun = (isAlbumTail = false) => {
      const photos: SizedPhoto[] = run.map(item => {
        const image = item.content.photo.image;
        const width = available <= 0 ? null : image
          ? this.displayedImageWidth(image, available, 32 * metrics.rootFont)
          : Math.min(available, 32 * metrics.rootFont);
        return { item, width };
      });

      if (available <= 0) {
        for (const photo of photos) {
          rows.push({ kind: 'photos', key: `photo-${photo.item.id}`, photos: [photo], width: photo.width, gap: 0 });
        }
        run.length = 0;
        return;
      }

      const plans = new Array<PhotoRowPlan>(photos.length + 1);
      plans[photos.length] = {
        singletons: 0, tailSingleton: false, sparsePairs: 0, rows: 0,
        raggedness: 0, count: 0, width: 0, gap: 0
      };

      // Compare complete remaining runs so a full row does not strand one or two photos at the end.
      for (let start = photos.length - 1; start >= 0; start--) {
        let sum = 0;
        let best: PhotoRowPlan | null = null;
        for (let end = start + 1; end <= photos.length; end++) {
          sum += photos[end - 1].width ?? 0;
          if (sum > available) break;

          const count = end - start;
          const gap = count > 1 ? Math.min(available * 0.025, (sum / count) * 0.08) : 0;
          const width = sum + (count - 1) * gap;
          if (width > available) continue;
          // A lone sparse pair before a collection keeps its separate presentation.
          if (count === 2 && width < available * 0.95 && photos.length === 2 && !isAlbumTail) continue;

          const remaining = plans[end];
          const slack = (available - width) / available;
          const candidate: PhotoRowPlan = {
            singletons: remaining.singletons + (count === 1 ? 1 : 0),
            tailSingleton: end === photos.length ? count === 1 : remaining.tailSingleton,
            sparsePairs: remaining.sparsePairs + (count === 2 && width < available * 0.95 ? 1 : 0),
            rows: remaining.rows + 1,
            raggedness: remaining.raggedness + slack * slack,
            count, width, gap
          };
          if (this.preferPhotoPlan(candidate, best)) best = candidate;
        }
        plans[start] = best!;
      }

      for (let start = 0; start < photos.length;) {
        const plan = plans[start];
        const group = photos.slice(start, start + plan.count);
        rows.push({ kind: 'photos', key: `photo-${group[0].item.id}`, photos: group, width: plan.width, gap: plan.gap });
        start += plan.count;
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
    flushRun(true);

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
