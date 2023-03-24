import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StatementOfAccountAddnewComponent } from './add-new/add-new-soa.component';
import { StatementOfAccountDetailComponent } from './detail/detail-soa.component';
import { StatementOfAccountEditComponent } from './edit/edit-soa.component';
import { StatementOfAccountComponent } from './statement-of-account.component';

import { CollapseModule } from 'ngx-bootstrap/collapse';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TabsModule } from 'ngx-bootstrap/tabs';

import { NgSelectModule } from '@ng-select/ng-select';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { NgProgressModule } from '@ngx-progressbar/core';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessAccountingModule } from '../../share-business/share-bussines-accounting.module';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { ShareModulesModule } from '../../share-modules/share-modules.module';
import { ShareAccountingModule } from '../share-accouting.module';
import { StatementOfAccountFormCreateComponent } from './components/form-create-soa/form-create-soa.component';
import { ConfirmBillingFormSearchComponent } from './components/form-search-confirm-billing/form-search-confirm-billing.component';
import { ConfirmBillingListChargeComponent } from './components/list-charge-confirm-billing/list-charge-confirm-billing.component';
import { StatementOfAccountAddChargeComponent } from './components/poup/add-charge/add-charge.popup';
import { ConfirmBillingDatePopupComponent } from './components/poup/confirm-billing-date/confirm-billing-date.popup';
import { StatementOfAccountPaymentMethodComponent } from './components/poup/payment-method/soa-payment-method.popup';
import { StatementOfAccountSearchComponent } from './components/search-box-soa/search-box-soa.component';
import { StatementOfAccountSummaryComponent } from './components/summary/summary-soa.component';
import { ConfirmBillingComponent } from './confirm-billing/confirm-billing.component';
import { ConfirmBillingDetailComponent } from './confirm-billing/detail/detail-confirm-billing.component';
import { soaEffect } from './store/effects';
import { reducers } from './store/reducers';
import { ScrollingModule } from '@angular/cdk/scrolling';

const routing: Routes = [
    {
        path: "",
        data: { name: "" },
        children: [
            {
                path: "", component: StatementOfAccountComponent,
            },
            {
                path: "new", component: StatementOfAccountAddnewComponent, data: {
                    name: "New",
                }
            },
            {
                path: "detail", component: StatementOfAccountDetailComponent,
                data: {
                    name: "Detail",
                }
            },
            {
                path: "edit", component: StatementOfAccountEditComponent,
                data: {
                    name: "Edit",
                }
            },
            {
                path: 'confirm-billing', data: { name: 'Confirm Billing' },
                children: [
                    {
                        path: '', component: ConfirmBillingComponent, data: { name: '', title: 'eFMS Confirm Billing' }
                    },
                    {
                        path: ':vatInvoiceId', component: ConfirmBillingDetailComponent, data: { name: 'Detail', title: 'eFMS Detail Billing' },
                    }
                ]
            },
            {
                path: 'combine-billing', loadChildren: () => import('./combine-billing/combine-billing.module').then(m => m.CombineBillingModule),
                data: { name: 'Combine Billing', title: 'eFMS Combine Billing' }
            },
        ]
    },


];

const COMPONENTS = [
    StatementOfAccountSearchComponent,
    StatementOfAccountAddChargeComponent,
    StatementOfAccountSummaryComponent,
    StatementOfAccountFormCreateComponent,
    StatementOfAccountPaymentMethodComponent,
    ConfirmBillingFormSearchComponent,
    ConfirmBillingListChargeComponent,
    ConfirmBillingDatePopupComponent,
];

@NgModule({
    declarations: [
        StatementOfAccountComponent,
        StatementOfAccountAddnewComponent,
        StatementOfAccountEditComponent,
        StatementOfAccountDetailComponent,
        ConfirmBillingComponent,
        ConfirmBillingDetailComponent,
        ...COMPONENTS
    ],
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        NgSelectModule,
        NgxDaterangepickerMd,
        TabsModule.forRoot(),
        ModalModule.forRoot(),
        CollapseModule.forRoot(),
        PaginationModule.forRoot(),
        PerfectScrollbarModule,
        NgProgressModule,
        ShareAccountingModule,
        BsDropdownModule.forRoot(),
        ShareModulesModule,
        ShareBussinessModule,
        StoreModule.forFeature('soa', reducers),
        EffectsModule.forFeature(soaEffect),
        ShareBussinessAccountingModule,
        ScrollingModule
    ],
    exports: [],
    providers: [

    ],
})
export class StatementOfAccountModule {
    static routing = routing;
}
