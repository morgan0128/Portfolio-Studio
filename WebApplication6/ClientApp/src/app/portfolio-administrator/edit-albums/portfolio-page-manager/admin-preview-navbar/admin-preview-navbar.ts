import {Component, input, output} from '@angular/core';
import {PortfolioPageItemDto} from '../../../../models/PortfolioPageItemDto';

@Component({
  selector: 'app-admin-preview-navbar',
  imports: [],
  templateUrl: './admin-preview-navbar.html',
  styleUrl: './admin-preview-navbar.css',
})
export class AdminPreviewNavbar {
  public readonly navbarItems = input.required<PortfolioPageItemDto[]>();

  public readonly selectedMayBeAdded = input.required<boolean>();

  public readonly removeFromNavRequest = output<number>();
  public readonly reorderInNavForwardRequest = output<number>();
  public readonly reorderInNavBackwardRequest = output<number>();
  public readonly navInsertSelectedAtOrder = output<number>();
}
