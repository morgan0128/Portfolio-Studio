import {
  Component, computed,
  inject,
  input,
  linkedSignal,
  signal,
} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {toObservable, toSignal} from '@angular/core/rxjs-interop';
import {of, pipe, startWith, Subject, switchMap} from 'rxjs';
import {Router} from '@angular/router';
import {PortfolioApiService} from '../../../api/portfolio-api-service';
import {AlbumItem} from '../../../models/AlbumItem';
import {CreatePortfolioPageFromAlbumRequest} from '../../../models/PortfolioPageItemDto';
import {AdminPreviewNavbar} from './admin-preview-navbar/admin-preview-navbar';

@Component({
  selector: 'app-portfolio-manager',
  imports: [
    FormsModule,
    AdminPreviewNavbar
  ],
  templateUrl: './portfolio-page-manager.html',
  styleUrl: './portfolio-page-manager.css',
})
export class PortfolioPageManager {
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

  readonly reorderGlows = signal<Record<number, number>>({});

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

  // protected readonly newPositionValue = signal<number | null>(null)

  readonly selectedMayBeAdded = computed<boolean>(() => this.portfolioPage() != null &&
    this.portfolioPage()!.navbarOrder <= -1 && this.portfolioPage()!.published && this.navbarItems().length < 5);

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
    if (this.portfolioPage() === null || navOrder === null || navOrder > 4 || navOrder < 0) return;
    // let interpretedPosition = navOrder - 1;
    this.portfolioApi.applyNavPosition(this.portfolioPage()!.id, navOrder).subscribe({
      next: () => {
        this.portfolioPage.update(current => current === null ? null : { ...current, navbarOrder: navOrder } );
        this.refreshNavbarState();
      }
    })
  }

  navReorderPageBackward(ppId: number){
    this.navbarItems().forEach((value, index) => {
      if (value.id == ppId){
        if (index === 0){
          return;
        } else {
          let pp1 = value;
          let pp2 = this.navbarItems()[index-1];
          this.portfolioApi.swapNavOrder(pp1.id, pp2.id).subscribe({
            next: () => {
              this.reorderGlows.update(counts => ({
                ...counts,
                [ppId]: (counts[ppId] ?? 0) + 1
              }));

              this.refreshNavbarState();
            }
          })
        }
      }
    })
  }

  navReorderPageForward(ppId: number){
    this.navbarItems().forEach((value, index) => {
      if (value.id == ppId){
        if (index === this.navbarItems().length - 1){
          return;
        } else {
          let pp1 = value;
          let pp2 = this.navbarItems()[index+1];
          this.portfolioApi.swapNavOrder(pp1.id, pp2.id).subscribe({
            next: () => {
              this.reorderGlows.update(counts => ({
                ...counts,
                [ppId]: (counts[ppId] ?? 0) + 1
              }));

              this.refreshNavbarState();
            }
          })
        }
      }
    })

  }


}
