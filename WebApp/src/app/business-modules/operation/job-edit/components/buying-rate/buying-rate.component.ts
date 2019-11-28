import { Component, Input, ViewChild, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { BaseService, SortService, DataService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { EditBuyingRatePopupComponent } from '../../charge-list/edit-buying-rate-popup/edit-buying-rate-popup.component';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { AddBuyingRatePopupComponent } from '../../charge-list/add-buying-rate-popup/add-buying-rate-popup.component';
import { SystemConstants } from 'src/constants/system.const';
import { OpsCdNoteDetailPopupComponent } from '../popup/ops-cd-note-detail/ops-cd-note-detail.popup';

@Component({
    selector: 'job-management-buying-rate',
    templateUrl: './buying-rate.component.html',
})

export class JobManagementBuyingRateComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteCharge: ConfirmPopupComponent;
    @ViewChild(EditBuyingRatePopupComponent, { static: false }) editBuyingRatePopup: EditBuyingRatePopupComponent;
    @ViewChild(OpsCdNoteDetailPopupComponent, { static: false }) detailCDPopup: OpsCdNoteDetailPopupComponent;
    @ViewChild(AddBuyingRatePopupComponent, { static: false }) addBuyingRatePopup: AddBuyingRatePopupComponent;

    @Output() onChange: EventEmitter<any> = new EventEmitter<any>();
    @Input() data: any = [];
    @Input() opsTransaction: OpsTransaction = null;
    lstPartners: any[];
    headers: CommonInterface.IHeaderTable[];

    chargeIdToDelete: string = '';
    BuyingRateChargeToEdit: CsShipmentSurcharge = null;
    CDNoteDetails: AcctCDNoteDetails = null;

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService,
        private _data: DataService

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
        this.BuyingRateChargeToEdit = charge;
        if (!!this._data.getDataByKey(SystemConstants.CSTORAGE.PARTNER)) {
            this.lstPartners = this._data.getDataByKey(SystemConstants.CSTORAGE.PARTNER);
        }
        setTimeout(() => {
            this.editBuyingRatePopup.lstPartners = this.lstPartners;
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

    openCreditDebitNote(cdNo: string) {        
        this.detailCDPopup.jobId = this.opsTransaction.id;
        this.detailCDPopup.cdNote = cdNo;
        this.detailCDPopup.getDetailCdNote(this.opsTransaction.id, cdNo);
        this.detailCDPopup.show();
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
