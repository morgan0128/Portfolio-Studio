import {PhotoItem} from './AlbumInterfacing';

export type FullViewAlbumPhoto = {
  albumId: number,
  photo: PhotoItem,
  order: number,
  displaysName: boolean,
  displaysDescription: boolean,
  displaysYearContentCreated: boolean
}
