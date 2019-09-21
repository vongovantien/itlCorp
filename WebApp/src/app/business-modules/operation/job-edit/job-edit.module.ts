import { NgModule } from '@angular/core';
import { BillingCustomDeclarationComponent } from './custom-declaration/billing-custom-declaration.component';
import { OpsModuleBillingJobEditComponent } from './job-edit.component';
import { OpsModuleCreditDebitNoteComponent } from './credit-debit-note/ops-module-credit-debit-note.component';
import { OpsModuleStageManagementComponent } from './stage-management/stage-management.component';
import { OpsModuleCreditDebitNoteAddnewComponent } from './credit-debit-note/ops-module-credit-debit-note-addnew/ops-module-credit-debit-note-addnew.component';
import { OpsModuleCreditDebitNoteRemainingChargeComponent } from './credit-debit-note/ops-module-credit-debit-note-remaining-charge/ops-module-credit-debit-note-remaining-charge.component';
import { OpsModuleCreditDebitNoteDetailComponent } from './credit-debit-note/ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component';
import { OpsModuleCreditDebitNoteEditComponent } from './credit-debit-note/ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
import { OpsModuleStageManagementDetailComponent } from './stage-management/detail/detail-stage-popup.component';
import { OpsModuleStageManagementAddStagePopupComponent } from './stage-management/add/add-stage.popup.component';
import { NotSelectedAlertModalComponent } from './credit-debit-note/ops-module-credit-debit-note-addnew/not-selected-alert-modal/not-selected-alert-modal.component';
import { ChangePartnerConfirmModalComponent } from './credit-debit-note/ops-module-credit-debit-note-addnew/change-partner-confirm-modal/change-partner-confirm-modal.component';
import { ContainerListComponent } from './container-list/container-list.component';
import { CancelCreateJobPopupComponent } from './job-confirm-popup/cancel-create-job-popup/cancel-create-job-popup.component';
import { CanNotDeleteJobPopupComponent } from './job-confirm-popup/can-not-delete-job-popup/can-not-delete-job-popup.component';
import { ConfirmCancelJobPopupComponent } from './job-confirm-popup/confirm-cancel-job-popup/confirm-cancel-job-popup.component';
import { ConfirmDeleteJobPopupComponent } from './job-confirm-popup/confirm-delete-job-popup/confirm-delete-job-popup.component';
import { AddBuyingRatePopupComponent } from './charge-list/add-buying-rate-popup/add-buying-rate-popup.component';
import { EditBuyingRatePopupComponent } from './charge-list/edit-buying-rate-popup/edit-buying-rate-popup.component';
import { AddSellingRatePopupComponent } from './charge-list/add-selling-rate-popup/add-selling-rate-popup.component';
import { EditSellingRatePopupComponent } from './charge-list/edit-selling-rate-popup/edit-selling-rate-popup.component';
import { AddObhRatePopupComponent } from './charge-list/add-obh-rate-popup/add-obh-rate-popup.component';
import { EditObhRatePopupComponent } from './charge-list/edit-obh-rate-popup/edit-obh-rate-popup.component';
import { PlSheetPopupComponent } from './pl-sheet-popup/pl-sheet.popup';
import { Routes, RouterModule } from '@angular/router';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';
import { TabsModule, ModalModule, PaginationModule } from 'ngx-bootstrap';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { JobManagementBuyingRateComponent } from './components/buying-rate/buying-rate.component';
import { JobManagementSellingRateComponent } from './components/selling-rate/selling-rate.component';
import { JobManagementOBHComponent } from './components/obh/obh.component';
import { AddMoreModalComponent } from './custom-declaration/add-more-modal/add-more-modal.component';
import { ContainerImportComponent } from './container-list/container-import/container-import.component';
import { HttpClientModule } from '@angular/common/http';

const routing: Routes = [
    {
        path: ":id",
        component: OpsModuleBillingJobEditComponent,
        data: {
            name: "Job Edit",
            level: 3
        }
    },

];


const LIB = [
    NgxDaterangepickerMd,
    SelectModule,
    TabsModule.forRoot(),
    ModalModule.forRoot(),
];

const COMPONENTS = [
    JobManagementBuyingRateComponent,
    JobManagementSellingRateComponent,
    JobManagementOBHComponent
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        PaginationModule.forRoot(),
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        HttpClientModule,
        ...LIB
    ],
    exports: [],
    declarations: [
        OpsModuleBillingJobEditComponent,

        BillingCustomDeclarationComponent,
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

        AddBuyingRatePopupComponent,
        EditBuyingRatePopupComponent,
        AddSellingRatePopupComponent,
        EditSellingRatePopupComponent,
        AddObhRatePopupComponent,
        EditObhRatePopupComponent,

        PlSheetPopupComponent,
        AddMoreModalComponent,
        ContainerImportComponent,
        ...COMPONENTS,

    ],
    providers: [
        
    ],
    bootstrap: [
        OpsModuleBillingJobEditComponent
    ],
})
export class JobEditModule { }
