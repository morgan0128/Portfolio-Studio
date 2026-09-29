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
type PortfolioRow =
  | { kind: 'photos'; key: string; photos: SizedPhoto[]; width: number | null }
  | { kind: 'collection'; key: string; item: PhotoDisplayCollectionItem };


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

  @ViewChild('portfolioItems') private portfolioItems?: ElementRef<HTMLElement>;

  public album = input.required<AlbumDto>();
  protected items = signal<AlbumItemDto[]>([]);
  private readonly rowMetrics = signal({ width: 0, rootFont: 16 });
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

    this.resizeObserver = new ResizeObserver(entries => {
      const width = entries[0]?.contentRect.width ?? 0;
      const rootFont = Number.parseFloat(getComputedStyle(document.documentElement).fontSize) || 16;
      const previous = this.rowMetrics();
      if (Math.abs(width - previous.width) < 0.5 && Math.abs(rootFont - previous.rootFont) < 0.01) return;

      this.zone.run(() => this.rowMetrics.set({ width, rootFont }));
    });
    this.resizeObserver.observe(this.portfolioItems.nativeElement);
  }

  ngOnDestroy() {
    this.resizeObserver?.disconnect();
  }

  private buildRows(items: AlbumItemDto[], metrics: { width: number; rootFont: number }): PortfolioRow[] {
    const rows: PortfolioRow[] = [];
    const run: PhotoDisplayItem[] = [];
    const available = metrics.width;
    const minGap = metrics.rootFont;
    // Group only when modest gaps can fill at least 95% of the usable width
    const maxGap = Math.min(3 * metrics.rootFont, Math.max(1.5 * metrics.rootFont, available * 0.04));

    const flushRun = () => {
      const photos: SizedPhoto[] = run.map(item => {
        const image = item.content.photo.image;
        const width = available <= 0 ? null : image && image.width > 0 && image.height > 0
          ? Math.min(available, image.width, 32 * metrics.rootFont * image.width / image.height)
          : Math.min(available, 32 * metrics.rootFont);
        return { item, width };
      });

      for (let start = 0; start < photos.length;) {
        let end = start + 1;
        let qualifyingSum = 0;
        let sum = photos[start].width ?? 0;

        if (available > 0) {
          for (let next = start + 1; next < photos.length; next++) {
            sum += photos[next].width ?? 0;
            const gaps = next - start;
            if (sum + gaps * minGap > available) break;
            if (sum + gaps * maxGap >= available * 0.95) {
              end = next + 1;
              qualifyingSum = sum;
            }
          }
        }

        const group = photos.slice(start, end);
        const width = group.length === 1
          ? group[0].width
          : Math.min(available, qualifyingSum + (group.length - 1) * maxGap);
        rows.push({ kind: 'photos', key: `photo-${group[0].item.id}`, photos: group, width });
        start = end;
      }
      run.length = 0;
    };

    for (const item of items) {
      if (item.kind === 'photoDisplay') {
        run.push(item);
      } else {
        flushRun();
        rows.push({ kind: 'collection', key: `collection-${item.id}`, item });
      }
    }
    flushRun();
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
