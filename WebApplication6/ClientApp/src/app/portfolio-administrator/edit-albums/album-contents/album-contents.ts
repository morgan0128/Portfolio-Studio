import {Component, input, output, signal} from '@angular/core';
import {AdminViewPhotoCard} from './admin-view-photo-card/admin-view-photo-card';
import {AlbumItem} from '../../../models/AlbumItem';
import {AlbumPhotoItemDto} from '../../../models/AlbumPhotoItemDto';
import {DetailedPhotoView} from './detailed-photo-view/detailed-photo-view';


@Component({
  selector: 'app-album-contents',
  imports: [AdminViewPhotoCard, DetailedPhotoView],
  templateUrl: './album-contents.html',
  styleUrl: './album-contents.css',
})
export class AlbumContents {
  // private

  public readonly selectedAlbum = input<AlbumItem | null>(null);
  public readonly selectedAlbumId = input.required<number>();
  public readonly loadingPhotos = input<boolean>(false);
  public readonly photos = input<AlbumPhotoItemDto[]>([]);

  protected readonly detailedViewPhoto = signal<AlbumPhotoItemDto | null>(null);

  readonly photoStateChange = output<AlbumPhotoItemDto>();
  readonly itemsStateChange = output(); // reload all photos

  protected readonly groupingView = signal<boolean>(false);
  protected readonly optInPhotoGroupIds = signal<number[]>([])

  handleDetailedPhotoViewRequest(requestPhoto: AlbumPhotoItemDto){
    this.detailedViewPhoto.set(requestPhoto);
  }

  protected toggleCreatePhotoGroupView(){
    this.groupingView.set(!this.groupingView());
    this.optInPhotoGroupIds.set([]);
  }

  // protected getOptStatusForPhotoCard(photo: AlbumPhotoItemDto): boolean {
  //   const val = this.optInPhotoGroupIds().find(value => value === photo.id);
  //   return (val != undefined);
  // }

  protected photoOptStatus(photoId: number): boolean {
    const val = this.optInPhotoGroupIds().find(value => value === photoId);
    return (val != undefined);
  }

  protected handlePhotoOptChangeEvent(photoId: number, newOptStatus: boolean){
    if (this.photos().find(value => value.id == photoId) == undefined){
      return;
    }
    const currOptStatus = this.photoOptStatus(photoId);

    if (currOptStatus && !newOptStatus){
      // opt out
      this.optInPhotoGroupIds.update(ids => ids.filter(id => id !== photoId));
    }
    else if (!currOptStatus && newOptStatus) {
      // opt in
      this.optInPhotoGroupIds.update(ids => ids.includes(photoId) ? ids : [...ids, photoId]);
    }
  }

  protected createPhotoGroup(){



    this.toggleCreatePhotoGroupView();
  }

}
