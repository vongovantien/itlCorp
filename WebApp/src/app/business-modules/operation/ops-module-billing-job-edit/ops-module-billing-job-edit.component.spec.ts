import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleBillingJobEditComponent } from './ops-module-billing-job-edit.component';

describe('OpsModuleBillingJobEditComponent', () => {
  let component: OpsModuleBillingJobEditComponent;
  let fixture: ComponentFixture<OpsModuleBillingJobEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleBillingJobEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleBillingJobEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
