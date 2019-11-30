import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { NgxCurrencyModule } from 'ngx-currency';
import { ModalModule, BsDropdownModule, PaginationModule, TooltipModule } from 'ngx-bootstrap';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';
import { ShareContainerImportComponent } from './components/container-import/container-import.component';
import { PipeModule } from 'src/app/shared/pipes/pipe.module';

import { ShareBussinessSellingChargeComponent } from './components/selling-charge/selling-charge.component';
import { ShareBussinessOBHChargeComponent } from './components/obh-charge/obh-charge.component';
import { ShareBussinessProfitSummaryComponent } from './components/profit-summary/profit-summary.component';
import { ShareBussinessBuyingChargeComponent } from '.';
import { ShareBussinessShipmentGoodSummaryComponent } from './components/shipment-good-summary/shipment-good-summary.component';
import { ShareBussinessContainerListPopupComponent } from './components/container-list/container-list.popup';
import { ShareBussinessGrantTotalProfitComponent } from './components/grant-total-profit/grant-total-profit.component';
import { ShareBussinessCdNoteListComponent } from './components/cd-note/cd-note-list/cd-note-list.component';
import { ShareBussinessCdNoteAddPopupComponent } from './components/cd-note/add-cd-note/add-cd-note.popup';
import { ShareBussinessCdNoteAddRemainingChargePopupComponent } from './components/cd-note/add-remaining-charge/add-remaining-charge.popup';
import { ShareBussinessCdNoteDetailPopupComponent } from './components/cd-note/detail-cd-note/detail-cd-note.popup';

import { reducers, effects } from './store';
import { ShareBusinessImportJobDetailPopupComponent } from './components/import-job-detail/import-job-detail.popup';
import { ShareBusinessFormSearchImportJobComponent } from './components/import-job-detail/components/form-search-import/form-search-import-job-detail.component';



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
        ReactiveFormsModule,
        ModalModule.forRoot(),
        NgxDaterangepickerMd.forRoot(),
        DirectiveModule,
        PipeModule,
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
