import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ARCustomerPaymentFormSearchComponent } from './customer-payment/components/form-search/form-search-customer-payment.component';

const routing: Routes = [
    {
        path: "",
        data: { name: "" },
        children: [
            {
                path: '', redirectTo: 'customer'
            },
            {
                path: '', loadChildren: () => import('./customer-payment/customer-payment.module').then(m => m.ARCustomerPaymentModule),
                data: { name: 'Account Payment' }
            },
            {
                path: 'agency', loadChildren: () => import('./agency-payment/agency-payment.module').then(m => m.ARAgencyPaymentModule),
                data: { name: 'Agency Payment' }
            },
            {
                path: 'receivable', loadChildren: () => import('./account-receivable/account-receivable.module').then(m => m.AccountReceivableModule),
                data: { name: 'A.R Summary' }
            },
            {
                path: 'history-payment', loadChildren: () => import('./history-payment/history-payment.module').then(m => m.ARHistoryPaymentModule),
                data: { name: 'history Payment' }
            },

        ]
    },
];


@NgModule({
    declarations: [
    ],
    imports: [
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
})
export class AccountReceivePayableModule { }
