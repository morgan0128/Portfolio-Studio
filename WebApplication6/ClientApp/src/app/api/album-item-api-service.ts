import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {AlbumDto} from '../models/AlbumDto';
import {Observable} from 'rxjs';
import {PhotoDto} from '../models/PhotoDto';

@Injectable({
  providedIn: 'root',
})
export class AlbumItemApiService {

  constructor() {};

  private readonly http = inject(HttpClient);
  private readonly apiAlbumItemUrl = '/api/album';



}



// export class PhotoSpecDTO {
//   // id: number | null = null;
//   name: string = '';
//   description: string = '';
//   yearContentCreated: number = 2003;
//   // image: ImageDTO | null = null;
// }
//
// export class PhotoUploadSpecification {
//   FileComponent: File;
//   PhotoSpecComponent: PhotoSpecDTO;
//
//   constructor(file: File, photoSpecDTO: PhotoSpecDTO){
//     this.FileComponent = file;
//     this.PhotoSpecComponent = photoSpecDTO;
//   }
// }
