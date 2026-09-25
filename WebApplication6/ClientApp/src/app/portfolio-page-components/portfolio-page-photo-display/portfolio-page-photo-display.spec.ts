import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioPagePhotoDisplay } from './portfolio-page-photo-display';

describe('PortfolioPagePhotoDisplay', () => {
  let component: PortfolioPagePhotoDisplay;
  let fixture: ComponentFixture<PortfolioPagePhotoDisplay>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortfolioPagePhotoDisplay]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortfolioPagePhotoDisplay);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
