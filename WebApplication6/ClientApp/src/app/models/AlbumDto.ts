import {PageLayoutPreset} from './ApiEnums';

export type AlbumDto = {
  id: number,
  name: string | null,
  description: string | null,
  navTitle: string,
  published: boolean,
  navbarOrder: number,
  layoutPreset: PageLayoutPreset,
}
