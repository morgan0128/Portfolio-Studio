import {Component, computed, inject, OnInit, signal} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {forkJoin} from 'rxjs';
import {AlbumApiService} from '../../api/album-api-service';
import {AlbumItemApiService} from '../../api/album-item-api-service';
import {AlbumDto} from '../../models/AlbumDto';
import {AlbumItemDto} from '../../models/AlbumItemDto';
import {PortfolioPagePhotoDisplay} from '../portfolio-page-photo-display/portfolio-page-photo-display';
import {PortfolioPagePhotoDisplayCollection} from '../portfolio-page-photo-display-collection/portfolio-page-photo-display-collection';

@Component({
  selector: 'app-portfolio-page-spooky',
  imports: [PortfolioPagePhotoDisplay, PortfolioPagePhotoDisplayCollection],
  templateUrl: './portfolio-page-spooky.html',
  styleUrl: './portfolio-page-spooky.css',
})
export class PortfolioPageSpooky implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly albumApi = inject(AlbumApiService);
  private readonly albumItemApi = inject(AlbumItemApiService);

  protected readonly album = signal<AlbumDto | null>(null);
  protected readonly items = signal<AlbumItemDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal(false);
  protected readonly albumTitle = computed(() => this.album()?.name?.trim() || this.album()?.navTitle || 'Untitled album');

  ngOnInit(): void {
    const albumId = Number(this.route.snapshot.paramMap.get('albumId'));
    if (!Number.isInteger(albumId) || albumId <= 0) {
      this.loadError.set(true);
      this.loading.set(false);
      return;
    }

    forkJoin({
      album: this.albumApi.getAlbum(albumId),
      items: this.albumItemApi.fetchAlbumItems(albumId)
    }).subscribe({
      next: ({album, items}) => {
        if (album === null) {
          this.loadError.set(true);
        } else {
          this.album.set(album);
          this.items.set(items);
        }
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set(true);
        this.loading.set(false);
      }
    });
  }

  protected returnToAlbums(): void {
    this.router.navigate(['/edit-albums'], {
      state: {selectedAlbumId: this.album()?.id}
    });
  }
}
