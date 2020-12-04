import { Component, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { ReportPreviewComponent } from '@common';

@Component({
    selector: 'shipment-item',
    templateUrl: './shipment-item.component.html',
})

export class SettlementShipmentItemComponent extends AppList {
    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;

    @Output() onCheck: EventEmitter<any> = new EventEmitter<any>();
    @Output() onClick: EventEmitter<any> = new EventEmitter<any>();
    @Output() onPrintPlUSD: EventEmitter<any> = new EventEmitter<any>();
    @Output() onPrintPlVND: EventEmitter<any> = new EventEmitter<any>();

    @Input() data: ISettlementShipmentGroup = null;

    headers: CommonInterface.IHeaderTable[];

    initCheckbox: boolean = false;

    constructor(
    ) {
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

    showPaymentManagement($event: Event, data: any): any {
        this.onClick.emit({ event: $event, data: data });
    }

    checkUncheckAllRequest($event: Event) {
        /* 
         * prevent collapse/expand within accordion-head
         */
        $event.stopPropagation();
        $event.preventDefault();

        this.initCheckbox = !this.initCheckbox;
        this.isCheckAll = this.initCheckbox;
        this.onCheck.emit(this.isCheckAll);

        return false;

    }

    previewPLsheet($event, currency: string) {
        $event.stopPropagation();
        $event.preventDefault();
        if (currency === 'VND') {
            this.onPrintPlVND.emit(this.data);
            return;
        }
        this.onPrintPlUSD.emit(this.data);
    }

}

interface ISettlementShipmentGroup {
    advanceAmount: number;
    advanceNo: string;
    balance: number;
    chargeSettlements: any[]
    currencyShipment: string;
    customNo: string;
    hbl: string;
    hblId: string;
    jobId: string;
    mbl: string;
    settlementNo: string;
    shipmentId: string;
    totalAmount: number;
    type: string;
}
