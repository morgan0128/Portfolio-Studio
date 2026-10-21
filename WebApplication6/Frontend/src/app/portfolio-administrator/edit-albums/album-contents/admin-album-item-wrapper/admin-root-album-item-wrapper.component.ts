import {Component, computed, input, output, viewChild} from '@angular/core';
import {AlbumItemDto} from '../../../../models/AlbumItemDto';
import {AdminPhotoDisplayCard} from './admin-photo-display-card/admin-photo-display-card';
import {
  AdminPhotoDisplayCollectionCard
} from './admin-photo-display-collection-card/admin-photo-display-collection-card';
import {PhotoDto} from '../../../../models/PhotoDto';
import {PhotoDisplayFieldsChange, PhotoDisplayTarget} from '../album-item-actions';

@Component({
  selector: 'app-admin-root-album-item-wrapper',
  imports: [
    AdminPhotoDisplayCard,
    AdminPhotoDisplayCollectionCard
  ],
  templateUrl: './admin-root-album-item-wrapper.component.html',
  styleUrl: './admin-root-album-item-wrapper.component.css',
})
export class AdminRootAlbumItemWrapper {
  public readonly item = input.required<AlbumItemDto>();

  public readonly collectionGroupingView = input.required<boolean>();
  public readonly collectionGroupingOptStatus = input<boolean>(false);
  public readonly mutationPending = input(false);

  public readonly collectionGroupingOptStatusChanged = output<boolean>();
  public readonly photoDetailedViewRequest = output<PhotoDto>();
  public readonly photoDisplayFieldsChangeRequested = output<PhotoDisplayFieldsChange>();
  public readonly photoDisplayRemoveRequested = output<number>();
  public readonly photoDisplayEditRequested = output<PhotoDisplayTarget>();
  public readonly dissolveRequested = output<number>();
  public readonly rootReorderLeftRequested = output<AlbumItemDto>();
  public readonly rootReorderRightRequested = output<AlbumItemDto>();

  private readonly collectionCard = viewChild(AdminPhotoDisplayCollectionCard);

  protected readonly editablePhotoDisplay = computed(() => {
    const item = this.item();
    return item.kind === 'photoDisplay' ? item : this.collectionCard()?.selectedPhotoDisplay() ?? null;
  });

  editPhotoDisplay() {
    const photoDisplay = this.editablePhotoDisplay();
    if (this.mutationPending() || !photoDisplay) return;

    this.photoDisplayEditRequested.emit({
      rootItemId: this.item().id,
      photoDisplayId: photoDisplay.id,
    });
  }

  protected handleGroupingOptChange(){
    if (this.mutationPending() || this.item().kind !== 'photoDisplay') return;

    this.collectionGroupingOptStatusChanged.emit(!this.collectionGroupingOptStatus());
  }

  dissolveCollection(){
    const item = this.item();
    if (this.mutationPending() || item.kind !== 'photoDisplayCollection') return;

    this.dissolveRequested.emit(item.id);
  }

  removePhotoDisplay(){
    const item = this.item();
    if (this.mutationPending() || item.kind !== 'photoDisplay') return;

    this.photoDisplayRemoveRequested.emit(item.id);
  }
}
