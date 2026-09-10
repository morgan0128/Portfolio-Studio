import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioPageManager } from './portfolio-page-manager';

describe('PortfolioPageManager', () => {
  let component: PortfolioPageManager;
  let fixture: ComponentFixture<PortfolioPageManager>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortfolioPageManager]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortfolioPageManager);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
