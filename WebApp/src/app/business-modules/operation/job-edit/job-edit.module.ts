import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';
import { NgxCurrencyModule } from 'ngx-currency';
import { TabsModule, ModalModule, PaginationModule } from 'ngx-bootstrap';

import { OpsModuleBillingJobEditComponent } from './job-edit.component';
import { AddBuyingRatePopupComponent } from './charge-list/add-buying-rate-popup/add-buying-rate-popup.component';
import { EditBuyingRatePopupComponent } from './charge-list/edit-buying-rate-popup/edit-buying-rate-popup.component';
import { AddSellingRatePopupComponent } from './charge-list/add-selling-rate-popup/add-selling-rate-popup.component';
import { EditSellingRatePopupComponent } from './charge-list/edit-selling-rate-popup/edit-selling-rate-popup.component';
import { AddObhRatePopupComponent } from './charge-list/add-obh-rate-popup/add-obh-rate-popup.component';
import { EditObhRatePopupComponent } from './charge-list/edit-obh-rate-popup/edit-obh-rate-popup.component';
import { PlSheetPopupComponent } from './pl-sheet-popup/pl-sheet.popup';
import { SharedModule } from 'src/app/shared/shared.module';
import { JobManagementBuyingRateComponent } from './components/buying-rate/buying-rate.component';
import { JobManagementSellingRateComponent } from './components/selling-rate/selling-rate.component';
import { JobManagementOBHComponent } from './components/obh/obh.component';
import { JobEditLazyLoadComponentModule } from './job-edit-lazy-load-component.module';
import { JobEditShareModule } from './job-edit-share.module';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { ChargeConstants } from 'src/constants/charge.const';

const routing: Routes = [
    {
        path: ":id", component: OpsModuleBillingJobEditComponent, data: { name: "Job Edit", level: 3, serviceId: ChargeConstants.CL_CODE }
    },

];


const LIB = [
    NgxDaterangepickerMd,
    SelectModule,
    TabsModule.forRoot(),
    ModalModule.forRoot(),
    DragDropModule
];

const COMPONENTS = [
    JobManagementBuyingRateComponent,
    JobManagementSellingRateComponent,
    JobManagementOBHComponent,
    AddBuyingRatePopupComponent,
    EditBuyingRatePopupComponent,
    AddSellingRatePopupComponent,
    EditSellingRatePopupComponent,
    AddObhRatePopupComponent,
    EditObhRatePopupComponent,

    PlSheetPopupComponent,
];

const customCurrencyMaskConfig = {
    align: "right",
    allowNegative: false,
    allowZero: true,
    decimal: ".",
    precision: 0,
    prefix: "",
    suffix: "",
    thousands: ",",
    nullable: true
};

@NgModule({
    imports: [
        CommonModule,
        JobEditShareModule,  // ? Share Module with Credit/Debit Note
        RouterModule.forChild(routing),
        PaginationModule.forRoot(),
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
        JobEditLazyLoadComponentModule, // ? Lazy loading module with 3 tab component (CD, Credit/Debit, Stage),
        ShareBussinessModule,
        ...LIB,

    ],
    exports: [],
    declarations: [
        OpsModuleBillingJobEditComponent,
        ...COMPONENTS,
    ],
    providers: [
    ],
    bootstrap: [
        OpsModuleBillingJobEditComponent
    ],
})
export class JobEditModule { }
