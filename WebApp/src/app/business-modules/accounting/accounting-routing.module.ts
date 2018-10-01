import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AccountReceivablePayableComponent } from './account-receivable-payable/account-receivable-payable.component';
import { AdvancePaymentComponent } from './advance-payment/advance-payment.component';
import { SettlementPaymentComponent } from './settlement-payment/settlement-payment.component';
import { StatementOfAccountComponent } from './statement-of-account/statement-of-account.component';

const routes: Routes = [
  {
    path:'',
    redirectTo:'statement-of-account',
    pathMatch:'full'
  },
  {
    path:'account-receivable-payable',
    component:AccountReceivablePayableComponent
  },
  {
    path:'advance-payment',
    component:AdvancePaymentComponent
  },
  {
    path:'settlement-payment',
    component:SettlementPaymentComponent
  },
  {
    path:'statement-of-account',
    component:StatementOfAccountComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountingRoutingModule { }
