import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SystemRoutingModule } from './system-routing.module';
import { UserManagementComponent } from './user-management/user-management.component';
import { GroupComponent } from './group/group.component';
import { RoleComponent } from './role/role.component';
import { PermissionComponent } from './permission/permission.component';
import { SharedModule } from '../../shared/shared.module';
import { AddGroupComponent } from './group/add/add-group/add-group.component';
import { FormSearchGroupComponent } from './group/components/form-search-group/form-search-group.component';


@NgModule({
  imports: [
    CommonModule,
    SystemRoutingModule,
    SharedModule
  ],
  declarations: [UserManagementComponent, GroupComponent, RoleComponent, PermissionComponent, AddGroupComponent, FormSearchGroupComponent ]
})
export class SystemModule { }
