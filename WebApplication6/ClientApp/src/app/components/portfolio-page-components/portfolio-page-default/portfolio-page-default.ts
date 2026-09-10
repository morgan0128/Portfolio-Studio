import {Component, inject, input, OnInit, signal} from '@angular/core';
import {AlbumApiService} from '../../../services/album-api-service';
import {PortfolioApiService} from '../../../services/portfolio-api-service';
import {AlbumItem, PhotoItem} from '../../../models/AlbumInterfacing';
import {FullViewAlbumPhoto} from '../../../models/FullViewAlbumPhoto';
import {PortfolioPhotoCard} from '../portfolio-photo-card/portfolio-photo-card';
import {PAGE_LAYOUT_PRESETS} from '../../../models/PortfolioInterfacing';

@Component({
  selector: 'app-portfolio-page-default',
  imports: [
    PortfolioPhotoCard
  ],
  templateUrl: './portfolio-page-default.html',
  styleUrl: './portfolio-page-default.css',
})
export class PortfolioPageDefault implements OnInit {
  protected readonly albumApi = inject(AlbumApiService);
  protected readonly portfolioApi = inject(PortfolioApiService);

  public album = input.required<AlbumItem>();
  protected photos = signal<PhotoItem[]>([]);

  protected loadingPhotos = signal<boolean>(true);
  protected loadingPhotosError = signal<boolean>(false);


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


  protected readonly PAGE_LAYOUT_PRESETS = PAGE_LAYOUT_PRESETS;
}
