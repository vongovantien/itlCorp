import { Component, OnInit } from '@angular/core';
import { AppList } from '@app';
import { catchError, finalize } from 'rxjs/operators';
import { SortService } from '@services';

import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { Router } from '@angular/router';
import { TrialOfficialOtherModel } from '@models';
import { RoutingConstants } from '@constants';

@Component({
    selector: 'list-other-account-receivable',
    templateUrl: './list-other-account-receivable.component.html',
})
export class AccountReceivableListOtherComponent extends AppList implements OnInit {

    //
    otherList: any[] = [];

    constructor(private _sortService: SortService,
        private _progressService: NgProgress,
        private _accountingRepo: AccountingRepo,
        private _router: Router,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortOtherList;
        this.requestList = this.getPagingList;
    }
    ngOnInit() {
        this.headers = [
            { title: 'Partner Id', field: 'partnerCode', sortable: true },
            { title: 'Partner Name', field: 'partnerNameAbbr', sortable: true },

            { title: 'Debit Amount', field: 'debitAmount', sortable: true },

            { title: 'Billing', field: 'billingAmount', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paidAmount', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },

            { title: 'Status', field: 'agreementStatus', sortable: true },
        ];

    }

    sortOtherList(sortField: string, order: boolean) {
        this.otherList = this._sortService.sort(this.otherList, sortField, order);
    }

    getPagingList() {
        this._progressRef.start();
        this.isLoading = true;

        this._accountingRepo.receivablePaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.isLoading = false;
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.otherList = (res.data || []).map((item: TrialOfficialOtherModel) => new TrialOfficialOtherModel(item));
                    this.totalItems = res.totalItems;
                },
            );
    }
    viewDetail(agreementId: string, partnerId: string) {

        if (!!agreementId) {
            this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/summary/detail`], {
                queryParams: {
                    agreementId: agreementId,
                    subTab: 'other',
                }
            });
        } else {
            this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/summary/detail`], {
                queryParams: {
                    partnerId: partnerId,
                    subTab: 'other',
                }
            });
        }

    }
}