import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';

import { TabsModule } from 'ngx-bootstrap/tabs';
import { ModalModule } from 'ngx-bootstrap/modal';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';

import { AccountingManagementFormSearchComponent } from './components/form-search/form-search-accounting-management.component';
import { AccountingManagementDebitCreditInvoiceComponent } from './debit-credit/accounting-debit-credit-invoice.component';
import { AccountingManagementVatInvoiceComponent } from './vat/accounting-vat-invoice.component';
import { AccountingManagementVoucherComponent } from './voucher/accounting-voucher.component';
import { AccountingManagementCreateVATInvoiceComponent } from './vat/create/accounting-create-vat-invoice.component';
import { AccountingManagementFormCreateVATInvoiceComponent } from './components/form-create-vat-invoice/form-create-vat-invoice.component';
import { AccountingManagementInputRefNoPopupComponent } from './components/popup/input-ref-no/input-ref-no.popup';
import { AccountingManagementSelectPartnerPopupComponent } from './components/popup/select-partner/select-partner.popup';
import { AccountingManagementListChargeComponent } from './components/list-charge/list-charge-accouting-management.component';
import { AccountingManagementFormSearchVatVoucherComponent } from './components/form-search/vat-voucher/form-search-vat-voucher.component';

import { reducers } from './store';
import { AccountingManagementDetailVatInvoiceComponent } from './vat/detail/accounting-detail-vat-invoice.component';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { AccountingDetailCdNoteComponent } from './components/popup/detail-cd-note/detail-cd-note.component';
import { AccountingManagementCreateVoucherComponent } from './voucher/create/accounting-create-voucher.component';
import { AccountingManagementFormCreateVoucherComponent } from './components/form-create-voucher/form-create-voucher.component';
import { AccountingManagementDetailVoucherComponent } from './voucher/detail/accounting-detail-voucher.component';
import { AccountingManagementImportVatInvoiceComponent } from './vat/import/accounting-import-vat-invoice.component';
import { NgxCurrencyModule } from 'ngx-currency';
import { ShareAccountingModule } from '../share-accouting.module';

const routing: Routes = [
    {
        path: "", data: { name: "", title: 'eFMS Accounting Management' }, redirectTo: 'cd-invoice',
    },
    {
        path: 'cd-invoice', component: AccountingManagementDebitCreditInvoiceComponent, data: { name: 'Debit/Credit/Invoice' }
    },
    {
        path: 'vat-invoice', data: { name: 'VAT Invoice' },
        children: [
            {
                path: '', component: AccountingManagementVatInvoiceComponent, data: { name: '' }
            },
            {
                path: 'new', component: AccountingManagementCreateVATInvoiceComponent, data: { name: 'New' }
            },
            {
                path: 'import', component: AccountingManagementImportVatInvoiceComponent, data: { name: "Import" }
            },
            {
                path: ':vatInvoiceId', component: AccountingManagementDetailVatInvoiceComponent, data: { name: 'Edit' }
            },

        ]
    },
    {
        path: 'voucher', data: { name: 'Voucher' },
        children: [
            {
                path: '', component: AccountingManagementVoucherComponent, data: { name: '' }
            },
            {
                path: 'new', component: AccountingManagementCreateVoucherComponent, data: { name: 'New' }
            },
            {
                path: ':voucherId', component: AccountingManagementDetailVoucherComponent, data: { name: 'Edit' }
            }
        ]
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
        ModalModule,
        FormsModule,
        // StoreModule.forFeature('accounting-management', reducers), // * Dua ra shaeAccounting
        PaginationModule.forRoot(),
        NgxCurrencyModule.forRoot({
            align: "right",
            allowNegative: false,
            allowZero: true,
            decimal: ".",
            precision: 3,
            prefix: "",
            suffix: "",
            thousands: ",",
            nullable: true
        }),
        ShareAccountingModule
    ],
    declarations: [
        AccountingManagementDebitCreditInvoiceComponent,
        AccountingManagementVatInvoiceComponent,
        AccountingManagementVoucherComponent,
        AccountingManagementFormSearchComponent,
        AccountingManagementFormSearchVatVoucherComponent,
        AccountingManagementCreateVATInvoiceComponent,
        AccountingManagementFormCreateVATInvoiceComponent,
        AccountingManagementInputRefNoPopupComponent,
        AccountingManagementSelectPartnerPopupComponent,
        AccountingManagementListChargeComponent,
        AccountingManagementDetailVatInvoiceComponent,
        AccountingDetailCdNoteComponent,
        AccountingManagementCreateVoucherComponent,
        AccountingManagementFormCreateVoucherComponent,
        AccountingManagementDetailVoucherComponent,
        AccountingManagementImportVatInvoiceComponent
    ],

    exports: [],
    providers: [],
})
export class AccountingManagementModule { }