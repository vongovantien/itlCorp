import { NgModule } from '@angular/core';
import { ARCustomerPaymentComponent } from './customer-payment.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ARCustomerPaymentFormSearchComponent } from './components/form-search/form-search-customer-payment.component';
import { Routes, RouterModule } from '@angular/router';
import { TabsModule } from 'ngx-bootstrap/tabs';

import { ARCustomerPaymentCreateReciptComponent } from './create-receipt/create-receipt.component';
import { ARCustomerPaymentDetailReceiptComponent } from './detail-receipt/detail-receipt.component';
import { ARCustomerPaymentFormCreateReceiptComponent } from './components/form-create-receipt/form-create-receipt.component';
import { ARCustomerPaymentReceiptPaymentListComponent } from './components/receipt-payment-list/receipt-payment-list.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { reducers } from './store/reducers';
import { effects } from './store/effects';
import { ModalModule } from 'ngx-bootstrap/modal';
import { ARCustomerPaymentReceiptCreditListComponent } from './components/receipt-credit-list/receipt-credit-list.component';
import { ARCustomerPaymentReceiptDebitListComponent } from './components/receipt-debit-list/receipt-debit-list.component';
import { ARCustomerPaymentFormSearchCustomerAgentCDInvoiceComponent } from './components/form-search-agent-customer/form-search-customer-agent-cd-invoice.component';
import { ARCustomerPaymentCustomerAgentDebitPopupComponent } from './components/customer-agent-debit/customer-agent-debit.popup';
import { ARCustomerPaymentFormQuickUpdateReceiptPopupComponent } from './components/popup/form-quick-update-receipt-popup/form-quick-update-receipt.popup';
import { ARCustomerPaymentCreateReciptCombineComponent } from './create-receipt-combine/create-receipt-combine.component';
import { ARCustomerPaymentFormCreateReceiptCombineComponent } from './components/form-combine-receipt/form-combine-receipt.component';
import { ARCustomerPaymentReceiptGeneralCombineComponent } from './components/general-combine/receipt-general-combine.component';
import { ARCustomerPaymentReceiptCDCombineComponent } from './components/cd-combine/receipt-cd-combine.component';

const routing: Routes = [
    {
        path: '', data: { name: '' }, children: [
            { path: '', component: ARCustomerPaymentComponent },
            { path: 'receipt/:type/new', component: ARCustomerPaymentCreateReciptComponent, data: { name: 'New' } },
            { path: 'receipt/:id', component: ARCustomerPaymentDetailReceiptComponent },
            { path: 'receipt/combine/new-combine', component: ARCustomerPaymentCreateReciptCombineComponent, data: { type: 'new' } },
        ]
    },

];


@NgModule({
    declarations: [
        ARCustomerPaymentComponent,
        ARCustomerPaymentCreateReciptComponent,
        ARCustomerPaymentDetailReceiptComponent,
        ARCustomerPaymentFormSearchComponent,
        ARCustomerPaymentFormCreateReceiptComponent,
        ARCustomerPaymentReceiptPaymentListComponent,
        ARCustomerPaymentReceiptCreditListComponent,
        ARCustomerPaymentReceiptDebitListComponent,
        ARCustomerPaymentFormSearchCustomerAgentCDInvoiceComponent,
        ARCustomerPaymentCustomerAgentDebitPopupComponent,
        ARCustomerPaymentFormQuickUpdateReceiptPopupComponent,
        ARCustomerPaymentCreateReciptCombineComponent,
        ARCustomerPaymentFormCreateReceiptCombineComponent,
        ARCustomerPaymentReceiptGeneralCombineComponent,
        ARCustomerPaymentReceiptCDCombineComponent
    ],
    imports: [
        SharedModule,
        RouterModule.forChild(routing),
        TabsModule.forRoot(),
        NgSelectModule,
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
        StoreModule.forFeature('customer-payment', reducers),
        EffectsModule.forFeature(effects),

    ],
    exports: [],
    providers: [],
})
export class ARCustomerPaymentModule { }
