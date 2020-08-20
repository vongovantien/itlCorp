import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { AccountPaymentListInvoicePaymentComponent } from './components/list-invoice-payment/list-invoice-account-payment.component';
import { AccountPaymentListOBHPaymentComponent } from './components/list-obh-payment/list-obh-account-payment.component';
import { PaymentType } from './components/form-search/account-payment/form-search-account-payment.component';
import { AccountReceivableListTrialOfficialComponent } from './components/list-trial-official/list-trial-official-account-receivable.component';
import { AccountReceivableListGuaranteedComponent } from './components/list-guaranteed/list-guaranteed-account-receivable.component';
import { AccountReceivableListOtherComponent } from './components/list-other/list-other-account-receivable.component';
import { AccountReceivableFormSearchComponent } from './components/form-search/account-receivable/form-search-account-receivable.component';
import { CommonEnum } from '@enums';
import { Router, ActivatedRoute } from '@angular/router';
type TAB = 'INVOICE' | 'OBH';


@Component({
    selector: 'app-account-receivable-payable',
    templateUrl: './account-receivable-payable.component.html',

})
export class AccountReceivablePayableComponent extends AppList implements OnInit {

    @ViewChild(AccountPaymentListInvoicePaymentComponent, { static: false }) invoiceListComponent: AccountPaymentListInvoicePaymentComponent;
    @ViewChild(AccountPaymentListOBHPaymentComponent, { static: false }) obhSOAListComponent: AccountPaymentListOBHPaymentComponent;


    selectedTab: TAB | string = "INVOICE";

    //selectedTab:
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

        this._activeRouter.queryParams.subscribe(param => {
            if (param.tab === 'receivable') {
                this.isAccountPaymentTab = false;
            }
            else {
                this.isAccountPaymentTab = true;
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
    //when selected tab
    changeTabAccount(tab: string) {
        if (tab === 'Payment') {
            this._router.navigate(['/home/accounting/account-receivable-payable'], { queryParams: { tab: 'payment' } });
            this.isAccountPaymentTab = true;
            this.selectedTab = 'INVOICE';
            this.dataSearch.paymentStatus = ["UnPaid", "Paid A Part"];


        } else {
            this._router.navigate(['/home/accounting/account-receivable-payable'], {
                queryParams:
                {
                    tab: 'receivable'
                }
            });
            this.isAccountPaymentTab = false;
            this.selectedTab = 'TRIAL_OFFICAL';
            this.dataSearch = {};

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
        // this.invoiceListComponent.isLoading = true;
        this._accountingRepo.paymentPaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    // this.invoiceListComponent.isLoading = false;

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
