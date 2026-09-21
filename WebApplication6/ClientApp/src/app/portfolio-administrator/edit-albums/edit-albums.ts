import {Component, inject, OnInit, signal, Signal} from '@angular/core';
import {Location} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {AlbumContents} from './album-contents/album-contents';
import {AlbumDto} from '../../models/AlbumDto';
import {PortfolioPageManager} from './portfolio-page-manager/portfolio-page-manager';
import {PhotoDto} from '../../models/PhotoDto';
import {AlbumApiService, PhotoSpecDTO, PhotoUploadSpecification} from '../../api/album-api-service';

@Component({
  selector: 'app-edit-albums',
  imports: [
    FormsModule,
    AlbumContents,
    PortfolioPageManager,
  ],
  templateUrl: './edit-albums.html',
  styleUrl: './edit-albums.css',
})
export class EditAlbums implements OnInit {
  // private readonly http = inject(HttpClient);
  // private readonly apiAlbumUrl = '/api/Album';

  private readonly albumApi = inject(AlbumApiService);
  private readonly location = inject(Location);

  protected readonly creatingAlbum = signal<boolean>(false);
  protected readonly creatingAlbumError = signal<boolean>(false);
  protected newAlbumName = '';
  protected newAlbumDescription = '';

  protected readonly restoringAlbumSelection = signal<boolean>(false);
  protected readonly loadingAlbums = signal<boolean>(true);
  protected readonly loadingAlbumsError = signal<boolean>(false);

  // protected selectedAlbum: AlbumItem | null = null;
  protected selectedAlbum = signal<AlbumDto | null>(null);
  protected selectedAlbumId = signal<number | null>(null);
  // selectedAlbumId: number | null = null;
  protected readonly selectingAlbumError = signal<boolean>(false);

  protected readonly proposingDelete = signal<boolean>(false);

  protected newPhotoName = '';
  protected newPhotoDescription = '';
  newPhotoSelectedImageFile: File | null = null;
  protected previewImageUrl: string | null = null;
  protected imagePreviewUploadError: string | null = null;
  protected imagePreviewUploading = false;

  protected readonly uploadingPhoto = signal<boolean>(false);
  protected uploadPhotoError: string | null = null;

  protected readonly loadingPhotos = signal<boolean>(false);
  protected readonly loadingPhotosError = signal<boolean>(false);

  protected readonly albumDTOs = signal<AlbumDto[]>([]);
  protected readonly photos = signal<PhotoDto[]>([]);


  protected readonly createPhotoGroupView = signal<boolean>(false);



  ngOnInit() {
    const state = this.location.getState() as {
      selectedAlbumId?: unknown;
    } | null;

    const id = state?.selectedAlbumId;

    this.loadAlbums((typeof id === 'number' && Number.isInteger(id)) ? id : null);
  }

  loadAlbums(restoreAlbumFromId: number | null = null){
    this.loadingAlbums.set(true);
    this.loadingAlbumsError.set(false);

    if (restoreAlbumFromId !== null){
      this.restoringAlbumSelection.set(true);
      this.selectingAlbumError.set(false);

      this.albumApi.getAlbum(restoreAlbumFromId).subscribe({
        next: album => {
          this.selectAlbum(album);
          this.restoringAlbumSelection.set(false);
        },
        error: () => {
          this.selectingAlbumError.set(true);
          this.restoringAlbumSelection.set(false);
        }
      });
    }

     this.albumApi.getAlbums().subscribe({
        next: albums => {
          this.albumDTOs.set(albums);
          this.loadingAlbums.set(false);
        },
        error: () => {
          this.loadingAlbums.set(false);
          this.loadingAlbumsError.set(true);
        }
      })
  }

  createAlbum(){
    // this.proposingDelete.set(false);
    this.creatingAlbum.set(true);
    this.creatingAlbumError.set(false);
    const name = this.newAlbumName.trim();
    const description = this.newAlbumDescription.trim();

    let request = this.albumApi.postAlbum(name, description);
    this.creatingAlbum.set(false);
    request.subscribe({
      next: () => {
        this.loadAlbums(); // TODO: excess load
      },
      error: () => {
        this.creatingAlbumError.set(true);
      }
    })
  }

  private selectAlbum(album: AlbumDto){
    this.selectingAlbumError.set(false);
    this.selectedAlbumId.set(album.id);
    this.selectedAlbum.set(album);
    this.photos.set([]);
    this.loadAlbumSelection();
  }

  onSelectedAlbumChange(){
    // return;
    // this.proposingDelete.set(false);

    if (this.selectedAlbumId() === null){
      this.selectedAlbum.set(null);
      this.photos.set([]);
      return; // default selection value
    }
    if (this.selectedAlbum()?.id === this.selectedAlbumId()){
      return; // already selected
    }

    this.selectingAlbumError.set(false);

    const albumDTO = this.albumDTOs().find(a => a.id === this.selectedAlbumId());

    if (!albumDTO){
      this.selectingAlbumError.set(true);
      return;
    }


    this.selectAlbum(albumDTO);
  }

  loadAlbumSelection(){
    if (this.selectedAlbumId() === null) {
      this.loadingPhotosError.set(true);
      return;
    }

    this.loadingPhotos.set(true);
    this.loadingPhotosError.set(false);
    this.photos.set([]);

    let request = this.albumApi.getPhotos(<number>this.selectedAlbumId());
    request.subscribe({
      next: photos => {
        this.photos.set(photos);
        this.loadingPhotos.set(false);
      },
      error: () => {
        this.loadingPhotos.set(false);
        this.loadingPhotosError.set(true);
      }
    })
  }

  protected onFileSelected(event: Event): void {
    this.imagePreviewUploadError = null;

    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    const maxSizeMb = 5;
    if (file.size > maxSizeMb * 1024 * 1024) {
      this.imagePreviewUploadError = `Image must be under ${maxSizeMb} MB.`
      this.newPhotoSelectedImageFile = null;
      return;
    }

    this.newPhotoSelectedImageFile = file;
    this.previewImageUrl = URL.createObjectURL(file);
  }

  uploadPhotoToSelected(){
    if (this.selectedAlbumId() === null || this.newPhotoSelectedImageFile === null){
      return;
    }

    this.uploadingPhoto.set(true);
    this.uploadPhotoError = null;

    const name = this.newPhotoName.trim();
    const description = this.newPhotoDescription.trim();
    let yearContentCreated = 2003;

    const photoSpec = new PhotoSpecDTO();
    photoSpec.name = name;
    photoSpec.description = description;
    photoSpec.yearContentCreated = yearContentCreated;

    const uploadSpecification = new PhotoUploadSpecification(this.newPhotoSelectedImageFile, photoSpec);

    let request = this.albumApi.uploadPhoto(<number>this.selectedAlbumId(), uploadSpecification);
    request.subscribe({
      next: () => {
        this.uploadingPhoto.set(false);
        this.loadAlbumSelection(); // TODO: currently performs an excessive full reload
        return;
        // this.loadAlbums();
      },
      error: () => {
        this.uploadPhotoError = "error";
        this.uploadingPhoto.set(false);
      }
    })
  }

  // TODO: Have strong "Are you sure?" confirmation (e.g., enter the name of the Album)
  deleteSelected(){
    if (this.selectedAlbumId() === null){
      return;
    }

    let request = this.albumApi.deleteAlbum(<number>this.selectedAlbumId());
    request.subscribe({
      next: () => {
        this.selectedAlbum.set(null);
        this.loadAlbums(); // TODO: excessive
      },
      error: () => {

      }
    });


  }

  photoChangedStateUpdate(updatedPhoto: PhotoDto) {
    this.photos.update(photos =>
      photos.map(photo =>
        photo.id === updatedPhoto.id ? updatedPhoto : photo
      )
    );
  }


}
