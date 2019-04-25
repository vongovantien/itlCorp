import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportCreditAndDebitNoteDetailComponent } from './sea-lcl-export-credit-and-debit-note-detail.component';

describe('SeaLclExportCreditAndDebitNoteDetailComponent', () => {
  let component: SeaLclExportCreditAndDebitNoteDetailComponent;
  let fixture: ComponentFixture<SeaLclExportCreditAndDebitNoteDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportCreditAndDebitNoteDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportCreditAndDebitNoteDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
