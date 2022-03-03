import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingRepo, ExportRepo } from '@repositories';
import { SortService } from '@services';
import { catchError, finalize, map, takeUntil, withLatestFrom } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';

import { TrialOfficialOtherModel } from '@models';
import { RoutingConstants, SystemConstants } from '@constants';
import { getAccountReceivableListState, getAccountReceivablePagingState, getAccountReceivableSearchState, IAccountReceivableState, getAccountReceivableLoadingListState } from '../../account-receivable/store/reducers';
import { Store } from '@ngrx/store';
import { getMenuUserSpecialPermissionState } from '@store';
import { AccReceivableDebitDetailPopUpComponent } from '../popup/account-receivable-debit-detail-popup.component';
import { LoadListAccountReceivable } from '../../account-receivable/store/actions';
import { ToastrService } from 'ngx-toastr';
import { HttpResponse } from '@angular/common/http';

@Component({
    selector: 'list-trial-official-account-receivable',
    templateUrl: './list-trial-official-account-receivable.component.html',
    styleUrls: ['./list-trial-official-account-receivable.component.scss'],
})
export class AccountReceivableListTrialOfficialComponent extends AppList implements OnInit {
    @ViewChild(AccReceivableDebitDetailPopUpComponent) debitDetailPopupComponent: AccReceivableDebitDetailPopUpComponent;

    trialOfficialList: TrialOfficialOtherModel[] = [];

    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _store: Store<IAccountReceivableState>,
        private _exportRepo: ExportRepo,
        private _toastService: ToastrService,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortTrialOfficalList;
        this.requestList = this.getPagingList;
    }
    ngOnInit() {

        this.headers = [
            { title: 'Billing', field: 'billingAmount', sortable: true },
            { title: 'Paid a Part', field: 'paidAmount', sortable: true },
            { title: 'OutStanding Balance', field: 'billingUnpaid', sortable: true },
            // { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'over1To15Day', sortable: true },
            { title: 'Over 16-30 days', field: 'over16To30Day', sortable: true },
            { title: 'Over 30 days', field: 'over30Day', sortable: true },
            { title: 'Currency', field: 'agreementCurrency', sortable: true },
            { title: 'Debit Amount', field: 'debitAmount', sortable: true },
            { title: 'Credit Limited', field: 'creditLimited', sortable: true },
            { title: 'Over Credit Amount', field: 'overAmount', sortable: true },
            { title: 'Salesman', field: 'agreementSalesmanName', sortable: true },
            { title: 'Contract No', field: 'agreementNo', sortable: true },
            { title: 'Expired Date', field: 'expriedDate', sortable: true },
            { title: 'Expired Days', field: 'expriedDay', sortable: true },
            { title: 'Parent Partner', field: 'partnerNameAbbr', sortable: true },
        ];

        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this._store.select(getAccountReceivableSearchState)
            .pipe(
                withLatestFrom(this._store.select(getAccountReceivablePagingState)),
                takeUntil(this.ngUnsubscribe),
                map(([dataSearch, pagingData]) => ({ page: pagingData.page, pageSize: pagingData.pageSize, dataSearch: dataSearch }))
            )
            .subscribe(
                (data) => {
                    this.dataSearch = data.dataSearch;
                    this.page = data.page;
                    this.pageSize = data.pageSize;
                }
            );

        this.isLoading = this._store.select(getAccountReceivableLoadingListState);

    }

    sortTrialOfficalList(sortField: string, order: boolean) {
        this.trialOfficialList = this._sortService.sort(this.trialOfficialList, sortField, order);
    }

    getPagingList() {
        this._store.dispatch(LoadListAccountReceivable({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
        this._store.select(getAccountReceivableListState)
            .pipe(
                catchError(this.catchError),
                map((store: any) => { return store })
            ).subscribe(
                (res: any) => {
                    this.trialOfficialList = res.data || [];
                    this.totalItems = res.totalItems;
                },
            );
    }

    //
    viewDetail(agreementId: string, partnerId: string) {
        if (!!agreementId) {
            this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/summary/detail`], {
                queryParams: {
                    agreementId: agreementId,
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

    exportExcel() {
        if (!this.trialOfficialList.length) {
            this._toastService.warning('No Data To View, Please Re-Apply Filter');
            return;
        } else {
            this._exportRepo.exportAccountingReceivableArSumary(this.dataSearch)
                .subscribe(
                    (res: HttpResponse<any>) => {
                        this.downLoadFile(res.body, SystemConstants.FILE_EXCEL, res.headers.get('efms-file-name'));
                    }
                );
        }
    }

    showDebitDetail(agreementId, option) {
        let offi = "";
        let overDueDay = 0;
        if(this.dataSearch && this.dataSearch.officeIds){offi = this.dataSearch.officeIds.join("|");}
        if(this.dataSearch && this.dataSearch.overDueDay){overDueDay = this.dataSearch.overDueDay;}
        this._accountingRepo.getDataDebitDetailList(agreementId, option,offi,'',overDueDay)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.debitDetailPopupComponent.dataDebitList = res || [];
                        this.debitDetailPopupComponent.calculateTotal();
                        this.debitDetailPopupComponent.show();
                    }
                },
            );
    }
}

