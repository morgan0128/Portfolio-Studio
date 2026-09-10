import {Component, input, output} from '@angular/core';
import {AlbumPhotoItemDto} from '../../../../models/AlbumPhotoItemDto';


@Component({
  selector: 'app-detailed-photo-view',
  imports: [],
  templateUrl: './detailed-photo-view.html',
  styleUrl: './detailed-photo-view.css',
})
export class DetailedPhotoView {
  photo = input.required<AlbumPhotoItemDto>();

  public closePreview = output<void>();
}
