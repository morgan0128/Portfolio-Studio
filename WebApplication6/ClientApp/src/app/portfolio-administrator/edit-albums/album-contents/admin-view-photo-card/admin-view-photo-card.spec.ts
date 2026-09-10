import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminViewPhotoCard } from './admin-view-photo-card';

describe('AdminViewPhotoCard', () => {
  let component: AdminViewPhotoCard;
  let fixture: ComponentFixture<AdminViewPhotoCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminViewPhotoCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminViewPhotoCard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
