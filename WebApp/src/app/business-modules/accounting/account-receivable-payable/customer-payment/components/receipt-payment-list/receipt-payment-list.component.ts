import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { TrialOfficialOtherModel } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { SortService } from '@services';
import { catchError, finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { extend } from 'validator';

@Component({
    selector: 'customer-payment-list-receipt',
    templateUrl: './receipt-payment-list.component.html'
})

export class ARCustomerPaymentReceiptPaymentListComponent extends AppList implements OnInit {
  
    trialOfficialList: TrialOfficialOtherModel[] = [];

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
            { title: 'Billing Ref No', field: 'paymentrefno', sortable: true },
            { title: 'Series No', field: 'customername', sortable: true },
            { title: 'Type', field: 'paymentamount', sortable: true },
            { title: 'Partner Nam', field: 'currency', sortable: true },
            { title: 'Taxcode', field: 'billingdate', sortable: true },
            { title: 'Unpaid Amount', field: 'syncstatus', sortable: true },
            { title: 'Currency', field: 'lostsync', sortable: true },
            { title: 'Paid Amount', field: 'description', sortable: true },
            { title: 'Balance Amount', field: 'status', sortable: true },
            { title: 'Reference Amount', field: 'creator', sortable: true },
            { title: 'Ref Curr', field: 'createdate', sortable: true },
            { title: 'Payment Status', field: 'modeifiedate', sortable: true },
            { title: 'Billing Date', field: 'modeifiedate', sortable: true },
            { title: 'Invoice Date', field: 'modeifiedate', sortable: true },
            { title: 'Note', field: 'modeifiedate', sortable: true },
        ];

    }

    sortTrialOfficalList(sortField: string, order: boolean) {
        this.trialOfficialList = this._sortService.sort(this.trialOfficialList, sortField, order);
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
                    this.trialOfficialList = (res.data || []).map((item: TrialOfficialOtherModel) => new TrialOfficialOtherModel(item));
                    this.totalItems = res.totalItems;
                },
            );
    }

    //
    viewDetail(agreementId: string) {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/`], {
            queryParams: {
                agreementId: agreementId,

            }
        });
    }
}
