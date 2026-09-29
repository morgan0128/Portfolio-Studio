import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AlbumApiService } from '../../api/album-api-service';
import { AlbumItemApiService } from '../../api/album-item-api-service';
import { AlbumDto } from '../../models/AlbumDto';
import { AlbumItemDto, PhotoDisplayCollectionItem, PhotoDisplayItem } from '../../models/AlbumItemDto';

@Component({
  selector: 'app-portfolio-page-cozy',
  imports: [],
  templateUrl: './portfolio-page-cozy.html',
  styleUrl: './portfolio-page-cozy.css',
})
export class PortfolioPageCozy implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly albumApi = inject(AlbumApiService);
  private readonly albumItemApi = inject(AlbumItemApiService);

  protected readonly album = signal<AlbumDto | null>(null);
  protected readonly items = signal<AlbumItemDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal(false);
  private readonly selectedCollectionIndexes = signal<Record<number, number>>({});

  ngOnInit(): void {
    const albumId = Number(this.route.snapshot.paramMap.get('albumId'));
    if (!Number.isInteger(albumId) || albumId <= 0) {
      this.loading.set(false);
      this.loadError.set(true);
      return;
    }

    forkJoin({
      album: this.albumApi.getAlbum(albumId),
      items: this.albumItemApi.fetchAlbumItems(albumId),
    }).subscribe({
      next: ({ album, items }) => {
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
      },
    });
  }

  protected selectedPhoto(collection: PhotoDisplayCollectionItem): PhotoDisplayItem | null {
    const photos = collection.content.photoDisplays;
    return photos[this.selectedCollectionIndexes()[collection.id] ?? 0] ?? null;
  }

  protected collectionPosition(collection: PhotoDisplayCollectionItem): number {
    return (this.selectedCollectionIndexes()[collection.id] ?? 0) + 1;
  }

  protected moveCollection(collection: PhotoDisplayCollectionItem, direction: number): void {
    const count = collection.content.photoDisplays.length;
    if (count <= 1) return;
    const current = this.selectedCollectionIndexes()[collection.id] ?? 0;
    this.selectedCollectionIndexes.update(indexes => ({
      ...indexes,
      [collection.id]: (current + direction + count) % count,
    }));
  }

  protected labelNumber(value: number): string {
    return String(value).padStart(2, '0');
  }

  protected returnToEditor(): void {
    this.router.navigate(['/edit-albums'], {
      state: { selectedAlbumId: this.album()?.id },
    });
  }
}
