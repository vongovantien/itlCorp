import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';

import { TabsModule } from 'ngx-bootstrap';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from 'src/app/shared/shared.module';

import { AccountingManagementFormSearchComponent } from './components/form-search/form-search-accounting-management.component';
import { AccountingManagementDebitCreditInvoiceComponent } from './debit-credit/accounting-debit-credit-invoice.component';
import { AccountingManagementVatInvoiceComponent } from './vat/accounting-vat-invoice.component';
import { AccountingManagementVoucherComponent } from './voucher/accounting-voucher.component';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { AccountingManagementFormSearchVatVoucherComponent } from './components/form-search/vat-voucher/form-search-vat-voucher.component';
import { SelectModule } from 'ng2-select';
import { ReactiveFormsModule } from '@angular/forms';

const routing: Routes = [
    {
        path: "", data: { name: "", title: 'eFMS Accounting Management' }, redirectTo: 'cd-invoice',
    },
    {
        path: 'cd-invoice', component: AccountingManagementDebitCreditInvoiceComponent, data: { name: 'Debit/Credit/Invoice' }
    },
    {
        path: 'vat-invoice', component: AccountingManagementVatInvoiceComponent, data: { name: 'VAT Invoice' }
    },
    {
        path: 'voucher', component: AccountingManagementVoucherComponent, data: { name: 'Voucher' }
    },

];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        ShareBussinessModule,
        SharedModule,
        TabsModule.forRoot(),
        NgxDaterangepickerMd,
        SelectModule,
        ReactiveFormsModule,
    ],
    declarations: [
        AccountingManagementDebitCreditInvoiceComponent,
        AccountingManagementVatInvoiceComponent,
        AccountingManagementVoucherComponent,
        AccountingManagementFormSearchComponent,
        AccountingManagementFormSearchVatVoucherComponent
    ],

    exports: [],
    providers: [],
})
export class AccountingManagementModule { }