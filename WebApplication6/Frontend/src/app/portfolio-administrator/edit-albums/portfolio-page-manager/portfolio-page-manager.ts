import {Component, computed, inject, input, linkedSignal, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {toObservable, toSignal} from '@angular/core/rxjs-interop';
import {of, startWith, Subject, switchMap} from 'rxjs';
import {Router} from '@angular/router';
import {AlbumDto} from '../../../models/AlbumDto';
import {PageLayoutPreset} from '../../../models/ApiEnums';
import {AdminPreviewNavbar} from './admin-preview-navbar/admin-preview-navbar';
import {AlbumApiService} from '../../../api/album-api-service';

@Component({
  selector: 'app-portfolio-manager',
  imports: [FormsModule, AdminPreviewNavbar],
  templateUrl: './portfolio-page-manager.html',
  styleUrl: './portfolio-page-manager.css',
})
export class PortfolioPageManager {
  private readonly router = inject(Router);
  private readonly albumApi = inject(AlbumApiService);

  public readonly selectedAlbum = input.required<AlbumDto | null>();
  private readonly selectedAlbum$ = toObservable(this.selectedAlbum);
  private readonly refreshAlbum$ = new Subject<void>();
  private readonly refreshNavbar$ = new Subject<void>();

  readonly navbarAlbumsOrdered = toSignal(this.refreshNavbar$.pipe(
    startWith(undefined),
    switchMap(() => this.albumApi.getNavAlbumsOrdered())
  ), {initialValue: []});

  protected readonly album = toSignal(this.selectedAlbum$.pipe(
    switchMap(selected => selected === null ? of(null) : this.refreshAlbum$.pipe(
      startWith(undefined),
      switchMap(() => this.albumApi.getAlbum(selected.id)),
      startWith(null)
    ))
  ), {initialValue: null});

  protected readonly stylingLayouts = toSignal(this.albumApi.getPageLayoutPresets(), {initialValue: null});
  protected readonly selectedStyleLayout = linkedSignal<AlbumDto | null, PageLayoutPreset | null>({
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
    return album !== null && album.published && album.navbarOrder === -1 && this.navbarAlbumsOrdered().length < 5;
  });

  onApplyStyling() {
    const album = this.album();
    const layoutPreset = this.selectedStyleLayout();
    if (album === null || layoutPreset === null) return;

    this.albumApi.updateAlbumLayoutPreset(album.id, layoutPreset).subscribe({
      next: () => this.refreshAlbum$.next()
    });
  }

  publish() {
    const album = this.album();
    if (album === null) return;
    this.albumApi.publishAlbum(album.id).subscribe({
      next: () => this.refreshPresentationState()
    });
  }

  unpublish() {
    const album = this.album();
    if (album === null) return;
    this.albumApi.unpublishAlbum(album.id).subscribe({
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
    this.albumApi.removeFromNavbar(albumId).subscribe({
      next: () => this.refreshPresentationState()
    });
  }

  applyNavPositionRequest(navOrder: number | null) {
    const album = this.album();
    if (album === null || navOrder === null || navOrder < 0 || navOrder > 4) return;
    this.albumApi.assignNavOrder(album.id, navOrder).subscribe({
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
    const albums = this.navbarAlbumsOrdered();
    const index = albums.findIndex(album => album.id === albumId);
    const neighbor = index + direction;
    if (index < 0 || neighbor < 0 || neighbor >= albums.length) return;

    this.albumApi.swapNavOrder(albumId, albums[neighbor].id).subscribe({
      next: () => {
        this.reorderGlows.update(counts => ({...counts, [albumId]: (counts[albumId] ?? 0) + 1}));
        this.refreshPresentationState();
      }
    });
  }
}
