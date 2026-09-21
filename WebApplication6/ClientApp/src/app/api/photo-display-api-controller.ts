import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {AlbumDto} from '../models/AlbumDto';
import {Observable} from 'rxjs';
import {PhotoDto} from '../models/PhotoDto';
import {PhotoDisplayDto} from '../models/PhotoDisplayDto';

@Injectable({
  providedIn: 'root',
})
export class AlbumApiService {

  constructor() {};

  private readonly http = inject(HttpClient);
  private readonly apiPhotoDisplayUrlPrefix = '/api/album';


  /* POST */
  uploadImagePostPhotoPostDisplay(albumId: number, uploadSpecification: PhotoUploadSpecification){
    let requestPath = this.apiPhotoDisplayUrlPrefix + '/' + albumId + '/photo-display';

    const formData: FormData = new FormData();
    formData.append('file', uploadSpecification.FileComponent, uploadSpecification.FileComponent.name);
    formData.append('name', uploadSpecification.PhotoSpecComponent.name);
    formData.append('description', uploadSpecification.PhotoSpecComponent.description)
    formData.append('yearContentCreated', uploadSpecification.PhotoSpecComponent.yearContentCreated.toString());

    return this.http.post(requestPath, formData);
  }

  getPhotoDisplays(albumId: number): (Observable<PhotoDisplayDto[]>) {
    let requestPath = this.apiPhotoDisplayUrlPrefix + '/' + albumId + '/photo-display'
    return this.http.get<PhotoDisplayDto[]>(requestPath);
  }

  toggleDisplaysName(albumId: number, photoDisplayId: number){
    let request = new PhotoDisplayFieldsDisplayedRequest();
    request.displaysName = true;
    return this.modifyFieldsDisplayed(albumId, photoDisplayId, request);
  }

  toggleDisplaysDescription(albumId: number, photoDisplayId: number){
    let request = new PhotoDisplayFieldsDisplayedRequest();
    request.displaysDescription = true;
    return this.modifyFieldsDisplayed(albumId, photoDisplayId, request);
  }

  toggleDisplaysYearCC(albumId: number, photoDisplayId: number){
    let request = new PhotoDisplayFieldsDisplayedRequest();
    request.displaysYearContentCreated = true;
    return this.modifyFieldsDisplayed(albumId, photoDisplayId, request);
  }

  modifyFieldsDisplayed(albumId: number, photoDisplayId: number, requestObject: PhotoDisplayFieldsDisplayedRequest){
    let requestPath = this.apiPhotoDisplayUrlPrefix + '/' + albumId + '/photo-display/' + photoDisplayId + '/fields-displayed';
    return this.http.patch(requestPath, requestObject);
  }

}


export class PhotoDisplayFieldsDisplayedRequest {
  displaysName: boolean | null = null;
  displaysDescription: boolean | null = null;
  displaysYearContentCreated: boolean | null = null;
}

export class PhotoDisplaySpecification {
  name: string = '';
  description: string = '';
  yearContentCreated: number = 2003;
}

export class PhotoUploadSpecification {
  FileComponent: File;
  PhotoSpecComponent: PhotoDisplaySpecification;

  constructor(file: File, photoSpecDTO: PhotoDisplaySpecification){
    this.FileComponent = file;
    this.PhotoSpecComponent = photoSpecDTO;
  }
}
