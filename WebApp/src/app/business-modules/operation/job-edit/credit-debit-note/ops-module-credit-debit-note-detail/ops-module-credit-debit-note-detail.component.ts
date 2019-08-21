import { Component, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { PopupBase } from 'src/app/popup.base';
import { OpsModuleCreditDebitNoteEditComponent } from '../ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { SortService } from 'src/app/shared/services';
import { ReportPreviewComponent } from 'src/app/shared/common';

@Component({
    selector: 'app-ops-module-credit-debit-note-detail',
    templateUrl: './ops-module-credit-debit-note-detail.component.html'
})
export class OpsModuleCreditDebitNoteDetailComponent extends PopupBase {

    @ViewChild(OpsModuleCreditDebitNoteEditComponent, { static: false }) popupEdit: OpsModuleCreditDebitNoteEditComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;

    @Output() isCloseModal = new EventEmitter<any>();
    @Input() CDNoteDetails: AcctCDNoteDetails = null;

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService
    ) {
        super();
    }

    currentJob: OpsTransaction;

    STORAGE_DATA: any = null;
    currentCDNo: String = null;
    // currentJobID: string = null;

    dataReport: any;

    totalCredit: number = 0;
    totalDebit: number = 0;

    isDesc = true;
    sortKey: string = '';

    ngOnInit() {
    }

    async editCDNote() {
        const currentCDNoteDetail = this.CDNoteDetails;
        // this.CDNoteDetails = null;
        this.hide();
        this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + currentCDNoteDetail.jobId + "&cdNo=" + currentCDNoteDetail.cdNote.code);
        // this.baseServices.setData("CDNoteDetails", this.CDNoteDetails);
        this.popupEdit.currentCDNo = currentCDNoteDetail.cdNote.code;
        this.popupEdit.currentJob = this.currentJob;
        this.popupEdit.EditingCDNote.id = this.CDNoteDetails.cdNote.id;
        this.popupEdit.EditingCDNote.partnerId = this.CDNoteDetails.partnerId;
        this.popupEdit.EditingCDNote.code = this.popupEdit.currentCDNo;
        this.popupEdit.EditingCDNote.partnerName = this.CDNoteDetails.partnerNameEn;
        this.popupEdit.EditingCDNote.type = this.CDNoteDetails.cdNote.type;
        this.popupEdit.getListCharges(this.CDNoteDetails.partnerId);
        this.popupEdit.show();
    }

    close() {
        this.isCloseModal.emit(true);
        this.hide();
    }

    async closeEditModal(event: any) {
        this.currentCDNo = this.CDNoteDetails.cdNote.code;
        this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + this.currentJob.id + "&cdNo=" + this.currentCDNo);
        this.show();
    }

    async Preview() {
        this.dataReport = null;
        if (this.CDNoteDetails.listSurcharges.length === 0) {
            this.baseServices.errorToast("This credit debit node must have at least 1 surcharge !");
        } else {
            const response = await this.baseServices.postAsync(this.api_menu.Documentation.AcctSOA.previewCDNote, this.CDNoteDetails);
            this.dataReport = response;

            // * wait to report form submited
            setTimeout(() => {
                this.reportPopup.show();
            }, 100);
        }
    }

    sort(property: any) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.CDNoteDetails.listSurcharges = this.sortService.sort(this.CDNoteDetails.listSurcharges, property, this.isDesc);
    }

}
