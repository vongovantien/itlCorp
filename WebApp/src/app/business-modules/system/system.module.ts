import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SystemRoutingModule } from './system-routing.module';
import { UserManagementComponent } from './user-management/user-management.component';
import { GroupComponent } from './group/group.component';
import { RoleComponent } from './role/role.component';
import { PermissionComponent } from './permission/permission.component';
import { CompanyInfoComponent } from './company-info/company-info.component';
import { SharedModule } from '../../shared/shared.module';
import { OfficeAddNewComponent } from './office/addnew/office.addnew.component';
import { OfficeComponent } from './office/office.component';
import { OfficeDetailsComponent } from './office/details/office-details.component';
import { DepartmentComponent } from './department/department.component';


@NgModule({
  imports: [
    CommonModule,
    SystemRoutingModule,
    SharedModule
  ],
  declarations: [UserManagementComponent, DepartmentComponent, GroupComponent, RoleComponent, PermissionComponent, CompanyInfoComponent,]
})
export class SystemModule { }
