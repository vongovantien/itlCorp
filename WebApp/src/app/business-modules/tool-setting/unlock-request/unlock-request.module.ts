import { UnlockRequestComponent } from "./unlock-request.component";
import { Route, RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";
import { NgxDaterangepickerMd } from "ngx-daterangepicker-material";
import { ModalModule, PaginationModule } from "ngx-bootstrap";
import { SharedModule } from "src/app/shared/shared.module";
import { SelectModule } from "ng2-select";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgModule } from "@angular/core";
import { UnlockRequestFormSearchComponent } from "./components/form-search-unlock-request/form-search-unlock-request.component";
import { UnlockRequestAddNewComponent } from "./add/add-unlock-request.component";
import { UnlockRequestDetailComponent } from "./detail/detail-unlock-request.component";
import { FroalaEditorModule } from "angular-froala-wysiwyg";
import { UnlockRequestListJobComponent } from "./components/list-job-unlock-request/list-job-unlock-request.component";

const routing: Route[] = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: UnlockRequestComponent
            },
            {
                path: "new", component: UnlockRequestAddNewComponent,
                data: { name: "New", }
            },
            {
                path: ":id", component: UnlockRequestDetailComponent,
                data: { name: "Detail", }
            }
        ]
    }
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        SharedModule,
        SelectModule,
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
        FormsModule,
        PaginationModule.forRoot(),
        ReactiveFormsModule,
        FroalaEditorModule.forRoot(),
    ],
    exports: [],
    declarations: [
        UnlockRequestComponent,
        UnlockRequestFormSearchComponent,
        UnlockRequestAddNewComponent,
        UnlockRequestDetailComponent,
        UnlockRequestListJobComponent
    ],
    providers: [],
})
export class UnlockRequestModule { }