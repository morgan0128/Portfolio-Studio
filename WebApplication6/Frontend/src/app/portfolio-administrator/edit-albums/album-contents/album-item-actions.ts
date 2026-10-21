import type {AlbumItemDto, PhotoDisplayDto, PhotoDisplayItem} from '../../../models/AlbumItemDto';

export type PhotoDisplayTarget = {
  rootItemId: number;
  photoDisplayId: number;
};

export function resolvePhotoDisplay(
  rootItem: AlbumItemDto | undefined,
  photoDisplayId: number,
): PhotoDisplayItem | undefined {
  if (!rootItem) return undefined;
  return rootItem.kind === 'photoDisplay'
    ? rootItem.id === photoDisplayId ? rootItem : undefined
    : rootItem.content.photoDisplays.find(photoDisplay => photoDisplay.id === photoDisplayId);
}

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
