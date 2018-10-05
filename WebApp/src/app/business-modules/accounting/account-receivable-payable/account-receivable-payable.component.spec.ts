import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AccountReceivablePayableComponent } from './account-receivable-payable.component';

describe('AccountReceivablePayableComponent', () => {
  let component: AccountReceivablePayableComponent;
  let fixture: ComponentFixture<AccountReceivablePayableComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AccountReceivablePayableComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AccountReceivablePayableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
