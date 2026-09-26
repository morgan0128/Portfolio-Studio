import {Component, input, output} from '@angular/core';
import {AlbumDto} from '../../../../models/AlbumDto';
import {ReorderGlowDirective} from './reorder-glow-directive';

@Component({
  selector: 'app-admin-preview-navbar',
  imports: [
    ReorderGlowDirective
  ],
  templateUrl: './admin-preview-navbar.html',
  styleUrl: './admin-preview-navbar.css',
})
export class AdminPreviewNavbar {
  public readonly navbarItems = input.required<AlbumDto[]>();

  public readonly reorderGlows = input<Record<number, number>>({});

  public readonly selectedMayBeAdded = input.required<boolean>();

  public readonly removeFromNavRequest = output<number>();
  public readonly reorderInNavForwardRequest = output<number>();
  public readonly reorderInNavBackwardRequest = output<number>();
  public readonly navInsertSelectedAtOrder = output<number>();
}
