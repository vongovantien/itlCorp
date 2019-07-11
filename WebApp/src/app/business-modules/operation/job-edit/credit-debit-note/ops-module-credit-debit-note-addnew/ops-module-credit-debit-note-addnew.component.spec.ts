import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleCreditDebitNoteAddnewComponent } from './ops-module-credit-debit-note-addnew.component';

describe('OpsModuleCreditDebitNoteAddnewComponent', () => {
  let component: OpsModuleCreditDebitNoteAddnewComponent;
  let fixture: ComponentFixture<OpsModuleCreditDebitNoteAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleCreditDebitNoteAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleCreditDebitNoteAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
