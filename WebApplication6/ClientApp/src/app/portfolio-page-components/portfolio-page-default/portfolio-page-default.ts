import {Component, computed, inject, input, OnInit, output, signal} from '@angular/core';
import {AlbumApiService} from '../../api/album-api-service';
import {PortfolioApiService} from '../../api/portfolio-api-service';
import {AlbumItem} from '../../models/AlbumItem';
import {PAGE_LAYOUT_PRESETS} from '../../models/PortfolioPageItemDto';
import {AlbumPhotoItemDto} from '../../models/AlbumPhotoItemDto';
import {Navbar} from '../navbar/navbar';
import {ImageItem} from '../../models/ImageItem';
import {ImageDisplay} from '../image-display/ImageDisplay';


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
  protected readonly albumApi = inject(AlbumApiService);
  protected readonly portfolioApi = inject(PortfolioApiService);

  public album = input.required<AlbumItem>();
  protected photos = signal<AlbumPhotoItemDto[]>([]);

  protected displayIndex = signal<number>(0);
  protected displayedPhoto = computed<AlbumPhotoItemDto | null>(() => this.photos().length > 0 ? this.photos()[this.displayIndex()] : null);
  protected displayedImage = computed<ImageItem | null>(() => this.displayedPhoto() !== null ? this.displayedPhoto()!.image : null);

  protected loadingPhotos = signal<boolean>(true);
  protected loadingPhotosError = signal<boolean>(false);

  public requestNavToPortfolioPageOfId = output<number>();
  adminModeBlockedNavigation = output<void>();
  public requestNavToEditAlbums = output<void>();


  ngOnInit(){
    this.albumApi.getPhotos(this.album().id).subscribe({
      next: photos => {
        this.photos.set(photos);
        this.loadingPhotos.set(false);
      },
      error: () => {
        this.loadingPhotosError.set(true);
        this.loadingPhotos.set(false);
      }
    })
  }

  shouldIncludeTextContentContainer(photo: AlbumPhotoItemDto): boolean {
    return (photo.displaysName && photo.name !== '' && photo.name === null) ||
      (photo.displaysDescription && photo.description !== '' && photo.description !== null) ||
      (photo.displaysYearCC && photo.yearContentCreated !== null);
  }

  nextPhoto() {
    if (this.photos().length <= 1){
      return;
    }

    if (this.displayIndex() + 1 >= this.photos().length){
      this.displayIndex.set(0);
    } else {
      this.displayIndex.set(this.displayIndex() + 1);
    }

  }

  prevPhoto() {
    if (this.photos().length <= 1){
      return;
    }

    if (this.displayIndex() -1 < 0){
      this.displayIndex.set(this.photos().length - 1);
    } else {
      this.displayIndex.set(this.displayIndex() - 1);
    }

  }

  protected readonly PAGE_LAYOUT_PRESETS = PAGE_LAYOUT_PRESETS;
  protected readonly alert = alert;
  // protected readonly AlbumPhotoDisplay = ImageDisplay;
}
