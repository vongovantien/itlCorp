import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';

import { SortService } from '@services';

import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';





@Component({
    selector: 'list-guaranteed-account-receivable',
    templateUrl: './list-guaranteed-account-receivable.component.html',
})
export class AccountReceivableListGuaranteedComponent extends AppList implements OnInit {
    guaranteedList: any[] = [];
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
        private _accountingRepo: AccountingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortGuaranteedList;
        this.requestList = this.getPagingList;
    }
    ngOnInit() {

        this.headers = [
            { title: 'Sales Name (En)', field: 'salesmanNameEn', sortable: true },
            { title: 'Sales Full Name', field: 'salesFullName', sortable: true },
            { title: 'Credit Limited', field: 'totalCreditLimited', sortable: true },
            { title: 'Debit Amount', field: 'totalDebitAmount', sortable: true },
            { title: 'Debit Rate (%)', field: 'totalDebitRate', sortable: true },
            { title: 'Billing', field: 'totalBillingAmount', sortable: true },
            { title: 'Billing Unpaid', field: 'totalBillingUnpaid', sortable: true },
            { title: 'Paid', field: 'totalPaidAmount', sortable: true },
            { title: 'OBH Amount', field: 'totalObhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'totalOver1To15Day', sortable: true },
            { title: 'Over 16-30 days', field: 'totalOver16To30Day', sortable: true },
            { title: 'Over 30 Days', field: 'totalOver30Day', sortable: true },

        ];
        this.subHeaders = [
            { title: 'Partner Id', field: 'partnerCode', sortable: true },
            { title: 'Partner Name', field: 'partnerNameAbbr', sortable: true },

            { title: 'Debit Amount', field: 'debitAmount', sortable: true },

            { title: 'Billing', field: 'billingAmount', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paidAmount', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'over1To15Day', sortable: true },
            { title: 'Over 16-30 days', field: 'over16To30Day', sortable: true },
            { title: 'Over 30 Days', field: 'over30Day', sortable: true },

            { title: 'Status', field: 'partnerStatus', sortable: true },
        ];
        //seed paging

    }
    sortGuaranteedList(sortField: string, order: boolean) {
        this.guaranteedList = this._sortService.sort(this.guaranteedList, sortField, order);
    }

    sortDetailGuaranteed(sortField: string, order: boolean) {
        this.subSeedData = this._sortService.sort(this.subSeedData, sortField, order);
    }

    getPagingList() {
        this._progressRef.start();
        this.isLoading = true;
        //call api
        this._accountingRepo.receivablePaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.isLoading = false;
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.guaranteedList = res.data || [];
                    console.log("data trả về: ", res.data);

                    this.totalItems = res.totalItems;
                },
            );
    }
}