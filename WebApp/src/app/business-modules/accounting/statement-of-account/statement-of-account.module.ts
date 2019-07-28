import { NgModule } from 'node_modules/@angular/core';
import { CommonModule } from 'node_modules/@angular/common';
import { StatementOfAccountComponent } from './statement-of-account.component';
import { AccountReceivePayableComponent } from './account-receive-payable/account-receive-payable.component';
import { StatementOfAccountDetailComponent } from './detail/detail-soa.component';
import { StatementOfAccountEditComponent } from './edit/edit-soa.component';
import { StatementOfAccountAddnewComponent } from './add-new/add-new-soa.component';
import { Routes, RouterModule } from 'node_modules/@angular/router';
import { FormsModule } from 'node_modules/@angular/forms';
import { SelectModule } from 'node_modules/ng2-select';
import { TabsModule, ModalModule, CollapseModule, PaginationModule, AccordionModule } from 'node_modules/ngx-bootstrap';
import { NgxDaterangepickerMd } from 'node_modules/ngx-daterangepicker-material';
import { StatementOfAccountSearchComponent } from './components/search-box-soa/search-box-soa.component';
import { StatementOfAccountAddChargeComponent } from './components/poup/add-charge/add-charge.popup';
import { SharedModule } from 'src/app/shared/shared.module';
import { StatementOfAccountFormCreateComponent } from './components/form-create-soa/form-create-soa.component';
import { PerfectScrollbarModule } from 'node_modules/ngx-perfect-scrollbar';

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
    StatementOfAccountFormCreateComponent
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
        // CommonModule,
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
    providers: [],
    bootstrap: [
        StatementOfAccountComponent
    ]
})
export class StatementOfAccountModule {
    static routing = routing;
}
