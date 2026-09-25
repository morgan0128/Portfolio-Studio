import {Component, computed, input} from '@angular/core';
import {ImageDisplay} from "../image-display/ImageDisplay";
import {PhotoDisplayDto, PhotoDisplayItem} from '../../models/AlbumItemDto';

@Component({
  selector: 'app-portfolio-page-photo-display',
    imports: [
        ImageDisplay
    ],
  templateUrl: './portfolio-page-photo-display.html',
  styleUrl: './portfolio-page-photo-display.css',
})
export class PortfolioPagePhotoDisplay {

  readonly item = input.required<PhotoDisplayItem>();
  readonly photoDisplay = computed(() => this.item().content);
  readonly photo = computed(() => this.photoDisplay().photo);
  readonly image = computed(() => this.photo().image);

  shouldIncludeTextContentContainer(photoDisplay: PhotoDisplayDto): boolean {
    const photo = photoDisplay.photo;
    return (photoDisplay.displaysName && photo.name !== '' && photo.name !== null) ||
      (photoDisplay.displaysDescription && photo.description !== '' && photo.description !== null) ||
      (photoDisplay.displaysYearContentCreated && photo.yearContentCreated !== null);
  }

}
