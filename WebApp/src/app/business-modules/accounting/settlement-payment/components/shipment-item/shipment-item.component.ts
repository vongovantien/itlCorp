import { Component, ViewChild, ContentChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { SettlementPaymentManagementPopupComponent } from '../popup/payment-management/payment-management.popup';

@Component({
    selector: 'shipment-item',
    templateUrl: './shipment-item.component.html'
})

export class SettlementShipmentItemComponent extends AppList {

    @ViewChild(SettlementPaymentManagementPopupComponent, { static: false }) paymentManagementPopup: SettlementPaymentManagementPopupComponent;
    @ContentChild("data", { static: false }) table: any;

    headers: CommonInterface.IHeaderTable[];

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

    showPaymentManagement($event: Event): boolean {
        // * prevent collapse/expand within accordion-heading
        $event.stopPropagation();
        $event.preventDefault();

        this.paymentManagementPopup.show();
        return false;
    }

    checkUncheckAllRequest() {

    }
}
