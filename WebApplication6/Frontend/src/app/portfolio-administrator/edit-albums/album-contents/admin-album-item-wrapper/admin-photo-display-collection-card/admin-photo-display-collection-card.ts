import {Component, computed, input, linkedSignal, output} from '@angular/core';
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

  // Keep the selected member when a refresh replaces or reorders this collection.
  private readonly selectedPhotoDisplayId = linkedSignal<PhotoDisplayCollectionItem, number | null>({
    source: this.item,
    computation: (item, previous) => {
      const photoDisplays = item.content.photoDisplays;
      return previous?.source.id === item.id && photoDisplays.some(photo => photo.id === previous.value)
        ? previous.value
        : photoDisplays[0]?.id ?? null;
    },
  });
  readonly selectedPhotoDisplay = computed<PhotoDisplayItem | null>(() => {
    const selectedId = this.selectedPhotoDisplayId();
    return this.photoDisplays().find(photo => photo.id === selectedId) ?? null;
  });
  readonly displayIndex = computed(() =>
    this.photoDisplays().findIndex(photo => photo.id === this.selectedPhotoDisplayId()));

  nextPhoto() {
    this.navigate(1);
  }

  prevPhoto() {
    this.navigate(-1);
  }

  private navigate(direction: number) {
    const photoDisplays = this.photoDisplays();
    if (photoDisplays.length <= 1) return;

    const nextIndex = (this.displayIndex() + direction + photoDisplays.length) % photoDisplays.length;
    this.selectedPhotoDisplayId.set(photoDisplays[nextIndex].id);
  }

}
