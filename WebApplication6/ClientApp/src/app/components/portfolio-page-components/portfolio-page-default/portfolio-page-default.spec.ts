import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioPageDefault } from './portfolio-page-default';

describe('PortfolioPageDefault', () => {
  let component: PortfolioPageDefault;
  let fixture: ComponentFixture<PortfolioPageDefault>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortfolioPageDefault]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortfolioPageDefault);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
