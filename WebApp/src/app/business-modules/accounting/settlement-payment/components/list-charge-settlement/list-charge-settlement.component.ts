import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'settle-payment-list-charge',
    templateUrl: './list-charge-settlement.component.html'
})

export class SettlementListChargeComponent extends AppList {
    headers: CommonInterface.IHeaderTable[];
    items: any[] = [1, 2, 3, 4, 5];
    constructor() {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Job', field: 'jobId', sortable: true },
            { title: 'HBL', field: 'jobId', sortable: true },
            { title: 'MBL', field: 'jobId', sortable: true },
            { title: 'Total Amount', field: 'jobId', sortable: true },
            { title: 'Job', field: 'jobId', sortable: true },
            { title: 'Job', field: 'jobId', sortable: true },
        ]
    }

    checkUncheckAllRequest() {

    }
}