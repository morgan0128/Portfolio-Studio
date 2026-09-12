import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImageDisplay } from './ImageDisplay';

describe('ImageDisplay', () => {
  let component: ImageDisplay;
  let fixture: ComponentFixture<ImageDisplay>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImageDisplay]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ImageDisplay);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
