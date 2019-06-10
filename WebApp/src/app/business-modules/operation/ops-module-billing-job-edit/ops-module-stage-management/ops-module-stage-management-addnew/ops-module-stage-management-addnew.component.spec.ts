import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OpsModuleStageManagementAddnewComponent } from './ops-module-stage-management-addnew.component';

describe('OpsModuleStageManagementAddnewComponent', () => {
  let component: OpsModuleStageManagementAddnewComponent;
  let fixture: ComponentFixture<OpsModuleStageManagementAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OpsModuleStageManagementAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OpsModuleStageManagementAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
