import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { NgProgressModule } from '@ngx-progressbar/core';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { NgxCurrencyModule } from "ngx-currency";
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessAccountingModule } from '../../share-business/share-bussines-accounting.module';
import { ApproveAdvancePaymentComponent } from '../approve-payment/advance/approve-advance.component';
import { ShareApprovePaymentModule } from '../approve-payment/components/share-approve-payment.module';
import { ShareAccountingModule } from '../share-accouting.module';
import { AdvancePaymentAddNewComponent } from './add/add-new-advance-payment.component';
import { AdvancePaymentComponent } from './advance-payment.component';
import { AdvancePaymentFormCreateComponent } from './components/form-create-advance-payment/form-create-advance-payment.component';
import { AdvancePaymentFormsearchComponent } from './components/form-search-advance-payment/form-search-advance-payment.component';
import { ListAdvancePaymentCarrierComponent } from './components/list-advance-payment-carrier/list-advance-payment-carrier.component';
import { AdvancePaymentListRequestComponent } from './components/list-advance-payment-request/list-advance-payment-request.component';
import { AdvancePaymentAddRequestPopupComponent } from './components/popup/add-advance-payment-request/add-advance-payment-request.popup';
import { AdvancePaymentsPopupComponent } from './components/popup/advance-payments/advance-payments.popup';
import { AdvancePaymentShipmentExistedPopupComponent } from './components/popup/shipment-existed/shipment-existed.popup';
import { UpdatePaymentVoucherPopupComponent } from './components/popup/update-payment-voucher/update-payment-voucher.popup';
import { AdvancePaymentDetailComponent } from './detail/detail-advance-payment.component';
import { ImportVoucherAdvancePaymentComponent } from './import/import-voucher-advance-payment.component';
import { reducers } from './store';
import { advanceEffects } from './store/effects/index';

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
    AdvancePaymentsPopupComponent,
    AdvancePaymentShipmentExistedPopupComponent,
    ListAdvancePaymentCarrierComponent
];

const customCurrencyMaskConfig = {
    align: "right",
    allowNegative: true,
    allowZero: true,
    decimal: ".",
    precision: 2,
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
        NgSelectModule,
        EffectsModule.forFeature(advanceEffects),
        TabsModule.forRoot(),
        ShareAccountingModule,
        ShareBussinessAccountingModule
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
