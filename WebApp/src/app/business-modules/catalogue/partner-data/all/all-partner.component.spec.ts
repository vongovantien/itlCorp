import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllPartnerComponent } from './all-partner.component';

describe('AllComponent', () => {
  let component: AllPartnerComponent;
  let fixture: ComponentFixture<AllPartnerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllPartnerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllPartnerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
