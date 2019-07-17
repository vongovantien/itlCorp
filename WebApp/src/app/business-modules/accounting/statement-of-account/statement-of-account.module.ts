import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatementOfAccountComponent } from './statement-of-account.component';
import { AccountReceivePayableComponent } from './account-receive-payable/account-receive-payable.component';
import { StatementOfAccountDetailComponent } from './detail/detail-soa.component';
import { StatementOfAccountEditComponent } from './edit/edit-soa.component';
import { StatementOfAccountAddnewComponent } from './add-new/add-new-soa.component';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { TabsModule, ModalModule, CollapseModule, PaginationModule } from 'ngx-bootstrap';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { StatementOfAccountSearchComponent } from './components/search-box-soa/search-box-soa.component';
import { StatementOfAccountAddChargeComponent } from './components/poup/add-charge/add-charge.popup';
import { SharedModule } from 'src/app/shared/shared.module';
import { StatementOfAccountFormCreateComponent } from './components/form-create-soa/form-create-soa.component';

const routing: Routes = [
    { path: "", component: StatementOfAccountComponent },
    { path: "new", component: StatementOfAccountAddnewComponent },
    { path: "detail", component: StatementOfAccountDetailComponent },
    { path: "edit", component: StatementOfAccountEditComponent, },
    
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
        CommonModule,
        RouterModule.forChild(routing),
        SharedModule,
        FormsModule,
        SelectModule,
        NgxDaterangepickerMd,
        TabsModule.forRoot(),
        ModalModule.forRoot(),
        CollapseModule.forRoot(),
        PaginationModule.forRoot()

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
