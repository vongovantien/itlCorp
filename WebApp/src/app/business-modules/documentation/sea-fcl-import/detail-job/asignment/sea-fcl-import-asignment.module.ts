import { NgModule } from '@angular/core';
import { SeaFCLImportAsignmentComponent } from './sea-fcl-import-asignment.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { OpsModuleStageManagementComponent } from 'src/app/business-modules/operation/job-edit/stage-management/stage-management.component';
import { OpsModuleStageManagementDetailComponent } from 'src/app/business-modules/operation/job-edit/stage-management/detail/detail-stage-popup.component';
import { OpsModuleStageManagementAddStagePopupComponent } from 'src/app/business-modules/operation/job-edit/stage-management/add/add-stage.popup.component';
import { AssignStagePopupComponent } from 'src/app/business-modules/operation/job-edit/stage-management/assign-stage/assign-stage.popup';
import { StateManagmentModule } from 'src/app/business-modules/operation/job-edit/stage-management/stage-management.module';


@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        SharedModule,
        ReactiveFormsModule,
        SelectModule,
        ModalModule,
        NgxDaterangepickerMd,
        StateManagmentModule

    ],
    exports: [],
    declarations: [
        SeaFCLImportAsignmentComponent,


    ],
    providers: [],
    entryComponents: [
        SeaFCLImportAsignmentComponent
    ]
})
export class SeaFCLImportAsignmentModule {
    static rootComponent = SeaFCLImportAsignmentComponent;

}
