import {ImageItem} from './ImageItem';

export type AlbumPhotoItemDto = {
  id: number,
  name: string | null,
  description: string | null,
  yearContentCreated: number | null,
  image: ImageItem | null,
  order: number | null,
  displaysName: boolean,
  displaysDescription: boolean,
  displaysYearCC: boolean
}
