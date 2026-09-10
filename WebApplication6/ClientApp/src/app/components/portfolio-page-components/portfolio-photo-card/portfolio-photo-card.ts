import {Component, input, signal} from '@angular/core';
import {FullViewAlbumPhoto} from '../../../models/FullViewAlbumPhoto';
import {PhotoItem} from '../../../models/AlbumInterfacing';
import {PageLayoutPreset} from '../../../models/PortfolioInterfacing';
import {NgOptimizedImage} from '@angular/common';

@Component({
  selector: 'app-portfolio-photo-card',
  imports: [
    NgOptimizedImage
  ],
  templateUrl: './portfolio-photo-card.html',
  styleUrl: './portfolio-photo-card.css',
})
export class PortfolioPhotoCard {
  public albumId = input.required<number>();
  public photo = input.required<PhotoItem>();
  public layout = input.required<PageLayoutPreset>();
}
