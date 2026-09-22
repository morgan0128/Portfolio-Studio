import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminViewPhotoDisplayCard } from './admin-view-photo-display-card';

describe('AdminViewPhotoDisplayCard', () => {
  let component: AdminViewPhotoDisplayCard;
  let fixture: ComponentFixture<AdminViewPhotoDisplayCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminViewPhotoDisplayCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminViewPhotoDisplayCard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
