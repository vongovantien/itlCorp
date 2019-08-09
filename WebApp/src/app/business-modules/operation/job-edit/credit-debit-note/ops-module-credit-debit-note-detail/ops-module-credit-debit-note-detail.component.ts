import { Component, OnInit, AfterViewChecked, OnDestroy, ChangeDetectorRef, ViewChild, Output, EventEmitter, Input } from '@angular/core';

import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { Subject } from 'rxjs';
import { PopupBase } from 'src/app/popup.base';
import { OpsModuleCreditDebitNoteEditComponent } from '../ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { SortService } from 'src/app/shared/services';
declare var $: any;

@Component({
  selector: 'app-ops-module-credit-debit-note-detail',
  templateUrl: './ops-module-credit-debit-note-detail.component.html'
})
export class OpsModuleCreditDebitNoteDetailComponent extends PopupBase {

  @ViewChild(OpsModuleCreditDebitNoteEditComponent, { static: false }) popupEdit: OpsModuleCreditDebitNoteEditComponent;

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
    this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + currentCDNoteDetail.jobId + "&soaNo=" + currentCDNoteDetail.cdNote.code);
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
    this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + this.currentJob.id + "&soaNo=" + this.currentCDNo);
    this.show();
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

}
