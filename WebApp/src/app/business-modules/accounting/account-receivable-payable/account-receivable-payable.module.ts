import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { TabsModule } from 'ngx-bootstrap/tabs';
import { ModalModule } from 'ngx-bootstrap/modal';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

import { AccountPaymentFormSearchComponent } from './components/form-search/account-payment/form-search-account-payment.component';
import { AccountReceivablePayableComponent } from './account-receivable-payable.component';

import { SharedModule } from 'src/app/shared/shared.module';
import { AccountPaymentUpdateExtendDayPopupComponent } from './components/popup/update-extend-day/update-extend-day.popup';
import { PaymentImportComponent } from './components/payment-import/payment-import.component';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { AccountReceivablePayableImportOBHPaymentComponent } from './components/import-obh/import-obh-account-receivable-payable.component';


import { AccountPaymentListInvoicePaymentComponent } from './components/list-invoice-payment/list-invoice-account-payment.component';
import { AccountPaymentListOBHPaymentComponent } from './components/list-obh-payment/list-obh-account-payment.component';
import { NgSelectModule } from '@ng-select/ng-select';



const routing: Routes = [
    {
        path: "",
        data: { name: "" },
        children: [
            {
                path: '', component: AccountReceivablePayableComponent, redirectTo: 'customer',
                data: { name: 'Account Payment' }
            },
            {
                path: 'customer', loadChildren: () => import('./customer-payment/customer-payment.module').then(m => m.ARCustomerPaymentModule),
                data: { name: 'Customer Payment' }
            },
            {
                path: 'agency', loadChildren: () => import('./agency-payment/agency-payment.module').then(m => m.ARAgencyPaymentModule),
                data: { name: 'Agency Payment' }
            },
            {
                path: 'payment-import', component: PaymentImportComponent, data: { name: "Import" }
            },
            {
                path: 'import-obh', component: AccountReceivablePayableImportOBHPaymentComponent, data: { name: "Import OBH" }
            },
            {
                path: 'receivable', loadChildren: () => import('./account-receivable/account-receivable.module').then(m => m.AccountReceivableModule),
                data: { name: 'A.R' }
            }
        ]
    },
];


@NgModule({
    declarations: [
        AccountReceivablePayableComponent,
        AccountPaymentFormSearchComponent,
        AccountPaymentListInvoicePaymentComponent,
        AccountPaymentListOBHPaymentComponent,
        AccountPaymentUpdateExtendDayPopupComponent,
        PaymentImportComponent,
        AccountReceivablePayableImportOBHPaymentComponent,

    ],
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        TabsModule.forRoot(),
        ModalModule.forRoot(),
        PaginationModule.forRoot(),
        NgxDaterangepickerMd,
        NgSelectModule,
        PerfectScrollbarModule,
    ],
    exports: [],
    providers: [],
})
export class AccountReceivePayableModule { }
