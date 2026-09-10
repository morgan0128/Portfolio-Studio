import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioPhotoCard } from './portfolio-photo-card';

describe('PortfolioPhotoCard', () => {
  let component: PortfolioPhotoCard;
  let fixture: ComponentFixture<PortfolioPhotoCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortfolioPhotoCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortfolioPhotoCard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
