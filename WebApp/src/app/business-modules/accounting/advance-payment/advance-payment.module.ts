import { NgModule } from '@angular/core';
import { AdvancePaymentComponent } from './advance-payment.component';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { AdvancePaymentAddNewComponent } from './add/add-new-advance-payment.component';
import { AdvancePaymentFormCreateComponent } from './components/form-create-advance-payment/form-create-advance-payment.component';
import { AdvancePaymentListRequestComponent } from './components/list-advance-payment-request/list-advance-payment-request.component';
import { AdvancePaymentAddRequestPopupComponent } from './components/popup/add-advance-payment-request/add-advance-payment-request.popup';
import { AdvancePaymentFormsearchComponent } from './components/form-search-advance-payment/form-search-advance-payment.component';
import { AdvancePaymentDetailComponent } from './detail/detail-advance-payment.component';
import { ApproveAdvancePaymentComponent } from '../approve-payment/advance/approve-advance.component';
import { NgProgressModule } from '@ngx-progressbar/core';
import { NgxCurrencyModule } from "ngx-currency";
import { ShareApprovePaymentModule } from '../approve-payment/components/share-approve-payment.module';
import { UpdatePaymentVoucherPopupComponent } from './components/popup/update-payment-voucher/update-payment-voucher.popup';
import { ImportVoucherAdvancePaymentComponent } from './import/import-voucher-advance-payment.component';
import { SelectModule } from 'ng2-select';
import { AdvancePaymentsPopupComponent } from './components/popup/advance-payments/advance-payments.popup';
const routing: Routes = [
    {
        path: "",
        data: { name: "", title: 'eFMS Advance Payment' },
        children: [
            {
                path: '', component: AdvancePaymentComponent
            },
            {
                path: "new", component: AdvancePaymentAddNewComponent,
                data: { name: "New", }
            },
            {

                path: 'import-voucher', component: ImportVoucherAdvancePaymentComponent, data: { name: "Import Voucher" }
            },
            {
                path: ":id", component: AdvancePaymentDetailComponent,
                data: { name: "Detail" }
            },
            {
                path: ":id/approve", component: ApproveAdvancePaymentComponent,
                data: { name: "Approve" }
            }
        ]
    },


];

const COMPONENTS = [
    AdvancePaymentFormCreateComponent,
    AdvancePaymentListRequestComponent,
    AdvancePaymentAddRequestPopupComponent,
    UpdatePaymentVoucherPopupComponent,
    ImportVoucherAdvancePaymentComponent,
    AdvancePaymentFormsearchComponent,
    AdvancePaymentsPopupComponent
];

const customCurrencyMaskConfig = {
    align: "right",
    allowNegative: false,
    allowZero: true,
    decimal: ".",
    precision: 0,
    prefix: "",
    suffix: "",
    thousands: ",",
    nullable: true
};

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
        PaginationModule.forRoot(),
        PerfectScrollbarModule,
        RouterModule.forChild(routing),
        ReactiveFormsModule,
        NgProgressModule,
        NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
        ShareApprovePaymentModule,
        SelectModule,
    ],
    declarations: [
        AdvancePaymentComponent,
        AdvancePaymentAddNewComponent,
        AdvancePaymentDetailComponent,
        ApproveAdvancePaymentComponent,
        ...COMPONENTS,
    ],
    providers: [
    ],
})
export class AdvancePaymentModule { }
