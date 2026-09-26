import {Component, input, output} from '@angular/core';
import {PhotoDto} from '../../../../models/PhotoDto';


@Component({
  selector: 'app-detailed-photo-view',
  imports: [],
  templateUrl: './detailed-photo-view.html',
  styleUrl: './detailed-photo-view.css',
})
export class DetailedPhotoView {
  photo = input.required<PhotoDto>();

  public closePreview = output<void>();
}
