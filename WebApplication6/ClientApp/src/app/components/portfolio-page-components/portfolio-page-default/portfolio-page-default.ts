import {Component, inject, input, signal} from '@angular/core';
import {AlbumApiService} from '../../../services/album-api-service';
import {PortfolioApiService} from '../../../services/portfolio-api-service';
import {AlbumItem} from '../../../models/AlbumInterfacing';
import {FullViewAlbumPhoto} from '../../../models/FullViewAlbumPhoto';

@Component({
  selector: 'app-portfolio-page-default',
  imports: [],
  templateUrl: './portfolio-page-default.html',
  styleUrl: './portfolio-page-default.css',
})
export class PortfolioPageDefault {
  protected readonly albumApi = inject(AlbumApiService);
  protected readonly portfolioApi = inject(PortfolioApiService);

  public album = input.required<AlbumItem | null>();
  protected fullViewPhotos = signal<FullViewAlbumPhoto[]>([]);

  protected loadingPhotos = signal<boolean>(true);






}
