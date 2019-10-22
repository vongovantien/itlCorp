import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PermissionComponent } from './permission.component';
import { Routes, RouterModule } from '@angular/router';
import { PermissionFormSearchComponent } from './components/form-create-permission/form-search-permission.component';
import { SharedModule } from 'src/app/shared/shared.module';

const routing: Routes = [
    { path: '', component: PermissionComponent, pathMatch: 'full', data: { name: "Permission", path: "permission", level: 2 } }
];

const COMPONENTS = [
    PermissionFormSearchComponent
];

@NgModule({
    declarations: [
        PermissionComponent,
        ...COMPONENTS
    ],
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
})
export class PermissionModule { }

