import { Component, Input, ViewChild, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { BaseService, SortService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import cloneDeep from 'lodash/cloneDeep';
import { EditBuyingRatePopupComponent } from '../../charge-list/edit-buying-rate-popup/edit-buying-rate-popup.component';
import { OpsModuleCreditDebitNoteDetailComponent } from '../../credit-debit-note/ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { AddBuyingRatePopupComponent } from '../../charge-list/add-buying-rate-popup/add-buying-rate-popup.component';

@Component({
    selector: 'job-management-buying-rate',
    templateUrl: './buying-rate.component.html',
})

export class JobManagementBuyingRateComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteCharge: ConfirmPopupComponent;
    @ViewChild(EditBuyingRatePopupComponent, { static: false }) editBuyingRatePopup: EditBuyingRatePopupComponent;
    @ViewChild(OpsModuleCreditDebitNoteDetailComponent, { static: false }) detailCDPopup: OpsModuleCreditDebitNoteDetailComponent;
    @ViewChild(AddBuyingRatePopupComponent, { static: false }) addBuyingRatePopup: AddBuyingRatePopupComponent;

    @Output() onChange: EventEmitter<any> = new EventEmitter<any>();
    @Input() data: any = [];
    @Input() opsTransaction: OpsTransaction = null;
    headers: CommonInterface.IHeaderTable[];

    chargeIdToDelete: string = '';
    BuyingRateChargeToEdit: CsShipmentSurcharge = null;
    CDNoteDetails: AcctCDNoteDetails = null;

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService

    ) {
        super();
        this.requestSort = this.sortBuyingRateCharges;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Partner', field: 'partnerName', sortable: true },
            { title: 'Name EN', field: 'nameEn', sortable: true },
            { title: 'Quantity', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unit', sortable: true },
            { title: 'Unit price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Total', field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'SOA', field: 'soano', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Credit/ Debit note', field: 'cdno', sortable: true },
            { title: 'Settle payment', field: 'settlementCode', sortable: true },
            { title: 'Exchange rate date', field: 'exchangeDate', sortable: true },
        ];

    }

    showConfirmDeleteCharge(chargeId: string = null) {
        this.chargeIdToDelete = chargeId;
        this.confirmDeleteCharge.show();
    }

    async deleteCharge() {
        const res = await this.baseServices.deleteAsync(this.api_menu.Documentation.CsShipmentSurcharge.delete + "?chargId=" + this.chargeIdToDelete);
        if (res.status) {
            this.onChange.emit(res.status);
        }
        this.confirmDeleteCharge.hide();
    }

    prepareEditCharge(charge: any) {
        this.BuyingRateChargeToEdit = cloneDeep(charge);
        setTimeout(() => {
            this.editBuyingRatePopup.show();
        }, 100);
    }

    onSaveBuyingRate(data: any) {
        this.editBuyingRatePopup.hide();
        this.onChange.emit(data);
    }

    onSaveNewBuyingRate(event) {
        if (event === true) {
            this.onChange.emit(event);
        }
    }

    async openCreditDebitNote(cdNo: string) {
        this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?jobId=" + this.opsTransaction.id + "&cdNo=" + cdNo);
        if (this.CDNoteDetails != null) {
            if (this.CDNoteDetails.listSurcharges != null) {
                this.totalCreditDebitCalculate();
            }
            if (this.CDNoteDetails.cdNote.type === 'CREDIT') {
                this.CDNoteDetails.cdNote.type = 'Credit';
            }
            if (this.CDNoteDetails.cdNote.type === 'DEBIT') {
                this.CDNoteDetails.cdNote.type = 'Debit';
            }
            if (this.CDNoteDetails.cdNote.type === 'INVOICE') {
                this.CDNoteDetails.cdNote.type = 'Invoice';
            }

            this.detailCDPopup.currentJob = this.opsTransaction;
            this.detailCDPopup.show();
        }
    }

    totalCreditDebitCalculate() {
        let totalCredit = 0;
        let totalDebit = 0;
        // tslint:disable-next-line: prefer-for-of
        for (let i = 0; i < this.CDNoteDetails.listSurcharges.length; i++) {
            const c = this.CDNoteDetails.listSurcharges[i];
            if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.payerId)) {
                // calculate total credit
                totalCredit += (c.total * c.exchangeRate);
            }
            if (c.type === "SELL" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.objectBePaid)) {
                // calculate total debit 
                totalDebit += (c.total * c.exchangeRate);
            }

        }
        this.CDNoteDetails.totalCredit = totalCredit;
        this.CDNoteDetails.totalDebit = totalDebit;
    }

    openAddNewBuyingRatePopup() {
        this.addBuyingRatePopup.show();
    }

    sortBuyingRateCharges() {
        this.data = this.sortService.sort(this.data, this.sort, this.order);
    }



}
