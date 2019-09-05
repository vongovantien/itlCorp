import { NgModule } from '@angular/core';
import { SettlementPaymentComponent } from './settlement-payment.component';
import { Routes, RouterModule } from '@angular/router';
import { SettlementFormSearchComponent } from './components/form-search-settlement/form-search-settlement.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PaginationModule, AccordionModule, ModalModule } from 'ngx-bootstrap';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { NgProgressModule } from '@ngx-progressbar/core';
import { SettlementPaymentAddNewComponent } from './add/add-settlement-payment.component';
import { SettlementFormCreateComponent } from './components/form-create-settlement/form-create-settlement.component';
import { SettlementListChargeComponent } from './components/list-charge-settlement/list-charge-settlement.component';
import { SettlementPaymentManagementPopupComponent } from './components/popup/payment-management/payment-management.popup';
import { SettlementExistingChargePopupComponent } from './components/popup/existing-charge/existing-charge.popup';
import { SettlementFormChargePopupComponent } from './components/popup/form-charge/form-charge.popup';

const routing: Routes = [
    {
        path: '', component: SettlementPaymentComponent, data: {
            name: "Settlement Payment",
            level: 2
        }
    },
    {
        path: "new", component: SettlementPaymentAddNewComponent,
        data: { name: "New", path: "New", level: 3 }
    },
];

const COMPONENT = [
    SettlementFormSearchComponent,
    SettlementFormCreateComponent,
    SettlementListChargeComponent,
    SettlementPaymentManagementPopupComponent,
    SettlementExistingChargePopupComponent,
    SettlementFormChargePopupComponent 
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        NgxDaterangepickerMd,
        PaginationModule.forRoot(),
        PerfectScrollbarModule,
        ReactiveFormsModule,
        ModalModule.forRoot(),
        NgProgressModule,
        RouterModule.forChild(routing),
        AccordionModule.forRoot(),
    ],
    exports: [],
    declarations: [
        SettlementPaymentComponent,
        SettlementPaymentAddNewComponent,
        ...COMPONENT
    ],
    providers: [],
})
export class SettlementPaymentModule { }
