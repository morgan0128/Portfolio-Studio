import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeveloperLanding } from './developer-landing';

describe('DeveloperLanding', () => {
  let component: DeveloperLanding;
  let fixture: ComponentFixture<DeveloperLanding>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeveloperLanding]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeveloperLanding);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
