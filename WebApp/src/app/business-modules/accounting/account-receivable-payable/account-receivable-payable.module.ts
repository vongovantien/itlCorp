import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

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
                data: { name: 'Customer/Agency Payment', title: 'eFMS AR Payment' }
            },
            {
                path: 'summary', loadChildren: () => import('./account-receivable/account-receivable.module').then(m => m.AccountReceivableModule),
                data: { name: 'A.R Summary', title: 'eFMS AR Summary' }
            },
            {
                path: 'history-payment', loadChildren: () => import('./history-payment/history-payment.module').then(m => m.ARHistoryPaymentModule),
                data: { name: 'History Payment', title: 'eFMS AR History Payment' }
            },
            {
                path: 'prepaid-payment', loadChildren: () => import('./prepaid-payment/prepaid-payment.module').then(m => m.ARPrepaidPaymentModule),
                data: { name: 'PrePaid Payment', title: 'eFMS AR Pre Paid Payment' }
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
