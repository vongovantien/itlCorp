import { Component, ViewChild, EventEmitter, Output } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SortService } from 'src/app/shared/services';
import { DocumentationRepo, ExportRepo, AccountingRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize } from 'rxjs/operators';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { OpsCdNoteAddPopupComponent } from '../ops-cd-note-add/ops-cd-note-add.popup';
import { NgxSpinnerService } from 'ngx-spinner';
import { AccountingConstants } from '@constants';
import { ShareBussinessPaymentMethodPopupComponent } from 'src/app/business-modules/share-business/components/payment-method/payment-method.popup';
@Component({
    selector: 'ops-cd-note-detail',
    templateUrl: './ops-cd-note-detail.popup.html'
})
export class OpsCdNoteDetailPopupComponent extends PopupBase {
    @ViewChild(ConfirmPopupComponent) confirmCdNotePopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) canNotDeleteCdNotePopup: InfoPopupComponent;
    @ViewChild(ReportPreviewComponent) reportPopup: ReportPreviewComponent;
    @ViewChild(OpsCdNoteAddPopupComponent) cdNoteEditPopupComponent: OpsCdNoteAddPopupComponent; @Output() onDeleted: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild(ShareBussinessPaymentMethodPopupComponent) paymentMethodPopupComponent: ShareBussinessPaymentMethodPopupComponent;

    jobId: string = null;
    cdNote: string = null;
    confirmMessage: string = '';
    typeConfirm: string = '';
    isHouseBillID: boolean = true;

    headers: CommonInterface.IHeaderTable[];

    CdNoteDetail: any = null;
    totalCredit: string = '';
    totalDebit: string = '';
    balanceAmount: string = '';

    dataReport: any = null;
    paymentMethodSelected: string = '';

    constructor(
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _exportRepo: ExportRepo,
        private _accountantRepo: AccountingRepo,
        private _spinner: NgxSpinnerService,
    ) {
        super();
        this.requestSort = this.sortChargeCdNote;
    }

    ngOnInit() {
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true },
            { title: 'Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'nameEn', sortable: true },
            { title: 'Quantity', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unit', sortable: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: "Credit Value", field: 'credit', sortable: true },
            { title: "Debit Value", field: 'debit', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'Exc Rate', field: 'exchangeRate', sortable: true }
        ];
    }

    getDetailCdNote(jobId: string, cdNote: string) {
        this._documentationRepo.getDetailsCDNote(jobId, cdNote)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (dataCdNote: any) => {
                    dataCdNote.listSurcharges.forEach(element => {
                        element.debit = (element.type === 'SELL' || (element.type === 'OBH' && dataCdNote.partnerId === element.paymentObjectId)) ? element.total : null;
                        element.credit = (element.type === 'BUY' || (element.type === 'OBH' && dataCdNote.partnerId === element.payerId)) ? element.total : null;
                    });
                    this.CdNoteDetail = dataCdNote;
                    // Tính toán Amount Credit, Debit, Balance
                    this.calculatorAmount();
                },
            );
    }

    calculatorAmount() {
        // List currency có trong listCharges
        const listCurrency = [];
        const listCharge = [];
        for (const currency of this.CdNoteDetail.listSurcharges.map(m => m.currencyId)) {
            listCurrency.push(currency);
        }
        for (const charge of this.CdNoteDetail.listSurcharges) {
            listCharge.push(charge);
        }
        // List currency unique      
        const uniqueCurrency = [...new Set(listCurrency)]; // Remove duplicate
        this.totalCredit = '';
        this.totalDebit = '';
        this.balanceAmount = '';
        for (const currency of uniqueCurrency) {
            const _credit = listCharge.filter(f => f.currencyId === currency).reduce((credit, charge) => credit + charge.credit, 0);
            const _debit = listCharge.filter(f => f.currencyId === currency).reduce((debit, charge) => debit + charge.debit, 0);
            const _balance = _debit - _credit;
            this.totalCredit += this.formatNumberCurrency(_credit) + ' ' + currency + ' | ';
            this.totalDebit += this.formatNumberCurrency(_debit) + ' ' + currency + ' | ';
            this.balanceAmount += (_balance > 0 ? this.formatNumberCurrency(_balance) : '(' + this.formatNumberCurrency(Math.abs(_balance)) + ')') + ' ' + currency + ' | ';
        }
        this.totalCredit += "]";
        this.totalDebit += "]";
        this.balanceAmount += "]";
        this.totalCredit = this.totalCredit.replace("| ]", "").replace("]", "");
        this.totalDebit = this.totalDebit.replace("| ]", "").replace("]", "");
        this.balanceAmount = this.balanceAmount.replace("| ]", "").replace("]", "");
    }

    formatNumberCurrency(input: number) {
        return input.toLocaleString(
            'en-US', // leave undefined to use the browser's locale, or use a string like 'en-US' to override it.
            { minimumFractionDigits: 3 }
        );
    }

    closePopup() {
        this.hide();
    }

    checkDeleteCdNote(id: string) {
        this._documentationRepo.checkCdNoteAllowToDelete(id)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.confirmMessage = `All related information will be lost? Are you sure you want to delete this Credit/Debit Note?`;
                        this.typeConfirm = "DELETE";
                        this.confirmCdNotePopup.show();
                    } else {
                        this.canNotDeleteCdNotePopup.show();
                    }
                },
            );
    }

    deleteCdNote() {
        this._documentationRepo.deleteCdNote(this.CdNoteDetail.cdNote.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.confirmCdNotePopup.hide();
                })
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {
                        this._toastService.success(respone.message, 'Delete Success !');
                        this.onDeleted.emit();
                        this.closePopup();
                    }
                },
            );
    }

    openPopupEdit() {
        this.cdNoteEditPopupComponent.action = 'update';
        this.cdNoteEditPopupComponent.selectedPartner = { field: "id", value: this.CdNoteDetail.partnerId };
        this.cdNoteEditPopupComponent.selectedNoteType = this.CdNoteDetail.cdNote.type;
        this.cdNoteEditPopupComponent.CDNote = this.CdNoteDetail.cdNote;
        this.cdNoteEditPopupComponent.currentMBLId = this.CdNoteDetail.jobId;
        this.cdNoteEditPopupComponent.note.setValue(this.CdNoteDetail.note);
        this.cdNoteEditPopupComponent.getListCharges(this.CdNoteDetail.jobId, this.CdNoteDetail.partnerId, this.isHouseBillID, this.CdNoteDetail.cdNote.code);
        this.cdNoteEditPopupComponent.show();
    }

    onUpdateCdNote(dataRequest: any) {
        this.onDeleted.emit();
        this.getDetailCdNote(this.jobId, this.cdNote);
    }

    sortChargeCdNote(sort: string): void {
        if (this.CdNoteDetail) {
            this.CdNoteDetail.listSurcharges = this._sortService.sort(this.CdNoteDetail.listSurcharges, sort, this.order);
        }
    }

    preview(isOrigin: boolean) {
        this.CdNoteDetail.totalCredit = this.CdNoteDetail.listSurcharges.reduce((credit, charge) => credit + charge.credit, 0);
        this.CdNoteDetail.totalDebit = this.CdNoteDetail.listSurcharges.reduce((debit, charge) => debit + charge.debit, 0);
        this._documentationRepo.previewCDNote(this.CdNoteDetail, isOrigin)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res != null && res.dataSource.length > 0) {
                        this.dataReport = res;
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    exportCDNote() {
        const userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
        this._exportRepo.exportCDNote(this.CdNoteDetail.jobId, this.CdNoteDetail.cdNote.code, userLogged.officeId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'OPS - DEBIT NOTE.xlsx');
                    } else {
                        this._toastService.warning('No data found');
                    }
                },
            );
    }

    confirmSendToAcc() {
        this.confirmMessage = `Are you sure you want to send data to accountant system?`;
        this.typeConfirm = "CONFIRMED";
        this.confirmCdNotePopup.show();
    }

    showConfirmed() {
        if (this.CdNoteDetail.cdNote.type === 'CREDIT' && this.CdNoteDetail.creditPayment === 'Direct') {
            this.paymentMethodPopupComponent.show();
        } else {
            this.paymentMethodSelected = 'Other'; // CR 14979: 03-12-2020
            this.confirmSendToAcc();
        }
    }

    onApplyPaymentMethod($event) {
        this.paymentMethodSelected = $event;
        this.confirmSendToAcc();
    }

    onConfirmCdNote() {
        if (this.typeConfirm === "DELETE") {
            this.deleteCdNote();
        } else if (this.typeConfirm === "CONFIRMED") {
            this.syncCdNote();
        }
    }

    syncCdNote() {
        this.confirmCdNotePopup.hide();
        const cdNoteIds: AccountingInterface.IRequestGuidType[] = [];
        const cdNoteId: AccountingInterface.IRequestGuidType = {
            Id: this.CdNoteDetail.cdNote.id,
            type: this.CdNoteDetail.cdNote.type,
            action: this.CdNoteDetail.cdNote.syncStatus === AccountingConstants.SYNC_STATUS.REJECTED ? 'UPDATE' : 'ADD',
            paymentMethod: this.paymentMethodSelected
        };
        cdNoteIds.push(cdNoteId);
        this._spinner.show();
        this._accountantRepo.syncCdNoteToAccountant(cdNoteIds)
            .pipe(
                finalize(() => this._spinner.hide()),
                catchError(this.catchError),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (((res as CommonInterface.IResult).status)) {
                        this._toastService.success("Send Data to Accountant System Successful");
                        this.getDetailCdNote(this.jobId, this.cdNote);
                        // Gọi onDelete để refresh lại list cd note
                        this.onDeleted.emit();
                    } else {
                        this._toastService.error("Send Data Fail");
                    }
                },
                (error) => {
                    console.log(error);
                }
            );
    }
}
