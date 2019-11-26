import { Component, ViewChild, Output, EventEmitter, ElementRef } from "@angular/core";
import { PopupBase } from "src/app/popup.base";
import { DocumentationRepo } from "src/app/shared/repositories";
import { CdNoteAddPopupComponent } from "../add-cd-note/add-cd-note.popup";
import { catchError, finalize } from "rxjs/operators";
import { SortService } from "src/app/shared/services";
import { ToastrService } from "ngx-toastr";
import { ConfirmPopupComponent, InfoPopupComponent } from "src/app/shared/common/popup";
import { DomSanitizer } from "@angular/platform-browser";
import { API_MENU } from "src/constants/api-menu.const";
import { ModalDirective } from "ngx-bootstrap";
import { Crystal } from "src/app/shared/models/report/crystal.model";

@Component({
    selector: 'cd-note-detail-popup',
    templateUrl: './detail-cd-note.popup.html'
})
export class CdNoteDetailPopupComponent extends PopupBase {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteCdNotePopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteCdNotePopup: InfoPopupComponent;
    @ViewChild(CdNoteAddPopupComponent, { static: false }) cdNoteEditPopupComponent: CdNoteAddPopupComponent;
    @ViewChild('formPreviewCdNote', { static: false }) formPreviewCdNote: ElementRef;
    @ViewChild("popupReport", { static: false }) popupReport: ModalDirective;
    @Output() onDeleted: EventEmitter<any> = new EventEmitter<any>();

    jobId: string = null;
    cdNote: string = null;
    deleteMessage: string = '';
    isHouseBillID: boolean = false;

    headers: CommonInterface.IHeaderTable[];

    CdNoteDetail: any = null;
    totalCredit: string = '';
    totalDebit: string = '';
    balanceAmount: string = '';

    dataReport: any = null;

    constructor(
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private sanitizer: DomSanitizer,
        private api_menu: API_MENU,
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
            { title: "Credit Value (Local)", field: 'total', sortable: true },
            { title: "Debit Value (Local)", field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true }
        ];
    }

    getDetailCdNote(jobId: string, cdNote: string) {
        this._documentationRepo.getDetailsCDNote(jobId, cdNote)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (dataCdNote: any) => {
                    this.CdNoteDetail = dataCdNote;
                    //Tính toán Amount Credit, Debit, Balance
                    this.calculatorAmount();
                },
            );
    }

    calculatorAmount() {
        this.totalCredit = '';
        this.totalDebit = '';
        this.balanceAmount = '';
        const _credit = this.CdNoteDetail.listSurcharges.filter(f => (f.type === 'BUY' || (f.type === 'OBH' && this.CdNoteDetail.partnerId === f.payerId))).reduce((credit, charge) => credit + charge.total * charge.exchangeRate, 0);
        const _debit = this.CdNoteDetail.listSurcharges.filter(f => (f.type === 'SELL' || (f.type === 'OBH' && this.CdNoteDetail.partnerId === f.paymentObjectId))).reduce((debit, charge) => debit + charge.total * charge.exchangeRate, 0);
        const _balance = _debit - _credit;
        this.totalCredit = this.formatNumberCurrency(_credit);
        this.totalDebit = this.formatNumberCurrency(_debit);
        this.balanceAmount = (_balance > 0 ? this.formatNumberCurrency(_balance) : '(' + this.formatNumberCurrency(Math.abs(_balance)) + ')');
    }

    formatNumberCurrency(input: number) {
        return input.toLocaleString(
            undefined, // leave undefined to use the browser's locale, or use a string like 'en-US' to override it.
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
                        this.deleteMessage = `All related information will be lost? Are you sure you want to delete this Credit/Debit Note?`;
                        this.confirmDeleteCdNotePopup.show();
                    } else {
                        this.canNotDeleteCdNotePopup.show();
                    }
                },
            );
    }

    onDeleteCdNote() {
        this._documentationRepo.deleteCdNote(this.CdNoteDetail.cdNote.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.confirmDeleteCdNotePopup.hide();
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

    previewCdNote(data: string) {
        this._documentationRepo.previewSIFCdNote({ jobId: this.jobId, creditDebitNo: this.cdNote, currency: data })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = JSON.stringify(res);
                    if(res != null && res.dataSource.length > 0){
                        setTimeout(() => {
                            if (!this.popupReport.isShown) {
                                this.popupReport.config = this.options;
                                this.popupReport.show();
                            }
                            this.submitFormPreview();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    get scr() {
        return this.sanitizer.bypassSecurityTrustResourceUrl(this.api_menu.Report);
    }

    ngAfterViewInit() {
        if (!!this.dataReport) {
            this.formPreviewCdNote.nativeElement.submit();
        }
    }

    submitFormPreview() {
        this.formPreviewCdNote.nativeElement.submit();
    }

    onSubmitForm(event) {
        return true;
    }

    hidePreview() {
        this.popupReport.hide();
    }

}