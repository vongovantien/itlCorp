import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-prepaid-payment',
    templateUrl: './prepaid-payment.component.html',
})
export class ARPrePaidPaymentComponent extends AppList implements OnInit {
    debitNotes: any = [];
    constructor() {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Job ID', field: '', sortable: true },
            { title: 'MBL - HBL', field: '', sortable: true },
            { title: 'Partner Name', field: '', sortable: true },
            { title: 'Debit Note', field: '', sortable: true },
            { title: 'Debit Amount', field: '', sortable: true },
            { title: 'PrePaid Amount', field: '', sortable: true },
            { title: 'Currency', field: '', sortable: true },
            { title: 'Salesman', field: '', sortable: true },
            { title: 'AR Confirm', field: '', sortable: true },
        ];
    }
}
