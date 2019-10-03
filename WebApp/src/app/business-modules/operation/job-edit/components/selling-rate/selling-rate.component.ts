import { Component, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { AddSellingRatePopupComponent } from '../../charge-list/add-selling-rate-popup/add-selling-rate-popup.component';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import cloneDeep from 'lodash/cloneDeep';
import { EditSellingRatePopupComponent } from '../../charge-list/edit-selling-rate-popup/edit-selling-rate-popup.component';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { BaseService, SortService, DataService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { OpsModuleCreditDebitNoteDetailComponent } from '../../credit-debit-note/ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'job-management-selling-rate',
    templateUrl: './selling-rate.component.html'
})

export class JobManagementSellingRateComponent extends AppList {

    @ViewChild(AddSellingRatePopupComponent, { static: false }) addSellingRatePopup: AddSellingRatePopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteCharge: ConfirmPopupComponent;
    @ViewChild(EditSellingRatePopupComponent, { static: false }) editSellingRatePopup: EditSellingRatePopupComponent;
    @ViewChild(OpsModuleCreditDebitNoteDetailComponent, { static: false }) detailCDPopup: OpsModuleCreditDebitNoteDetailComponent;

    @Input() data: any = [];
    @Input() opsTransaction: OpsTransaction = null;
    @Output() onChange: EventEmitter<any> = new EventEmitter<any>();

    lstPartners: any[];
    headers: CommonInterface.IHeaderTable[];
    chargeIdToDelete: string = null;
    SellingRateChargeToEdit: CsShipmentSurcharge = null;
    CDNoteDetails: AcctCDNoteDetails = null;

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService,
        private _data: DataService

    ) {
        super();
        this.requestSort = this.sortSellingRateCharges;
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
            { title: 'Credit/ Debit note', field: 'cdno', sortable: true },
            { title: 'Settle payment', field: 'settlementCode', sortable: true },
            { title: 'Exchange rate date', field: 'exchangeDate', sortable: true },
        ];

    }

    openAddNewSellingRatePopup() {
        this.addSellingRatePopup.show();
    }

    prepareEditCharge(charge: CsShipmentSurcharge) {
        this.SellingRateChargeToEdit = charge;
        if (!!this._data.getDataByKey(SystemConstants.CSTORAGE.PARTNER)) {
            this.lstPartners = this._data.getDataByKey(SystemConstants.CSTORAGE.PARTNER);
        }
        if (this.SellingRateChargeToEdit) {
            setTimeout(() => {
                this.editSellingRatePopup.lstPartners = this.lstPartners;
                this.editSellingRatePopup.show({ backdrop: 'static' });
            }, 100);
        }
    }

    showConfirmDeleteCharge(chargeId: string = null) {
        this.chargeIdToDelete = chargeId;
        this.confirmDeleteCharge.show();
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

    onSaveNewSellingRate(event: any) {
        if (event) {
            this.onChange.emit(event);
        }
    }

    async deleteCharge() {
        const res = await this.baseServices.deleteAsync(this.api_menu.Documentation.CsShipmentSurcharge.delete + "?chargId=" + this.chargeIdToDelete);
        if (res.status) {
            this.onChange.emit(true);
        }
        this.confirmDeleteCharge.hide();
    }

    onSaveSellingRate() {
        this.onChange.emit(true);
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

    sortSellingRateCharges() {
        this.data = this.sortService.sort(this.data, this.sort, this.order);
    }

}
