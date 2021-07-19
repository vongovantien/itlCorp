import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingRepo } from '@repositories';
import { SortService } from '@services';
import { catchError, map } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';

import { TrialOfficialOtherModel } from '@models';
import { RoutingConstants } from '@constants';
import { getAccountReceivableListState, IAccountReceivableState } from '../../account-receivable/store/reducers';
import { Store } from '@ngrx/store';

@Component({
    selector: 'list-trial-official-account-receivable',
    templateUrl: './list-trial-official-account-receivable.component.html',
})
export class AccountReceivableListTrialOfficialComponent extends AppList implements OnInit {

    trialOfficialList: TrialOfficialOtherModel[] = [];

    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _store:Store<IAccountReceivableState>
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortTrialOfficalList;
        this.requestList = this.getPagingList;
    }
    ngOnInit() {

        this.headers = [
            { title: 'Partner Id', field: 'partnerCode', sortable: true },
            { title: 'Partner Name', field: 'partnerNameAbbr', sortable: true },
            { title: 'Debit Amount', field: 'debitAmount', sortable: true },
            { title: 'Debit Rate (%)', field: 'debitRate', sortable: true },
            { title: 'Billing (Unpaid)', field: 'billingAmount', sortable: true },
            { title: 'Paid a Part', field: 'paidAmount', sortable: true },
            { title: 'OutStanding Balance', field: 'billingUnpaid', sortable: true },
            // { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'over1To15Day', sortable: true },
            { title: 'Over 16-30 days', field: 'over16To30Day', sortable: true },
            { title: 'Over 30 days', field: 'over30Day', sortable: true },
            { title: 'Currency', field: 'creditCurrency', sortable: true },
            { title: 'Credit Limited', field: 'creditLimited', sortable: true },
            { title: 'Contract No', field: 'agreementNo', sortable: true },
            { title: 'Expired Date', field: 'expriedDate', sortable: true },
            { title: 'Expired Days', field: 'expriedDay', sortable: true },
            { title: 'Parent Partner', field: 'partnerNameAbbr', sortable: true },
        ];

    }

    sortTrialOfficalList(sortField: string, order: boolean) {
        this.trialOfficialList = this._sortService.sort(this.trialOfficialList, sortField, order);
    }

    getPagingList() {
        this._store.select(getAccountReceivableListState)
        .pipe(
            catchError(this.catchError),
            map((data: any) => {
                return {
                    data: !!data.data ? data.data.map((item: any) => new TrialOfficialOtherModel(item)) : [],
                    totalItems: data.totalItems,
                };
            })
        ).subscribe(
            (res: any) => {
                    this.trialOfficialList = res.data || [];
                    this.totalItems = res.totalItems;
            },
        );
    }

    //
    viewDetail(agreementId: string) {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/summary/detail`], {
            queryParams: {
                agreementId: agreementId,

            }
        });
    }
}

