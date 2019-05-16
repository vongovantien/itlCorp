import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleBillingComponent } from './ops-module-billing.component';

describe('OpsModuleBillingComponent', () => {
  let component: OpsModuleBillingComponent;
  let fixture: ComponentFixture<OpsModuleBillingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleBillingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleBillingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
