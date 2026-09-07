import {Component, computed, effect, inject, input, output, signal} from '@angular/core';
import {PhotoItem} from '../../models/AlbumInterfacing';
import {NgOptimizedImage} from '@angular/common';
import { AlbumApiCaller } from '../../services/album-api-caller';
import {CdkDrag, CdkDragHandle} from '@angular/cdk/drag-drop';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-admin-view-photo-card',
  imports: [
    NgOptimizedImage,
    FormsModule,
    // CdkDrag,
    // CdkDragHandle
  ],
  templateUrl: './admin-view-photo-card.html',
  styleUrl: './admin-view-photo-card.css',
})
export class AdminViewPhotoCard {
  private readonly albumApi = inject(AlbumApiCaller);

  albumId = input.required<number>();
  photo = input.required<PhotoItem>();

  displaysNameColor = computed<string | null>(() => (this.photo().displaysName) ? 'green' : 'red');
  displaysDescColor = computed<string | null>(() => (this.photo().displaysDescription) ? 'green' : 'red');
  displaysYearCCColor = computed<string | null>(() => (this.photo().displaysYearCC) ? 'green' : 'red');

  readonly photoStateChange = output<PhotoItem>();

  readonly orderChanged = output(); // reload all photos

  protected readonly editingOrder = signal<boolean>(false);
  newOrderValue: number = -1;

  readonly photoDetailedViewRequest = output<PhotoItem>();

  constructor() {
    effect(() => {
      if (this.photo().order != null){
        this.newOrderValue = this.photo().order!;
      }
      });
  }

  toggleOrderVisibilityView(){
    this.editingOrder.set(!this.editingOrder());
  }

  updateOrder(val: number){
    if (this.photo().id == null) return;
    let request = this.albumApi.reorder(this.albumId(), this.photo().id!, this.newOrderValue);
    request.subscribe({
      next: () => {
        this.orderChanged.emit();
      }
    })
  }

  toggleDisplaysName(){
    if (this.photo().id == null) return;
    let request = this.albumApi.toggleDisplaysName(this.albumId(), this.photo().id!);
    request.subscribe({
      next: () => {
        const updatedPhoto = {
          ...this.photo(),
          displaysName: !this.photo().displaysName
        };

        this.photoStateChange.emit(updatedPhoto);
        return;
      }
    })
  }

  toggleDisplaysDesc(){
    if (this.photo().id == null) return;
    let request = this.albumApi.toggleDisplaysDescription(this.albumId(), this.photo().id!);
    request.subscribe({
      next: () => {
        const updatedPhoto = {
          ...this.photo(),
          displaysDescription: !this.photo().displaysDescription
        };

        this.photoStateChange.emit(updatedPhoto);
        return;
      }
    })
  }

  toggleDisplaysYearCC(){
    if (this.photo().id == null) return;
    let request = this.albumApi.toggleDisplaysYearCC(this.albumId(), this.photo().id!);
    request.subscribe({
      next: () => {
        const updatedPhoto = {
          ...this.photo(),
          displaysYearCC: !this.photo().displaysYearCC
        };

        this.photoStateChange.emit(updatedPhoto);
        return;
      }
    })
  }

}
