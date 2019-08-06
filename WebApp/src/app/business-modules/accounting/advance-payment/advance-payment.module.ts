import { NgModule, LOCALE_ID } from '@angular/core';
import { AdvancePaymentComponent } from './advance-payment.component';
import { registerLocaleData, CommonModule } from '@angular/common';
import localeVi from '@angular/common/locales/vi';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { ModalModule, PaginationModule, } from 'ngx-bootstrap';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { AdvancePaymentAddNewComponent } from './add/add-new-advance-payment.component';
import { AdvancePaymentFormCreateComponent } from './components/form-create-advance-payment/form-create-advance-payment.component';

registerLocaleData(localeVi, 'vi');
const routing: Routes = [
    {
        path: "", component: AdvancePaymentComponent, pathMatch: 'full',
        data: { name: "Advance Payment", path: "advance-payment", level: 2 }
    },
    {
        path: "new", component: AdvancePaymentAddNewComponent,
        data: { name: "New", path: "New", level: 3 }
    }

];

const COMPONENTS = [
    AdvancePaymentFormCreateComponent
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
        RouterModule.forChild(routing)
    ],
    declarations: [
        AdvancePaymentComponent,
        AdvancePaymentAddNewComponent,
        ...COMPONENTS
    ],
    providers: [
        { provide: LOCALE_ID, useValue: 'vi' },
    ],
    bootstrap: [
        AdvancePaymentComponent
    ],
})
export class AdvancePaymentModule { }
