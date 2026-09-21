import {PhotoDisplayDto} from './PhotoDisplayDto';
import {PhotoDisplayCollectionDto} from './PhotoDisplayCollectionDto';

export type AlbumItemDto = {
  id: number,
  order: number
  item: PhotoDisplayDto | PhotoDisplayCollectionDto | null
}
