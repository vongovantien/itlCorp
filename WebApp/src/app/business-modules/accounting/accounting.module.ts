import { NgModule } from '@angular/core';
import { routing } from './accounting-routing.module';
import { SettlementPaymentComponent } from './settlement-payment/settlement-payment.component';
import { AccountReceivablePayableComponent } from './account-receivable-payable/account-receivable-payable.component';

const PAGES = [
  SettlementPaymentComponent,
  AccountReceivablePayableComponent,
];

@NgModule({
  imports: [
    routing,
  ],
  declarations: [
    ...PAGES
  ],
  bootstrap: []
})
export class AccountingModule { }
