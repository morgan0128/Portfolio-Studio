import {Component, inject, input, signal} from '@angular/core';
import {AlbumItem} from '../models/AlbumInterfacing';
import {toObservable} from '@angular/core/rxjs-interop';
import {PageLayoutPreset} from '../models/PortfolioInterfacing';
import {Router} from '@angular/router';

@Component({
  selector: 'app-admin-view-page-preview',
  imports: [],
  templateUrl: './admin-view-page-preview.html',
  styleUrl: './admin-view-page-preview.css',
})
export class AdminViewPagePreview {
  private router = inject(Router);

  public readonly forAlbum = signal<AlbumItem | null>(null);
  // private readonly forAlbum$ = toObservable(this.forAlbum);

  public readonly withStyle = signal<PageLayoutPreset>('default');
  // private readonly withStyle$ = toObservable(this.withStyle);

  constructor(){
    let j = this.router.parseUrl(this.router.url).queryParamMap;
    if (j.has('style')){
      // this.withStyle.set(j.get('style'));

    }
  }


}
