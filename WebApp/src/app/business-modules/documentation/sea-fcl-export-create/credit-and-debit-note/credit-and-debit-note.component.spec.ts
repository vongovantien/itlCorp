import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditAndDebitNoteComponent } from './credit-and-debit-note.component';

describe('CreditAndDebitNoteComponent', () => {
  let component: CreditAndDebitNoteComponent;
  let fixture: ComponentFixture<CreditAndDebitNoteComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreditAndDebitNoteComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreditAndDebitNoteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
