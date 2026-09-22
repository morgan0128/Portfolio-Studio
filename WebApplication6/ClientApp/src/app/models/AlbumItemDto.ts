import type {PhotoDisplayDto} from './PhotoDisplayDto';
import type {PhotoDisplayCollectionDto} from './PhotoDisplayCollectionDto';

export type AlbumItemDto =
  | {
      kind: 'photoDisplay';
      id: number;
      order: number;
      content: PhotoDisplayDto;
    }
  | {
      kind: 'photoDisplayCollection';
      id: number;
      order: number;
      content: PhotoDisplayCollectionDto;
    };
