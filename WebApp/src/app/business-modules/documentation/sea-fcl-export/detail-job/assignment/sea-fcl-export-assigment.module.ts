import { NgModule } from '@angular/core';
import { SeaFCLExportAssignmentComponent } from './sea-fcl-export-assignment.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { ModalModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/shared/shared.module';
import { CommonModule } from '@angular/common';
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
        ShareBussinessModule,

    ],
    exports: [],
    declarations: [
        SeaFCLExportAssignmentComponent


    ],
    providers: [],
    entryComponents: [
        SeaFCLExportAssignmentComponent
    ]
})
export class SeaFCLExportAssignmentModule {
    static rootComponent = SeaFCLExportAssignmentComponent;
}
