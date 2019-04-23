import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportCreditAndDebitNoteAddnewComponent } from './sea-lcl-export-credit-and-debit-note-addnew.component';

describe('SeaLclExportCreditAndDebitNoteAddnewComponent', () => {
  let component: SeaLclExportCreditAndDebitNoteAddnewComponent;
  let fixture: ComponentFixture<SeaLclExportCreditAndDebitNoteAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportCreditAndDebitNoteAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportCreditAndDebitNoteAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
