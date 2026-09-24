import {Component, computed, input, output, signal} from '@angular/core';
import {AlbumItemDto} from '../../../../models/AlbumItemDto';
import {PhotoDisplayDto} from '../../../../models/PhotoDisplayDto';
import {PhotoDisplayCollectionDto} from '../../../../models/PhotoDisplayCollectionDto';
import {AdminPhotoDisplayCard} from '../admin-photo-display-card/admin-photo-display-card';

@Component({
  selector: 'app-admin-photo-display-collection-card',
  imports: [
    AdminPhotoDisplayCard
  ],
  templateUrl: './admin-photo-display-collection-card.html',
  styleUrl: './admin-photo-display-collection-card.css',
})
export class AdminPhotoDisplayCollectionCard {
  readonly albumId = input.required<number>();
  readonly item = input.required<AlbumItemDto>();

  readonly photoDisplayCollection = input.required<PhotoDisplayCollectionDto>();

  readonly ownStateChange = output<AlbumItemDto>();
  readonly displayPhotoInternalStateChange = output<AlbumItemDto>();
  readonly itemStatesChange = output();

  readonly displayMode = computed<number>(() =>  this.photoDisplayCollection().displayMode);
  readonly photoDisplays = computed<PhotoDisplayDto[]>(() => this.photoDisplayCollection().photoDisplays);

  readonly displayIndex = signal<number>(0);
  readonly selectedPhotoDisplay = computed<PhotoDisplayDto | null>(() =>
    (this.displayIndex() >= 0 && this.displayIndex() < this.photoDisplays().length) ?
      this.photoDisplays()[this.displayIndex()] : null);


  nextPhoto() {
    if (this.photoDisplays().length <= 1){
      return;
    }

    if (this.displayIndex() + 1 >= this.photoDisplays().length){
      this.displayIndex.set(0);
    } else {
      this.displayIndex.set(this.displayIndex() + 1);
    }

  }

  prevPhoto() {
    if (this.photoDisplays().length <= 1){
      return;
    }

    if (this.displayIndex() -1 < 0){
      this.displayIndex.set(this.photoDisplays().length - 1);
    } else {
      this.displayIndex.set(this.displayIndex() - 1);
    }

  }

}
