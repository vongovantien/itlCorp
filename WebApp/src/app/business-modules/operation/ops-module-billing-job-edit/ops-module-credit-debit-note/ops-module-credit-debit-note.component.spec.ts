import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleCreditDebitNoteComponent } from './ops-module-credit-debit-note.component';

describe('OpsModuleCreditDebitNoteComponent', () => {
  let component: OpsModuleCreditDebitNoteComponent;
  let fixture: ComponentFixture<OpsModuleCreditDebitNoteComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleCreditDebitNoteComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleCreditDebitNoteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
