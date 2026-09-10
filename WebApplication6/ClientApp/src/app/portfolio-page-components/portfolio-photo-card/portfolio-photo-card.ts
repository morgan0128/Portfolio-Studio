import {Component, input, signal} from '@angular/core';
import {NgOptimizedImage} from '@angular/common';
import {AlbumPhotoItemDto} from '../../models/AlbumPhotoItemDto';
import {PageLayoutPreset} from '../../models/PortfolioPageItemDto';

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
  public photo = input.required<AlbumPhotoItemDto>();
  public layout = input.required<PageLayoutPreset>();
}
