import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingRepo, ExportRepo } from '@repositories';
import { SortService } from '@services';
import { Store } from '@ngrx/store';
import { getMenuUserSpecialPermissionState, IAppState } from '@store';
import { SystemConstants } from 'src/constants/system.const';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { ConfirmPopupComponent, InfoPopupComponent } from '@common';


import { PaymentModel, AccountingPaymentModel } from '@models';



@Component({
    selector: 'list-trial-official-account-receivable',
    templateUrl: './list-trial-official-account-receivable.component.html',
})
export class AccountReceivableListTrialOfficialComponent extends AppList implements OnInit {
    //
    trialOfficialList: any[] = [];



    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _router: Router,
        private _accountingRepo: AccountingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortTrialOfficalList;
        this.requestList = this.getPagingList;
    }
    ngOnInit() {

        this.headers = [
            { title: 'Partner Id', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerNameAbbr', sortable: true },
            { title: 'Credit Limited', field: 'creditLimited', sortable: true },
            { title: 'Debit Amount', field: 'debitAmount', sortable: true },
            { title: 'Debit Rate (%)', field: 'debitRate', sortable: true },
            { title: 'Billing', field: 'billingAmount', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paidAmount', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'over1To15Day', sortable: true },
            { title: 'Over 16-30 days', field: 'over16To30Day', sortable: true },
            { title: 'Over 30 days', field: 'over30Day', sortable: true },
            { title: 'Contract No', field: 'agreementNo', sortable: true },
            { title: 'Contract Type', field: 'agreementType', sortable: true },
            { title: 'Expired Date', field: 'expriedDate', sortable: true },
            { title: 'Expired Days', field: 'expriedDay', sortable: true },
            { title: 'Status', field: 'agreementStatus', sortable: true },
        ];

    }

    sortTrialOfficalList(sortField: string, order: boolean) {
        this.trialOfficialList = this._sortService.sort(this.trialOfficialList, sortField, order);
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
                    this.trialOfficialList = res.data || [];
                    console.log("data trả về: ", res.data);

                    this.totalItems = res.totalItems;
                },
            );
    }

    //
    viewDetail(id: string) {
        this._router.navigate([`/home/accounting/account-receivable-payable/detail/${id}`]);
    }
}

