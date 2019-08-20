import { NgModule, LOCALE_ID } from '@angular/core';
import { CommonModule, registerLocaleData } from '@angular/common';
import { StatementOfAccountComponent } from './statement-of-account.component';
import { AccountReceivePayableComponent } from './account-receive-payable/account-receive-payable.component';
import { StatementOfAccountDetailComponent } from './detail/detail-soa.component';
import { StatementOfAccountEditComponent } from './edit/edit-soa.component';
import { StatementOfAccountAddnewComponent } from './add-new/add-new-soa.component';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { TabsModule, ModalModule, CollapseModule, PaginationModule, AccordionModule } from 'ngx-bootstrap';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { StatementOfAccountSearchComponent } from './components/search-box-soa/search-box-soa.component';
import { StatementOfAccountAddChargeComponent } from './components/poup/add-charge/add-charge.popup';
import { SharedModule } from 'src/app/shared/shared.module';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import localeVi from '@angular/common/locales/vi';
import { StatementOfAccountSummaryComponent } from './components/summary/summary-soa.component';

registerLocaleData(localeVi, 'vi');
const routing: Routes = [
    {
        path: "", component: StatementOfAccountComponent,
        data: {
            name: "Statement Of Account",
            path: "statement-of-account",
            level: 2
        }
    },
    {
        path: "new", component: StatementOfAccountAddnewComponent, data: {
            name: "New",
            path: "new",
            level: 3
        }
    },
    {
        path: "detail", component: StatementOfAccountDetailComponent,
        data: {
            name: "Detail",
            path: "detail",
            level: 3
        }
    },
    {
        path: "edit", component: StatementOfAccountEditComponent,
        data: {
            name: "Edit",
            path: "detail",
            level: 3
        }
    },

];

const COMPONENTS = [
    StatementOfAccountSearchComponent,
    StatementOfAccountAddChargeComponent,
    StatementOfAccountSummaryComponent
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
        FormsModule,
        SelectModule,
        NgxDaterangepickerMd,
        TabsModule.forRoot(),
        ModalModule.forRoot(),
        CollapseModule.forRoot(),
        PaginationModule.forRoot(),
        PerfectScrollbarModule,
        AccordionModule.forRoot(),
    ],
    exports: [],
    providers: [
        { provide: LOCALE_ID, useValue: 'vi' },

    ],
    bootstrap: [
        StatementOfAccountComponent
    ],

})
export class StatementOfAccountModule {
    static routing = routing;
}
