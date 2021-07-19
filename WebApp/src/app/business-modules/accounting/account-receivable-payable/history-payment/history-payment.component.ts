import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize, map, takeUntil, withLatestFrom } from 'rxjs/operators';

import { Router, ActivatedRoute } from '@angular/router';
import { RoutingConstants } from '@constants';
import { ARHistoryPaymentListInvoiceComponent } from './components/list-invoice-payment/list-invoice-history-payment.component';
import { Store } from '@ngrx/store';
import { getDataSearchHistoryPaymentState, getHistoryPaymentListState, getHistoryPaymentPagingState, IHistoryPaymentState } from './store/reducers';
import { LoadListHistoryPayment, LoadListHistoryPaymentSuccess } from './store/actions';
import { AccountingPaymentModel } from '@models';


type TAB = 'HISTORY' | 'OBH';

enum PAYMENT_TAB {
    CUSTOMER = 'CUSTOMER',
    AGENCY = 'AGENCY',
    ARSUMMARY = 'ARSUMMARY',
    HISTORY = 'HISTORY'

}
@Component({
    selector: 'app-history-payment',
    templateUrl: './history-payment.component.html',

})

export class ARHistoryPaymentComponent extends AppList implements OnInit {

    @ViewChild(ARHistoryPaymentListInvoiceComponent) invoiceListComponent: ARHistoryPaymentListInvoiceComponent;

    //selectedTab: TAB | string = "HISTORY";
    selectedTabAR: string = 'payment';

    isAccountPaymentTab: boolean = true;
    selectedTab: string = PAYMENT_TAB.CUSTOMER;

    constructor(
        private _router: Router,
        private _activeRouter: ActivatedRoute,
        private _ngProgessSerice: NgProgress,
        private _cd: ChangeDetectorRef,
        private _accountingRepo: AccountingRepo,
        private _store: Store<IHistoryPaymentState>) {
        super();
        this._progressRef = this._ngProgessSerice.ref();
        this.requestList = this.requestLoadListHistoryPayment;
    }

    ngOnInit() {

        this._activeRouter.queryParams
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(param => {
                if (param.tab) {
                    this.selectedTabAR = param.tab;
                }
            });

        this.requestSearchShipment();

        this._store.select(getDataSearchHistoryPaymentState)
            .pipe(
                withLatestFrom(this._store.select(getHistoryPaymentPagingState)),
                takeUntil(this.ngUnsubscribe),
                map(([dataSearch, pagingData]) => ({ page: pagingData.page, pageSize: pagingData.pageSize, dataSearch: dataSearch }))
            )
            .subscribe(
                (data) => {
                    if (!!data.dataSearch) {
                        this.dataSearch = data.dataSearch;
                    }

                    this.page = data.page;
                    this.pageSize = data.pageSize;

                    this.requestLoadListHistoryPayment();
                }
            );
    }

    requestLoadListHistoryPayment() {
        this._store.dispatch(LoadListHistoryPayment({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
    }

    ngAfterViewInit() {
        this.dataSearch.paymentStatus = ["UnPaid", "Paid A Part"];
        this.invoiceListComponent.dataSearch = this.dataSearch;

        this.invoiceListComponent.getPagingData();
        this._cd.detectChanges();

    }
    changeTabAccount(tab: string) {
        this.selectedTabAR = tab;

        if (tab === 'payment') {
            this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}`]);
            this.selectedTab = 'HISTORY';
            this.dataSearch.paymentStatus = ["UnPaid", "Paid A Part"];
        } else if (tab === 'receivable') {
            this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/receivable`]);

        } else if (tab === 'customer-payment') {
            //// huy
            this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/customer-payment`]);
        }

    }

    getPaymentType() {
        let paymentType: number;
        if (this.selectedTab === "HISTORY") {
            paymentType = 0;
        }
        return paymentType;
    }

    onSearchPayment(event) {
        debugger
        this.dataSearch = event;
        this.dataSearch.paymentType = this.getPaymentType();
        if (this.dataSearch.paymentType === 0) {
            this.invoiceListComponent.dataSearch = this.dataSearch;
            this.requestSearchShipment();
        }
    }

    requestSearchShipment() {
        debugger
        // this._progressRef.start();

        // this._accountingRepo.paymentPaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
        //     .pipe(
        //         catchError(this.catchError),
        //         finalize(() => {
        //             this._progressRef.complete();
        //         })
        //     ).subscribe(
        //         (res: CommonInterface.IResponsePaging) => {
        //             if (this.selectedTab === "HISTORY") {
        //                 this.invoiceListComponent.refPaymens = res.data || [];
        //                 this.invoiceListComponent.totalItems = res.totalItems;
        //             }
        //         },
        //     );

        this._store.select(getHistoryPaymentListState)
            .pipe(
                catchError(this.catchError),
                map((data: any) => {
                    return {
                        data: !!data.data ? data.data.map((item: any) => new AccountingPaymentModel(item)) : [],
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    if (this.selectedTab === "HISTORY") {
                        this.invoiceListComponent.refPaymens = res.data || [];
                        this.invoiceListComponent.totalItems = res.totalItems;
                    }
                },
            );
    }

    handleUpdateExtendDateOfInvoice() {
        this.requestSearchShipment();
    }

    onSelectTab(tabName: PAYMENT_TAB | string) {
        switch (tabName) {
            case 'PAYMENT':
                this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}`]);
                break;
            case 'AR':
                this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/summary`]);
                break;
            default:
                break;
        }
        this.selectedTab = tabName;
    }
}
