import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {AlbumDto} from '../models/AlbumDto';
import {Observable} from 'rxjs';
import {PhotoDto} from '../models/PhotoDto';
import {PageLayoutPreset} from '../models/ApiEnums';

@Injectable({
  providedIn: 'root',
})
export class AlbumApiService {

  constructor() {};

  private readonly http = inject(HttpClient);
  private readonly apiAlbumUrl = '/api/albums';


  /* POST */
  postAlbum(name: string | null, description: string | null): Observable<number>{
    let requestPath = this.apiAlbumUrl;
    let requestObject = new CreateAlbumRequest();
    requestObject.name = name;
    requestObject.description = description;
    return this.http.post<number>(requestPath, requestObject);
  }


  /* GET */
  getAlbums(): (Observable<AlbumDto[]>) {
    let requestPath = this.apiAlbumUrl;
    return this.http.get<AlbumDto[]>(requestPath);
  }

  getAlbumIds(): (Observable<number[]>){
    let requestPath = this.apiAlbumUrl + '/ids';
    return this.http.get<number[]>(requestPath);
  }

  getAlbum(id: number): (Observable<AlbumDto | null>) {
    let requestPath = this.apiAlbumUrl + '/' + id;
    return this.http.get<AlbumDto>(requestPath);
  }

  getPageLayoutPresets() {
    return this.http.get<PageLayoutPreset[]>(this.apiAlbumUrl + '/styling-enums');
  }

  getPublishedAlbums(): (Observable<AlbumDto[]>){
    let requestPath = this.apiAlbumUrl + '/published';
    return this.http.get<AlbumDto[]>(requestPath);
  }

  getPublishedNotInNavAlbums(): (Observable<AlbumDto[]>){
    let requestPath = this.apiAlbumUrl + '/published/not-in-nav';
    return this.http.get<AlbumDto[]>(requestPath);
  }

  getNavAlbumsOrdered(): (Observable<AlbumDto[]>){
    let requestPath = this.apiAlbumUrl + '/nav-ordered';
    return this.http.get<AlbumDto[]>(requestPath);
  }


  /* PUT, PATCH */
  updateAlbumLayoutPreset(albumId: number, layoutPreset: PageLayoutPreset) {
    return this.http.patch<void>(this.apiAlbumUrl + '/' + albumId + '/modify/layout-preset', { layoutPreset });
  }

  publishAlbum(albumId: number, navOrder: number | null = null): Observable<AlbumDto | null> {
    let requestPath = this.apiAlbumUrl + '/' + albumId + '/visibility';
    let requestObject = new AlbumVisibilityRequest(true, navOrder);
    return this.http.patch<AlbumDto | null>(requestPath, requestObject);
  }

  unpublishAlbum(albumId: number) {
    let requestPath = this.apiAlbumUrl + '/' + albumId + '/visibility';
    let requestObject = new AlbumVisibilityRequest(false);
    return this.http.patch<AlbumDto | null>(requestPath, requestObject);
  }

  assignNavOrder(albumId: number, navOrder: number) {
    return this.http.patch<void>(this.apiAlbumUrl + '/' + albumId + '/nav-order', { navOrder });
  }

  removeFromNavbar(albumId: number) {
    return this.http.patch<void>(this.apiAlbumUrl + '/' + albumId + '/remove-from-nav', {});
  }

  swapNavOrder(albumId1: number, albumId2: number) {
    let requestPath = this.apiAlbumUrl + '/modify/nav-order/swap';
    let requestObject = new NavOrderSwapRequest(albumId1, albumId2);
    return this.http.patch<void>(this.apiAlbumUrl + '/modify/nav-order/swap', requestObject);
  }


  /* DELETE */
  deleteAlbum(albumId: number){
    let requestPath = this.apiAlbumUrl + '/' + albumId;
    return this.http.delete(requestPath);
  }



  // getPhotos(albumId: number): (Observable<PhotoDto[]>) {
  //   let requestPath = this.apiAlbumUrl + '/' + albumId + '/photos'
  //   return this.http.get<PhotoDto[]>(requestPath);
  // }
  //
  // uploadPhoto(albumId: number, uploadSpecification: PhotoUploadSpecification){
  //   let requestPath = this.apiAlbumUrl + '/' + albumId + '/upload';
  //
  //   const formData: FormData = new FormData();
  //   formData.append('file', uploadSpecification.FileComponent, uploadSpecification.FileComponent.name);
  //   formData.append('name', uploadSpecification.PhotoSpecComponent.name);
  //   formData.append('description', uploadSpecification.PhotoSpecComponent.description)
  //   formData.append('yearContentCreated', uploadSpecification.PhotoSpecComponent.yearContentCreated.toString());
  //
  //   return this.http.post(requestPath, formData);
  // }
  //
  //
  //
  // reorder(albumId: number, photoId: number, toDest: number){
  //   let requestPath = this.apiAlbumUrl + '/' + albumId + '/' + photoId + '/reorder/' + toDest;
  //   return this.http.put(requestPath, null);
  // }
  //
  // toggleDisplaysName(albumId: number, photoId: number){
  //   let requestPath = this.apiAlbumUrl + '/' + albumId + '/' + photoId + '/displaysName';
  //   return this.http.patch(requestPath, null);
  // }
  //
  // toggleDisplaysDescription(albumId: number, photoId: number){
  //   let requestPath = this.apiAlbumUrl + '/' + albumId + '/' + photoId + '/displaysDescription';
  //   return this.http.patch(requestPath, null);
  // }
  //
  // toggleDisplaysYearCC(albumId: number, photoId: number){
  //   let requestPath = this.apiAlbumUrl + '/' + albumId + '/' + photoId + '/displaysYearCC';
  //   return this.http.patch(requestPath, null);
  // }

}


export class CreateAlbumRequest {
  name: string | null = null;
  description: string | null = null;
}

export class AlbumVisibilityRequest {
  published: boolean;
  navOrder: number | null;

  constructor(published: boolean, navOrder: number | null = null){
    this.published = published;
    this.navOrder = navOrder;
  }
}

export class NavOrderSwapRequest {
  albumId1: number;
  albumId2: number;

  constructor(albumId1: number, albumId2: number) {
    this.albumId1 = albumId1;
    this.albumId2 = albumId2;
  }
}


