import type {PhotoDisplayDto} from '../../../models/AlbumItemDto';

export type PhotoDisplayFields = Pick<PhotoDisplayDto,
  'displaysName' | 'displaysDescription' | 'displaysYearContentCreated'>;

export type PhotoDisplayFieldsChange = {
  photoDisplayId: number;
  changes: Partial<PhotoDisplayFields>;
};

export type AlbumPhotoDisplayFieldsChange = PhotoDisplayFieldsChange & {
  albumId: number;
  rootItemId: number;
};
