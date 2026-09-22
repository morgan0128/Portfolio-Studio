import {Component, computed, effect, inject, input, output, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {NgOptimizedImage} from '@angular/common';
import {AlbumApiService} from '../../../../api/album-api-service';
import {PhotoDto} from '../../../../models/PhotoDto';
import {PhotoDisplayDto} from '../../../../models/PhotoDisplayDto';
import {AlbumItemDto} from '../../../../models/AlbumItemDto';
import {PhotoDisplayApiService, PhotoDisplayFieldsDisplayedRequest} from '../../../../api/photo-display-api-service';

@Component({
  selector: 'app-admin-view-photo-card',
  imports: [
    NgOptimizedImage,
    FormsModule,
  ],
  templateUrl: './admin-view-photo-display-card.html',
  styleUrl: './admin-view-photo-display-card.css',
})
export class AdminViewPhotoDisplayCard {
  // private readonly albumApi = inject(AlbumApiService);
  private readonly photoDisplayApi = inject(PhotoDisplayApiService);

  readonly albumId = input.required<number>();
  readonly item = input.required<AlbumItemDto>();

  readonly incompatibleAlbumItemError = computed<boolean>(() => (this.item().kind !== 'photoDisplay'));

  readonly photoDisplay = computed<PhotoDisplayDto | null>(() =>
    !this.incompatibleAlbumItemError() ? <PhotoDisplayDto>this.item().content : null);

  readonly displaysNameColor = computed<string | null>(() =>
    this.photoDisplay() !== null ? ((this.photoDisplay()!.displaysName) ? 'green' : 'red') : null);
  readonly displaysDescColor = computed<string | null>(() =>
    this.photoDisplay() !== null ? ((this.photoDisplay()!.displaysDescription) ? 'green' : 'red') : null);
  readonly displaysYearColor = computed<string | null>(() =>
    this.photoDisplay() !== null ? ((this.photoDisplay()!.displaysYearContentCreated) ? 'green' : 'red') : null);

  readonly photo = computed<PhotoDto | null>(() => this.photoDisplay() !== null ? this.photoDisplay()!.photo : null);

  readonly stateChanged = output<AlbumItemDto>();
  readonly itemStatesChanged = output(); // reload all photos // TODO: operations affecting multiple item states belongs in album-contents

  /* TODO: operations affecting multiple item states belongs in album-contents
  // protected readonly editingOrder = signal<boolean>(false);
  // newOrderValue: number = -1;
  */

  readonly photoDetailedViewRequest = output<PhotoDto>();

  readonly groupingView = input.required<boolean>();
  readonly includeInNewGroup = input<boolean>(false);
  readonly collectionGroupingOptStatusChanged = output<boolean>();

  // constructor() {
  //   // effect(() => {
  //   //   if (this.photo().order !== null){
  //   //     this.newOrderValue = this.photo().order!;
  //   //   }
  //   //   });
  // }

  /* TODO: operations affecting multiple item states belongs in album-contents
  // toggleOrderVisibilityView(){
    // this.editingOrder.set(!this.editingOrder());
  // }


  updateOrder(val: number){
    if (this.photo().id === null) return;
    let request = this.albumApi.reorder(this.albumId(), this.photo().id!, this.newOrderValue);
    request.subscribe({
      next: () => {
        this.orderChanged.emit();
      }
    })
  }
  */

  sendPhotoViewRequest(){
    if (this.photo() === null) return;
    this.photoDetailedViewRequest.emit(this.photo()!);
  }

  toggleDisplaysName(){
    if (this.item() === null || this.incompatibleAlbumItemError()) return;
    const displaysName = this.photoDisplay()?.displaysName;
    if (displaysName === undefined) return; // incompatibleAlbumItemError incongruency

    const updatedSettings = new PhotoDisplayFieldsDisplayedRequest();
    updatedSettings.displaysName = !displaysName;
    this.modifyDisplayedFields(this.albumId(), this.item().id, updatedSettings);
  }

  toggleDisplaysDesc(){
    if (this.item() === null || this.incompatibleAlbumItemError()) return;
    const displaysDescription = this.photoDisplay()?.displaysDescription;
    if (displaysDescription === undefined) return; // incompatibleAlbumItemError incongruency

    const updatedSettings = new PhotoDisplayFieldsDisplayedRequest();
    updatedSettings.displaysDescription = !displaysDescription;
    this.modifyDisplayedFields(this.albumId(), this.item().id, updatedSettings);
  }

  toggleDisplaysYearCC(){
    if (this.item() === null || this.incompatibleAlbumItemError()) return;
    const displaysYearContentCreated = this.photoDisplay()?.displaysYearContentCreated;
    if (displaysYearContentCreated === undefined) return; // incompatibleAlbumItemError incongruency

    const updatedSettings = new PhotoDisplayFieldsDisplayedRequest();
    updatedSettings.displaysYearContentCreated = !displaysYearContentCreated;
    this.modifyDisplayedFields(this.albumId(), this.item().id, updatedSettings);
  }

  private modifyDisplayedFields(albumId: number, photoDisplayId: number, updatedDisplaySettings: PhotoDisplayFieldsDisplayedRequest){
    let request = this.photoDisplayApi.modifyFieldsDisplayed(albumId, photoDisplayId, updatedDisplaySettings);
    request.subscribe({
      next: () => {
        if (this.item().id !== photoDisplayId) return;
        const updated = this.item();
        if (updated.kind != 'photoDisplay') return; // incompatibleAlbumItemError incongruency

        updated.content.displaysName = updatedDisplaySettings.displaysName ?? updated.content.displaysName;
        updated.content.displaysDescription = updatedDisplaySettings.displaysDescription ?? updated.content.displaysDescription;
        updated.content.displaysYearContentCreated = updatedDisplaySettings.displaysYearContentCreated ?? updated.content.displaysYearContentCreated;

        this.stateChanged.emit(updated);
      }
    });
  }

}
