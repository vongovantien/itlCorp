import { NgModule, LOCALE_ID } from '@angular/core';
import { JobManagementComponent } from './job-management.component';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule, registerLocaleData } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SelectModule } from 'ng2-select';
import { TabsModule, ModalModule, PaginationModule } from 'ngx-bootstrap';
import { NgProgressModule } from '@ngx-progressbar/core';
import { JobManagementCreateJobComponent } from './create/create-job.component';
import { JobManagementDetailJobComponent } from './detail/detail-job.component';
import { OpsModuleBillingJobEditComponent } from '../job-edit/job-edit.component';
import { BillingCustomDeclarationComponent } from '../job-edit/custom-declaration/billing-custom-declaration.component';
import { OpsModuleCreditDebitNoteComponent } from '../job-edit/credit-debit-note/ops-module-credit-debit-note.component';
import { OpsModuleStageManagementComponent } from '../job-edit/stage-management/stage-management.component';
import { OpsModuleCreditDebitNoteAddnewComponent } from '../job-edit/credit-debit-note/ops-module-credit-debit-note-addnew/ops-module-credit-debit-note-addnew.component';
import { OpsModuleCreditDebitNoteRemainingChargeComponent } from '../job-edit/credit-debit-note/ops-module-credit-debit-note-remaining-charge/ops-module-credit-debit-note-remaining-charge.component';
import { OpsModuleCreditDebitNoteDetailComponent } from '../job-edit/credit-debit-note/ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component';
import { OpsModuleCreditDebitNoteEditComponent } from '../job-edit/credit-debit-note/ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
import { OpsModuleStageManagementDetailComponent } from '../job-edit/stage-management/detail/detail-stage-popup.component';

import { OpsModuleStageManagementAddStagePopupComponent } from '../job-edit/stage-management/add/add-stage.popup.component';
import { NotSelectedAlertModalComponent } from '../job-edit/credit-debit-note/ops-module-credit-debit-note-addnew/not-selected-alert-modal/not-selected-alert-modal.component';
import { ChangePartnerConfirmModalComponent } from '../job-edit/credit-debit-note/ops-module-credit-debit-note-addnew/change-partner-confirm-modal/change-partner-confirm-modal.component';
import { ContainerListComponent } from '../job-edit/container-list/container-list.component';
import { CancelCreateJobPopupComponent } from '../job-edit/job-confirm-popup/cancel-create-job-popup/cancel-create-job-popup.component';
import { CanNotDeleteJobPopupComponent } from '../job-edit/job-confirm-popup/can-not-delete-job-popup/can-not-delete-job-popup.component';
import { ConfirmCancelJobPopupComponent } from '../job-edit/job-confirm-popup/confirm-cancel-job-popup/confirm-cancel-job-popup.component';
import { ConfirmDeleteJobPopupComponent } from '../job-edit/job-confirm-popup/confirm-delete-job-popup/confirm-delete-job-popup.component';
import { ChargeListComponent } from '../job-edit/charge-list/charge-list.component';
import { AddBuyingRatePopupComponent } from '../job-edit/charge-list/add-buying-rate-popup/add-buying-rate-popup.component';
import { EditBuyingRatePopupComponent } from '../job-edit/charge-list/edit-buying-rate-popup/edit-buying-rate-popup.component';
import { AddSellingRatePopupComponent } from '../job-edit/charge-list/add-selling-rate-popup/add-selling-rate-popup.component';
import { EditSellingRatePopupComponent } from '../job-edit/charge-list/edit-selling-rate-popup/edit-selling-rate-popup.component';
import { AddObhRatePopupComponent } from '../job-edit/charge-list/add-obh-rate-popup/add-obh-rate-popup.component';
import { EditObhRatePopupComponent } from '../job-edit/charge-list/edit-obh-rate-popup/edit-obh-rate-popup.component';
import { PlSheetPopupComponent } from '../job-edit/pl-sheet-popup/pl-sheet.popup';
import { JobManagementFormSearchComponent } from './components/form-search-job/form-search-job.component';
import localeVi from '@angular/common/locales/vi';

registerLocaleData(localeVi, 'vi');

const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: JobManagementComponent, data: {
            name: "Job Management",
            level: 2
        }
    },
    {
        path: "job-create",
        component: JobManagementCreateJobComponent,
        data: {
            name: "Job Create",
            level: 3
        }
    },
    {
        path: "job-edit/:id",
        component: OpsModuleBillingJobEditComponent,
        data: {
            name: "Job Edit",
            level: 3
        }
    },
    
];


const LIB = [
    NgxDaterangepickerMd,
    DragDropModule,
    SelectModule,
    TabsModule.forRoot(),
    ModalModule.forRoot(),
    PaginationModule.forRoot(),
    NgProgressModule,
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        ...LIB
    ],
    exports: [],
    declarations: [
        JobManagementComponent,
        JobManagementCreateJobComponent,
        JobManagementDetailJobComponent,

        BillingCustomDeclarationComponent,

        OpsModuleBillingJobEditComponent,
        OpsModuleCreditDebitNoteComponent,
        OpsModuleStageManagementComponent,
        OpsModuleCreditDebitNoteAddnewComponent,
        OpsModuleCreditDebitNoteRemainingChargeComponent,
        OpsModuleCreditDebitNoteDetailComponent,
        OpsModuleCreditDebitNoteEditComponent,
        OpsModuleStageManagementDetailComponent,
        OpsModuleStageManagementAddStagePopupComponent,

        NotSelectedAlertModalComponent,

        ChangePartnerConfirmModalComponent,
        ContainerListComponent,
        CancelCreateJobPopupComponent,
        CanNotDeleteJobPopupComponent,
        ConfirmCancelJobPopupComponent,
        ConfirmDeleteJobPopupComponent,
        ChargeListComponent,

        AddBuyingRatePopupComponent,
        EditBuyingRatePopupComponent,
        AddSellingRatePopupComponent,
        EditSellingRatePopupComponent,
        AddObhRatePopupComponent,
        EditObhRatePopupComponent,
        
        PlSheetPopupComponent,

        JobManagementFormSearchComponent

    ],
    providers: [
        { provide: LOCALE_ID, useValue: 'vi' },
    ],
    bootstrap: [
        JobManagementComponent
    ],
})
export class JobManagementModule { }
