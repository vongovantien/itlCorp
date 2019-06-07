import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleStageManagementDetailComponent } from './ops-module-stage-management-detail.component';

describe('OpsModuleStageManagementDetailComponent', () => {
  let component: OpsModuleStageManagementDetailComponent;
  let fixture: ComponentFixture<OpsModuleStageManagementDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleStageManagementDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleStageManagementDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
