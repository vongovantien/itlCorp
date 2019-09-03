import { NgModule, LOCALE_ID } from '@angular/core';
import { routing } from './accounting-routing.module';
import { AccountReceivablePayableComponent } from './account-receivable-payable/account-receivable-payable.component';
import { registerLocaleData } from '@angular/common';

import localeVi from '@angular/common/locales/vi';
registerLocaleData(localeVi, 'vi');

const PAGES = [
  AccountReceivablePayableComponent,
];

@NgModule({
  imports: [
    routing,
  ],
  declarations: [
    ...PAGES
  ],
  bootstrap: [],
  providers: [
    { provide: LOCALE_ID, useValue: 'vi' },
  ],
})
export class AccountingModule { }
