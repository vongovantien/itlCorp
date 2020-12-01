import { NgModule } from '@angular/core';
import { ARCustomerPaymentComponent } from './customer-payment.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ARCustomerPaymentFormSearchComponent } from './components/form-search/form-search-customer-payment.component';
import { Routes, RouterModule } from '@angular/router';
import { TabsModule } from 'ngx-bootstrap/tabs';

import { ARCustomerPaymentCreateReciptComponent } from './create-receipt/create-receipt.component';
import { ARCustomerPaymentDetailReceiptComponent } from './detail-receipt/detail-receipt.component';
import { ARCustomerPaymentFormCreateReceiptComponent } from './components/form-create-receipt/form-create-receipt.component';
import { ARCustomerPaymentReceiptSummaryComponent } from './components/receipt-summary/receipt-summary.component';
import { ARCustomerPaymentReceiptPaymentListComponent } from './components/receipt-payment-list/receipt-payment-list.component';

const routing: Routes = [
    {
        path: '', children: [
            { path: '', component: ARCustomerPaymentComponent },
            { path: 'receipt/new', component: ARCustomerPaymentCreateReciptComponent },
            { path: 'receipt/:id', component: ARCustomerPaymentDetailReceiptComponent },
        ]
    },


];

@NgModule({
    declarations: [
        ARCustomerPaymentComponent,
        ARCustomerPaymentFormSearchComponent,
        ARCustomerPaymentCreateReciptComponent,
        ARCustomerPaymentDetailReceiptComponent,
        ARCustomerPaymentFormCreateReceiptComponent,
        ARCustomerPaymentReceiptSummaryComponent,
        ARCustomerPaymentReceiptPaymentListComponent
    ],
    imports: [
        SharedModule,
        RouterModule.forChild(routing),
        TabsModule
    ],
    exports: [],
    providers: [],
})
export class ARCustomerPaymentModule { }
