import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { SettlementPaymentManagementPopupComponent } from '../popup/payment-management/payment-management.popup';
import { SettlementExistingChargePopupComponent } from '../popup/existing-charge/existing-charge.popup';
import { SettlementFormChargePopupComponent } from '../popup/form-charge/form-charge.popup';

@Component({
    selector: 'settle-payment-list-charge',
    templateUrl: './list-charge-settlement.component.html'
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
            { title: 'Charge Name', field: 'jobId', sortable: true },
            { title: 'Qty', field: 'jobId', sortable: true },
            { title: 'Unit Price', field: 'jobId', sortable: true },
            { title: 'Currency', field: 'jobId', sortable: true },
            { title: 'VAT', field: 'jobId', sortable: true },
            { title: 'Amount', field: 'jobId', sortable: true },
            { title: 'Payer', field: 'jobId', sortable: true },
            { title: 'OBH Partner', field: 'jobId', sortable: true },
            { title: 'Invoice No', field: 'jobId', sortable: true },
            { title: 'Series No', field: 'jobId', sortable: true },
            { title: 'Inv Date', field: 'jobId', sortable: true },
            { title: 'Custom No', field: 'jobId', sortable: true },
            { title: 'Cont No', field: 'jobId', sortable: true },
            { title: 'Note', field: 'jobId', sortable: true },
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

