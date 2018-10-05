import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SettlementPaymentComponent } from './settlement-payment.component';

describe('SettlementPaymentComponent', () => {
  let component: SettlementPaymentComponent;
  let fixture: ComponentFixture<SettlementPaymentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SettlementPaymentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SettlementPaymentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
