import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';

import { SortService } from '@services';

import { NgProgress } from '@ngx-progressbar/core';





@Component({
    selector: 'list-guaranteed-account-receivable',
    templateUrl: './list-guaranteed-account-receivable.component.html',
})
export class AccountReceivableListGuaranteedComponent extends AppList implements OnInit {
    seedData: any[] = [
        {
            salesNameEn: 'Alex Phuong',
            salesFullName: 'Chị Phương',
            creditLimited: 1000000000,
            creditAmount: 1200000000,
            creditRate: 50,
            billing: 1900000000,
            billingUnpaid: 1800000000,
            paid: 1100000000,
            obhAmount: 1200000000,
            between1_15days: 1400000000,
            between16_30days: 1300000000,
            over30days: 1100000000,
        },
        {
            salesNameEn: 'Kenny Thuong',
            salesFullName: 'Anh Thương',
            creditLimited: 2000000000,
            creditAmount: 1300000000,
            creditRate: 60,
            billing: 2900000000,
            billingUnpaid: 2800000000,
            paid: 2100000000,
            obhAmount: 2200000000,
            between1_15days: 2400000000,
            between16_30days: 2300000000,
            over30days: 2100000000,
        },
        {
            salesNameEn: 'Samuel An',
            salesFullName: 'Anh Ân',
            creditLimited: 3000000000,
            creditAmount: 1400000000,
            creditRate: 140,
            billing: 3900000000,
            billingUnpaid: 3800000000,
            paid: 3100000000,
            obhAmount: 3200000000,
            between1_15days: 3400000000,
            between16_30days: 3300000000,
            over30days: 3100000000,
        },
        {
            salesNameEn: 'Andy Hoa',
            salesFullName: 'Anh Hóa',
            creditLimited: 4000000000,
            creditAmount: 1500000000,
            creditRate: 120,
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
            partnerId: '09455656',
            partnerName: 'Paypal',
            creditAmount: 1200000000,
            creditRate: 120,
            billing: 900000000,
            billingUnpaid: 800000000,
            paid: 100000000,
            obhAmount: 200000000,
            between1_15days: 400000000,
            between16_30days: 300000000,
            over30days: 100000000,
            status: false,
        },
        {
            partnerId: '0945565643',
            partnerName: 'Momo',
            creditAmount: 21200000000,
            creditRate: 20,
            billing: 2900000000,
            billingUnpaid: 2800000000,
            paid: 2100000000,
            obhAmount: 2200000000,
            between1_15days: 2400000000,
            between16_30days: 2300000000,
            over30days: 2100000000,
            status: true,
        },
        {
            partnerId: '094556599',
            partnerName: 'WeScan',
            creditAmount: 11200000000,
            creditRate: 80,
            billing: 1900000000,
            billingUnpaid: 1800000000,
            paid: 1100000000,
            obhAmount: 1200000000,
            between1_15days: 1400000000,
            between16_30days: 1300000000,
            over30days: 1100000000,
            status: false,
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
            { title: 'Sales Name (En)', field: 'salesNameEn', sortable: true },
            { title: 'Sales Full Name', field: 'salesFullName', sortable: true },
            { title: 'Credit Limited', field: 'creditLimited', sortable: true },
            { title: 'Credit Amount', field: 'creditAmount', sortable: true },
            { title: 'Credit Rate (%)', field: 'creditRate', sortable: true },
            { title: 'Billing', field: 'billing', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paid', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'between1_15days', sortable: true },
            { title: 'Over 16-30 days', field: 'between16_30days', sortable: true },
            { title: 'Over 30 Days', field: 'over30days', sortable: true },

        ];
        this.subHeaders = [
            { title: 'PartnerId', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },

            { title: 'Credit Amount', field: 'creditAmount', sortable: true },
            { title: 'Credit Rate (%)', field: 'creditRate', sortable: true },
            { title: 'Billing', field: 'billing', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paid', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'between1_15days', sortable: true },
            { title: 'Over 16-30 days', field: 'between16_30days', sortable: true },
            { title: 'Over 30 Days', field: 'over30days', sortable: true },

            { title: 'Status', field: 'status', sortable: true },
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