import {Component, input} from '@angular/core';
import {AlbumPhotoItemDto} from '../../models/AlbumPhotoItemDto';
import {ImageItem} from '../../models/ImageItem';
import {NgOptimizedImage} from '@angular/common';

@Component({
  selector: 'app-image-display',
  imports: [
    NgOptimizedImage
  ],
  templateUrl: './ImageDisplay.html',
  styleUrl: './ImageDisplay.css',
})
export class ImageDisplay {
  public displayedImage = input.required<ImageItem>();
}
