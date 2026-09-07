import {Component, input, InputSignal, output} from '@angular/core';
import {NgOptimizedImage} from "@angular/common";
import {PhotoItem} from '../../models/AlbumInterfacing';
import {AdminViewPhotoCard} from '../admin-view-photo-card/admin-view-photo-card';
// import {CdkDragDrop, CdkDropList, moveItemInArray} from '@angular/cdk/drag-drop';

@Component({
  selector: 'app-photos-display',
  imports: [
    AdminViewPhotoCard,
    // CdkDropList
  ],
  templateUrl: './photos-display.html',
  styleUrl: './photos-display.css',
})
export class PhotosDisplay {
  public readonly albumId = input.required<number>();
  public readonly displayPhotos = input<PhotoItem[]>([]);

  readonly photoStateChange = output<PhotoItem>();
  readonly orderChanged = output(); // reload all photos
  readonly photoDetailedViewRequest = output<PhotoItem>();

}
