import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';

import { SortService } from '@services';

import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'detail-account-receivable',
    templateUrl: 'detail-account-receivable.component.html'
})

export class AccountReceivableDetailComponent extends AppList implements OnInit {
    seedData: any[] = [
        {
            office: 'TP.HCM',
            debitAmount: 1500000000,
            billing: 4900000000,
            billingUnpaid: 4800000000,
            paid: 4100000000,
            obhAmount: 4200000000,
            between1_15days: 4400000000,
            between16_30days: 4300000000,
            over30days: 4100000000,
        },
        {
            office: 'TP.HN',
            debitAmount: 1500000000,
            billing: 4900000000,
            billingUnpaid: 4800000000,
            paid: 4100000000,
            obhAmount: 4200000000,
            between1_15days: 4400000000,
            between16_30days: 4300000000,
            over30days: 4100000000,
        },
        {
            office: 'TP.HP',
            debitAmount: 1500000000,
            billing: 4900000000,
            billingUnpaid: 4800000000,
            paid: 4100000000,
            obhAmount: 4200000000,
            between1_15days: 4400000000,
            between16_30days: 4300000000,
            over30days: 4100000000,
        },
        {
            office: 'TP.DN',
            debitAmount: 1500000000,
            billing: 4900000000,
            billingUnpaid: 4800000000,
            paid: 4100000000,
            obhAmount: 4200000000,
            between1_15days: 4400000000,
            between16_30days: 4300000000,
            over30days: 4100000000,
        }
    ];
    subSeedData: any[] = [
        {

            service: 'Paypal',
            creditAmount: 11200000000,
            billing: 1900000000,
            billingUnpaid: 1800000000,
            paid: 1100000000,
            obhAmount: 1200000000,
            between1_15days: 1400000000,
            between16_30days: 1300000000,
            over30days: 1100000000,

        },
        {

            service: 'Momo',
            creditAmount: 11200000000,
            billing: 1900000000,
            billingUnpaid: 1800000000,
            paid: 1100000000,
            obhAmount: 1200000000,
            between1_15days: 1400000000,
            between16_30days: 1300000000,
            over30days: 1100000000,

        },
        {

            service: 'WeScan',
            creditAmount: 11200000000,
            billing: 1900000000,
            billingUnpaid: 1800000000,
            paid: 1100000000,
            obhAmount: 1200000000,
            between1_15days: 1400000000,
            between16_30days: 1300000000,
            over30days: 1100000000,

        }
    ];
    subHeaders: any[];
    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortGuaranteedList;
        this.requestList = this.getPagingGuaranteed;
    }
    ngOnInit() {

        this.headers = [
            { title: 'Office (branch)', field: 'salesNameEn', sortable: true },
            { title: 'Debit Amount', field: 'salesFullName', sortable: true },
            { title: 'Billing', field: 'billing', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paid', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'between1_15days', sortable: true },
            { title: 'Over 16-30 days', field: 'between16_30days', sortable: true },
            { title: 'Over 30 Days', field: 'over30days', sortable: true },

        ];
        this.subHeaders = [
            { title: 'Service', field: 'partnerName', sortable: true },
            { title: 'Credit Amount', field: 'creditAmount', sortable: true },
            { title: 'Billing', field: 'billing', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paid', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'between1_15days', sortable: true },
            { title: 'Over 16-30 days', field: 'between16_30days', sortable: true },
            { title: 'Over 30 Days', field: 'over30days', sortable: true },
        ];
        //seed paging
        this.totalItems = this.seedData.length;
        this.page = 1;
        this.pageSize = 15;
    }
    sortGuaranteedList(sortField: string, order: boolean) {
        this.seedData = this._sortService.sort(this.seedData, sortField, order);
    }

    sortDetailGuaranteed(sortField: string, order: boolean) {
        this.subSeedData = this._sortService.sort(this.subSeedData, sortField, order);
    }

    getPagingGuaranteed() {

    }
}