import {Component, input} from '@angular/core';
import {PhotoDto} from '../../models/PhotoDto';
import {ImageDto} from '../../models/ImageDto';
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
  public displayedImage = input.required<ImageDto>();
}
