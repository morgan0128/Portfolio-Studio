import type {PhotoDto} from './PhotoDto';

export type PhotoDisplayDto = {
  photo: PhotoDto;
  photoDisplayCollectionId: number | null;
  displaysName: boolean;
  displaysDescription: boolean;
  displaysYearContentCreated: boolean;
};
