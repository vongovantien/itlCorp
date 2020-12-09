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
import { NgSelectModule } from '@ng-select/ng-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { reducers } from './store/reducers';
import { effects } from './store/effects';
import { NgxCurrencyModule } from 'ngx-currency';

const routing: Routes = [
    {
        path: '', data: { name: '' }, children: [
            { path: '', component: ARCustomerPaymentComponent },
            { path: 'receipt/new', component: ARCustomerPaymentCreateReciptComponent, data: { name: 'New' }, },
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
        TabsModule.forRoot(),
        NgSelectModule,
        NgxDaterangepickerMd,
        StoreModule.forFeature('customer-payment', reducers),
        EffectsModule.forFeature(effects),
        NgxCurrencyModule.forRoot({
            align: "left",
            allowNegative: false,
            allowZero: true,
            decimal: ".",
            precision: 2,
            prefix: "",
            suffix: "",
            thousands: ",",
            nullable: true
        })
    ],
    exports: [],
    providers: [],
})
export class ARCustomerPaymentModule { }
