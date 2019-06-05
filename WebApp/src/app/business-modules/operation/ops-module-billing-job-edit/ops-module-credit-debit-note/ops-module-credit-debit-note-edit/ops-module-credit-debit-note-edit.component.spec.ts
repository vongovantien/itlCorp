import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleCreditDebitNoteEditComponent } from './ops-module-credit-debit-note-edit.component';

describe('OpsModuleCreditDebitNoteEditComponent', () => {
  let component: OpsModuleCreditDebitNoteEditComponent;
  let fixture: ComponentFixture<OpsModuleCreditDebitNoteEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleCreditDebitNoteEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleCreditDebitNoteEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
