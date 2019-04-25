import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportCreditAndDebitNoteComponent } from './sea-lcl-export-credit-and-debit-note.component';

describe('SeaLclExportCreditAndDebitNoteComponent', () => {
  let component: SeaLclExportCreditAndDebitNoteComponent;
  let fixture: ComponentFixture<SeaLclExportCreditAndDebitNoteComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportCreditAndDebitNoteComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportCreditAndDebitNoteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
