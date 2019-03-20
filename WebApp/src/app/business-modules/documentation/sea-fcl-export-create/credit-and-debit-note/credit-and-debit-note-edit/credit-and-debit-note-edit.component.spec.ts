import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditAndDebitNoteEditComponent } from './credit-and-debit-note-edit.component';

describe('CreditAndDebitNoteEditComponent', () => {
  let component: CreditAndDebitNoteEditComponent;
  let fixture: ComponentFixture<CreditAndDebitNoteEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreditAndDebitNoteEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreditAndDebitNoteEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
