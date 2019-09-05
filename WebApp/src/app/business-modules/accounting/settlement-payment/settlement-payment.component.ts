import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-settlement-payment',
    templateUrl: './settlement-payment.component.html',
})
export class SettlementPaymentComponent extends AppList {

    headers: CommonInterface.IHeaderTable[];
    settlements: any[] = [];
    dataSearch: any = {};
    
    constructor() {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Settlemenent No', field: 'settlementNo' },
            { title: 'Amount', field: 'settlementNo' },
            { title: 'Currency', field: 'settlementNo' },
            { title: 'Request Date', field: 'settlementNo' },
            { title: 'Advance Date', field: 'settlementNo' },
            { title: 'Advance No', field: 'settlementNo' },
            { title: 'Status Approval', field: 'settlementNo' },
            { title: 'Status Payment', field: 'settlementNo' },
            { title: 'Description', field: 'settlementNo' },
        ];
    }
    getRequestAdvancePaymentGroup() {

    }

    deleteAdvancePayment() {

    }


}
