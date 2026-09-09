import {Component, inject, input, output, signal} from '@angular/core';
// import {NavbarItem} from '../../../models/NavbarItem';
import {PortfolioApiService} from '../../../services/portfolio-api-service';
import {PortfolioPageItem} from '../../../models/PortfolioInterfacing';

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

  protected navbarItems = signal<PortfolioPageItem[]>([]);

  protected populatingNavbar = signal<boolean>(true);
  protected navbarPopulationFailure = signal<boolean>(false);

  public adminPreviewMode = input<boolean>(false);
  // protected adminPreviewModeBlockedRequest // display message like 'cannot navigate to other pages in preview mode'

  public requestNavToPageOfPPId = output<number>();

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
      // TODO
      // display message
      // this.adminPreviewModeBlockedRequest.....

      return;
    }

    this.requestNavToPageOfPPId.emit(this.navbarItems()[order].id);
  }

}
