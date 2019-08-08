import { Component, OnInit, AfterViewChecked, OnDestroy, ChangeDetectorRef, ViewChild, Output, EventEmitter, Input } from '@angular/core';

import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { Subject } from 'rxjs';
import { PopupBase } from 'src/app/popup.base';
import { OpsModuleCreditDebitNoteEditComponent } from '../ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
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
    private cdr: ChangeDetectorRef
  ) {
    super();
  }
  @Output() openEditCDNote = new EventEmitter<any>();
  @Output() isCloseModal = new EventEmitter<any>();
  @Input() CDNoteDetails: AcctCDNoteDetails = null;

  STORAGE_DATA: any = null;
  currentSOANo: string = null;
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
    this.baseServices.setData("CDNoteDetails", this.CDNoteDetails);
    this.popupEdit.StateChecking();
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
  closeEditModal(event) {
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


}
