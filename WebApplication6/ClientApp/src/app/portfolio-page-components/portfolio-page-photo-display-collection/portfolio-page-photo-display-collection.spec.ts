import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioPagePhotoDisplayCollection } from './portfolio-page-photo-display-collection';

describe('PortfolioPagePhotoDisplayCollection', () => {
  let component: PortfolioPagePhotoDisplayCollection;
  let fixture: ComponentFixture<PortfolioPagePhotoDisplayCollection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortfolioPagePhotoDisplayCollection]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortfolioPagePhotoDisplayCollection);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
