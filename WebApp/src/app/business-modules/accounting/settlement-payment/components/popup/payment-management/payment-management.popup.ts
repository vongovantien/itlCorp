import { Component, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { AdvancePayment } from 'src/app/shared/models';

@Component({
    selector: 'payment-management-popup',
    templateUrl: './payment-management.popup.html'
})

export class SettlementPaymentManagementPopupComponent extends PopupBase {

    headerAdvance: CommonInterface.IHeaderTable[];
    headerSettlement: CommonInterface.IHeaderTable[];

    data: any = null;
    constructor() {
        super();
    }

    ngOnInit() {
        this.headerAdvance = [
            { title: 'Advance No', field: 'AdvanceNo', sortable: true },
            { title: 'Description', field: 'AdvanceNo', sortable: true },
            { title: 'Total Amount', field: 'AdvanceNo', sortable: true },
            { title: 'Currency', field: 'AdvanceNo', sortable: true },
            { title: 'Advance Date', field: 'AdvanceNo', sortable: true },
        ];

        this.headerSettlement = [
            { title: 'Settlement No', field: 'AdvanceNo', sortable: true },
            { title: 'Charge Name', field: 'AdvanceNo', sortable: true },
            { title: 'Total Amount', field: 'AdvanceNo', sortable: true },
            { title: 'Currency', field: 'AdvanceNo', sortable: true },
            { title: 'Settlement Date', field: 'AdvanceNo', sortable: true },
            { title: 'OBH Partner', field: 'AdvanceNo', sortable: true },
        ];

        this.data = {
            jobId: 'SE32423424',
            hbl: '32123131',
            mbl: '1231312',
            totalShipment: 42125000,
            totalAmount: 42125000,
            advancePayment: [
                new AdvancePayment()
            ],
            settlementPayment: []
        };
    }

    ngOnChanges() {

    }

    
}
