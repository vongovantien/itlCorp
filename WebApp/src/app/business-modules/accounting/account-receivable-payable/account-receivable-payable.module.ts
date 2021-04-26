import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddCustomerDebitComponent } from './components/popup/customer-debit/add-customer-debit/add-customer-debit.component';
import { AddCustomerDebit } from './components/popup/add-customer-debit.popup/add-customer-debit.popup.component';

const routing: Routes = [
    {
        path: "",
        data: { name: "" },
        children: [
            {
                path: '', redirectTo: 'customer'
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
    AddCustomerDebitComponent,
    AddCustomerDebit.PopupComponent],
    imports: [
        RouterModule.forChild(routing),
    ],
    exports: [],
    providers: [],
})
export class AccountReceivePayableModule { }
