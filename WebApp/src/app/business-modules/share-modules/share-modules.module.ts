import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { ModalModule } from 'ngx-bootstrap/modal/';
import { DirectiveModule } from "src/app/shared/directives/directive.module";
import { PipeModule } from "src/app/shared/pipes/pipe.module";
import { CommercialBankListComponent } from './../commercial/components/bank/commercial-bank-list.component';
import { PayableComponent } from './../commercial/components/payable/payable.component';
import { FormUpdateEmailContractComponent } from './../commercial/components/popup/form-update-email-contract/form-update-email-contract.component';
import { FormContractCommercialPopupComponent, PartnerRejectPopupComponent, ShareModulesInputShipmentPopupComponent, ShareModulesReasonRejectPopupComponent } from './components';

import { CollapseModule } from 'ngx-bootstrap/collapse';
import { TabsModule } from 'ngx-bootstrap/tabs';

import { NgSelectModule } from "@ng-select/ng-select";
import { NgxDaterangepickerMd } from "ngx-daterangepicker-material";
import { CommonComponentModule } from "src/app/shared/common/common.module";
import { CommercialBranchSubListComponent } from "../commercial/components/branch-sub/commercial-branch-sub-list.component";
import { CommercialContractListComponent } from "../commercial/components/contract/commercial-contract-list.component";
import { CommercialEmailListComponent } from '../commercial/components/email/commercial-email-list.component';
import { FormSearchExportComponent } from '../commercial/components/popup/form-search-export/form-search-export.popup';
import { SalesmanCreditLimitPopupComponent } from "../commercial/components/popup/salesman-credit-limit.popup";
import { ShareBussinessAdjustDebitValuePopupComponent } from './components/adjust-debit-value/adjust-debit-value.popup';
import { FormBankCommercialCatalogueComponent } from './components/form-bank-commercial-catalogue/form-bank-commercial-catalogue.component';
import { FormUpdateEmailCommercialCatalogueComponent } from './components/form-update-email-commercial-catalogue/form-update-email-commercial-catalogue.popup';

const COMPONENTS = [
    ShareModulesInputShipmentPopupComponent,
    FormContractCommercialPopupComponent,
    CommercialContractListComponent,
    SalesmanCreditLimitPopupComponent,
    PartnerRejectPopupComponent,
    CommercialBranchSubListComponent,
    CommercialEmailListComponent,
    CommercialBankListComponent,
    FormUpdateEmailCommercialCatalogueComponent,
    FormUpdateEmailContractComponent,
    ShareModulesReasonRejectPopupComponent,
    FormSearchExportComponent,
    ShareBussinessAdjustDebitValuePopupComponent,
    PayableComponent,
    FormBankCommercialCatalogueComponent
];

@NgModule({
    declarations: [
        ...COMPONENTS,
    ],
    imports: [
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
        CommonComponentModule,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        DirectiveModule,
        PipeModule,
        NgSelectModule,
        TabsModule.forRoot(),
        CollapseModule.forRoot(),
        NgxDaterangepickerMd,
        FormsModule
    ],
    exports: [
        ...COMPONENTS
    ],
    providers: [],
})
export class ShareModulesModule {

}
