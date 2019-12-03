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
import {
    ShareBussinessFormCreateSeaImportComponent,
    ShareBussinessBuyingChargeComponent,
    ShareBussinessSellingChargeComponent,
    ShareBussinessOBHChargeComponent, ShareBussinessProfitSummaryComponent,
    ShareBussinessShipmentGoodSummaryComponent, ShareBussinessContainerListPopupComponent,
    ShareBussinessGrantTotalProfitComponent,
    ShareBusinessImportJobDetailPopupComponent,
    ShareBusinessFormSearchImportJobComponent,
    ShareBussinessCdNoteListComponent,
    ShareBussinessCdNoteAddPopupComponent,
    ShareBussinessCdNoteAddRemainingChargePopupComponent,
    ShareBussinessCdNoteDetailPopupComponent,
    ShareBussinessShipmentGoodSummaryLCLComponent,
    ShareBussinessHBLGoodSummaryComponent
} from './components';


import { reducers, effects } from './store';
import { ShareBusinessAssignStagePopupComponent } from './components/stage-management/assign-stage/assign-stage.popup';
import { ShareBusinessAsignmentComponent } from './components/asignment/asignment.component';
import { ShareBusinessStageManagementDetailComponent } from './components/stage-management/detail/detail-stage-popup.component';
import { ShareBusinessAddHblToManifestComponent } from './components/manifest/popup/add-hbl-to-manifest.popup';
import { ShareBusinessFormManifestComponent } from './components/manifest/form-manifest/components/form-manifest.component';

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
    ShareBusinessFormManifestComponent,
    ShareBusinessAddHblToManifestComponent,

    ShareBussinessFormCreateSeaImportComponent,
    ShareBussinessShipmentGoodSummaryLCLComponent,
    ShareBusinessAssignStagePopupComponent,
    ShareBusinessAsignmentComponent,
    ShareBusinessStageManagementDetailComponent,
    ShareBussinessHBLGoodSummaryComponent
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
        SelectModule,
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
