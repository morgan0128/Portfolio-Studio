import {
  Component,
  inject,
  inputBinding,
  OnInit,
  outputBinding,
  signal,
  ViewContainerRef
} from '@angular/core';
import {PortfolioPageDefault} from '../../portfolio-page-components/portfolio-page-default/portfolio-page-default';
import {PortfolioPageCozy} from '../../portfolio-page-components/portfolio-page-cozy/portfolio-page-cozy';
import {PortfolioPageSpooky} from '../../portfolio-page-components/portfolio-page-spooky/portfolio-page-spooky';
import {ActivatedRoute, Router} from '@angular/router';
import {AlbumItem} from '../../models/AlbumItem';
import {PageLayoutPreset} from '../../models/PortfolioPageItemDto';
import {AlbumApiService} from '../../api/album-api-service';


@Component({
  selector: 'app-admin-view-page-preview',
  imports: [],
  template: ``,
  styles: ``
})
export class AdminViewPagePreview implements OnInit {
  private viewComponent = inject(ViewContainerRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private readonly albumApi = inject(AlbumApiService);

  protected readonly initializing = signal<boolean>(true);
  protected readonly initializationError = signal<boolean>(false);


  public readonly forAlbum = signal<AlbumItem | null>(null);
  // private readonly forAlbum$ = toObservable(this.forAlbum);

  public readonly withStyle = signal<PageLayoutPreset>('default');
  // private readonly withStyle$ = toObservable(this.withStyle);

  ngOnInit(){
    const albumIdText = this.route.snapshot.paramMap.get('albumId');
    const styleText = this.route.snapshot.paramMap.get('style');

    const albumId = albumIdText !== null ? Number.parseInt(albumIdText, 10) : null;
    if (albumId === null){
      this.router.navigate(['/edit-albums']);
    }
    const style: PageLayoutPreset = styleText === 'cozy' || styleText === 'spooky' ? styleText : 'default';

    this.albumApi.getAlbum(albumId!).subscribe({
      next: album => {
        this.forAlbum.set(album);
        this.withStyle.set(style);
        this.initializing.set(false);
        if (this.forAlbum() !== null){
          switch (this.withStyle()){
            case ('cozy'):
                this.viewComponent.createComponent(PortfolioPageCozy);
                break;
            case ('spooky'):
                this.viewComponent.createComponent(PortfolioPageSpooky)
                break;
            default:
                this.viewComponent.createComponent(PortfolioPageDefault, {
                  bindings: [
                    inputBinding('album', () => this.forAlbum()!),
                    outputBinding('requestNavToEditAlbums', () => this.navigateToEditAlbums())
                  ]
                });
                // <app-portfolio-page-default [album]="forAlbum()!" (requestNavToEditAlbums)="navigateToEditAlbums()" />
            break;
          }
        }
      },
      error: () => {
        this.initializationError.set(true);
        this.withStyle.set(style);
        this.initializing.set(false);
        return;
      }
    })
  }

  navigateToEditAlbums(){
    this.router.navigateByUrl("/edit-albums");
  }


  protected readonly alert = alert;
}
