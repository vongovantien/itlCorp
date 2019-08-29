import { Component, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { AddObhRatePopupComponent } from '../../charge-list/add-obh-rate-popup/add-obh-rate-popup.component';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { BaseService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { OpsModuleCreditDebitNoteDetailComponent } from '../../credit-debit-note/ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component';
import cloneDeep from 'lodash/cloneDeep';
import { EditObhRatePopupComponent } from '../../charge-list/edit-obh-rate-popup/edit-obh-rate-popup.component';

@Component({
    selector: 'job-management-obh',
    templateUrl: './obh.component.html'
})

export class JobManagementOBHComponent extends AppList {

    @ViewChild(AddObhRatePopupComponent, { static: false }) addOHBRatePopup: AddObhRatePopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteCharge: ConfirmPopupComponent;
    @ViewChild(OpsModuleCreditDebitNoteDetailComponent, { static: false }) poupDetail: OpsModuleCreditDebitNoteDetailComponent;
    @ViewChild(EditObhRatePopupComponent, { static: false }) editOHBRatePopup: EditObhRatePopupComponent;

    @Output() onChange: EventEmitter<any> = new EventEmitter<any>();
    @Input() data: any = [];
    @Input() opsTransaction: OpsTransaction = null;

    headers: CommonInterface.IHeaderTable[];
    chargeIdToDelete: string = null;
    OBHChargeToEdit: any = null;
    CDNoteDetails: AcctCDNoteDetails = null;

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Receiver', field: 'partnerName', sortable: true },
            { title: 'Payer ', field: 'partnerName', sortable: true },
            { title: 'Name EN', field: 'nameEn', sortable: true },
            { title: 'Quantity', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unit', sortable: true },
            { title: 'Unit price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Total', field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'Docs', field: 'docNo', sortable: true },
            { title: 'SOA', field: 'soano', sortable: true },
            { title: 'Credit/ Debit note', field: 'cdno', sortable: true },
            { title: 'Settle payment', field: 'settlementCode', sortable: true },
            { title: 'Exchange rate date', field: 'exchangeDate', sortable: true },
        ];
    }

    openAddNewOBHRatePopup() {
        this.addOHBRatePopup.show({ backdrop: 'static' });
    }

    onSaveNewOBHRate($event: boolean) {
        if ($event) {
            this.onChange.emit($event);
        }
    }

    prepareEditCharge(charge: any) {
        this.OBHChargeToEdit = cloneDeep(charge);
        if (this.OBHChargeToEdit) {
            setTimeout(() => {
                this.editOHBRatePopup.show({ backdrop: 'static' });
            }, 100);
        }
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

    onSaveOHBRate($event) {
        this.onChange.emit($event);
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
            this.poupDetail.currentJob = this.opsTransaction;
            this.poupDetail.show({ backdrop: 'static' });
            this.poupDetail.show({ backdrop: 'static' });
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

}

