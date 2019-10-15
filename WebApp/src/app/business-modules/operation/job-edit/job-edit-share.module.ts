import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OpsModuleCreditDebitNoteDetailComponent } from './credit-debit-note/ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component';
import { OpsModuleCreditDebitNoteEditComponent } from './credit-debit-note/ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { OpsModuleCreditDebitNoteRemainingChargeComponent } from './credit-debit-note/ops-module-credit-debit-note-remaining-charge/ops-module-credit-debit-note-remaining-charge.component';
import { FormsModule } from '@angular/forms';
import { NotSelectedAlertModalComponent } from './credit-debit-note/ops-module-credit-debit-note-addnew/not-selected-alert-modal/not-selected-alert-modal.component';
import { OpsModuleCreditDebitNoteAddnewComponent } from './credit-debit-note/ops-module-credit-debit-note-addnew/ops-module-credit-debit-note-addnew.component';
import { SelectModule } from 'ng2-select';
import { ChangePartnerConfirmModalComponent } from './credit-debit-note/ops-module-credit-debit-note-addnew/change-partner-confirm-modal/change-partner-confirm-modal.component';

@NgModule({
    declarations: [
        OpsModuleCreditDebitNoteDetailComponent,
        OpsModuleCreditDebitNoteEditComponent,
        OpsModuleCreditDebitNoteRemainingChargeComponent,
        NotSelectedAlertModalComponent,
        OpsModuleCreditDebitNoteAddnewComponent,
        ChangePartnerConfirmModalComponent

    ],
    imports: [
        CommonModule,
        SharedModule,
        ModalModule.forRoot(),
        FormsModule,
        SelectModule,
    ],
    exports: [
        // ? Components Share with Credit/Debit note and Job Edit.
        OpsModuleCreditDebitNoteDetailComponent,
        OpsModuleCreditDebitNoteEditComponent,
        OpsModuleCreditDebitNoteRemainingChargeComponent,
        NotSelectedAlertModalComponent,
        OpsModuleCreditDebitNoteAddnewComponent,
        ChangePartnerConfirmModalComponent
    ],
    providers: [],
})
export class JobEditShareModule { }