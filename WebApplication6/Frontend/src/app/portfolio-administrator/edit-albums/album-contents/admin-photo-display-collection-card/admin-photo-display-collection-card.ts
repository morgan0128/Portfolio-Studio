import {Component, computed, input, output, signal} from '@angular/core';
// import {AlbumItemDto} from '../../../../models/AlbumItemDto';
// import {PhotoDisplayDto} from '../../../../models/PhotoDisplayDto';
// import {PhotoDisplayCollectionDto} from '../../../../models/PhotoDisplayCollectionDto';
import {
  AlbumItemDto,
  PhotoDisplayCollectionDto,
  PhotoDisplayCollectionItem,
  PhotoDisplayItem
} from '../../../../models/AlbumItemDto'
import {AdminPhotoDisplayCard} from '../admin-photo-display-card/admin-photo-display-card';
import {PhotoDto} from '../../../../models/PhotoDto';

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
  readonly item = input.required<PhotoDisplayCollectionItem>();
  readonly photoDisplayCollection = computed<PhotoDisplayCollectionDto>(() => this.item().content);

  readonly stateChange = output<PhotoDisplayCollectionItem>();
  // readonly internalItemStateChange = output<PhotoDisplayItem>();
  // readonly externalStateChange = output();

  readonly internalPhotoDetailedViewRequest = output<PhotoDto>();

  readonly displayMode = computed<number>(() =>  this.photoDisplayCollection().displayMode);
  readonly photoDisplays = computed<PhotoDisplayItem[]>(() => this.photoDisplayCollection().photoDisplays);

  readonly displayIndex = signal<number>(0);
  readonly selectedPhotoDisplay = computed<PhotoDisplayItem | null>(() =>
    (this.displayIndex() >= 0 && this.displayIndex() < this.photoDisplays().length) ?
      this.photoDisplays()[this.displayIndex()] : null);


  internalPhotoDisplayStateChange(changedPhotoDisplay: PhotoDisplayItem){
    const collection = this.item();
    this.stateChange.emit({
      ...collection,
      content: {
        ...collection.content,
        photoDisplays: collection.content.photoDisplays.map(photo =>
          photo.id === changedPhotoDisplay.id ? changedPhotoDisplay : photo
        ),
      },
    });
  }

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
