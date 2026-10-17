import {Component, computed, input, output, signal} from '@angular/core';
import {AdminPhotoDisplayCard} from '../admin-photo-display-card/admin-photo-display-card';
import {
  PhotoDisplayCollectionDto,
  PhotoDisplayCollectionItem,
  PhotoDisplayItem
} from '../../../../../models/AlbumItemDto';
import {PhotoDto} from '../../../../../models/PhotoDto';
import {PhotoDisplayFieldsChange} from '../../album-item-actions';


@Component({
  selector: 'app-admin-photo-display-collection-card',
  imports: [
    AdminPhotoDisplayCard
  ],
  templateUrl: './admin-photo-display-collection-card.html',
  styleUrl: './admin-photo-display-collection-card.css',
})
export class AdminPhotoDisplayCollectionCard {
  readonly item = input.required<PhotoDisplayCollectionItem>();
  readonly mutationPending = input(false);
  readonly photoDisplayCollection = computed<PhotoDisplayCollectionDto>(() => this.item().content);

  readonly photoDisplayFieldsChangeRequested = output<PhotoDisplayFieldsChange>();
  readonly photoDetailedViewRequest = output<PhotoDto>();

  readonly displayMode = computed<number>(() =>  this.photoDisplayCollection().displayMode);
  readonly photoDisplays = computed<PhotoDisplayItem[]>(() => this.photoDisplayCollection().photoDisplays);

  readonly displayIndex = signal<number>(0);
  readonly selectedPhotoDisplay = computed<PhotoDisplayItem | null>(() =>
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
