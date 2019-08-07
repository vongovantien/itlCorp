import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'adv-payment-list-request',
    templateUrl: './list-advance-payment-request.component.html',
    styleUrls: ['./list-advance-payment-request.component.scss']
})

export class AdvancePaymentListRequestComponent extends AppList {

    headers: CommonInterface.IHeaderTable[];
    requests: any[] = [];

    constructor() {
        super();
        this.requestList = this.getRequestAdvancePayment;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Description', field: 'description', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'JobID', field: 'JobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];
        this.getRequestAdvancePayment();
    }

    getRequestAdvancePayment() {
        this.requests = [
            { description: 'AAAAAAA', customNo: 'Custom No', jobId: 'XXXX', hbl: 'XXXX', amount: 23452011.3, currency: 'VND', type: 'Approve', note: 'XXXXX' },
            { description: 'AAAAAAA', customNo: 'Custom No', jobId: 'XXXX', hbl: 'XXXX', amount: 2345000, currency: 'VND', type: 'Approve', note: 'XXXXX' },
            { description: 'AAAAAAA', customNo: 'Custom No', jobId: 'XXXX', hbl: 'XXXX', amount: 2345000, currency: 'VND', type: 'Approve', note: 'XXXXX' },
            { description: 'AAAAAAA', customNo: 'Custom No', jobId: 'XXXX', hbl: 'XXXX', amount: 2345000, currency: 'VND', type: 'Approve', note: 'XXXXX' },
            { description: 'AAAAAAA', customNo: 'Custom No', jobId: 'XXXX', hbl: 'XXXX', amount: 2345000, currency: 'VND', type: 'Approve', note: 'XXXXX' },
            { description: 'AAAAAAA', customNo: 'Custom No', jobId: 'XXXX', hbl: 'XXXX', amount: 2345000, currency: 'VND', type: 'Approve', note: 'XXXXX' },
            { description: 'AAAAAAA', customNo: 'Custom No', jobId: 'XXXX', hbl: 'XXXX', amount: 2345000, currency: 'VND', type: 'Approve', note: 'XXXXX' },
            { description: 'AAAAAAA', customNo: 'Custom No', jobId: 'XXXX', hbl: 'XXXX', amount: 2345000, currency: 'VND', type: 'Approve', note: 'XXXXX' },
        ];

    }
}
