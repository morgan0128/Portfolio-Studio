import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditAlbums } from './edit-albums';

describe('EditAlbums', () => {
  let component: EditAlbums;
  let fixture: ComponentFixture<EditAlbums>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditAlbums]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditAlbums);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
