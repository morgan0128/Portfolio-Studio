import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminPhotoDisplayCollectionCard } from './admin-photo-display-collection-card';

describe('AdminPhotoDisplayCollectionCard', () => {
  let component: AdminPhotoDisplayCollectionCard;
  let fixture: ComponentFixture<AdminPhotoDisplayCollectionCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminPhotoDisplayCollectionCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminPhotoDisplayCollectionCard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
