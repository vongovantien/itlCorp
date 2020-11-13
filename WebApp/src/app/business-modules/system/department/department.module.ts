import { NgModule } from '@angular/core';
import { DepartmentFormSearchComponent } from './components/form-search-department/form-search-department.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { DepartmentComponent } from './department.component';
import { DepartmentAddNewComponent } from './add/add-department.component';
import { DepartmentDetailComponent } from './detail/detail-department.component';
import { SelectModule } from 'ng2-select';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { ShareSystemModule } from '../../share-system/share-system.module';
import { ShareSystemDetailPermissionComponent } from '../../share-system/components/permission/permission-detail.component';
const routing: Routes = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: DepartmentComponent
            },
            {
                path: "new", component: DepartmentAddNewComponent,
                data: { name: "New", }
            },
            {
                path: ":id", component: DepartmentDetailComponent,
                data: { name: "Detail", }
            },
            {
                path: ':id/:ido/:uid/:type', component: ShareSystemDetailPermissionComponent, data: { name: "UserPermission" }
            }
        ]
    },
]
@NgModule({
    imports: [
        SharedModule,
        SelectModule,
        PaginationModule.forRoot(),
        RouterModule.forChild(routing),
        ShareSystemModule,
        TabsModule.forRoot()
    ],
    exports: [],
    declarations: [
        DepartmentComponent,
        DepartmentFormSearchComponent,
        DepartmentAddNewComponent,
        DepartmentDetailComponent
    ],
    providers: [],
})
export class DepartmentModule { }
