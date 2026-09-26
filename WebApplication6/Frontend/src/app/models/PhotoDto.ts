import {ImageDto} from './ImageDto';

export type PhotoDto = {
  id: number,
  image: ImageDto | null,
  name: string,
  description: string,
  yearContentCreated: number | null,
}
