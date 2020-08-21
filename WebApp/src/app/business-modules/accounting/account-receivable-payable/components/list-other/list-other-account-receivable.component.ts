import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';

import { SortService } from '@services';

import { NgProgress } from '@ngx-progressbar/core';





@Component({
    selector: 'list-other-account-receivable',
    templateUrl: './list-other-account-receivable.component.html',
})
export class AccountReceivableListOtherComponent extends AppList implements OnInit {

    //seedData
    seedData: any[] = [
        {
            partnerId: '111111',
            partnerName: 'Cristian',

            debitAmount: 1200000000,

            billing: 900000000,
            billingUnpaid: 800000000,
            paid: 100000000,
            obhAmount: 200000000,

            status: false,
        },
        {
            partnerId: '222222',
            partnerName: 'Maple',

            debitAmount: 1500000000,

            billing: 1000000000,
            billingUnpaid: 900000000,
            paid: 120000000,
            obhAmount: 220000000,

            status: true,
        },
        {
            partnerId: '333333',
            partnerName: 'Maple',

            debitAmount: 1500000000,

            billing: 1000000000,
            billingUnpaid: 900000000,
            paid: 120000000,
            obhAmount: 220000000,

            status: true,
        },
        {
            partnerId: '444444',
            partnerName: 'Maple',

            debitAmount: 1500000000,

            billing: 1000000000,
            billingUnpaid: 900000000,
            paid: 120000000,
            obhAmount: 220000000,

            status: true,
        },
        {
            partnerId: '555555',
            partnerName: 'Maple',

            debitAmount: 1500000000,

            billing: 1000000000,
            billingUnpaid: 900000000,
            paid: 120000000,
            obhAmount: 220000000,

            status: true,
        },
    ];

    constructor(private _sortService: SortService,
        private _progressService: NgProgress,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortOtherList;
        this.requestList = this.getPagingOtherList;
    }
    ngOnInit() {
        this.headers = [
            { title: 'PartnerId', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },

            { title: 'Debit Amount', field: 'debitAmount', sortable: true },

            { title: 'Billing', field: 'billing', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paid', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },

            { title: 'Status', field: 'status', sortable: true },
        ];
        //seed page
        this.totalItems = this.seedData.length;
        this.page = 1;
        this.pageSize = 15;
    }
    sortOtherList(sortField: string, order: boolean) {
        this.seedData = this._sortService.sort(this.seedData, sortField, order);
    }
    getPagingOtherList() {

    }
}