import { Routes, RouterModule } from '@angular/router';
import { AccountReceivablePayableComponent } from './account-receivable-payable/account-receivable-payable.component';
import { AdvancePaymentComponent } from './advance-payment/advance-payment.component';
import { SettlementPaymentComponent } from './settlement-payment/settlement-payment.component';
import { ModuleWithProviders } from '@angular/compiler/src/core';

const routes: Routes = [
  {
    path: '', redirectTo: 'statement-of-account'
  },
  {
    path: 'statement-of-account', loadChildren: () => import('./statement-of-account/statement-of-account.module').then(m => m.StatementOfAccountModule),
  },
  {
    path: 'account-receivable-payable', component: AccountReceivablePayableComponent, data: {
      name: "Account Receivable Payable",
      level: 2
    }
  },
  {
    path: 'account-receivable-payable', component: AccountReceivablePayableComponent, data: {
      name: "Account Receivable Payable",
      level: 2
    }
  },
  {
    path: 'advance-payment', component: AdvancePaymentComponent, data: {
      name: "Advance Payment",
      level: 2
    }
  },
  {
    path: 'settlement-payment', component: SettlementPaymentComponent, data: {
      name: "Settlement Payment",
      level: 2
    }
  },
  // TODO another MODULE...
];

export const routing: ModuleWithProviders = RouterModule.forChild(routes);
