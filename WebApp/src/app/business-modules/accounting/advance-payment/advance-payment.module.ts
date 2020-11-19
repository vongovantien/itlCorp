import { NgModule } from '@angular/core';
import { AdvancePaymentComponent } from './advance-payment.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
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
import { StoreModule } from '@ngrx/store';
import { reducers } from './store';
import { AdvancePaymentsPopupComponent } from './components/popup/advance-payments/advance-payments.popup';
import { NgSelectModule } from '@ng-select/ng-select';
const routing: Routes = [
    {
        path: "",
        data: { name: "", },
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
                data: { name: "Approve", title: 'eFMS Approve Advance' }
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
        SharedModule,
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
        PaginationModule.forRoot(),
        PerfectScrollbarModule,
        RouterModule.forChild(routing),
        NgProgressModule,
        NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
        ShareApprovePaymentModule,
        StoreModule.forFeature('advance-payment', reducers),
        NgSelectModule

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
