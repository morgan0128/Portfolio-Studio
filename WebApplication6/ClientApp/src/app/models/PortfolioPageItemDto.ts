export const PAGE_LAYOUT_PRESETS = ['default', 'cozy', 'spooky'] as const;
export type PageLayoutPreset = (typeof PAGE_LAYOUT_PRESETS)[number];

export type PortfolioPageItemDto = {
  id: number,
  navTitle: string,
  title: string,
  published: boolean,
  navbarOrder: number,
  albumId: number,
  layoutPreset: PageLayoutPreset,
}

export class CreatePortfolioPageFromAlbumRequest {
  constructor(albumId: number, name: string){
    this.albumId = albumId;
    this.name = name;
  }

  albumId: number = -1;
  name: string = '';
}


