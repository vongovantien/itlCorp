import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
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
    path: 'company', loadChildren: () => import('./company/company-information.module').then(m => m.CompanyInformationModule),
  },
  {
    path: 'department', loadChildren: () => import('./department/department.module').then(m => m.DepartmentModule),
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
  },
  {
    path: 'office', loadChildren: () => import('./office/office.module').then(m => m.OfficeModule),
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SystemRoutingModule { }
