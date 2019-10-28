import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { RoleComponent } from './role/role.component';

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
        path: 'group', loadChildren: () => import('./group/group.module').then(m => m.GroupModule),
    },
    {
        path: 'permission', loadChildren: () => import('./permission/permission.module').then(m => m.PermissionModule),
    },
    {
        path: 'role',
        component: RoleComponent
    },
    {
    path: 'user-management', loadChildren: () => import('./user-management/user-management.module').then(m => m.UserManagementModule)
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
