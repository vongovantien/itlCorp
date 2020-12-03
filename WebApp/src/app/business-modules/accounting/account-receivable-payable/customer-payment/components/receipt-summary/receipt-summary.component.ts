import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { TrialOfficialOtherModel } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { SortService } from '@services';
import { catchError, finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'customer-payment-receipt-summary',
    templateUrl: './receipt-summary.component.html',
})
export class ARCustomerPaymentReceiptSummaryComponent extends AppList implements OnInit {
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
            { title: 'Total Summary', field: 'paymentrefno', sortable: true },
            { title: 'Total Debit', field: 'customername', sortable: true },
            { title: 'Total OBH', field: 'paymentamount', sortable: true },
            { title: 'Total ADV', field: 'currency', sortable: true },
            { title: 'Total Credit', field: 'billingdate', sortable: true },
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

