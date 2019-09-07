import { Component, ViewChild } from '@angular/core';
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

    @ViewChild(SettlementPaymentManagementPopupComponent, { static: false }) paymentManagementPopup: SettlementPaymentManagementPopupComponent;
    @ViewChild(SettlementExistingChargePopupComponent, { static: false }) existingChargePopup: SettlementExistingChargePopupComponent;
    @ViewChild(SettlementFormChargePopupComponent, { static: false }) formChargePopup: SettlementFormChargePopupComponent;


    headers: CommonInterface.IHeaderTable[];
    items: any[] = [1, 2, 3, 4, 5];

    constructor() {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Name', field: 'jobId', },
            { title: 'Qty', field: 'jobId', },
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
    }

    checkUncheckAllRequest() {

    }

    showPaymentManagement($event: Event): boolean {
        // * prevent collapse/expand within accordion-heading
        $event.stopPropagation();
        $event.preventDefault();

        this.paymentManagementPopup.show();
        return false;
    }

    showExistingCharge() {
        this.existingChargePopup.show();
    }

    showCreateCharge() {
        this.formChargePopup.show();
    }
}

