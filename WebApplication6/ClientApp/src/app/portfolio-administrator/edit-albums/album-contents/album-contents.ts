import {Component, input, output} from '@angular/core';
import {AdminViewPhotoCard} from './admin-view-photo-card/admin-view-photo-card';
import {AlbumItem} from '../../../models/AlbumItem';
import {AlbumPhotoItemDto} from '../../../models/AlbumPhotoItemDto';


@Component({
  selector: 'app-album-contents',
  imports: [AdminViewPhotoCard],
  templateUrl: './album-contents.html',
  styleUrl: './album-contents.css',
})
export class AlbumContents {
  public readonly selectedAlbum = input<AlbumItem | null>(null);
  public readonly selectedAlbumId = input.required<number>();
  public readonly loadingPhotos = input<boolean>(false);
  public readonly photos = input<AlbumPhotoItemDto[]>([]);

  readonly photoStateChange = output<AlbumPhotoItemDto>();
  readonly orderChanged = output(); // reload all photos
  readonly photoDetailedViewRequest = output<AlbumPhotoItemDto>();


}
