import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {AlbumItem} from '../models/AlbumItem';
import {CreatePortfolioPageFromAlbumRequest, PageLayoutPreset, PortfolioPageItemDto} from '../models/PortfolioPageItemDto';

@Injectable({
  providedIn: 'root',
})
export class PortfolioApiService {
  constructor() {
  };

  private readonly http = inject(HttpClient);
  private readonly apiPortfolioUrl = '/api/Portfolio';

  getPageLayoutPresets(): (Observable<PageLayoutPreset[]>) {
    let requestPath = this.apiPortfolioUrl + '/styling-enums';
    return this.http.get<PageLayoutPreset[]>(requestPath);
  }

  fetchOrCreatePortfolioPage(albumModel: CreatePortfolioPageFromAlbumRequest): (Observable<PortfolioPageItemDto | null>){
    let requestPath = this.apiPortfolioUrl + '/fetch-or-create';
    return this.http.post<PortfolioPageItemDto | null>(requestPath, albumModel);
  }

  applyPageLayoutPreset(ppId: number, layoutPreset: PageLayoutPreset) {
    let requestPath = this.apiPortfolioUrl + '/' + ppId + '/modify/layout-preset';
    return this.http.patch(requestPath, { layoutPreset });
  }

  publishPortfolioPage(ppId: number){
    let requestPath = this.apiPortfolioUrl + '/publish/' + ppId;
    return this.http.patch(requestPath, {});
  }

  unpublishPortfolioPage(ppId: number){
    let requestPath = this.apiPortfolioUrl + '/unpublish/' + ppId;
    return this.http.patch(requestPath, {});
  }

  getPublishedInNavbarOrdered(){
    let requestPath = this.apiPortfolioUrl + '/published/in-nav/ordered'
    return this.http.get<PortfolioPageItemDto[]>(requestPath)
  }

  removeFromNavbar(ppId: number){
    let requestPath = this.apiPortfolioUrl + "/remove-from-nav/" + ppId;
    return this.http.patch(requestPath, {});
  }

  applyNavPosition(ppId: number, navOrder: number){
    // if (position < 0 || position > 4) return;
    let requestPath = this.apiPortfolioUrl + "/" + ppId + "/modify/nav-order";
    return this.http.patch(requestPath, { navOrder });
  }

}
