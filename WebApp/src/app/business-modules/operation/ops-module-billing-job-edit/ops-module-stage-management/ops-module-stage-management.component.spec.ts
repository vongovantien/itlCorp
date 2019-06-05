import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleStageManagementComponent } from './ops-module-stage-management.component';

describe('OpsModuleStageManagementComponent', () => {
  let component: OpsModuleStageManagementComponent;
  let fixture: ComponentFixture<OpsModuleStageManagementComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleStageManagementComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleStageManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
