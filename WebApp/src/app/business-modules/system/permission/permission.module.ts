import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PermissionComponent } from './permission.component';
import { Routes, RouterModule } from '@angular/router';
import { PermissionFormSearchComponent } from './components/form-search-permission/form-search-permission.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { PermissionFormCreateComponent } from './components/form-create-permission/form-create-permission.component';
import { PermissionCreateComponent } from './add/add-permission.component';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { PermissionDetailComponent } from './detail/detail-permission.component';

const routing: Routes = [
    { path: '', component: PermissionComponent, pathMatch: 'full', data: { name: "Permission", path: "permission", level: 2 } },
    { path: 'new', component: PermissionCreateComponent, data: { name: "New", path: "new", level: 3 } },
    { path: ':id', component: PermissionDetailComponent, data: { name: "New", path: "Detail", level: 3 } }


];

const COMPONENTS = [
    PermissionFormSearchComponent,
    PermissionFormCreateComponent
];

@NgModule({
    declarations: [
        PermissionComponent,
        PermissionCreateComponent,
        PermissionDetailComponent,
        ...COMPONENTS
    ],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        TabsModule.forRoot(),
        PaginationModule.forRoot(),
        ReactiveFormsModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
})
export class PermissionModule { }

