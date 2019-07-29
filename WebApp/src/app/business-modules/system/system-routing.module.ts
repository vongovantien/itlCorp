import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CompanyInfoComponent } from './company-info/company-info.component';
import { DepartmentComponent } from './department/department.component';
import { GroupComponent } from './group/group.component';
import { PermissionComponent } from './permission/permission.component';
import { RoleComponent } from './role/role.component';
import { UserManagementComponent } from './user-management/user-management.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'company-info',
    pathMatch: 'full'
  },
  {
    path: 'company-info',
    component: CompanyInfoComponent
  },
  {
    path: 'department',
    component: DepartmentComponent
  },
  {
    path: 'group',
    component: GroupComponent
  },
  {
    path: 'permission',
    component: PermissionComponent
  },
  {
    path: 'role',
    component: RoleComponent
  },
  {
    path: 'user-management',
    component: UserManagementComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SystemRoutingModule { }
