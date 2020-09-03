import { Routes, RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';

const routes: Routes = [
    {
        path: '', redirectTo: 'statement-of-account'
    },
    {
        path: 'statement-of-account', loadChildren: () => import('./statement-of-account/statement-of-account.module').then(m => m.StatementOfAccountModule),
        data: {
            name: 'Statement Of Account'
        }
    },
    {
        path: 'advance-payment', loadChildren: () => import('./advance-payment/advance-payment.module').then(m => m.AdvancePaymentModule),
        data: {
            name: 'Advance Payment'
        }
    },
    {
        path: 'settlement-payment', loadChildren: () => import('./settlement-payment/settlement-payment.module').then(m => m.SettlementPaymentModule),
        data: {
            name: 'Settlement Payment'
        }
    },
    {
        path: 'account-receivable-payable', loadChildren: () => import('./account-receivable-payable/account-receivable-payable.module').then(m => m.AccountReceivePayableModule),
        data: {
            name: 'Accounts Receivable Payable'
        }
    },
    {
        path: 'management', loadChildren: () => import('./accounting-management/accounting-managment.module').then(m => m.AccountingManagementModule), data: { name: 'Accounting Management' }
    },
    // TODO another MODULE...
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class AccountingRouting { }
