import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SystemRoutingModule } from './system-routing.module';
import { UserManagementComponent } from './user-management/user-management.component';
import { GroupComponent } from './group/group.component';
import { RoleComponent } from './role/role.component';
import { PermissionComponent } from './permission/permission.component';
import { SharedModule } from '../../shared/shared.module';
import { DepartmentComponent } from './department/department.component';


@NgModule({
  imports: [
    CommonModule,
    SystemRoutingModule,
    SharedModule
  ],
  declarations: [UserManagementComponent, GroupComponent, RoleComponent, PermissionComponent, DepartmentComponent]
})
export class SystemModule { }
