import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportCreditAndDebitNoteEditComponent } from './sea-lcl-export-credit-and-debit-note-edit.component';

describe('SeaLclExportCreditAndDebitNoteEditComponent', () => {
  let component: SeaLclExportCreditAndDebitNoteEditComponent;
  let fixture: ComponentFixture<SeaLclExportCreditAndDebitNoteEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportCreditAndDebitNoteEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportCreditAndDebitNoteEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
