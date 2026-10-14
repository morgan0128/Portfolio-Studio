import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {PhotoUploadSpecification} from '../models/PhotoUploadSpecification';
import {Observable} from 'rxjs';
import {PhotoDto} from '../models/PhotoDto';


@Injectable({
  providedIn: 'root',
})
export class PhotoApiService {

  constructor() {
  };

  private readonly http = inject(HttpClient);
  private readonly apiPhotoUrl = '/api/photos';


  /* POST */
  uploadImagePostPhoto(uploadSpecification: PhotoUploadSpecification){
    let requestPath = this.apiPhotoUrl;

    // const formData: FormData = new FormData();
    // formData.append('file', uploadSpecification.FileComponent, uploadSpecification.FileComponent.name);
    // formData.append('name', uploadSpecification.PhotoSpecComponent.name);
    // formData.append('description', uploadSpecification.PhotoSpecComponent.description)
    // formData.append('yearContentCreated', uploadSpecification.PhotoSpecComponent.yearContentCreated.toString());

    // return this.http.post(requestPath, formData);
    return this.http.post(requestPath, uploadSpecification.asFormData());

  }

  /* GET */
  getPhotos(): Observable<PhotoDto[]> {
    let requestPath = this.apiPhotoUrl;
    return this.http.get<PhotoDto[]>(requestPath);
  }

  getPhoto(id: number): Observable<PhotoDto> {
    let requestPath = this.apiPhotoUrl + '/' + id;
    return this.http.get<PhotoDto>(requestPath);
  }

  // getPhotos(albumId: number): (Observable<PhotoDto[]>) {
  //   let requestPath = this.apiAlbumUrl + '/' + albumId + '/photos'
  //   return this.http.get<PhotoDto[]>(requestPath);
  // }

  /* PUT, PATCH */


  /* DELETE */
}
