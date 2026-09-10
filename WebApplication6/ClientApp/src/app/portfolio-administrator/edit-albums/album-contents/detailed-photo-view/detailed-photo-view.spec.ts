import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetailedPhotoView } from './detailed-photo-view';

describe('DetailedPhotoView', () => {
  let component: DetailedPhotoView;
  let fixture: ComponentFixture<DetailedPhotoView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetailedPhotoView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DetailedPhotoView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
