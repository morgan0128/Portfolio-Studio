import {Component, input, output, signal} from '@angular/core';
import {AdminPhotoDisplayCard} from './admin-photo-display-card/admin-photo-display-card';
import {AlbumDto} from '../../../models/AlbumDto';
import {PhotoDto} from '../../../models/PhotoDto';
import {DetailedPhotoView} from './detailed-photo-view/detailed-photo-view';
import {AlbumItemDto} from '../../../models/AlbumItemDto';


@Component({
  selector: 'app-album-contents',
  imports: [AdminPhotoDisplayCard, DetailedPhotoView],
  templateUrl: './album-contents.html',
  styleUrl: './album-contents.css',
})
export class AlbumContents {
  // private

  public readonly selectedAlbum = input<AlbumDto | null>(null);
  public readonly selectedAlbumId = input.required<number>();
  // public readonly loadingPhotos = input<boolean>(false);
  public readonly loadingItems = input<boolean>(false);
  public readonly items = input<AlbumItemDto[]>([]);

  protected readonly detailedViewPhoto = signal<PhotoDto | null>(null);

  // readonly photoStateChange = output<PhotoDto>();
  readonly itemStateChanged = output<AlbumItemDto>();
  readonly multipleItemStatesChanged = output(); // reload all photos

  protected readonly collectionGroupingView = signal<boolean>(false);
  protected readonly collectionGroupingOptInIds = signal<number[]>([])

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
