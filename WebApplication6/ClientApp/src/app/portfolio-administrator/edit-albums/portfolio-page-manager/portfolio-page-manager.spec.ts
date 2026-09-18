import {ComponentFixture, TestBed} from '@angular/core/testing';
import {Router} from '@angular/router';
import {of} from 'rxjs';
import {PortfolioPageManager} from './portfolio-page-manager';
import {PortfolioApiService} from '../../../api/portfolio-api-service';
import {AlbumItem} from '../../../models/AlbumItem';

describe('PortfolioPageManager', () => {
  let component: PortfolioPageManager;
  let fixture: ComponentFixture<PortfolioPageManager>;
  let api: jasmine.SpyObj<PortfolioApiService>;
  let router: jasmine.SpyObj<Router>;
  let album: AlbumItem;

  beforeEach(async () => {
    album = {id: 42, name: 'Gallery', description: null, navTitle: 'Gallery', published: false,
      navbarOrder: -1, layoutPreset: 'default'};
    api = jasmine.createSpyObj<PortfolioApiService>('PortfolioApiService', [
      'getAlbum', 'getPageLayoutPresets', 'getPublishedInNavbarOrdered', 'publishAlbum',
      'unpublishAlbum', 'applyPageLayoutPreset', 'removeFromNavbar', 'applyNavPosition', 'swapNavOrder'
    ]);
    api.getAlbum.and.callFake(() => of({...album}));
    api.getPageLayoutPresets.and.returnValue(of(['default', 'cozy', 'spooky']));
    api.getPublishedInNavbarOrdered.and.callFake(() => of(album.navbarOrder >= 0 ? [{...album}] : []));
    api.publishAlbum.and.callFake(() => { album.published = true; return of(-1); });
    api.unpublishAlbum.and.callFake(() => {
      album.published = false;
      album.navbarOrder = -1;
      return of(undefined);
    });
    api.applyNavPosition.and.callFake(() => { album.navbarOrder = 0; return of(undefined); });
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [PortfolioPageManager],
      providers: [{provide: PortfolioApiService, useValue: api}, {provide: Router, useValue: router}]
    }).compileComponents();
    fixture = TestBed.createComponent(PortfolioPageManager);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('selectedAlbum', {...album});
    fixture.detectChanges();
  });

  it('loads settings using the selected album identity', () => {
    expect(api.getAlbum).toHaveBeenCalledWith(42);
    expect(fixture.nativeElement.textContent).toContain('This item is not published');
  });

  it('publishes, inserts, and unpublishes the same album and refreshes its state', () => {
    component.publish();
    fixture.detectChanges();
    expect(api.publishAlbum).toHaveBeenCalledWith(42);
    expect(component.selectedMayBeAdded()).toBeTrue();

    component.applyNavPositionRequest(4);
    fixture.detectChanges();
    expect(api.applyNavPosition).toHaveBeenCalledWith(42, 4);
    expect(component.navbarItems()[0].navbarOrder).toBe(0);
    expect(component.selectedMayBeAdded()).toBeFalse();

    component.unpublish();
    fixture.detectChanges();
    expect(api.unpublishAlbum).toHaveBeenCalledWith(42);
    expect(component.navbarItems()).toEqual([]);
    expect(fixture.nativeElement.textContent).toContain('This item is not published');
  });

  it('keeps an unapplied layout choice when refreshing navbar settings', () => {
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('select[name="stylings"]');
    select.selectedIndex = 1;
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    component.requestNavbarView();
    fixture.detectChanges();
    component.previewRequest();
    expect(router.navigate).toHaveBeenCalledWith(['/admin-view-page-preview', 42, 'cozy']);
  });

  it('previews with the album ID and its current layout', () => {
    component.previewRequest();
    expect(router.navigate).toHaveBeenCalledWith(['/admin-view-page-preview', 42, 'default']);
  });
});
