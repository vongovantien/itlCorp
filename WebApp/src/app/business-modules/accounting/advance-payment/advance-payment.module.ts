import { NgModule, LOCALE_ID } from '@angular/core';
import { AdvancePaymentComponent } from './advance-payment.component';
import { registerLocaleData, CommonModule } from '@angular/common';
import localeVi from '@angular/common/locales/vi';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { ModalModule, PaginationModule, } from 'ngx-bootstrap';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { AdvancePaymentAddNewComponent } from './add/add-new-advance-payment.component';
import { AdvancePaymentFormCreateComponent } from './components/form-create-advance-payment/form-create-advance-payment.component';
import { AdvancePaymentListRequestComponent } from './components/list-advance-payment-request/list-advance-payment-request.component';
import { AdvancePaymentAddRequestPopupComponent } from './components/popup/add-advance-payment-request/add-advance-payment-request.popup';
import { AdvancePaymentFormsearchComponent } from './components/form-search-advance-payment/form-search-advance-payment.component';
import { AdvancePaymentDetailComponent } from './detail/detail-advance-payment.component';

registerLocaleData(localeVi, 'vi');
const routing: Routes = [
    {
        path: "", component: AdvancePaymentComponent, pathMatch: 'full',
        data: { name: "Advance Payment", path: "advance-payment", level: 2 }
    },
    {
        path: "new", component: AdvancePaymentAddNewComponent,
        data: { name: "New", path: "New", level: 3 }
    },
    {
        path: ":id", component: AdvancePaymentDetailComponent,
        data: { name: "Detail", path: "Detail", level: 3 }
    }

];

const COMPONENTS = [
    AdvancePaymentFormCreateComponent,
    AdvancePaymentListRequestComponent,
    AdvancePaymentAddRequestPopupComponent,
    AdvancePaymentFormsearchComponent
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        SelectModule,
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
        PaginationModule.forRoot(),
        PerfectScrollbarModule,
        RouterModule.forChild(routing),
        ReactiveFormsModule
    ],
    declarations: [
        AdvancePaymentComponent,
        AdvancePaymentAddNewComponent,
        AdvancePaymentDetailComponent,
        ...COMPONENTS,
    ],
    providers: [
        { provide: LOCALE_ID, useValue: 'vi' },
    ],
    bootstrap: [
        AdvancePaymentComponent
    ],
})
export class AdvancePaymentModule { }
