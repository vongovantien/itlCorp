import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { AccountingRepo } from 'src/app/shared/repositories';
import { Charge, Surcharge } from 'src/app/shared/models';
import { catchError, map } from 'rxjs/operators';

@Component({
    selector: 'copy-settlement-popup',
    templateUrl: './copy-settlement.popup.html',
})
export class SettlementFormCopyPopupComponent extends PopupBase {
    headersCharge: CommonInterface.IHeaderTable[];
    headerShipment: CommonInterface.IHeaderTable[];

    searchOptions: CommonInterface.ICommonTitleValue[];
    charges: Surcharge[] = [];
    settlementNo: string = '';

    constructor(
        private _accountingRepo: AccountingRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this.headersCharge = [
            { title: 'Charge Name', field: 'chargeName', sortable: true, width: 200 },
            { title: 'Shipment', field: 'jobId', sortable: true, width: 200 },
            { title: 'Qty', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unitName', sortable: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Partner', field: 'payer', sortable: true, width: 200 },
            { title: 'OBH Partner', field: 'obhPartnerName', sortable: true, width: 200 },
            { title: 'Custom No', field: 'clearanceNo', sortable: true },
            { title: 'Charge Code', field: 'chargeCode', sortable: true, width: 200 },
        ];

        this.headerShipment = [
            { title: 'No', field: 'chargeName', sortable: true, width: 50 },
            { title: 'Shipment ID', field: 'jobId', sortable: true, width: 200 },
            { title: 'Customer', field: 'quantity', sortable: true },
            { title: 'HBL', field: 'unitName', sortable: true },
            { title: 'MBL', field: 'unitPrice', sortable: true },
            { title: 'Custom No', field: 'currencyId', sortable: true },
            { title: 'Service', field: 'vatrate', sortable: true },
        ];

        this.searchOptions = [
            { title: 'Job No', value: 'JOBNO' },
            { title: 'HBL/HAWB', value: 'HBL' },
            { title: 'MBL/MAWB', value: 'MBL' },
            { title: 'Custom No', value: 'CUSTOMNO' },
        ];


    }

    submitCopyCharge() {

    }

    sortSurcharge($event) {

    }
    checkUncheckAllCharge() {

    }

    searchShipment() {
        if (!!this.settlementNo) {
            this._accountingRepo.getListChargeSettlementBySettlementNo(this.settlementNo)
                .pipe(
                    catchError(this.catchError),
                    map(response => (response || []).map((charge: Surcharge) => new Surcharge(charge)))
                )
                .subscribe(
                    (res: any) => {
                        this.charges = res;
                        console.log(this.charges);
                    }
                );
        }

    }
}
