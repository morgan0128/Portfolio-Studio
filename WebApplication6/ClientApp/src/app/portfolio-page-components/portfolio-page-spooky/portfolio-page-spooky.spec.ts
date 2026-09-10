import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioPageSpooky } from './portfolio-page-spooky';

describe('PortfolioPageSpooky', () => {
  let component: PortfolioPageSpooky;
  let fixture: ComponentFixture<PortfolioPageSpooky>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortfolioPageSpooky]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortfolioPageSpooky);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
