import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { StateManagmentModule } from 'src/app/business-modules/operation/job-edit/stage-management/stage-management.module';
import { ShareBusinessAsignmentComponent } from 'src/app/business-modules/share-business/components/asignment/asignment.component';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        SharedModule,
        ReactiveFormsModule,
        SelectModule,
        ModalModule,
        NgxDaterangepickerMd,
        StateManagmentModule,
        ShareBussinessModule
    ],
    exports: [],
    declarations: [
    ],
    providers: [],
    entryComponents: [
        ShareBusinessAsignmentComponent
    ]
})
export class SeaFCLImportAsignmentModule {
    static rootComponent = ShareBusinessAsignmentComponent;

}
