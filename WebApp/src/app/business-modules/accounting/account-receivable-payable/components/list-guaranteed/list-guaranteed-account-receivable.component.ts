import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';

import { SortService } from '@services';

import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { GuaranteedModel } from '@models';





@Component({
    selector: 'list-guaranteed-account-receivable',
    templateUrl: './list-guaranteed-account-receivable.component.html',
})
export class AccountReceivableListGuaranteedComponent extends AppList implements OnInit {
    guaranteedList: any[] = [];

    subHeaders: any[];
    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _accountingRepo: AccountingRepo,
        private _router: Router,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortGuaranteedList;
        this.requestList = this.getPagingList;
    }
    ngOnInit() {

        this.headers = [
            { title: 'Sales Name (En)', field: 'salesmanNameEn', sortable: true },
            { title: 'Sales Full Name', field: 'salesmanFullName', sortable: true },
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

            { title: 'Status', field: 'agreementStatus', sortable: true },
        ];


    }
    sortGuaranteedList(sortField: string, order: boolean) {
        this.guaranteedList = this._sortService.sort(this.guaranteedList, sortField, order);
    }

    sortDetailGuaranteed(item: any, sortField: string, order: boolean) {
        item.arPartners = this._sortService.sort(item.arPartners, sortField, order);
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
                    this.guaranteedList = (res.data || []).map((item: GuaranteedModel) => new GuaranteedModel(item));
                    this.totalItems = res.totalItems;
                },
            );
    }
    //
    viewDetail(agreementId: string) {
        this._router.navigate([`/home/accounting/account-receivable-payable/receivable/detail`], {
            queryParams: {
                agreementId: agreementId,
                subTab: 'guaranteed',
            }
        });
    }
}