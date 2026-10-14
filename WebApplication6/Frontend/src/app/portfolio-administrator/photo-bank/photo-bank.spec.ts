import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoBank } from './photo-bank';

describe('PhotoBank', () => {
  let component: PhotoBank;
  let fixture: ComponentFixture<PhotoBank>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoBank]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotoBank);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
