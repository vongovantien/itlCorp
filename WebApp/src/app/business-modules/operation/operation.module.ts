import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule} from '@angular/forms';
import { OperationRoutingModule } from './operation-routing.module';
import { JobMangementComponent } from './job-mangement/job-mangement.component';
import { AssigmentComponent } from './assigment/assigment.component';
import { TruckingAssignmentComponent } from './trucking-assignment/trucking-assignment.component';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from '../../shared/shared.module';
import { OpsModuleBillingJobCreateComponent } from './ops-module-billing-job-create/ops-module-billing-job-create.component';
import { OpsModuleBillingComponent } from './ops-module-billing/ops-module-billing.component';
import { OpsModuleBillingJobEditComponent } from './ops-module-billing-job-edit/ops-module-billing-job-edit.component';
import { BillingCustomDeclarationComponent } from './ops-module-billing-job-edit/billing-custom-declaration/billing-custom-declaration.component';
import { OpsModuleCreditDebitNoteComponent } from './ops-module-billing-job-edit/ops-module-credit-debit-note/ops-module-credit-debit-note.component';
import { OpsModuleStageManagementComponent } from './ops-module-billing-job-edit/ops-module-stage-management/ops-module-stage-management.component';
import { OpsModuleCreditDebitNoteAddnewComponent } from './ops-module-billing-job-edit/ops-module-credit-debit-note/ops-module-credit-debit-note-addnew/ops-module-credit-debit-note-addnew.component';
import { OpsModuleCreditDebitNoteRemainingChargeComponent } from './ops-module-billing-job-edit/ops-module-credit-debit-note/ops-module-credit-debit-note-remaining-charge/ops-module-credit-debit-note-remaining-charge.component';
import { OpsModuleCreditDebitNoteDetailComponent } from './ops-module-billing-job-edit/ops-module-credit-debit-note/ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component';
import { OpsModuleCreditDebitNoteEditComponent } from './ops-module-billing-job-edit/ops-module-credit-debit-note/ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';


@NgModule({
  imports: [
    CommonModule,
    OperationRoutingModule,
    SelectModule,
    FormsModule,
    SharedModule,
    NgxDaterangepickerMd
  ],
  declarations: [JobMangementComponent, AssigmentComponent, TruckingAssignmentComponent, OpsModuleBillingJobCreateComponent, OpsModuleBillingComponent, OpsModuleBillingJobEditComponent, BillingCustomDeclarationComponent, OpsModuleCreditDebitNoteComponent, OpsModuleStageManagementComponent, OpsModuleCreditDebitNoteAddnewComponent, OpsModuleCreditDebitNoteRemainingChargeComponent, OpsModuleCreditDebitNoteDetailComponent, OpsModuleCreditDebitNoteEditComponent],
})
export class OperationModule { }
