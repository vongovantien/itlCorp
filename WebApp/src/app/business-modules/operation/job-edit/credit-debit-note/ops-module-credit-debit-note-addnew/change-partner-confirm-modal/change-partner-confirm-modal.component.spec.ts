import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangePartnerConfirmModalComponent } from './change-partner-confirm-modal.component';

describe('ChangePartnerConfirmModalComponent', () => {
  let component: ChangePartnerConfirmModalComponent;
  let fixture: ComponentFixture<ChangePartnerConfirmModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChangePartnerConfirmModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChangePartnerConfirmModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
