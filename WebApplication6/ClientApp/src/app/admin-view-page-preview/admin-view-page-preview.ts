import {Component, inject, input, signal} from '@angular/core';
import {AlbumItem} from '../models/AlbumInterfacing';
import {PageLayoutPreset} from '../models/PortfolioInterfacing';
import {ActivatedRoute, Router} from '@angular/router';
import {AlbumApiService} from '../services/album-api-service';
import {
  PortfolioPageDefault
} from '../components/portfolio-page-components/portfolio-page-default/portfolio-page-default';
import {PortfolioPageCozy} from '../components/portfolio-page-components/portfolio-page-cozy/portfolio-page-cozy';
import {PortfolioPageSpooky} from '../components/portfolio-page-components/portfolio-page-spooky/portfolio-page-spooky';

@Component({
  selector: 'app-admin-view-page-preview',
  imports: [
    PortfolioPageDefault,
    PortfolioPageCozy,
    PortfolioPageSpooky
  ],
  template: `
    @switch (withStyle()){
      @case ('cozy'){
        <app-portfolio-page-cozy />
      }
      @case ('spooky'){
        <app-portfolio-page-spooky />
      }
      @default {
        <app-portfolio-page-default [album]="forAlbum()"/>
      }
    }
  `,
})
export class AdminViewPagePreview {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private readonly albumApi = inject(AlbumApiService);


  public readonly forAlbum = signal<AlbumItem | null>(null);
  // private readonly forAlbum$ = toObservable(this.forAlbum);

  public readonly withStyle = signal<PageLayoutPreset>('default');
  // private readonly withStyle$ = toObservable(this.withStyle);

  constructor(){
    const albumIdText = this.route.snapshot.paramMap.get('albumId');
    const styleText = this.route.snapshot.paramMap.get('style');

    const albumId = albumIdText !== null ? Number.parseInt(albumIdText, 10) : null;
    if (albumId === null){
      this.router.navigate(['/edit-albums']);
    }
    const style: PageLayoutPreset = styleText === 'cozy' || styleText === 'spooky' ? styleText : 'default';

    this.albumApi.getAlbum(albumId!)
    this.forAlbum.set(null);
    this.withStyle.set(style);
  }


}
