import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportCreditAndDebitNoteRemainingChargeComponent } from './sea-lcl-export-credit-and-debit-note-remaining-charge.component';

describe('SeaLclExportCreditAndDebitNoteRemainingChargeComponent', () => {
  let component: SeaLclExportCreditAndDebitNoteRemainingChargeComponent;
  let fixture: ComponentFixture<SeaLclExportCreditAndDebitNoteRemainingChargeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportCreditAndDebitNoteRemainingChargeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportCreditAndDebitNoteRemainingChargeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
