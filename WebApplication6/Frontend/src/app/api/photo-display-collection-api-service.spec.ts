import { TestBed } from '@angular/core/testing';

import { PhotoDisplayCollectionApiService } from './photo-display-collection-api-service';

describe('PhotoDisplayCollectionApiService', () => {
  let service: PhotoDisplayCollectionApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PhotoDisplayCollectionApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
