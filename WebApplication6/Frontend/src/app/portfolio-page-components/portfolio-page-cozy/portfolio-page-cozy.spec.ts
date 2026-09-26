import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioPageCozy } from './portfolio-page-cozy';

describe('PortfolioPageCozy', () => {
  let component: PortfolioPageCozy;
  let fixture: ComponentFixture<PortfolioPageCozy>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortfolioPageCozy]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortfolioPageCozy);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
