import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { AccountReceivablePayableListInvoicePaymentComponent } from './components/list-invoice-payment/list-invoice-account-receivable-payable.component';
import { AccountReceivablePayableListOBHPaymentComponent } from './components/list-obh-payment/list-obh-account-receivable-payable.component';
import { PaymentType } from './components/form-search/form-search-account-receivable-payable.component';
type TAB = 'INVOICE' | 'OBH';

@Component({
    selector: 'app-account-receivable-payable',
    templateUrl: './account-receivable-payable.component.html',
})
export class AccountReceivablePayableComponent extends AppList implements OnInit {

    @ViewChild(AccountReceivablePayableListInvoicePaymentComponent, { static: false }) invoiceListComponent: AccountReceivablePayableListInvoicePaymentComponent;
    @ViewChild(AccountReceivablePayableListOBHPaymentComponent, { static: false }) obhSOAListComponent: AccountReceivablePayableListOBHPaymentComponent;

    selectedTab: TAB | string = "INVOICE";

    constructor(private _ngProgessSerice: NgProgress,
        private _accountingRepo: AccountingRepo) {
        super();
        this._progressRef = this._ngProgessSerice.ref();
    }

    ngOnInit() {
    }

    ngAfterViewInit() {
        this.dataSearch.paymentStatus = ["UnPaid", "Paid A Part"];
        this.invoiceListComponent.dataSearch = this.dataSearch;
        this.obhSOAListComponent.dataSearch = this.dataSearch;

        this.invoiceListComponent.getPagingData();
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
