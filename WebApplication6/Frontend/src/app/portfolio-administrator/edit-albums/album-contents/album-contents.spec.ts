import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AlbumContents } from './album-contents';

describe('AlbumContents', () => {
  let component: AlbumContents;
  let fixture: ComponentFixture<AlbumContents>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlbumContents]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AlbumContents);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
