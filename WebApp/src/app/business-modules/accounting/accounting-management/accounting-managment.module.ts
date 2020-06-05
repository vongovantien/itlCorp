import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

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
import { StoreModule } from '@ngrx/store';
import { reducer } from './store';
import { AccountingManagementFormSearchVatVoucherComponent } from './components/form-search/vat-voucher/form-search-vat-voucher.component';


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
                path: 'new', component: AccountingManagementCreateVATInvoiceComponent, data: { name: '' }
            }
        ]
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
        ModalModule,
        FormsModule,
        StoreModule.forFeature('accounting-management', reducer),
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
        AccountingManagementSelectPartnerPopupComponent
    ],

    exports: [],
    providers: [],
})
export class AccountingManagementModule { }