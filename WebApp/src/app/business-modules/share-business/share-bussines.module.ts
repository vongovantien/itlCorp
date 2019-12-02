import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { NgxCurrencyModule } from 'ngx-currency';
import { ModalModule, BsDropdownModule, PaginationModule, TooltipModule } from 'ngx-bootstrap';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';

import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';
import { ShareContainerImportComponent } from './components/container-import/container-import.component';
import { PipeModule } from 'src/app/shared/pipes/pipe.module';
import { ShareBussinessFormCreateSeaImportComponent, ShareBussinessBuyingChargeComponent, ShareBussinessSellingChargeComponent, ShareBussinessOBHChargeComponent, ShareBussinessProfitSummaryComponent, ShareBussinessShipmentGoodSummaryComponent, ShareBussinessContainerListPopupComponent, ShareBussinessGrantTotalProfitComponent, ShareBusinessImportJobDetailPopupComponent, ShareBusinessFormSearchImportJobComponent, ShareBussinessCdNoteListComponent, ShareBussinessCdNoteAddPopupComponent, ShareBussinessCdNoteAddRemainingChargePopupComponent, ShareBussinessCdNoteDetailPopupComponent, ShareBussinessShipmentGoodSummaryLCLComponent } from './components';


import { reducers, effects } from './store';
import { ShareBusinessAssignStagePopupComponent } from './components/stage-management/assign-stage/assign-stage.popup';
import { ShareBusinessAsignmentComponent } from './components/asignment/asignment.component';
import { ShareBusinessStageManagementDetailComponent } from './components/stage-management/detail/detail-stage-popup.component';

const COMPONENTS = [
    ShareBussinessBuyingChargeComponent,
    ShareBussinessSellingChargeComponent,
    ShareBussinessOBHChargeComponent,
    ShareContainerImportComponent,
    ShareBussinessProfitSummaryComponent,
    ShareBussinessShipmentGoodSummaryComponent,
    ShareBussinessContainerListPopupComponent,
    ShareBussinessGrantTotalProfitComponent,
    ShareBusinessImportJobDetailPopupComponent,
    ShareBusinessFormSearchImportJobComponent,
    ShareBussinessCdNoteListComponent,
    ShareBussinessCdNoteAddPopupComponent,
    ShareBussinessCdNoteAddRemainingChargePopupComponent,
    ShareBussinessCdNoteDetailPopupComponent,
    ShareBussinessFormCreateSeaImportComponent,
    ShareBussinessShipmentGoodSummaryLCLComponent,
    ShareBusinessAssignStagePopupComponent,
    ShareBusinessAsignmentComponent,
    ShareBusinessStageManagementDetailComponent
];


const customCurrencyMaskConfig = {
    align: "right",
    allowNegative: false,
    allowZero: true,
    decimal: ".",
    precision: 2,
    prefix: "",
    suffix: "",
    thousands: ",",
    nullable: true
};

@NgModule({
    declarations: [
        ...COMPONENTS
    ],
    imports: [
        CommonModule,
        CommonComponentModule,
        FormsModule,
        SelectModule,
        ReactiveFormsModule,
        ModalModule.forRoot(),
        NgxDaterangepickerMd.forRoot(),
        DirectiveModule,
        PipeModule,
        SelectModule,
        PaginationModule.forRoot(),
        BsDropdownModule.forRoot(),
        TooltipModule.forRoot(),
        NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
        StoreModule.forFeature('share-bussiness', reducers),
        EffectsModule.forFeature(effects),


    ],
    exports: [
        ...COMPONENTS
    ],
    providers: [],
})
export class ShareBussinessModule {

}
