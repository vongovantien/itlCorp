import { NgModule } from '@angular/core';
import { AuthorizationFormSearchComponent } from './components/form-search-authorization/form-search-authorization.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { ModalModule } from 'ngx-bootstrap/modal';

import { AuthorizationComponent } from './authorization.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { AuthorizationAddPopupComponent } from './components/popup/add-authorization/add-authorization.popup';
import { AuthorizedApprovalFormSearchComponent } from './components/form-search-authorized-approval/form-search-authorized-approval.component';
import { AuthorizedApprovalListComponent } from './components/list-authorized-approval/list-authorized-approval';
import { AuthorizedApprovalPopupComponent } from './components/popup/add-authorized-approval/add-authorized-approval.popup';
const routing: Routes = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: AuthorizationComponent
            },
            // {
            //     path: "new", component: DepartmentAddNewComponent,
            //     data: { name: "New", }
            // },
            // {
            //     path: ":id", component: DepartmentDetailComponent,
            //     data: { name: "Detail", }
            // },
        ]
    },
]
@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        SelectModule,
        FormsModule,
        PaginationModule.forRoot(),
        ReactiveFormsModule,
        RouterModule.forChild(routing),
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
        TabsModule.forRoot()
    ],
    exports: [],
    declarations: [
        AuthorizationComponent,
        AuthorizationFormSearchComponent,
        AuthorizationAddPopupComponent,
        AuthorizedApprovalFormSearchComponent,
        AuthorizedApprovalListComponent,
        AuthorizedApprovalPopupComponent
    ],
    providers: [],
})
export class AuthorizationModule { }
