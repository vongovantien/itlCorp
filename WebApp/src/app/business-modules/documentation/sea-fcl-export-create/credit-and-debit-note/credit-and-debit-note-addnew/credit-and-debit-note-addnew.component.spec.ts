import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditAndDebitNoteAddnewComponent } from './credit-and-debit-note-addnew.component';

describe('CreditAndDebitNoteAddnewComponent', () => {
  let component: CreditAndDebitNoteAddnewComponent;
  let fixture: ComponentFixture<CreditAndDebitNoteAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreditAndDebitNoteAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreditAndDebitNoteAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
