import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { SettlementExistingChargePopupComponent } from '../popup/existing-charge/existing-charge.popup';
import { SettlementFormChargePopupComponent } from '../popup/form-charge/form-charge.popup';
import _groupBy from 'lodash/groupBy';
import _toPair from 'lodash/toPairs';
import _mapKeys from 'lodash/mapKeys';
import _mapValues from 'lodash/mapValues';

@Component({
    selector: 'settle-payment-list-charge',
    templateUrl: './list-charge-settlement.component.html',
    styleUrls: ['./list-charge-settlement.component.scss']
})

export class SettlementListChargeComponent extends AppList {

    @ViewChild(SettlementExistingChargePopupComponent, { static: false }) existingChargePopup: SettlementExistingChargePopupComponent;
    @ViewChild(SettlementFormChargePopupComponent, { static: false }) formChargePopup: SettlementFormChargePopupComponent;

    groupShipments: any[] = [];
    headers: CommonInterface.IHeaderTable[];

    surcharges: any[] = [];
    selectedSurcharge: any = {};
    selectedIndexSurcharge: number = 0;

    stateFormCharge: string = 'create';
    constructor() {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'JobId', field: 'JobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'Charge Name', field: 'jobId', sortable: true },
            { title: 'Qty', field: 'jobId', },
            { title: 'Unit', field: 'jobId', },
            { title: 'Unit Price', field: 'jobId', },
            { title: 'Currency', field: 'jobId', },
            { title: 'VAT', field: 'jobId', },
            { title: 'Amount', field: 'jobId', },
            { title: 'Payer', field: 'jobId', },
            { title: 'OBH Partner', field: 'jobId', },
            { title: 'Invoice No', field: 'jobId', },
            { title: 'Series No', field: 'jobId', },
            { title: 'Inv Date', field: 'jobId', },
            { title: 'Custom No', field: 'jobId', },
            { title: 'Cont No', field: 'jobId', },
            { title: 'Note', field: 'jobId', },
        ];

        this.surcharges = [
            { jobId: 'SE123321', hbl: '321123', mbl: '100011', chargeName: 'Phi nhien lieu', chargeId: '62cb1fb8-467e-49a2-b4d6-006e7e8a2a5b', typeCharge: 'CREDIT', quantity: 2, unitName: 'Tạ', unitId: 111, unitPrice: 232000, currency: 'VND', vatRate: 10, amount: 4200000, payer: 'ITL', obhPartner: 'FTL', invoiceNo: 'INVXXX', seriesNo: 'SERNOXXX', invoiceDate: '10/09/2019', customNo: '102815969911', contNo: 10, note: 'note', isFromShipment: false }
        ];

        this.selectedSurcharge = this.surcharges[0];
    }

    showExistingCharge() {
        this.existingChargePopup.show();
    }

    showCreateCharge() {
        this.stateFormCharge = 'create';
        this.formChargePopup.show();
    }

    onRequestSurcharge(surcharge: any) {
        this.surcharges.push(surcharge);
    }

    onUpdateRequestSurcharge(surcharge: any) {
        this.surcharges[this.selectedIndexSurcharge] = surcharge;
    }

    openSurchargeDetail(surcharge: any, index: number) {
        this.selectedSurcharge = surcharge;
        this.selectedIndexSurcharge = index;
        this.stateFormCharge = 'update';

        this.formChargePopup.initFormUpdate(this.selectedSurcharge);
        this.formChargePopup.calculateTotalAmount();

        this.formChargePopup.show();
    }

    returnShipmet(item: any) {
        return item.shipment.jobId;
    }
}


