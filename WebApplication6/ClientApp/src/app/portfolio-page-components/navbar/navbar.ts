import {Component, inject, input, output, signal} from '@angular/core';
import {PortfolioApiService} from '../../api/portfolio-api-service';
import {PortfolioPageItemDto} from '../../models/PortfolioPageItemDto';


interface onInit {
}

@Component({
  selector: 'app-navbar',
  imports: [],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements onInit {
  private portfolioApi = inject(PortfolioApiService)

  protected navbarItems = signal<PortfolioPageItemDto[]>([]);

  protected populatingNavbar = signal<boolean>(true);
  protected navbarPopulationFailure = signal<boolean>(false);

  public adminPreviewMode = input<boolean>(false);
  // protected adminPreviewModeBlockedRequest // display message like 'cannot navigate to other pages in preview mode'

  public requestNavToPortfolioPageOfId = output<number>();
  adminModeBlockedNavigation = output<void>();
  public requestNavToEditAlbums = output<void>();

  ngOnInit() {
    this.portfolioApi.getPublishedInNavbarOrdered().subscribe({
      next: ppItems => {
        this.navbarItems.set(ppItems);
        this.populatingNavbar.set(false);
      },
      error: () => {
        this.navbarPopulationFailure.set(true);
        this.populatingNavbar.set(false);
      }
    });
  }

  navigateToPage(order: number) {
    if (order < 0 || order >= this.navbarItems().length){
      return;
    }

    if (this.adminPreviewMode()){
      this.adminModeBlockedNavigation.emit();

      return;
    }

    this.requestNavToPortfolioPageOfId.emit(this.navbarItems()[order].id);
  }


  returnToEditAlbums(){
    if (!this.adminPreviewMode()){
      return;
    }

    this.requestNavToEditAlbums.emit();
  }

}
