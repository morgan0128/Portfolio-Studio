import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class PhotoDisplayCollectionApiService {
  constructor(){};

  private readonly http = inject(HttpClient);
  private readonly apiUrlPrefix = '/api/album';
  private readonly apiUrlSuffix = '/photo-display-collection'

  /* POST */
  Post(albumId: number, photoDisplayIds: number[]){
    let requestPath = this.apiUrlPrefix + '/' + albumId + this.apiUrlSuffix;
    let requestObject = { photoDisplayIds };
    return this.http.post(requestPath, requestObject);
  }

}
