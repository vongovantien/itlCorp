import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditAndDebitNoteRemainingChargeComponent } from './credit-and-debit-note-remaining-charge.component';

describe('CreditAndDebitNoteRemainingChargeComponent', () => {
  let component: CreditAndDebitNoteRemainingChargeComponent;
  let fixture: ComponentFixture<CreditAndDebitNoteRemainingChargeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreditAndDebitNoteRemainingChargeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreditAndDebitNoteRemainingChargeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
