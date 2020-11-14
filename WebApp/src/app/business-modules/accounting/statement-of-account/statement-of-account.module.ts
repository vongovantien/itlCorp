import { NgModule } from '@angular/core';
import { StatementOfAccountComponent } from './statement-of-account.component';
import { AccountReceivePayableComponent } from './account-receive-payable/account-receive-payable.component';
import { StatementOfAccountDetailComponent } from './detail/detail-soa.component';
import { StatementOfAccountEditComponent } from './edit/edit-soa.component';
import { StatementOfAccountAddnewComponent } from './add-new/add-new-soa.component';
import { Routes, RouterModule } from '@angular/router';
import { SelectModule } from 'ng2-select';

import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { CollapseModule } from 'ngx-bootstrap/collapse';
import { TabsModule } from 'ngx-bootstrap/tabs';

import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { StatementOfAccountSearchComponent } from './components/search-box-soa/search-box-soa.component';
import { StatementOfAccountAddChargeComponent } from './components/poup/add-charge/add-charge.popup';
import { SharedModule } from 'src/app/shared/shared.module';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { StatementOfAccountSummaryComponent } from './components/summary/summary-soa.component';
import { NgProgressModule } from '@ngx-progressbar/core';
import { StatementOfAccountFormCreateComponent } from './components/form-create-soa/form-create-soa.component';
import { ShareAccountingModule } from '../share-accouting.module';
import { StatementOfAccountPaymentMethodComponent } from './components/poup/payment-method/soa-payment-method.popup';

const routing: Routes = [
    {
        path: "",
        data: { name: "", title: 'eFMS Statement Of Account' },
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
        ]
    },


];

const COMPONENTS = [
    StatementOfAccountSearchComponent,
    StatementOfAccountAddChargeComponent,
    StatementOfAccountSummaryComponent,
    StatementOfAccountFormCreateComponent,
    StatementOfAccountPaymentMethodComponent,
];

@NgModule({
    declarations: [
        StatementOfAccountComponent,
        StatementOfAccountAddnewComponent,
        StatementOfAccountEditComponent,
        StatementOfAccountDetailComponent,
        AccountReceivePayableComponent,
        ...COMPONENTS
    ],
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        SelectModule,
        NgxDaterangepickerMd,
        TabsModule.forRoot(),
        ModalModule.forRoot(),
        CollapseModule.forRoot(),
        PaginationModule.forRoot(),
        PerfectScrollbarModule,
        NgProgressModule,
        ShareAccountingModule,
        BsDropdownModule.forRoot(),
    ],
    exports: [],
    providers: [

    ],
})
export class StatementOfAccountModule {
    static routing = routing;
}
