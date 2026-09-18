import {Component, computed, inject, input, linkedSignal, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {toObservable, toSignal} from '@angular/core/rxjs-interop';
import {of, startWith, Subject, switchMap} from 'rxjs';
import {Router} from '@angular/router';
import {PortfolioApiService} from '../../../api/portfolio-api-service';
import {AlbumItem} from '../../../models/AlbumItem';
import {PageLayoutPreset} from '../../../models/PortfolioPageItemDto';
import {AdminPreviewNavbar} from './admin-preview-navbar/admin-preview-navbar';

@Component({
  selector: 'app-portfolio-manager',
  imports: [FormsModule, AdminPreviewNavbar],
  templateUrl: './portfolio-page-manager.html',
  styleUrl: './portfolio-page-manager.css',
})
export class PortfolioPageManager {
  private readonly router = inject(Router);
  private readonly portfolioApi = inject(PortfolioApiService);

  public readonly selectedAlbum = input.required<AlbumItem | null>();
  private readonly selectedAlbum$ = toObservable(this.selectedAlbum);
  private readonly refreshAlbum$ = new Subject<void>();
  private readonly refreshNavbar$ = new Subject<void>();

  readonly navbarItems = toSignal(this.refreshNavbar$.pipe(
    startWith(undefined),
    switchMap(() => this.portfolioApi.getPublishedInNavbarOrdered())
  ), {initialValue: []});

  protected readonly album = toSignal(this.selectedAlbum$.pipe(
    switchMap(selected => selected === null ? of(null) : this.refreshAlbum$.pipe(
      startWith(undefined),
      switchMap(() => this.portfolioApi.getAlbum(selected.id)),
      startWith(null)
    ))
  ), {initialValue: null});

  protected readonly stylingLayouts = toSignal(this.portfolioApi.getPageLayoutPresets(), {initialValue: null});
  protected readonly selectedStyleLayout = linkedSignal<AlbumItem | null, PageLayoutPreset | null>({
    source: this.album,
    computation: (album, previous) => {
      if (album !== null && album.id === previous?.source?.id &&
          album.layoutPreset === previous.source.layoutPreset) {
        return previous.value;
      }
      return album?.layoutPreset ?? null;
    }
  });

  public readonly viewingNavbarState = signal(false);
  readonly reorderGlows = signal<Record<number, number>>({});
  readonly selectedMayBeAdded = computed(() => {
    const album = this.album();
    return album !== null && album.published && album.navbarOrder === -1 && this.navbarItems().length < 5;
  });

  onApplyStyling() {
    const album = this.album();
    const layoutPreset = this.selectedStyleLayout();
    if (album === null || layoutPreset === null) return;

    this.portfolioApi.applyPageLayoutPreset(album.id, layoutPreset).subscribe({
      next: () => this.refreshAlbum$.next()
    });
  }

  publish() {
    const album = this.album();
    if (album === null) return;
    this.portfolioApi.publishAlbum(album.id).subscribe({
      next: () => this.refreshPresentationState()
    });
  }

  unpublish() {
    const album = this.album();
    if (album === null) return;
    this.portfolioApi.unpublishAlbum(album.id).subscribe({
      next: () => this.refreshPresentationState()
    });
  }

  private refreshPresentationState() {
    this.refreshAlbum$.next();
    this.refreshNavbar$.next();
  }

  requestNavbarView() {
    this.refreshPresentationState();
    this.viewingNavbarState.update(viewing => !viewing);
  }

  previewRequest() {
    const selected = this.selectedAlbum();
    const layout = this.selectedStyleLayout();
    if (selected !== null && layout !== null) {
      this.router.navigate(['/admin-view-page-preview', selected.id, layout]);
    }
  }

  removeFromNavRequest(albumId: number) {
    this.portfolioApi.removeFromNavbar(albumId).subscribe({
      next: () => this.refreshPresentationState()
    });
  }

  applyNavPositionRequest(navOrder: number | null) {
    const album = this.album();
    if (album === null || navOrder === null || navOrder < 0 || navOrder > 4) return;
    this.portfolioApi.applyNavPosition(album.id, navOrder).subscribe({
      next: () => this.refreshPresentationState()
    });
  }

  navReorderPageBackward(albumId: number) {
    this.swapWithNeighbor(albumId, -1);
  }

  navReorderPageForward(albumId: number) {
    this.swapWithNeighbor(albumId, 1);
  }

  private swapWithNeighbor(albumId: number, direction: number) {
    const albums = this.navbarItems();
    const index = albums.findIndex(album => album.id === albumId);
    const neighbor = index + direction;
    if (index < 0 || neighbor < 0 || neighbor >= albums.length) return;

    this.portfolioApi.swapNavOrder(albumId, albums[neighbor].id).subscribe({
      next: () => {
        this.reorderGlows.update(counts => ({...counts, [albumId]: (counts[albumId] ?? 0) + 1}));
        this.refreshPresentationState();
      }
    });
  }
}
