import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import type {AlbumItemDto} from '../models/AlbumItemDto';

@Injectable({
  providedIn: 'root',
})
export class AlbumItemApiService {

  constructor() {};

  private readonly http = inject(HttpClient);
  private readonly apiPhotoDisplayUrlPrefix = '/api/albums';

  rootLevelItemReorder(albumId: number, albumItemId: number, orderDestination: number) {
    let requestPath = this.apiPhotoDisplayUrlPrefix + '/' + albumId + '/items/reorder'
    let requestObject = new RootLevelReorderRequest(albumItemId, orderDestination);
    return this.http.post(requestPath, requestObject);
  }

  fetchAlbumItems(albumId: number): Observable<AlbumItemDto[]> {
    const requestPath = this.apiPhotoDisplayUrlPrefix + '/' + albumId + '/items';
    return this.http.get<AlbumItemDto[]>(requestPath);
  }

}


export class RootLevelReorderRequest {
  albumItemId: number;
  orderDestination: number;

  constructor(albumItemId: number, orderDestination: number){
    this.albumItemId = albumItemId;
    this.orderDestination = orderDestination;
  }
}
