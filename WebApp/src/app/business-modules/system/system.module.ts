import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SystemRoutingModule } from './system-routing.module';
import { RoleComponent } from './role/role.component';
import { PermissionComponent } from './permission/permission.component';
import { SharedModule } from '../../shared/shared.module';


@NgModule({
  imports: [
    CommonModule,
    SystemRoutingModule,
    SharedModule
  ],
  declarations: [RoleComponent, PermissionComponent]
})
export class SystemModule { }
