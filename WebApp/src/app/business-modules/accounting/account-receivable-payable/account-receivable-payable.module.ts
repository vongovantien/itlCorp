import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';


import { TabsModule } from 'ngx-bootstrap/tabs';
import { ModalModule } from 'ngx-bootstrap/modal';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';

import { AccountReceivePayableFormSearchComponent } from './components/form-search/form-search-account-receivable-payable.component';
import { AccountReceivablePayableComponent } from './account-receivable-payable.component';
import { AccountReceivablePayableListInvoicePaymentComponent } from './components/list-invoice-payment/list-invoice-account-receivable-payable.component';
import { AccountReceivablePayableListOBHPaymentComponent } from './components/list-obh-payment/list-obh-account-receivable-payable.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { AccountReceivablePayableUpdateExtendDayPopupComponent } from './components/popup/update-extend-day/update-extend-day.popup';
import { PaymentImportComponent } from './components/payment-import/payment-import.component';
import { PaginationModule } from 'ngx-bootstrap';
import { AccountReceivablePayableImportOBHPaymentComponent } from './components/import-obh/import-obh-account-receivable-payable.component';
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
        ]
    }
];


@NgModule({
    declarations: [
        AccountReceivePayableFormSearchComponent,
        AccountReceivablePayableComponent,
        AccountReceivablePayableListInvoicePaymentComponent,
        AccountReceivablePayableListOBHPaymentComponent,
        AccountReceivablePayableUpdateExtendDayPopupComponent,
        PaymentImportComponent,
        AccountReceivablePayableImportOBHPaymentComponent
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
        SelectModule
    ],
    exports: [],
    providers: [],
})
export class AccountReceivePayableModule { }
