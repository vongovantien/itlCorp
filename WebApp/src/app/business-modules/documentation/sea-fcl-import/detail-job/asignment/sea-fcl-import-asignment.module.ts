import { NgModule } from '@angular/core';
import { SeaFCLImportAsignmentComponent } from './sea-fcl-import-asignment.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
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
