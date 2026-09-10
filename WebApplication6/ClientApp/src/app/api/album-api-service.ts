import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {AlbumItem} from '../models/AlbumItem';
import {Observable} from 'rxjs';
import {AlbumPhotoItemDto} from '../models/AlbumPhotoItemDto';

@Injectable({
  providedIn: 'root',
})
export class AlbumApiService {

  constructor() {};

  private readonly http = inject(HttpClient);
  private readonly apiAlbumUrl = '/api/Album';

  getAlbums(): (Observable<AlbumItem[]>) {
    let requestPath = this.apiAlbumUrl + '/all';
    return this.http.get<AlbumItem[]>(requestPath);
  }

  getAlbum(id: number): (Observable<AlbumItem>) {
    let requestPath = this.apiAlbumUrl + '/' + id;
    return this.http.get<AlbumItem>(requestPath);
  }

  postAlbum(name: string | null, description: string | null){
    let requestPath = this.apiAlbumUrl + '';
    return this.http.post(requestPath, { name, description });
  }

  getPhotos(albumId: number): (Observable<AlbumPhotoItemDto[]>) {
    let requestPath = this.apiAlbumUrl + '/' + albumId + '/photos'
    return this.http.get<AlbumPhotoItemDto[]>(requestPath);
  }

  uploadPhoto(albumId: number, uploadSpecification: PhotoUploadSpecification){
    let requestPath = this.apiAlbumUrl + '/' + albumId + '/upload';

    const formData: FormData = new FormData();
    formData.append('file', uploadSpecification.FileComponent, uploadSpecification.FileComponent.name);
    formData.append('name', uploadSpecification.PhotoSpecComponent.name);
    formData.append('description', uploadSpecification.PhotoSpecComponent.description)
    formData.append('yearContentCreated', uploadSpecification.PhotoSpecComponent.yearContentCreated.toString());

    return this.http.post(requestPath, formData);
  }

  deleteAlbum(albumId: number){
    let requestPath = this.apiAlbumUrl + '/' + albumId;
    return this.http.delete(requestPath);
  }

  reorder(albumId: number, photoId: number, toDest: number){
    let requestPath = this.apiAlbumUrl + '/' + albumId + '/' + photoId + '/reorder/' + toDest;
    return this.http.put(requestPath, null);
  }

  toggleDisplaysName(albumId: number, photoId: number){
    let requestPath = this.apiAlbumUrl + '/' + albumId + '/' + photoId + '/displaysName';
    return this.http.patch(requestPath, null);
  }

  toggleDisplaysDescription(albumId: number, photoId: number){
    let requestPath = this.apiAlbumUrl + '/' + albumId + '/' + photoId + '/displaysDescription';
    return this.http.patch(requestPath, null);
  }

  toggleDisplaysYearCC(albumId: number, photoId: number){
    let requestPath = this.apiAlbumUrl + '/' + albumId + '/' + photoId + '/displaysYearCC';
    return this.http.patch(requestPath, null);
  }

}



// export class FileDTO {
//   file: File | null = null;
//   name: string = '';
// }

export class PhotoSpecDTO {
  // id: number | null = null;
  name: string = '';
  description: string = '';
  yearContentCreated: number = 2003;
  // image: ImageDTO | null = null;
}

export class PhotoUploadSpecification {
  FileComponent: File;
  PhotoSpecComponent: PhotoSpecDTO;

  constructor(file: File, photoSpecDTO: PhotoSpecDTO){
    this.FileComponent = file;
    this.PhotoSpecComponent = photoSpecDTO;
  }
}
