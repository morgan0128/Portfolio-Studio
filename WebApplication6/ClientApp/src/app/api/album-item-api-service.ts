import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class AlbumItemApiService {

  constructor() {};

  private readonly http = inject(HttpClient);
  private readonly apiPhotoDisplayUrlPrefix = '/api/album';

  rootLevelItemReorder(albumId: number, albumItemId: number, orderDestination: number) {
    let requestPath = this.apiPhotoDisplayUrlPrefix + '/' + albumId + '/album-item/reorder'
    let requestObject = new RootLevelReorderRequest(albumItemId, orderDestination);
    return this.http.post(requestPath, requestObject);
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
