import { DragDropModule } from '@angular/cdk/drag-drop';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { NgxCurrencyModule } from 'ngx-currency';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';


import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TabsModule } from 'ngx-bootstrap/tabs';

import { DeactivateGuardService } from '@core';
import { NgSelectModule } from '@ng-select/ng-select';
import { SharedModule } from 'src/app/shared/shared.module';
import { ChargeConstants } from 'src/constants/charge.const';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { SharedOperationModule } from '../shared-operation.module';
import { JobManagementFormEditComponent } from './components/form-edit/form-edit.component';
import { JobEditLazyLoadComponentModule } from './job-edit-lazy-load-component.module';
import { OpsModuleBillingJobEditComponent } from './job-edit.component';

const routing: Routes = [
    {
        path: ":id", component: OpsModuleBillingJobEditComponent, data: { name: "", serviceId: ChargeConstants.CL_CODE }, canDeactivate: [DeactivateGuardService]
    },

];


const LIB = [
    NgxDaterangepickerMd,
    NgSelectModule,
    TabsModule.forRoot(),
    ModalModule.forRoot(),
    DragDropModule
];

const customCurrencyMaskConfig = {
    align: "right",
    allowNegative: true,
    allowZero: true,
    decimal: ".",
    precision: 3,
    prefix: "",
    suffix: "",
    thousands: ",",
    nullable: true
};


@NgModule({
    imports: [
        RouterModule.forChild(routing),
        PaginationModule.forRoot(),
        SharedModule,
        NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
        JobEditLazyLoadComponentModule, // ? Lazy loading module with 3 tab component (CD, Credit/Debit, Stage),
        ShareBussinessModule,
        SharedOperationModule,
        ...LIB,

    ],
    exports: [],
    declarations: [
        OpsModuleBillingJobEditComponent,
        JobManagementFormEditComponent,
    ],
    providers: [
    ],
})
export class JobEditModule { }
