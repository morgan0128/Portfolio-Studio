import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {PortfolioApiService} from './portfolio-api-service';

describe('PortfolioApiService', () => {
  let service: PortfolioApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({providers: [provideHttpClient(), provideHttpClientTesting()]});
    service = TestBed.inject(PortfolioApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('reads an album without creating a separate page', () => {
    service.getAlbum(42).subscribe();
    const request = http.expectOne('/api/Portfolio/42');
    expect(request.request.method).toBe('GET');
    request.flush({id: 42, name: 'Album', navTitle: 'Album', published: false, navbarOrder: -1, layoutPreset: 'default'});
  });

  it('uses album IDs for publishing and layout changes', () => {
    service.publishAlbum(42).subscribe(order => expect(order).toBe(-1));
    const publish = http.expectOne('/api/Portfolio/publish/42');
    expect(publish.request.method).toBe('PATCH');
    publish.flush(-1);

    service.applyPageLayoutPreset(42, 'cozy').subscribe();
    const layout = http.expectOne('/api/Portfolio/42/modify/layout-preset');
    expect(layout.request.body).toEqual({layoutPreset: 'cozy'});
    layout.flush(null);
  });

  it('sends album identities when swapping navigation positions', () => {
    service.swapNavOrder(42, 73).subscribe();
    const request = http.expectOne('/api/Portfolio/modify/nav-order/swap');
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual({albumId1: 42, albumId2: 73});
    request.flush(null);
  });
});
