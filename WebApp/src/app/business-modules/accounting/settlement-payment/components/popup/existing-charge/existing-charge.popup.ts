import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'existing-charge-popup',
    templateUrl: './existing-charge.popup.html'
})

export class SettlementExistingChargePopupComponent extends PopupBase {

    headers: CommonInterface.IHeaderTable[];
    constructor() {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'Qty', field: 'chargeName', sortable: true },
            { title: 'Unit', field: 'chargeName', sortable: true },
            { title: 'Price', field: 'chargeName', sortable: true },
            { title: 'Currency', field: 'chargeName', sortable: true },
            { title: 'VAT', field: 'chargeName', sortable: true },
            { title: 'Amount', field: 'chargeName', sortable: true },
            { title: 'Settlement No', field: 'chargeName', sortable: true },
        ];
        
    }
}
