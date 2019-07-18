import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { OperationRoutingModule } from "./operation-routing.module";
import { JobMangementComponent } from "./job-mangement/job-mangement.component";
import { AssigmentComponent } from "./assigment/assigment.component";
import { TruckingAssignmentComponent } from "./trucking-assignment/trucking-assignment.component";
import { SelectModule } from "ng2-select";
import { NgxDaterangepickerMd } from "ngx-daterangepicker-material";
import { SharedModule } from "../../shared/shared.module";
import { OpsModuleBillingJobCreateComponent } from "./ops-module-billing-job-create/ops-module-billing-job-create.component";
import { OpsModuleBillingComponent } from "./ops-module-billing/ops-module-billing.component";
import { DragDropModule } from "@angular/cdk/drag-drop";
import { CustomClearanceComponent } from "./custom-clearance/custom-clearance.component";
import { CustomClearanceAddnewComponent } from "./custom-clearance-addnew/custom-clearance-addnew.component";
import { CustomClearanceEditComponent } from "./custom-clearance-edit/custom-clearance-edit.component";
import { CustomClearanceImportComponent } from "./custom-clearance-import/custom-clearance-import.component";
import { OpsModuleBillingJobEditComponent } from "./job-edit/job-edit.component";

import { OpsModuleCreditDebitNoteAddnewComponent } from "./job-edit/credit-debit-note/ops-module-credit-debit-note-addnew/ops-module-credit-debit-note-addnew.component";
import { OpsModuleCreditDebitNoteRemainingChargeComponent } from "./job-edit/credit-debit-note/ops-module-credit-debit-note-remaining-charge/ops-module-credit-debit-note-remaining-charge.component";
import { OpsModuleCreditDebitNoteDetailComponent } from "./job-edit/credit-debit-note/ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component";
import { OpsModuleCreditDebitNoteEditComponent } from "./job-edit/credit-debit-note/ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component";
import { BillingCustomDeclarationComponent } from "./job-edit/custom-declaration/billing-custom-declaration.component";
import { OpsModuleCreditDebitNoteComponent } from "./job-edit/credit-debit-note/ops-module-credit-debit-note.component";
import { OpsModuleStageManagementComponent } from "./job-edit/stage-management/stage-management.component";
import { OpsModuleStageManagementDetailComponent } from "./job-edit/stage-management/detail/detail-stage-popup.component";
import { ModalModule, TabsModule } from "ngx-bootstrap";
import { OpsModuleStageManagementAddStagePopupComponent } from "./job-edit/stage-management/add/add-stage.popup.component";
import { NotSelectedAlertModalComponent } from './job-edit/credit-debit-note/ops-module-credit-debit-note-addnew/not-selected-alert-modal/not-selected-alert-modal.component';


const LIB = [
    NgxDaterangepickerMd,
    DragDropModule,
    SelectModule,
    TabsModule.forRoot(),
    ModalModule.forRoot(),

]
@NgModule({
    imports: [
        CommonModule,
        OperationRoutingModule,
        FormsModule,
        ReactiveFormsModule,
        SharedModule,
        ...LIB

    ],
    declarations: [
        JobMangementComponent,
        AssigmentComponent,
        TruckingAssignmentComponent,
        OpsModuleBillingJobCreateComponent,
        OpsModuleBillingComponent,
        OpsModuleBillingJobEditComponent,
        BillingCustomDeclarationComponent,
        OpsModuleCreditDebitNoteComponent,
        OpsModuleStageManagementComponent,
        OpsModuleCreditDebitNoteAddnewComponent,
        OpsModuleCreditDebitNoteRemainingChargeComponent,
        OpsModuleCreditDebitNoteDetailComponent,
        OpsModuleCreditDebitNoteEditComponent,
        OpsModuleStageManagementDetailComponent,
        CustomClearanceComponent,
        CustomClearanceAddnewComponent,
        CustomClearanceEditComponent,
        CustomClearanceImportComponent,

        OpsModuleStageManagementAddStagePopupComponent,

        NotSelectedAlertModalComponent
    ]
})
export class OperationModule {}
