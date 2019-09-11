import { Component, ViewChild, ContentChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { SettlementPaymentManagementPopupComponent } from '../popup/payment-management/payment-management.popup';
import { SettlementExistingChargePopupComponent } from '../popup/existing-charge/existing-charge.popup';
import { SettlementFormChargePopupComponent } from '../popup/form-charge/form-charge.popup';

@Component({
    selector: 'settle-payment-list-charge',
    templateUrl: './list-charge-settlement.component.html',
    styleUrls: ['./list-charge-settlement.component.scss']
})

export class SettlementListChargeComponent extends AppList {

    @ViewChild(SettlementExistingChargePopupComponent, { static: false }) existingChargePopup: SettlementExistingChargePopupComponent;
    @ViewChild(SettlementFormChargePopupComponent, { static: false }) formChargePopup: SettlementFormChargePopupComponent;
    items: any[] = [1, 2, 3, 4, 5];

    constructor() {
        super();
    }

    ngOnInit() {
    
    }




    showExistingCharge() {
        this.existingChargePopup.show();
    }

    showCreateCharge() {
        this.formChargePopup.show();
    }

    onRequestSurcharge(surcharge: any) {
        console.log(surcharge);
    }
}

