import {Component, computed, inject, input, OnInit, output, signal} from '@angular/core';
import {AlbumApiService} from '../../api/album-api-service';
import {AlbumDto} from '../../models/AlbumDto';
import {PAGE_LAYOUT_PRESETS} from '../../models/ApiEnums';
import {PhotoDto} from '../../models/PhotoDto';
import {Navbar} from '../navbar/navbar';
import {ImageDto} from '../../models/ImageDto';
import {ImageDisplay} from '../image-display/ImageDisplay';
import {AlbumItemApiService} from '../../api/album-item-api-service';
import {AlbumItemDto} from '../../models/AlbumItemDto';
import {PhotoDisplayDto} from '../../models/PhotoDisplayDto';


@Component({
  selector: 'app-portfolio-page-default',
  imports: [
    Navbar,
    ImageDisplay
  ],
  templateUrl: './portfolio-page-default.html',
  styleUrl: './portfolio-page-default.css',
})
export class PortfolioPageDefault implements OnInit {
  // protected readonly albumApi = inject(AlbumApiService);
  private readonly albumItemApi = inject(AlbumItemApiService);

  public album = input.required<AlbumDto>();
  protected items = signal<AlbumItemDto[]>([]);
  protected photos = signal<PhotoDto[]>([]);

  protected displayIndex = signal<number>(0);

  // TODO: just setting up PhotoDisplays into the existing carousel for refactor verification (ironically, given the whole point of the refactor...)
  protected displayedItem = computed<AlbumItemDto | null>(() =>
    this.items().length > 0 ? this.items()[this.displayIndex()] : null); // TODO TODOOOO this setup just for testing purposes!!!
  protected displayedPhotoDisplay = computed<PhotoDisplayDto | null>(() => this.displayedItem() !== null ? <PhotoDisplayDto>this.displayedItem()!.content : null); // TODO big time

  protected displayedPhoto = computed<PhotoDto | null>(() => this.displayedPhotoDisplay() !== null ? this.displayedPhotoDisplay()!.photo : null);
  protected displayedImage = computed<ImageDto | null>(() => this.displayedPhoto() !== null ? this.displayedPhoto()!.image : null);
  // TODO TODO TODO OTODTODG


  protected loadingItems = signal<boolean>(true);
  protected loadingItemsError = signal<boolean>(false);

  // protected loadingPhotos = signal<boolean>(true);
  // protected loadingPhotosError = signal<boolean>(false);

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

  shouldIncludeTextContentContainer(photoDisplay: PhotoDisplayDto): boolean {
    const photo = photoDisplay.photo;
    return (photoDisplay.displaysName && photo.name !== '' && photo.name !== null) ||
      (photoDisplay.displaysDescription && photo.description !== '' && photo.description !== null) ||
      (photoDisplay.displaysYearContentCreated && photo.yearContentCreated !== null);
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
  // protected readonly AlbumPhotoDisplay = ImageDisplay;
}
