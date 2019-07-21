import { Component, OnInit, AfterViewChecked, OnDestroy, ChangeDetectorRef, ViewChild, Output, EventEmitter, Input } from '@angular/core';

import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { Subject } from 'rxjs';
import { PopupBase } from 'src/app/popup.base';
declare var $: any;

@Component({
  selector: 'app-ops-module-credit-debit-note-detail',
  templateUrl: './ops-module-credit-debit-note-detail.component.html'
})
export class OpsModuleCreditDebitNoteDetailComponent extends PopupBase {
  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU,
    private cdr: ChangeDetectorRef
  ) {
    super();
  }
  @Output() openEditCDNote = new EventEmitter<AcctCDNoteDetails>();
  @Input() CDNoteDetails: AcctCDNoteDetails = null;

  STORAGE_DATA: any = null;
  currentSOANo: string = null;
  currentJobID: string = null;

  previewModalId = "preview-modal";
  dataReport: any;

  subscribe: Subject<any> = new Subject();
  totalCredit: number = 0;
  totalDebit: number = 0;

  ngOnInit() {
  }

  editCDNote() {
    // $('#ops-credit-debit-note-detail-modal').modal('hide');
    // $('#ops-credit-debit-note-edit-modal').modal('show');
    this.hide();
    //this.popupEditCreditDebit.show();
    this.baseServices.setData("CDNoteDetails", this.CDNoteDetails);
    this.openEditCDNote.emit(this.CDNoteDetails);
  }

  ngOnDestroy(): void {
    // this.subscribe.next();
    // this.subscribe.complete();

    this.subscribe.unsubscribe();
  }
  Close() {
    $('#ops-credit-debit-note-detail-modal').modal('hide');
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
