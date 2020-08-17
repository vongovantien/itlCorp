import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';


import { TabsModule } from 'ngx-bootstrap/tabs';
import { ModalModule } from 'ngx-bootstrap/modal';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';

import { AccountPaymentFormSearchComponent } from './components/form-search/account-payment/form-search-account-payment.component';
import { AccountReceivableFormSearchComponent } from './components/form-search/account-receivable/form-search-account-receivable.component';
import { AccountReceivablePayableComponent } from './account-receivable-payable.component';

import { SharedModule } from 'src/app/shared/shared.module';
import { AccountPaymentUpdateExtendDayPopupComponent } from './components/popup/update-extend-day/update-extend-day.popup';
import { AccountReceivableDetailPopupComponent } from './components/popup/detail-account-receivable/detail-account-receivable.popup';
import { PaymentImportComponent } from './components/payment-import/payment-import.component';
import { PaginationModule } from 'ngx-bootstrap';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { AccountReceivablePayableImportOBHPaymentComponent } from './components/import-obh/import-obh-account-receivable-payable.component';
//import { AccountReceivableListTrialOfficialComponent } from ./components/list-trial-official/list-trial-official-account-receivable.componentt';
import { AccountReceivableListGuaranteedComponent } from './components/list-guaranteed/list-guaranteed-account-receivable.component';
import { AccountReceivableListOtherComponent } from './components/list-other/list-other-account-receivable.component';
import { AccountPaymentListInvoicePaymentComponent } from './components/list-invoice-payment/list-invoice-account-payment.component';
import { AccountPaymentListOBHPaymentComponent } from './components/list-obh-payment/list-obh-account-payment.component';
import { AccountReceivableDetailComponent } from './components/detail/detail-account-receivable.component';
import { AccountReceivableListTrialOfficialComponent } from './components/list-trial-official/list-trial-official-account-receivable.component';


const routing: Routes = [
    {
        path: "",
        data: { name: "", title: 'eFMS Receivable Payable' },
        children: [
            {
                path: '', component: AccountReceivablePayableComponent
            },
            {
                path: 'payment-import', component: PaymentImportComponent, data: { name: "Import" }
            },
            {
                path: 'import-obh', component: AccountReceivablePayableImportOBHPaymentComponent, data: { name: "Import OBH" }
            },
            {
                path: "detail/:id", component: AccountReceivableDetailComponent,

            },
        ]
    }
];


@NgModule({
    declarations: [
        AccountPaymentFormSearchComponent,
        AccountReceivablePayableComponent,
        AccountPaymentListInvoicePaymentComponent,
        AccountPaymentListOBHPaymentComponent,
        AccountReceivableListTrialOfficialComponent,
        AccountReceivableListGuaranteedComponent,
        AccountReceivableListOtherComponent,
        AccountPaymentUpdateExtendDayPopupComponent,
        AccountReceivableDetailPopupComponent,
        PaymentImportComponent,
        AccountReceivablePayableImportOBHPaymentComponent,
        AccountReceivableFormSearchComponent,
        AccountReceivableDetailComponent,
    ],
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        ReactiveFormsModule,
        SharedModule,
        FormsModule,
        TabsModule.forRoot(),
        ModalModule.forRoot(),
        PaginationModule.forRoot(),
        NgxDaterangepickerMd,
        SelectModule,
        PerfectScrollbarModule
    ],
    exports: [],
    providers: [],
})
export class AccountReceivePayableModule { }
