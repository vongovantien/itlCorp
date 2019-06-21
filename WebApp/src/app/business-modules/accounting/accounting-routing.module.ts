import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AccountReceivablePayableComponent } from './account-receivable-payable/account-receivable-payable.component';
import { AdvancePaymentComponent } from './advance-payment/advance-payment.component';
import { SettlementPaymentComponent } from './settlement-payment/settlement-payment.component';
import { StatementOfAccountComponent } from './statement-of-account/statement-of-account.component';
import { StatementOfAccountAddnewComponent } from './statement-of-account-addnew/statement-of-account-addnew.component';
import { StatementOfAccountEditComponent } from './statement-of-account-edit/statement-of-account-edit.component';

const routes: Routes = [
  {
    path:'',
    redirectTo:'statement-of-account',
    pathMatch:'full'
  },
  {
    path:'account-receivable-payable',
    component:AccountReceivablePayableComponent,
    data:{
        name:"Account Receivable Payable",
        level:2
    }
  },
  {
    path:'advance-payment',
    component:AdvancePaymentComponent,
    data:{
        name:"Advance Payment",
        level:2
    }
  },
  {
    path:'settlement-payment',
    component:SettlementPaymentComponent,
    data:{
        name:"Settlement Payment",
        level:2
    }
  },
  {
    path:'statement-of-account',
    component:StatementOfAccountComponent,
    data:{
        name:"Statement Of Account",
        level:2
    }
  },
  {
    path:'statement-of-account-addnew',
    component:StatementOfAccountAddnewComponent,
    data:{
        name:"Statement Of Account Add New",
        level:2
    }
  },
  {
    path:'statement-of-account-edit',
    component:StatementOfAccountEditComponent,
    data:{
        name:"Statement Of Account Edit",
        level:2
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountingRoutingModule { }
