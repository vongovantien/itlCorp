import { Component, OnInit, AfterViewChecked, OnDestroy, ChangeDetectorRef, ViewChild, Output, EventEmitter, Input } from '@angular/core';

import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { Subject } from 'rxjs';
import { PopupBase } from 'src/app/popup.base';
import { OpsModuleCreditDebitNoteEditComponent } from '../ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { SortService } from 'src/app/shared/services';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
declare var $: any;

@Component({
  selector: 'app-ops-module-credit-debit-note-detail',
  templateUrl: './ops-module-credit-debit-note-detail.component.html'
})
export class OpsModuleCreditDebitNoteDetailComponent extends PopupBase {

  @ViewChild(OpsModuleCreditDebitNoteEditComponent, { static: false }) popupEdit: OpsModuleCreditDebitNoteEditComponent;
  @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU,
    private cdr: ChangeDetectorRef,
    private sortService: SortService
  ) {
    super();
  }
  @Output() openEditCDNote = new EventEmitter<any>();
  @Output() isCloseModal = new EventEmitter<any>();
  @Input() CDNoteDetails: AcctCDNoteDetails = null;
  currentJob: OpsTransaction;

  STORAGE_DATA: any = null;
  currentCDNo: String = null;
  // currentJobID: string = null;

  previewModalId = "preview-modal";
  dataReport: any;

  subscribe: Subject<any> = new Subject();
  totalCredit: number = 0;
  totalDebit: number = 0;

  ngOnInit() {
  }

  async editCDNote() {
    const currentCDNoteDetail = this.CDNoteDetails;
    // this.CDNoteDetails = null;
    this.hide();
    this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + currentCDNoteDetail.jobId + "&cdNo=" + currentCDNoteDetail.cdNote.code);
    // this.baseServices.setData("CDNoteDetails", this.CDNoteDetails);
    if (this.CDNoteDetails != null) {
      this.totalCreditDebitCalculate();
    }
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

  ngOnDestroy(): void {
    // this.subscribe.next();
    // this.subscribe.complete();

    this.subscribe.unsubscribe();
  }
  close() {
    this.isCloseModal.emit(true);
    this.hide();
  }
  async closeEditModal(event) {
    this.currentCDNo = this.CDNoteDetails.cdNote.code;
    this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + this.currentJob.id + "&cdNo=" + this.currentCDNo);

    this.show();
    if (this.CDNoteDetails != null) {
      this.totalCreditDebitCalculate();
    }
  }
  async Preview() {
    this.dataReport = null;
    if (this.CDNoteDetails.listSurcharges.length === 0) {
      this.baseServices.errorToast("This credit debit node must have at least 1 surcharge !");
    } else {
      const response = await this.baseServices.postAsync(this.api_menu.Documentation.AcctSOA.previewCDNote, this.CDNoteDetails);
      console.log(response);
      this.dataReport = response;
      const _this = this;
      const checkExist = setInterval(function () {
        if ($('#frame').length) {
          console.log("Exists!");
          $('#' + _this.previewModalId).modal('show');
          clearInterval(checkExist);
        }
      }, 100);
    }
  }
  isDesc = true;
  sortKey: string = '';
  sort(property) {
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.CDNoteDetails.listSurcharges = this.sortService.sort(this.CDNoteDetails.listSurcharges, property, this.isDesc);
  }
  totalCreditDebitCalculate() {
    this.totalCredit = 0;
    this.totalDebit = 0;
    for (let i = 0; i < this.CDNoteDetails.listSurcharges.length; i++) {
      const c = this.CDNoteDetails.listSurcharges[i];
      if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.payerId)) {
        // calculate total credit
        this.totalCredit += (c.total * c.exchangeRate);
      }
      if (c.type === "SELL" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.paymentObjectId)) {
        // calculate total debit 
        this.totalDebit += (c.total * c.exchangeRate);
      }
    }
  }
  async deleteCDNote() {
    const res = await this.baseServices.deleteAsync(this.api_menu.Documentation.AcctSOA.delete + "?cdNoteId=" + this.CDNoteDetails.cdNote.id);
    if (res.status) {
      // this.getAllCDNote();
      this.isCloseModal.emit(true);
      this.confirmDeletePopup.hide();
      this.hide();
    }
  }
  showDeleteModal() {
    this.confirmDeletePopup.show();
  }
}
