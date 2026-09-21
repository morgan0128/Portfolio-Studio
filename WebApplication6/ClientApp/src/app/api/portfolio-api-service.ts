import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {AlbumDto} from '../models/AlbumDto';
import {PageLayoutPreset} from '../models/ApiEnums';

@Injectable({ providedIn: 'root' })
export class PortfolioApiService {
  private readonly http = inject(HttpClient);
  private readonly apiPortfolioUrl = '/api/Portfolio';

  // getAlbum(albumId: number) {
  //   return this.http.get<AlbumDto>(this.apiPortfolioUrl + '/' + albumId);
  // }
  //
  // getPageLayoutPresets() {
  //   return this.http.get<PageLayoutPreset[]>(this.apiPortfolioUrl + '/styling-enums');
  // }

  // applyPageLayoutPreset(albumId: number, layoutPreset: PageLayoutPreset) {
  //   return this.http.patch<void>(this.apiPortfolioUrl + '/' + albumId + '/modify/layout-preset', { layoutPreset });
  // }

  // publishAlbum(albumId: number) {
  //   return this.http.patch<number | null>(this.apiPortfolioUrl + '/publish/' + albumId, {});
  // }
  //
  // unpublishAlbum(albumId: number) {
  //   return this.http.patch<void>(this.apiPortfolioUrl + '/unpublish/' + albumId, {});
  // }

  // getPublishedInNavbarOrdered() {
  //   return this.http.get<AlbumDto[]>(this.apiPortfolioUrl + '/published/in-nav/ordered');
  // }

  // removeFromNavbar(albumId: number) {
  //   return this.http.patch<void>(this.apiPortfolioUrl + '/remove-from-nav/' + albumId, {});
  // }

  // applyNavPosition(albumId: number, navOrder: number) {
  //   return this.http.patch<void>(this.apiPortfolioUrl + '/' + albumId + '/modify/nav-order', { navOrder });
  // }

  // swapNavOrder(albumId1: number, albumId2: number) {
  //   return this.http.patch<void>(this.apiPortfolioUrl + '/modify/nav-order/swap', { albumId1, albumId2 });
  // }
}
