import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalModule, BsDropdownModule, PaginationModule, TooltipModule } from 'ngx-bootstrap';
import { FormsModule } from '@angular/forms';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { NgxCurrencyModule } from 'ngx-currency';
import { ShareContainerImportComponent } from './components/container-import/container-import.component';
import { StoreModule } from '@ngrx/store';
import { reducers, effects } from './store';
import { EffectsModule } from '@ngrx/effects';
import { PipeModule } from 'src/app/shared/pipes/pipe.module';
import { ShareBussinessSellingChargeComponent } from './components/selling-charge/selling-charge.component';
import { ShareBussinessOBHChargeComponent } from './components/obh-charge/obh-charge.component';
import { ShareBussinessProfitSummaryComponent } from './components/profit-summary/profit-summary.component';
import { ShareBussinessBuyingChargeComponent } from '.';
import { ShareBussinessShipmentGoodSummaryComponent } from './components/shipment-good-summary/shipment-good-summary.component';
import { ShareBussinessContainerListPopupComponent } from './components/container-list/container-list.popup';
import { ShareBussinessGrantTotalProfitComponent } from './components/grant-total-profit/grant-total-profit.component';
import { CdNoteListComponent } from './components/cd-note/cd-note-list/cd-note-list.component';
import { CdNoteAddPopupComponent } from './components/cd-note/add-cd-note/add-cd-note.popup';
import { CdNoteAddRemainingChargePopupComponent } from './components/cd-note/add-remaining-charge/add-remaining-charge.popup';
import { CdNoteDetailPopupComponent } from './components/cd-note/detail-cd-note/detail-cd-note.popup';

const COMPONENTS = [
    ShareBussinessBuyingChargeComponent,
    ShareBussinessSellingChargeComponent,
    ShareBussinessOBHChargeComponent,
    ShareContainerImportComponent,
    ShareBussinessProfitSummaryComponent,
    ShareBussinessShipmentGoodSummaryComponent,
    ShareBussinessContainerListPopupComponent,
    ShareBussinessGrantTotalProfitComponent,
    CdNoteListComponent,
    CdNoteAddPopupComponent,
    CdNoteAddRemainingChargePopupComponent,
    CdNoteDetailPopupComponent,
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
    declarations: [
        ...COMPONENTS
    ],
    imports: [
        CommonModule,
        CommonComponentModule,
        FormsModule,
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