import {Component, input, output} from '@angular/core';
import {AlbumItem} from '../../../../models/AlbumItem';
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
  public readonly navbarItems = input.required<AlbumItem[]>();

  public readonly reorderGlows = input<Record<number, number>>({});

  public readonly selectedMayBeAdded = input.required<boolean>();

  public readonly removeFromNavRequest = output<number>();
  public readonly reorderInNavForwardRequest = output<number>();
  public readonly reorderInNavBackwardRequest = output<number>();
  public readonly navInsertSelectedAtOrder = output<number>();
}
