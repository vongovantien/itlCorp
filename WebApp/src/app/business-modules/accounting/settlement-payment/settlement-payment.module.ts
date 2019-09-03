import { NgModule } from '@angular/core';
import { SettlementPaymentComponent } from './settlement-payment.component';
import { Routes, RouterModule } from '@angular/router';
import { SettlementFormSearchComponent } from './components/form-search-settlement/form-search-settlement.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PaginationModule } from 'ngx-bootstrap';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { NgProgressModule } from '@ngx-progressbar/core';
import { SettlementPaymentAddNewComponent } from './add/add-settlement-payment.component';

const routing: Routes = [
    {
        path: '', component: SettlementPaymentComponent, data: {
            name: "Settlement Payment",
            level: 2
        }
    },
    {
        path: "new", component: SettlementPaymentAddNewComponent,
        data: { name: "New", path: "New", level: 3 }
    },
];

const COMPONENT = [
    SettlementFormSearchComponent
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        NgxDaterangepickerMd,
        PaginationModule.forRoot(),
        PerfectScrollbarModule,
        ReactiveFormsModule,
        NgProgressModule,
        RouterModule.forChild(routing),
    ],
    exports: [],
    declarations: [
        SettlementPaymentComponent,
        SettlementPaymentAddNewComponent,
        ...COMPONENT
    ],
    providers: [],
})
export class SettlementPaymentModule { }
