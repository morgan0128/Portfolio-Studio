import {Component, computed, inject, input, OnInit, output, signal} from '@angular/core';
import {PhotoDto} from '../../models/PhotoDto';
import {NgOptimizedImage} from '@angular/common';
import {ImageDto} from '../../models/ImageDto';
import {ImageDisplay} from '../../common/image-display/ImageDisplay';
import {PhotoApiService} from '../../api/photo-api-service';
import {DetailedPhotoView} from '../edit-albums/album-contents/detailed-photo-view/detailed-photo-view';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-photo-bank',
  imports: [
    ImageDisplay,
    DetailedPhotoView,
    RouterLink
  ],
  templateUrl: './photo-bank.html',
  styleUrl: './photo-bank.css',
})
export class PhotoBank implements OnInit {
  private readonly photoApi = inject(PhotoApiService);

  readonly photos = signal<PhotoDto[]>([]);

  readonly displayIndex = signal<number>(0);
  // readonly selectedPhoto = computed<PhotoDto | null>(() =>
  //   (this.displayIndex() >= 0 && this.displayIndex() < this.photos().length) ?
  //     this.photos()[this.displayIndex()] : null);
  // readonly selectedImage = computed<ImageDto | null>(() =>
  //   this.selectedPhoto != null ? this.selectedPhoto()!.image : null);
  protected readonly detailedViewPhoto = signal<PhotoDto | null>(null);

  ngOnInit() {
    this.photoApi.getPhotos().subscribe({
        next: photos => {
          this.photos.set(photos);
        }
      }
    )
  }

  handleDetailedPhotoViewRequest(requestedPhotoId: number){
    let photo = this.photos().find(p => p.id == requestedPhotoId);
    if (photo != undefined){
      this.detailedViewPhoto.set(photo);
    }
  }

  // nextPhoto() {
  //   if (this.photos().length <= 1){
  //     return;
  //   }
  //
  //   if (this.displayIndex() + 1 >= this.photos().length){
  //     this.displayIndex.set(0);
  //   } else {
  //     this.displayIndex.set(this.displayIndex() + 1);
  //   }
  //
  // }
  //
  // prevPhoto() {
  //   if (this.photos().length <= 1){
  //     return;
  //   }
  //
  //   if (this.displayIndex() -1 < 0){
  //     this.displayIndex.set(this.photos().length - 1);
  //   } else {
  //     this.displayIndex.set(this.displayIndex() - 1);
  //   }
  // }

}
