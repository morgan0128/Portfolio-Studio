import {
  Component,
  inject,
  input,
  linkedSignal,
  signal,
} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {PortfolioApiService} from '../../services/portfolio-api-service';
import {AlbumItem, PhotoItem} from '../../models/AlbumInterfacing';
import {
  CreatePortfolioPageFromAlbumRequest,
  PageLayoutPreset,
  PortfolioPageItem
} from '../../models/PortfolioInterfacing';
import {toObservable, toSignal} from '@angular/core/rxjs-interop';
import {of, pipe, startWith, Subject, switchMap} from 'rxjs';
import {Router} from '@angular/router';

@Component({
  selector: 'app-portfolio-manager',
  imports: [
    FormsModule
  ],
  templateUrl: './portfolio-manager.html',
  styleUrl: './portfolio-manager.css',
})
export class PortfolioManager {
  private router = inject(Router);

  private readonly portfolioApi = inject(PortfolioApiService);

  public readonly selectedAlbum = input.required<AlbumItem | null>();
  private readonly selectedAlbum$ = toObservable(this.selectedAlbum);

  private readonly refreshNavbar$ = new Subject<void>();
  private readonly navbarItems$ = this.refreshNavbar$.pipe(
    startWith(undefined),
    switchMap(() => this.portfolioApi.getPublishedInNavbarOrdered())
  );

  readonly navbarItems = toSignal(this.navbarItems$, {
    initialValue: []
  });


  public readonly viewingNavbarState = signal<boolean>(false);

  // readonly requestNavbarState = output();

  protected readonly fetchedPortfolioPage = toSignal(
    this.selectedAlbum$.pipe(
      switchMap(album => {
          if (album === null) {
            return of(null);
          }

          const request = new CreatePortfolioPageFromAlbumRequest(album.id, album.name ?? '');

          return this.portfolioApi.fetchOrCreatePortfolioPage(request);
        })
      ),
      { initialValue: null }
  );

  protected readonly portfolioPage = linkedSignal(() => this.fetchedPortfolioPage());

  protected readonly stylingLayouts = toSignal(this.portfolioApi.getPageLayoutPresets(), {initialValue: null});

  protected readonly selectedStyleLayout = linkedSignal({
    source: this.fetchedPortfolioPage, computation: page =>
      page?.layoutPreset ?? null
    });

  protected readonly newPositionValue = signal<number | null>(null)

  onPortfolioPageItemLoaded(){
    // TODO select the stylingLayout associated with the portfolio page
  }

  onApplyStyling() {
    if (this.portfolioPage() === null || this.selectedStyleLayout() === null) return;
    const portfolio = this.portfolioPage()!;
    const layoutPreset = this.selectedStyleLayout()!;



    this.portfolioApi.applyPageLayoutPreset(portfolio.id, layoutPreset)
      .subscribe({
        next: () => {
          this.portfolioPage.update(current =>
            current === null ? null : { ...current, layoutPreset }
          );
        }
      });
  }

  publish(){
    if (this.portfolioPage() === null) return;

    this.portfolioApi.publishPortfolioPage(this.portfolioPage()!.id).subscribe({
      next: () => {
        this.portfolioPage.update(current => current === null ? null : { ...current, published: true } );
        this.refreshNavbarState();
      }
    });
  }

  unpublish(){
    if (this.portfolioPage() === null) return;

    this.portfolioApi.unpublishPortfolioPage(this.portfolioPage()!.id).subscribe({
      next: () => {
        this.portfolioPage.update(current => current === null ? null : { ...current, published: false, navbarOrder: -1 } );
        this.refreshNavbarState();
      }
    });
  }

  refreshNavbarState(){
    this.refreshNavbar$.next();
  }

  requestNavbarView(){
    this.refreshNavbarState();
    this.viewingNavbarState.set(!this.viewingNavbarState());
  }

  previewRequest(){
    if (this.selectedStyleLayout() !== null && this.selectedAlbum() !== null){
      this.router.navigate(['/admin-view-page-preview', this.selectedAlbum()!.id, this.selectedStyleLayout()]);
    }
  }

  removeFromNavRequest(ppId: number){
    this.portfolioApi.removeFromNavbar(ppId).subscribe({
      next: () => {
        if (this.portfolioPage() !== null && this.portfolioPage()?.id === ppId){
          this.portfolioPage.update(current => current === null ? null : { ...current, navbarOrder: -1 } );
        }
        this.refreshNavbarState();
      }
    });
  }

  applyNavPositionRequest(navOrder: number | null){
    if (this.portfolioPage() === null || navOrder === null || navOrder > 5 || navOrder <= 0) return;
    let interpretedPosition = navOrder - 1;
    this.portfolioApi.applyNavPosition(this.portfolioPage()!.id, interpretedPosition).subscribe({
      next: () => {
        this.portfolioPage.update(current => current === null ? null : { ...current, navbarOrder: interpretedPosition } );
        this.refreshNavbarState();
      }
    })


  }


}
