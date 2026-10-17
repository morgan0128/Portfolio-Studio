import {Component, computed, input, output} from '@angular/core';
import {PhotoDisplayItem} from '../../../../../models/AlbumItemDto';
import {PhotoDto} from '../../../../../models/PhotoDto';
import {ImageDisplay} from '../../../../../common/image-display/ImageDisplay';
import {PhotoDisplayFieldsChange} from '../../album-item-actions';

@Component({
  selector: 'app-admin-photo-display-card',
  imports: [ImageDisplay],
  templateUrl: './admin-photo-display-card.html',
  styleUrl: './admin-photo-display-card.css',
})
export class AdminPhotoDisplayCard {
  readonly item = input.required<PhotoDisplayItem>();
  readonly mutationPending = input(false);

  readonly photoDisplay = computed(() => this.item().content);
  readonly photo = computed(() => this.photoDisplay().photo);
  readonly displaysNameColor = computed(() => this.photoDisplay().displaysName ? 'green' : 'red');
  readonly displaysDescColor = computed(() => this.photoDisplay().displaysDescription ? 'green' : 'red');
  readonly displaysYearColor = computed(() => this.photoDisplay().displaysYearContentCreated ? 'green' : 'red');

  readonly photoDisplayFieldsChangeRequested = output<PhotoDisplayFieldsChange>();
  readonly photoDetailedViewRequest = output<PhotoDto>();

  sendPhotoViewRequest(){
    this.photoDetailedViewRequest.emit(this.photo());
  }

  toggleDisplaysName(){
    this.requestDisplayedFieldsChange({ displaysName: !this.photoDisplay().displaysName });
  }

  toggleDisplaysDesc(){
    this.requestDisplayedFieldsChange({ displaysDescription: !this.photoDisplay().displaysDescription });
  }

  toggleDisplaysYearCC(){
    this.requestDisplayedFieldsChange({ displaysYearContentCreated: !this.photoDisplay().displaysYearContentCreated });
  }

  private requestDisplayedFieldsChange(changes: PhotoDisplayFieldsChange['changes']){
    if (this.mutationPending()) return;

    this.photoDisplayFieldsChangeRequested.emit({ photoDisplayId: this.item().id, changes });
  }
}
