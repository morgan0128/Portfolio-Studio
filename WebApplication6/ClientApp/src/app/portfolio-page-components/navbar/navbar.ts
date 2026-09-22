import {Component, inject, input, output, signal} from '@angular/core';
import {AlbumDto} from '../../models/AlbumDto';
import {AlbumApiService} from '../../api/album-api-service';


interface onInit {
}

@Component({
  selector: 'app-navbar',
  imports: [],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements onInit {
  private albumApi = inject(AlbumApiService)

  protected navbarAlbums = signal<AlbumDto[]>([]);

  protected populatingNavbar = signal<boolean>(true);
  protected navbarPopulationFailure = signal<boolean>(false);

  public adminPreviewMode = input<boolean>(false);
  // protected adminPreviewModeBlockedRequest // display message like 'cannot navigate to other pages in preview mode'

  public requestNavToAlbumOfId = output<number>();
  adminModeBlockedNavigation = output<void>();
  public requestNavToEditAlbums = output<void>();

  ngOnInit() {
    this.albumApi.getNavAlbumsOrdered().subscribe({
      next: albums => {
        this.navbarAlbums.set(albums);
        this.populatingNavbar.set(false);
      },
      error: () => {
        this.navbarPopulationFailure.set(true);
        this.populatingNavbar.set(false);
      }
    });
  }

  navigateToPage(order: number) {
    if (order < 0 || order >= this.navbarAlbums().length){
      return;
    }

    if (this.adminPreviewMode()){
      this.adminModeBlockedNavigation.emit();

      return;
    }

    this.requestNavToAlbumOfId.emit(this.navbarAlbums()[order].id);
  }


  returnToEditAlbums(){
    if (!this.adminPreviewMode()){
      return;
    }

    this.requestNavToEditAlbums.emit();
  }

}
