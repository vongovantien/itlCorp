import { NgModule } from '@angular/core';
import { DepartmentFormSearchComponent } from './components/form-search-department/form-search-department.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { DepartmentComponent } from './department.component';
import { DepartmentAddNewComponent } from './add/add-department.component';
import { DepartmentDetailComponent } from './detail/detail-department.component';
import { ReactiveFormsModule } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { PaginationModule } from 'ngx-bootstrap';
const routing: Routes = [
    {
        path: '', component: DepartmentComponent, pathMatch: 'full',
        data: { name: "Department", level: 2 }
    },
    {
        path: "new", component: DepartmentAddNewComponent,
        data: { name: "New", path: "New", level: 3 }
    },
    {
        path: "detail", component: DepartmentDetailComponent,
        data: { name: "Detail", path: "Detail", level: 3 }
    },
]
@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        SelectModule,
        FormsModule,
        PaginationModule,
        ReactiveFormsModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        DepartmentComponent,
        DepartmentFormSearchComponent,
        DepartmentAddNewComponent,
        DepartmentDetailComponent
    ],
    providers: [],
    bootstrap: [
        DepartmentComponent
    ]
})
export class DepartmentModule { }
