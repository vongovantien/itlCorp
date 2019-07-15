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
import { TabsModule, ModalModule,CollapseModule, PaginationModule  } from 'ngx-bootstrap';

import { StatementOfAccountAddnewComponent } from './statement-of-account/add-new/add-new-soa.component';
import { StatementOfAccountDetailComponent } from './statement-of-account/detail/detail-soa.component';
import { StatementOfAccountEditComponent } from './statement-of-account/edit/edit-soa.component';
import { StatementOfAccountSearchComponent } from './statement-of-account/components/poup/search-box-soa/search-box-soa.component';
import { AccountReceivePayableComponent } from './statement-of-account/account-receive-payable/account-receive-payable.component';
import { StatementOfAccountAddChargeComponent } from './statement-of-account/components/poup/add-charge/add-charge.popup';

const COMPONENTS = [
  StatementOfAccountSearchComponent,
  StatementOfAccountAddChargeComponent
];

@NgModule({
  imports: [
    CommonModule,
    AccountingRoutingModule,
    SharedModule,
    FormsModule,
    SelectModule,
    NgxDaterangepickerMd,
    TabsModule.forRoot(),
    ModalModule.forRoot(),
    CollapseModule.forRoot(),
    PaginationModule.forRoot() 
  ],
  declarations: [
      StatementOfAccountComponent, 
      AdvancePaymentComponent, 
      SettlementPaymentComponent, 
      AccountReceivablePayableComponent, 
      StatementOfAccountAddnewComponent, 
      StatementOfAccountEditComponent, 
      StatementOfAccountDetailComponent,
      AccountReceivePayableComponent,
      ...COMPONENTS
    ]
})
export class AccountingModule { }
