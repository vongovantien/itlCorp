import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AccountingRoutingModule } from './accounting-routing.module';
import { StatementOfAccountComponent } from './statement-of-account/statement-of-account.component';
import { AdvancePaymentComponent } from './advance-payment/advance-payment.component';
import { SettlementPaymentComponent } from './settlement-payment/settlement-payment.component';
import { AccountReceivablePayableComponent } from './account-receivable-payable/account-receivable-payable.component';

@NgModule({
  imports: [
    CommonModule,
    AccountingRoutingModule
  ],
  declarations: [StatementOfAccountComponent, AdvancePaymentComponent, SettlementPaymentComponent, AccountReceivablePayableComponent]
})
export class AccountingModule { }
