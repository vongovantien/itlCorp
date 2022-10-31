import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';

import { NgxCurrencyModule } from 'ngx-currency';

import { CollapseModule } from 'ngx-bootstrap/collapse';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TooltipModule } from 'ngx-bootstrap/tooltip';


import { FroalaEditorModule } from 'angular-froala-wysiwyg';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';

import { PipeModule } from 'src/app/shared/pipes/pipe.module';
import {
    ShareBusinessAddHblToManifestComponent, ShareBusinessArrivalNoteAirComponent, ShareBusinessArrivalNoteComponent, ShareBusinessAsignmentComponent, ShareBusinessAssignStagePopupComponent, ShareBusinessAttachListHouseBillComponent, ShareBusinessDeliveryOrderComponent, ShareBusinessFormManifestComponent, ShareBusinessFormSearchHouseBillComponent, ShareBusinessFormSearchImportJobComponent, ShareBusinessFormSearchSeaComponent, ShareBusinessHousebillsInManifestComponent, ShareBusinessImportHouseBillDetailComponent, ShareBusinessImportJobDetailPopupComponent, ShareBusinessStageManagementDetailComponent, ShareBussinessBillInstructionHousebillsSeaExportComponent, ShareBussinessBuyingChargeComponent, ShareBussinessCdNoteAddAirPopupComponent, ShareBussinessCdNoteAddPopupComponent, ShareBussinessCdNoteAddRemainingChargeAirPopupComponent, ShareBussinessCdNoteAddRemainingChargePopupComponent, ShareBussinessCdNoteDetailAirPopupComponent, ShareBussinessCdNoteDetailPopupComponent, ShareBussinessCdNoteListAirComponent, ShareBussinessCdNoteListComponent, ShareBussinessContainerListPopupComponent, ShareBussinessDateTimeModifiedComponent, ShareBussinessFilesAttachComponent, ShareBussinessGoodsListPopupComponent, ShareBussinessGrantTotalProfitComponent, ShareBussinessHBLFCLContainerPopupComponent, ShareBussinessHBLGoodSummaryFCLComponent, ShareBussinessHBLGoodSummaryLCLComponent, ShareBussinessInputDailyExportPopupComponent, ShareBussinessJobDetailButtonListComponent, ShareBussinessOBHChargeComponent, ShareBussinessPaymentMethodPopupComponent, ShareBussinessProfitSummaryComponent, ShareBussinessSellingChargeComponent, ShareBussinessShipmentGoodSummaryComponent, ShareContainerImportComponent, ShareGoodsImportComponent
} from './components';


import { AppComboGridComponent } from '@common';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgxSpinnerModule } from 'ngx-spinner';
import { ValidatorModule } from 'src/app/shared/validators/validator.module';
import { ShareModulesModule } from '../share-modules/share-modules.module';
import { ShareBusinessAddAttachmentPopupComponent } from './components/add-attachment/add-attachment.popup';
import { ShareBussinessMassUpdatePodComponent } from './components/hbl/mass-update-pod/mass-update-pod.component';
import { ShareBusinessProofOfDelieveyComponent } from './components/hbl/proof-of-delivery/proof-of-delivery.component';
import { ShareBusinessReAlertComponent } from './components/pre-alert/pre-alert.component';
import { ShareBussinessAccountingModule } from './share-bussines-accounting.module';
import { effects, reducers } from './store';




const COMPONENTS = [
    ShareBussinessBuyingChargeComponent,
    ShareBussinessSellingChargeComponent,
    ShareBussinessOBHChargeComponent,
    ShareContainerImportComponent,
    ShareGoodsImportComponent,
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
    ShareBussinessGoodsListPopupComponent,
    ShareBusinessAssignStagePopupComponent,
    ShareBusinessAsignmentComponent,
    ShareBusinessStageManagementDetailComponent,
    ShareBusinessArrivalNoteComponent,
    ShareBusinessDeliveryOrderComponent,
    ShareBusinessFormSearchSeaComponent,
    ShareBussinessHBLGoodSummaryLCLComponent,
    ShareBusinessImportHouseBillDetailComponent,
    ShareBusinessFormSearchHouseBillComponent,
    ShareBussinessHBLGoodSummaryFCLComponent,
    ShareBussinessCdNoteListAirComponent,
    ShareBussinessCdNoteAddAirPopupComponent,
    ShareBussinessCdNoteAddRemainingChargeAirPopupComponent,
    ShareBussinessCdNoteDetailAirPopupComponent,
    ShareBussinessFilesAttachComponent,
    ShareBusinessHousebillsInManifestComponent,
    ShareBusinessArrivalNoteAirComponent,
    ShareBussinessDateTimeModifiedComponent,
    ShareBussinessHBLFCLContainerPopupComponent,
    ShareBusinessReAlertComponent,
    ShareBusinessAddAttachmentPopupComponent,
    ShareBussinessBillInstructionHousebillsSeaExportComponent,
    ShareBusinessAttachListHouseBillComponent,
    ShareBussinessInputDailyExportPopupComponent,
    ShareBussinessPaymentMethodPopupComponent,
    ShareBussinessJobDetailButtonListComponent,
    ShareBusinessProofOfDelieveyComponent,
    ShareBussinessMassUpdatePodComponent,

];



const customCurrencyMaskConfig = {
    align: "right",
    allowNegative: true,
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
        ...COMPONENTS,
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
        NgSelectModule,
        ValidatorModule,
        PaginationModule.forRoot(),
        BsDropdownModule.forRoot(),
        TooltipModule.forRoot(),
        FroalaEditorModule.forRoot(),
        NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
        StoreModule.forFeature('share-bussiness', reducers),
        EffectsModule.forFeature(effects),
        CollapseModule.forRoot(),
        NgxSpinnerModule,
        ShareModulesModule,
        ShareBussinessAccountingModule
    ],
    exports: [
        ...COMPONENTS
    ],
    providers: [],
    entryComponents: [
        AppComboGridComponent
    ]
})
export class ShareBussinessModule {

}
