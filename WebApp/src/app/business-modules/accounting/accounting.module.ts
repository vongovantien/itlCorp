import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { routing } from './accounting-routing.module';
import { AdvancePaymentComponent } from './advance-payment/advance-payment.component';
import { SettlementPaymentComponent } from './settlement-payment/settlement-payment.component';
import { AccountReceivablePayableComponent } from './account-receivable-payable/account-receivable-payable.component';

const PAGES = [
  AdvancePaymentComponent,
  SettlementPaymentComponent,
  AccountReceivablePayableComponent,
];

@NgModule({
  imports: [
    CommonModule,
    routing,
  ],
  declarations: [
    ...PAGES
  ],
  bootstrap: []
})
export class AccountingModule { }
