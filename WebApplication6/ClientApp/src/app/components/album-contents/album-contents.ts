import {Component, input, output} from '@angular/core';
import {AlbumItem, PhotoItem} from '../../models/AlbumInterfacing';
// import {PhotosDisplay} from '../photos-display/photos-display';
import {AdminViewPhotoCard} from '../admin-view-photo-card/admin-view-photo-card';

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
  public readonly photos = input<PhotoItem[]>([]);

  readonly photoStateChange = output<PhotoItem>();
  readonly orderChanged = output(); // reload all photos
  readonly photoDetailedViewRequest = output<PhotoItem>();


}
