import type {PhotoDto} from './PhotoDto';

export type PhotoEditFieldsRequest = Pick<PhotoDto,
  'name' | 'description' | 'yearContentCreated'>;
