import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize, takeUntil } from 'rxjs/operators';
import { AccountPaymentListInvoicePaymentComponent } from './components/list-invoice-payment/list-invoice-account-payment.component';
import { AccountPaymentListOBHPaymentComponent } from './components/list-obh-payment/list-obh-account-payment.component';
import { PaymentType } from './components/form-search/account-payment/form-search-account-payment.component';

import { Router, ActivatedRoute } from '@angular/router';
import { RoutingConstants } from '@constants';


type TAB = 'INVOICE' | 'OBH';


@Component({
    selector: 'app-account-receivable-payable',
    templateUrl: './account-receivable-payable.component.html',

})
export class AccountReceivablePayableComponent extends AppList implements OnInit {

    @ViewChild(AccountPaymentListInvoicePaymentComponent, { static: false }) invoiceListComponent: AccountPaymentListInvoicePaymentComponent;
    @ViewChild(AccountPaymentListOBHPaymentComponent, { static: false }) obhSOAListComponent: AccountPaymentListOBHPaymentComponent;

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
        this.obhSOAListComponent.dataSearch = this.dataSearch;

        this.invoiceListComponent.getPagingData();
        this._cd.detectChanges();

    }
    changeTabAccount(tab: string) {
        this.selectedTabAR = tab;

        if (tab === 'payment') {
            this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}`]);
            this.selectedTab = 'INVOICE';
            this.dataSearch.paymentStatus = ["UnPaid", "Paid A Part"];
        } else {
            this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/receivable`]);

        }
    }

    onSelectTabLocation(tabname: string) {
        this.selectedTab = tabname;
        this.dataSearch.paymentType = this.getPaymentType();

        if (tabname === 'OBH') {
            this.obhSOAListComponent.dataSearch = this.dataSearch;
        } else {
            this.invoiceListComponent.dataSearch = this.dataSearch;
        }
        this.requestSearchShipment();
    }

    getPaymentType() {
        let paymentType: number;
        if (this.selectedTab === "INVOICE") {
            paymentType = PaymentType.Invoice;
        } else {
            paymentType = PaymentType.OBH;
        }
        return paymentType;
    }

    onSearchPayment(event) {
        this.dataSearch = event;
        this.dataSearch.paymentType = this.getPaymentType();
        if (this.dataSearch.paymentType === 0) {
            this.invoiceListComponent.dataSearch = this.dataSearch;
            this.requestSearchShipment();
        } else {
            this.obhSOAListComponent.dataSearch = this.dataSearch;
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

                    } else {
                        this.obhSOAListComponent.refPaymens = res.data || [];
                        this.obhSOAListComponent.totalItems = res.totalItems;
                    }
                },
            );
    }

    handleUpdateExtendDateOfInvoice() {
        this.requestSearchShipment();
    }

    handleUpdateExtendDateOfOBH() {
        this.requestSearchShipment();
    }
}
