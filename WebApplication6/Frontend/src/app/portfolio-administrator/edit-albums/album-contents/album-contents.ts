import {Component, computed, inject, input, OnChanges, OnDestroy, output, signal, SimpleChanges} from '@angular/core';
import {AlbumDto} from '../../../models/AlbumDto';
import {PhotoDto} from '../../../models/PhotoDto';
import {DetailedPhotoView} from './detailed-photo-view/detailed-photo-view';
import {AlbumItemDto} from '../../../models/AlbumItemDto';
import {PhotoDisplayCollectionApiService} from '../../../api/photo-display-collection-api-service';
import {PhotoDisplayApiService, PhotoDisplayFieldsDisplayedRequest} from '../../../api/photo-display-api-service';
import {AdminRootAlbumItemWrapper} from './admin-album-item-wrapper/admin-root-album-item-wrapper.component';
import {AlbumPhotoDisplayFieldsChange, PhotoDisplayFieldsChange} from './album-item-actions';
import {AlbumItemApiService} from '../../../api/album-item-api-service';

@Component({
  selector: 'app-album-contents',
  imports: [DetailedPhotoView, AdminRootAlbumItemWrapper],
  templateUrl: './album-contents.html',
  styleUrl: './album-contents.css',
})
export class AlbumContents implements OnChanges, OnDestroy {
  private readonly albumItemApi = inject(AlbumItemApiService);
  private readonly photoDisplayCollectionApi = inject(PhotoDisplayCollectionApiService);
  private readonly photoDisplayApi = inject(PhotoDisplayApiService);
  private albumSession = 0;
  private destroyed = false;

  public readonly selectedAlbum = input.required<AlbumDto | null>();
  public readonly selectedAlbumId = input.required<number>();
  public readonly loadingItems = input<boolean>(false);
  public readonly items = input<AlbumItemDto[]>([]);

  readonly photoDisplayFieldsChanged = output<AlbumPhotoDisplayFieldsChange>();
  readonly refreshRequested = output<number>();

  protected readonly detailedViewPhoto = signal<PhotoDto | null>(null);
  protected readonly collectionGroupingView = signal(false);
  protected readonly collectionGroupingOptInIds = signal<number[]>([]);
  private readonly pendingMutation = signal<'createGroup' | 'fields' | 'dissolve' | 'rootReorder' | null>(null);
  protected readonly mutationPending = computed(() => this.pendingMutation() !== null);
  protected readonly creatingPhotoGroup = computed(() => this.pendingMutation() === 'createGroup');
  protected readonly mutationError = signal<string | null>(null);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['selectedAlbumId']) {
      this.albumSession++;
      this.pendingMutation.set(null);
      this.mutationError.set(null);
      this.resetGrouping();
      this.detailedViewPhoto.set(null);
    }
  }

  ngOnDestroy() {
    this.destroyed = true;
    this.albumSession++;
  }

  protected handleDetailedPhotoViewRequest(photo: PhotoDto) {
    this.detailedViewPhoto.set(photo);
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

  protected createPhotoGroup() {
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
    const photoDisplay = rootItem?.kind === 'photoDisplay'
      ? rootItem.id === change.photoDisplayId ? rootItem : undefined
      : rootItem?.content.photoDisplays.find(photo => photo.id === change.photoDisplayId);
    if (!photoDisplay || Object.keys(change.changes).length === 0) return;

    const albumId = this.selectedAlbumId();
    const session = this.albumSession;
    const savedChange: AlbumPhotoDisplayFieldsChange = {
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
