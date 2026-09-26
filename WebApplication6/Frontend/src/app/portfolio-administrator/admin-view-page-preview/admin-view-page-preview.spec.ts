import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminViewPagePreview } from './admin-view-page-preview';

describe('AdminViewPagePreview', () => {
  let component: AdminViewPagePreview;
  let fixture: ComponentFixture<AdminViewPagePreview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminViewPagePreview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminViewPagePreview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
