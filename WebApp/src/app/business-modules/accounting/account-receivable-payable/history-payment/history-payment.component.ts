import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize, takeUntil } from 'rxjs/operators';

import { Router, ActivatedRoute } from '@angular/router';
import { RoutingConstants } from '@constants';
import { ARHistoryPaymentListInvoiceComponent } from './components/list-invoice-payment/list-invoice-history-payment.component';


type TAB = 'INVOICE' | 'OBH';


@Component({
    selector: 'app-history-payment',
    templateUrl: './history-payment.component.html',

})
export class ARHistoryPaymentComponent extends AppList implements OnInit {

    @ViewChild(ARHistoryPaymentListInvoiceComponent) invoiceListComponent: ARHistoryPaymentListInvoiceComponent;

    selectedTab: TAB | string = "INVOICE";
    selectedTabAR: string = 'payment';

    isAccountPaymentTab: boolean = true;

    constructor(
        private _router: Router,
        private _activeRouter: ActivatedRoute,
        private _ngProgessSerice: NgProgress,
        private _cd: ChangeDetectorRef,
        private _accountingRepo: AccountingRepo) {
        super();
        this._progressRef = this._ngProgessSerice.ref();
    }

    ngOnInit() {

        this._activeRouter.queryParams
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(param => {
                if (param.tab) {
                    this.selectedTabAR = param.tab;
                }
            });
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
            this.selectedTab = 'INVOICE';
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
        if (this.selectedTab === "INVOICE") {
            paymentType = 0;
        }
        return paymentType;
    }

    onSearchPayment(event) {
        this.dataSearch = event;
        this.dataSearch.paymentType = this.getPaymentType();
        if (this.dataSearch.paymentType === 0) {
            this.invoiceListComponent.dataSearch = this.dataSearch;
            this.requestSearchShipment();
        }
    }

    requestSearchShipment() {
        this._progressRef.start();

        this._accountingRepo.paymentPaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    if (this.selectedTab === "INVOICE") {
                        this.invoiceListComponent.refPaymens = res.data || [];
                        this.invoiceListComponent.totalItems = res.totalItems;
                    }
                },
            );
    }

    handleUpdateExtendDateOfInvoice() {
        this.requestSearchShipment();
    }

    onSelectTab(tab: string) {
        switch (tab) {
            case 'customer':
                break;
            case 'agency':
                break;
            case 'summary':
                break;
            case 'history':
                break;
            default:
                break;
        }
    }
}
