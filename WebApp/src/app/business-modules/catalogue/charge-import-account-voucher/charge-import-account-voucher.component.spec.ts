import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChargeImportAccountVoucherComponent } from './charge-import-account-voucher.component';

describe('ChargeImportAccountVoucherComponent', () => {
  let component: ChargeImportAccountVoucherComponent;
  let fixture: ComponentFixture<ChargeImportAccountVoucherComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChargeImportAccountVoucherComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChargeImportAccountVoucherComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
