import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleCreditDebitNoteDetailComponent } from './ops-module-credit-debit-note-detail.component';

describe('OpsModuleCreditDebitNoteDetailComponent', () => {
  let component: OpsModuleCreditDebitNoteDetailComponent;
  let fixture: ComponentFixture<OpsModuleCreditDebitNoteDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleCreditDebitNoteDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleCreditDebitNoteDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
