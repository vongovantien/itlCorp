import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleCreditDebitNoteRemainingChargeComponent } from './ops-module-credit-debit-note-remaining-charge.component';

describe('OpsModuleCreditDebitNoteRemainingChargeComponent', () => {
  let component: OpsModuleCreditDebitNoteRemainingChargeComponent;
  let fixture: ComponentFixture<OpsModuleCreditDebitNoteRemainingChargeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleCreditDebitNoteRemainingChargeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleCreditDebitNoteRemainingChargeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
