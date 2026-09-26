import {Component, computed, input, signal} from '@angular/core';
import {PortfolioPagePhotoDisplay} from '../portfolio-page-photo-display/portfolio-page-photo-display';
import {PhotoDisplayCollectionDto, PhotoDisplayCollectionItem, PhotoDisplayItem} from '../../models/AlbumItemDto';

@Component({
  selector: 'app-portfolio-page-photo-display-collection',
  imports: [
    PortfolioPagePhotoDisplay
  ],
  templateUrl: './portfolio-page-photo-display-collection.html',
  styleUrl: './portfolio-page-photo-display-collection.css',
})
export class PortfolioPagePhotoDisplayCollection {
  readonly item = input.required<PhotoDisplayCollectionItem>();
  readonly photoDisplayCollection = computed<PhotoDisplayCollectionDto>(() => this.item().content);
  readonly displayMode = computed<number>(() =>  this.photoDisplayCollection().displayMode);
  readonly photoDisplays = computed<PhotoDisplayItem[]>(() => this.photoDisplayCollection().photoDisplays);

  readonly displayIndex = signal<number>(0);
  readonly selectedPhotoDisplay = computed<PhotoDisplayItem | null>(() =>
    (this.displayIndex() >= 0 && this.displayIndex() < this.photoDisplays().length) ?
      this.photoDisplays()[this.displayIndex()] : null);


  nextPhoto() {
    if (this.photoDisplays().length <= 1){
      return;
    }

    if (this.displayIndex() + 1 >= this.photoDisplays().length){
      this.displayIndex.set(0);
    } else {
      this.displayIndex.set(this.displayIndex() + 1);
    }

  }

  prevPhoto() {
    if (this.photoDisplays().length <= 1){
      return;
    }

    if (this.displayIndex() -1 < 0){
      this.displayIndex.set(this.photoDisplays().length - 1);
    } else {
      this.displayIndex.set(this.displayIndex() - 1);
    }

  }
}
