import { Component, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { PopupBase } from 'src/app/popup.base';
import { OpsModuleCreditDebitNoteEditComponent } from '../ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { SortService } from 'src/app/shared/services';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { CDNoteRepo, OperationRepo } from 'src/app/shared/repositories';
import { takeUntil, catchError, finalize } from 'rxjs/operators';
import { NgxSpinnerService } from 'ngx-spinner';
declare var $: any;

@Component({
  selector: 'app-ops-module-credit-debit-note-detail',
  templateUrl: './ops-module-credit-debit-note-detail.component.html'
})
export class OpsModuleCreditDebitNoteDetailComponent extends PopupBase {

  @ViewChild(OpsModuleCreditDebitNoteEditComponent, { static: false }) popupEdit: OpsModuleCreditDebitNoteEditComponent;
  @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
  @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;

  @Output() isCloseModal = new EventEmitter<any>();
  @Input() CDNoteDetails: AcctCDNoteDetails = null;

  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService,
    private cdNoteRepo: CDNoteRepo,
    private spinner: NgxSpinnerService,
    private _operationRepo: OperationRepo
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
    this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?jobId=" + currentCDNoteDetail.jobId + "&cdNo=" + currentCDNoteDetail.cdNote.code);
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

  close() {
    this.isCloseModal.emit(true);
    this.hide();
  }

  async closeEditModal(event: any) {
    this.currentCDNo = this.CDNoteDetails.cdNote.code;
    // this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + this.currentJob.id + "&cdNo=" + this.currentCDNo);
    this.cdNoteRepo.getDetails(this.currentJob.id, this.currentCDNo).pipe(
      takeUntil(this.ngUnsubscribe),
      catchError(this.catchError),
      finalize(() => { this.spinner.hide(); }),
    ).subscribe(
      (res: any) => {
        if (res instanceof Error) {
        } else {
          if (res != null) {
            this.CDNoteDetails = res;
            if (this.CDNoteDetails != null) {
              this.show();
              this.totalCreditDebitCalculate();
            } else {
              this.close();
            }
          }
        }
      },
      // error
      (errs: any) => {
        this.close();
        // this.handleErrors(errs)
      },
      // complete
      () => { }
    );
  }
  async Preview() {
    // if (this.CDNoteDetails.listSurcharges.length === 0) {
    //   this.baseServices.errorToast("This credit debit node must have at least 1 surcharge !");
    // } else {
    //   const response = await this.baseServices.postAsync(this.api_menu.Documentation.AcctSOA.previewCDNote, this.CDNoteDetails);
    //   this.dataReport = response;

    //   // * wait to report form submited
    //   setTimeout(() => {
    //     this.reportPopup.show();
    //   }, 100);
    // }
    this._operationRepo.previewCDNote(this.CDNoteDetails)
      .pipe(
        catchError(this.catchError),
        finalize(() => { })
      )
      .subscribe(
        (res: any) => {
          this.dataReport = res;
          setTimeout(() => {
            this.reportPopup.show();
          }, 1000);

        },
      );
  }

  sort(property: any) {
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.CDNoteDetails.listSurcharges = this.sortService.sort(this.CDNoteDetails.listSurcharges, property, this.isDesc);
  }
  // totalCreditDebitCalculate() {
  //   this.totalCredit = 0;
  //   this.totalDebit = 0;
  //   for (let i = 0; i < this.CDNoteDetails.listSurcharges.length; i++) {
  //     const c = this.CDNoteDetails.listSurcharges[i];
  //     if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.payerId)) {
  //       // calculate total credit
  //       this.totalCredit += (c.total * c.exchangeRate);
  //     }
  //     if (c.type === "SELL" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.paymentObjectId)) {
  //       // calculate total debit 
  //       this.totalDebit += (c.total * c.exchangeRate);
  //     }
  //   }
  // }
  totalCreditDebitCalculate() {
    let totalCredit = 0;
    let totalDebit = 0;
    for (let i = 0; i < this.CDNoteDetails.listSurcharges.length; i++) {
      const c = this.CDNoteDetails.listSurcharges[i];
      if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.payerId)) {
        // calculate total credit
        totalCredit += (c.total * c.exchangeRate);
      }
      if (c.type === "SELL" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.paymentObjectId)) {
        // calculate total debit 
        totalDebit += (c.total * c.exchangeRate);
      }

    }
    this.CDNoteDetails.totalCredit = totalCredit;
    this.CDNoteDetails.totalDebit = totalDebit;
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
