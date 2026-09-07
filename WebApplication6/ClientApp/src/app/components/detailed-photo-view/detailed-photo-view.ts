import {Component, input, output} from '@angular/core';
import {PhotoItem} from '../../models/AlbumInterfacing';

@Component({
  selector: 'app-detailed-photo-view',
  imports: [],
  templateUrl: './detailed-photo-view.html',
  styleUrl: './detailed-photo-view.css',
})
export class DetailedPhotoView {
  photo = input.required<PhotoItem>();

  public closePreview = output<void>();
}
