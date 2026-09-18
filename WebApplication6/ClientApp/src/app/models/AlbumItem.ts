import {PageLayoutPreset} from './PortfolioPageItemDto';

export type AlbumItem = {
  id: number,
  name: string | null,
  description: string | null,
  navTitle: string,
  published: boolean,
  navbarOrder: number,
  layoutPreset: PageLayoutPreset,
}
