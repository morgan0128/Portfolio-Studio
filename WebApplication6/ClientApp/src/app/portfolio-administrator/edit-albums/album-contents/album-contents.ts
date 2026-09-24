import {Component, input, output, signal} from '@angular/core';
import {AdminPhotoDisplayCard} from './admin-photo-display-card/admin-photo-display-card';
import {AlbumDto} from '../../../models/AlbumDto';
import {PhotoDto} from '../../../models/PhotoDto';
import {DetailedPhotoView} from './detailed-photo-view/detailed-photo-view';
import {AlbumItemDto, PhotoDisplayCollectionItem, PhotoDisplayItem} from '../../../models/AlbumItemDto';
import {
  AdminPhotoDisplayCollectionCard
} from './admin-photo-display-collection-card/admin-photo-display-collection-card';


@Component({
  selector: 'app-album-contents',
  imports: [AdminPhotoDisplayCard, DetailedPhotoView, AdminPhotoDisplayCollectionCard],
  templateUrl: './album-contents.html',
  styleUrl: './album-contents.css',
})
export class AlbumContents {
  public readonly selectedAlbum = input<AlbumDto | null>(null);
  public readonly selectedAlbumId = input.required<number>();
  // public readonly loadingPhotos = input<boolean>(false);
  public readonly loadingItems = input<boolean>(false);
  public readonly items = input<AlbumItemDto[]>([]);

  protected readonly detailedViewPhoto = signal<PhotoDto | null>(null);

  // readonly photoStateChange = output<PhotoDto>();
  readonly stateChange = output<AlbumItemDto>();
  readonly multipleItemStatesChange = output(); // reload all photos

  protected readonly collectionGroupingView = signal<boolean>(false);
  protected readonly collectionGroupingOptInIds = signal<number[]>([])

  internalPhotoDisplayStateChange(changed: PhotoDisplayItem){
    const current = this.items().find(
      item => item.id === changed.id
    );
    if (current?.kind !== 'photoDisplay') return;

    this.stateChange.emit({
      ...changed,
      kind: 'photoDisplay',
    });
  }

  internalPhotoDisplayCollectionStateChange(changed: PhotoDisplayCollectionItem){
    const current = this.items().find(
      item => item.id === changed.id
    );
    if (current?.kind !== 'photoDisplayCollection') return;

    this.stateChange.emit({
      ...changed,
      kind: 'photoDisplayCollection'
    });
  }

  handleDetailedPhotoViewRequest(requestPhoto: PhotoDto){
    this.detailedViewPhoto.set(requestPhoto);
  }

  protected toggleCreatePhotoGroupView(){
    this.collectionGroupingView.set(!this.collectionGroupingView());
    this.collectionGroupingOptInIds.set([]);
  }

  // protected getOptStatusForPhotoCard(photo: AlbumPhotoItemDto): boolean {
  //   const val = this.optInPhotoGroupIds().find(value => value === photo.id);
  //   return (val != undefined);
  // }

  protected getCollectionGroupingOptStatus(itemId: number): boolean {
    const optedInId = this.collectionGroupingOptInIds().find(value => value === itemId);
    return (optedInId != undefined);
  }

  protected handlePhotoOptChangeEvent(photoDisplayId: number, newOptStatus: boolean){
    const item = this.items().find(value => value.id === photoDisplayId)
    if (item === undefined || item.kind != 'photoDisplay') return;

    const currOptStatus = this.getCollectionGroupingOptStatus(photoDisplayId);

    if (currOptStatus && !newOptStatus){
      // opt out
      this.collectionGroupingOptInIds.update(ids => ids.filter(id => id !== photoDisplayId));
    }
    else if (!currOptStatus && newOptStatus) {
      // opt in
      this.collectionGroupingOptInIds.update(ids => ids.includes(photoDisplayId) ? ids : [...ids, photoDisplayId]);
    }
  }

  protected createPhotoGroup(){



    this.toggleCreatePhotoGroupView();
  }

}
