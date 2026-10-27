import {Component, computed, inject, input, OnChanges, OnDestroy, output, signal, SimpleChanges} from '@angular/core';
import {AlbumDto} from '../../../models/AlbumDto';
import {PhotoDto} from '../../../models/PhotoDto';
import {DetailedPhotoView} from './detailed-photo-view/detailed-photo-view';
import {AlbumItemDto, PhotoDisplayItem} from '../../../models/AlbumItemDto';
import {PhotoDisplayCollectionApiService} from '../../../api/photo-display-collection-api-service';
import {PhotoDisplayApiService, PhotoDisplayFieldsDisplayedRequest} from '../../../api/photo-display-api-service';
import {AdminRootAlbumItemWrapper} from './admin-album-item-wrapper/admin-root-album-item-wrapper.component';
import {AlbumPhotoDisplayFieldsVisibilityChange, PhotoDisplayFieldsChange, PhotoDisplayTarget, resolvePhotoDisplay} from './album-item-actions';
import {AlbumItemApiService} from '../../../api/album-item-api-service';
import {PhotoEditFieldsRequest} from '../../../models/PhotoEditFieldsRequest';
import {AdminPhotoEditor} from './photo-display-editor/admin-photo-editor.component';
import {PhotoApiService} from '../../../api/photo-api-service';

type PhotoDisplayEdit = PhotoDisplayTarget & {photoDisplay: PhotoDisplayItem};

@Component({
  selector: 'app-album-contents',
  imports: [DetailedPhotoView, AdminRootAlbumItemWrapper, AdminPhotoEditor],
  templateUrl: './album-contents.html',
  styleUrl: './album-contents.css',
})
export class AlbumContents implements OnChanges, OnDestroy {
  private readonly albumItemApi = inject(AlbumItemApiService);
  private readonly photoDisplayCollectionApi = inject(PhotoDisplayCollectionApiService);
  private readonly photoDisplayApi = inject(PhotoDisplayApiService);
  private readonly photoApi = inject(PhotoApiService);
  private albumSession = 0;
  private destroyed = false;

  public readonly selectedAlbum = input.required<AlbumDto | null>();
  public readonly selectedAlbumId = input.required<number>();
  public readonly loadingItems = input<boolean>(false);
  public readonly items = input<AlbumItemDto[]>([]);

  readonly photoDisplayFieldsChanged = output<AlbumPhotoDisplayFieldsVisibilityChange>();
  readonly refreshRequested = output<number>();

  protected readonly detailedViewPhoto = signal<PhotoDto | null>(null);
  protected readonly collectionGroupingView = signal(false);
  protected readonly collectionGroupingOptInIds = signal<number[]>([]);
  private readonly pendingMutation = signal<'createGroup' | 'fields' | 'dissolve' | 'rootReorder' | 'edit' | null>(null);
  protected readonly mutationPending = computed(() => this.pendingMutation() !== null);
  protected readonly creatingPhotoGroup = computed(() => this.pendingMutation() === 'createGroup');
  protected readonly mutationError = signal<string | null>(null);
  protected readonly photoDisplayEdit = signal<PhotoDisplayEdit | null>(null);
  protected readonly photoDisplayEditError = signal<string | null>(null);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['selectedAlbumId']) {
      this.albumSession++;
      this.pendingMutation.set(null);
      this.mutationError.set(null);
      this.resetGrouping();
      this.detailedViewPhoto.set(null);
      this.photoDisplayEdit.set(null);
      this.photoDisplayEditError.set(null);
    }

    const edit = this.photoDisplayEdit();
    if (edit && changes['items'] && !this.loadingItems() && !this.resolveEditTarget(edit)) {
      this.photoDisplayEdit.set(null);
      this.photoDisplayEditError.set(null);
    }
  }

  ngOnDestroy() {
    this.destroyed = true;
    this.albumSession++;
  }

  protected handleDetailedPhotoViewRequest(photo: PhotoDto) {
    this.detailedViewPhoto.set(photo);
  }

  protected editPhotoDisplay(target: PhotoDisplayTarget) {
    if (this.mutationPending() || this.loadingItems()) return;
    const photoDisplay = resolvePhotoDisplay(
      this.items().find(item => item.id === target.rootItemId), target.photoDisplayId);
    if (!photoDisplay) return;

    this.photoDisplayEdit.set({
      ...target,
      photoDisplay: {
        ...photoDisplay,
        content: {...photoDisplay.content, photo: {...photoDisplay.content.photo}},
      },
    });
    this.photoDisplayEditError.set(null);
    this.mutationError.set(null);
    this.detailedViewPhoto.set(null);
  }

  protected cancelPhotoDisplayEdit() {
    if (this.mutationPending()) return;
    this.photoDisplayEdit.set(null);
    this.photoDisplayEditError.set(null);
  }

  protected savePhotoDisplayDetails(request: PhotoEditFieldsRequest) {
    const edit = this.photoDisplayEdit();
    if (!edit || this.mutationPending() || this.loadingItems()) return;
    const photoDisplay = this.resolveEditTarget(edit);
    if (!photoDisplay) {
      this.photoDisplayEditError.set('This photo display is no longer available in this album.');
      return;
    }

    const albumId = this.selectedAlbumId();
    const session = this.albumSession;
    this.pendingMutation.set('edit');
    this.photoDisplayEditError.set(null);
    this.photoApi.editFields(photoDisplay.content.photo.id, {...request}).subscribe({
      next: () => {
        if (!this.completeMutation(albumId, session)) return;
        this.photoDisplayEdit.set(null);
        // Photo metadata may be shared by multiple displays; reload every occurrence.
        this.refreshRequested.emit(albumId);
      },
      error: (error: unknown) => {
        if (!this.isCurrentSession(albumId, session)) return;
        this.pendingMutation.set(null);
        this.photoDisplayEditError.set('Could not save the photo details. Please try again.');
      },
    });
  }

  private resolveEditTarget(edit: PhotoDisplayEdit): PhotoDisplayItem | undefined {
    const photoDisplay = resolvePhotoDisplay(
      this.items().find(item => item.id === edit.rootItemId), edit.photoDisplayId);
    return photoDisplay?.content.photo.id === edit.photoDisplay.content.photo.id
      ? photoDisplay : undefined;
  }

  protected startGrouping() {
    if (this.mutationPending() || this.loadingItems()) return;
    this.collectionGroupingOptInIds.set([]);
    this.collectionGroupingView.set(true);
    this.mutationError.set(null);
  }

  protected cancelGrouping() {
    if (this.mutationPending()) return;
    this.resetGrouping();
    this.mutationError.set(null);
  }

  protected getCollectionGroupingOptStatus(itemId: number): boolean {
    return this.collectionGroupingOptInIds().includes(itemId);
  }

  protected handlePhotoOptChangeEvent(photoDisplayId: number, selected: boolean) {
    if (!this.collectionGroupingView() || this.mutationPending() || this.loadingItems()) return;
    const item = this.items().find(item => item.id === photoDisplayId);
    if (item?.kind !== 'photoDisplay') return;

    this.collectionGroupingOptInIds.update(ids => selected
      ? ids.includes(photoDisplayId) ? ids : [...ids, photoDisplayId]
      : ids.filter(id => id !== photoDisplayId));
  }

  protected rootReorderLeft(albumItem: AlbumItemDto){
    if (albumItem.order == 0) return;
    this.rootReorder(albumItem, albumItem.order - 1)
  }

  protected rootReorderRight(albumItem: AlbumItemDto){
    this.rootReorder(albumItem, albumItem.order + 1)
  }

  protected rootReorder(albumItem: AlbumItemDto, toOrder: number){
    if (this.mutationPending() || this.loadingItems()) return;
    if (albumItem.order == toOrder) return;
    const albumId = this.selectedAlbumId();
    const session = this.albumSession;
    this.pendingMutation.set('rootReorder');
    this.mutationError.set(null);

    this.albumItemApi.rootLevelItemReorder(this.selectedAlbumId(), albumItem.id, toOrder).subscribe({
      next: () => {
        if (!this.completeMutation(albumId, session)) return;
        this.refreshRequested.emit(albumId);
      },
      error: () => {
        if (!this.isCurrentSession(albumId, session)) return;
        this.pendingMutation.set(null);
        this.mutationError.set('Failed to perform reordering.');
      }
    });
  }

  protected createPhotoCollection() {
    const ids = [...this.collectionGroupingOptInIds()];
    if (!this.collectionGroupingView() || ids.length === 0 || this.mutationPending() || this.loadingItems()) return;

    const albumId = this.selectedAlbumId();
    const session = this.albumSession;
    this.pendingMutation.set('createGroup');
    this.mutationError.set(null);
    this.photoDisplayCollectionApi.Post(albumId, ids).subscribe({
      next: () => {
        if (!this.completeMutation(albumId, session)) return;
        this.resetGrouping();
        this.refreshRequested.emit(albumId);
      },
      error: () => {
        if (!this.isCurrentSession(albumId, session)) return;
        this.pendingMutation.set(null);
        this.mutationError.set('Could not create the photo group. Selections has been kept, please try again.');
      },
    });
  }

  protected changePhotoDisplayFields(rootItemId: number, change: PhotoDisplayFieldsChange) {
    if (this.mutationPending() || this.loadingItems()) return;
    const rootItem = this.items().find(item => item.id === rootItemId);
    const photoDisplay = resolvePhotoDisplay(rootItem, change.photoDisplayId);
    if (!photoDisplay || Object.keys(change.changes).length === 0) return;

    const albumId = this.selectedAlbumId();
    const session = this.albumSession;
    const savedChange: AlbumPhotoDisplayFieldsVisibilityChange = {
      albumId,
      rootItemId,
      photoDisplayId: change.photoDisplayId,
      changes: {...change.changes},
    };
    const request = Object.assign(new PhotoDisplayFieldsDisplayedRequest(), savedChange.changes);
    this.pendingMutation.set('fields');
    this.mutationError.set(null);
    this.photoDisplayApi.modifyFieldsDisplayed(albumId, change.photoDisplayId, request).subscribe({
      next: () => {
        if (!this.completeMutation(albumId, session)) return;
        this.photoDisplayFieldsChanged.emit(savedChange);
      },
      error: () => {
        if (!this.isCurrentSession(albumId, session)) return;
        this.pendingMutation.set(null);
        this.mutationError.set('Could not save the photo display settings. Please try again.');
      },
    });
  }

  protected handlePhotoDisplayRemoval(photoDisplayId: number){
    if (this.mutationPending() || this.loadingItems()) return;
    const item = this.items().find(item => item.id === photoDisplayId);
    if (item?.kind !== 'photoDisplay') return;

    const albumId = this.selectedAlbumId();
    const session = this.albumSession;
    this.pendingMutation.set('dissolve');
    this.mutationError.set(null);
    this.photoDisplayApi.deletePhotoDisplay(albumId, photoDisplayId).subscribe({
      next: () => {
          if (!this.completeMutation(albumId, session)) return;
          this.refreshRequested.emit(albumId);
        },
      error: ()=> {
        if (!this.isCurrentSession(albumId, session)) return;
        this.pendingMutation.set(null);
        this.mutationError.set('Failed to delete the PhotoDisplay AlbumItem. Please try again.')
      }
    })
  }

  protected dissolveCollection(itemId: number) {
    if (this.mutationPending() || this.loadingItems()) return;
    const item = this.items().find(item => item.id === itemId);
    if (item?.kind !== 'photoDisplayCollection') return;

    const albumId = this.selectedAlbumId();
    const session = this.albumSession;
    this.pendingMutation.set('dissolve');
    this.mutationError.set(null);
    this.photoDisplayCollectionApi.Dissolve(albumId, itemId).subscribe({
      next: () => {
        if (!this.completeMutation(albumId, session)) return;
        this.refreshRequested.emit(albumId);
      },
      error: () => {
        if (!this.isCurrentSession(albumId, session)) return;
        this.pendingMutation.set(null);
        this.mutationError.set('Could not dissolve the collection. Please try again.');
      },
    });
  }

  private resetGrouping() {
    this.collectionGroupingView.set(false);
    this.collectionGroupingOptInIds.set([]);
  }

  private isCurrentSession(albumId: number, session: number): boolean {
    return !this.destroyed && this.albumSession === session && this.selectedAlbumId() === albumId;
  }

  private completeMutation(albumId: number, session: number): boolean {
    if (!this.isCurrentSession(albumId, session)) {
      if (!this.destroyed && this.selectedAlbumId() === albumId) this.refreshRequested.emit(albumId);
      return false;
    }

    this.pendingMutation.set(null);
    return true;
  }
}
