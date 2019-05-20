import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleBillingJobCreateComponent } from './ops-module-billing-job-create.component';

describe('OpsModuleBillingJobCreateComponent', () => {
  let component: OpsModuleBillingJobCreateComponent;
  let fixture: ComponentFixture<OpsModuleBillingJobCreateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleBillingJobCreateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleBillingJobCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
