import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditAndDebitNoteDetailComponent } from './credit-and-debit-note-detail.component';

describe('CreditAndDebitNoteDetailComponent', () => {
  let component: CreditAndDebitNoteDetailComponent;
  let fixture: ComponentFixture<CreditAndDebitNoteDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreditAndDebitNoteDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreditAndDebitNoteDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
