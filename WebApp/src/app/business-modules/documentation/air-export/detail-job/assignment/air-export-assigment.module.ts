import { NgModule } from '@angular/core';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { ModalModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/shared/shared.module';
import { CommonModule } from '@angular/common';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ShareBusinessAsignmentComponent } from 'src/app/business-modules/share-business';

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


    ],
    providers: [],
    entryComponents: [
        ShareBusinessAsignmentComponent
    ]
})
export class AirExportAssignmentModule {
    static rootComponent = ShareBusinessAsignmentComponent;
}
