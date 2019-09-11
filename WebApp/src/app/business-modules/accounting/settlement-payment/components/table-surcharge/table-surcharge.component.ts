import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'table-surcharge-settlement',
    templateUrl: './table-surcharge.component.html'
})

export class SettlementTableSurchargeComponent  extends AppList {

    headers: CommonInterface.IHeaderTable[];

    constructor() {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Name', field: 'jobId', sortable: true },
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

}