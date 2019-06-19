import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { AccountingRoutingModule } from './accounting-routing.module';
import { StatementOfAccountComponent } from './statement-of-account/statement-of-account.component';
import { AdvancePaymentComponent } from './advance-payment/advance-payment.component';
import { SettlementPaymentComponent } from './settlement-payment/settlement-payment.component';
import { AccountReceivablePayableComponent } from './account-receivable-payable/account-receivable-payable.component';
import { SharedModule } from '../../shared/shared.module';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

@NgModule({
  imports: [
    CommonModule,
    AccountingRoutingModule,
    SharedModule,
    FormsModule,
    SelectModule,
    NgxDaterangepickerMd
  ],
  declarations: [
      StatementOfAccountComponent, 
      AdvancePaymentComponent, 
      SettlementPaymentComponent, 
      AccountReceivablePayableComponent
    ]
})
export class AccountingModule { }
