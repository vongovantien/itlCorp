import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { AccountReceivablePayableListInvoicePaymentComponent } from './components/list-invoice-payment/list-invoice-account-receivable-payable.component';
import { AccountReceivablePayableListOBHPaymentComponent } from './components/list-obh-payment/list-obh-account-receivable-payable.component';

@Component({
    selector: 'app-account-receivable-payable',
    templateUrl: './account-receivable-payable.component.html',
})
export class AccountReceivablePayableComponent extends AppList implements OnInit {
    @ViewChild(AccountReceivablePayableListInvoicePaymentComponent, { static: false }) invoiceListComponent: AccountReceivablePayableListInvoicePaymentComponent;
    @ViewChild(AccountReceivablePayableListOBHPaymentComponent, { static: false }) obhSOAListComponent: AccountReceivablePayableListOBHPaymentComponent;
    selectedTab: string = "INVOICE";

    constructor(private _ngProgessSerice: NgProgress,
        private _accountingRepo: AccountingRepo) {
        super();
        this._progressRef = this._ngProgessSerice.ref();
    }

    ngOnInit() {
        this.dataSearch.paymentStatus = [];
        this.dataSearch.paymentType = 0;
        this.requestSearchShipment();
    }
    onSelectTabLocation(tabname) {
        this.selectedTab = tabname;
        if (tabname === "INVOICE") {
            this.dataSearch.paymentType = 0;
        } else {
            this.dataSearch.paymentType = 1;
        }
        this.requestSearchShipment();
    }
    onSearchPayment(event) {
        if (this.selectedTab === "INVOICE") {
            this.dataSearch.paymentType = 0;
        } else {
            this.dataSearch.paymentType = 1;
        }
        this.dataSearch = event;
        this.requestSearchShipment();
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
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    if (this.selectedTab === "INVOICE") {
                        this.invoiceListComponent.refPaymens = res.data || [];
                    } else {
                        this.obhSOAListComponent.refPaymens = res.data || [];
                        console.log(this.obhSOAListComponent.refPaymens);
                    }
                },
            );
    }
    //refresh page (tab Invoice)
    handleUpdateExtendDateOfInvoice() {
        this.requestSearchShipment();
    }
    //refresh page (tab OBH)
    handleUpdateExtendDateOfOBH() {
        this.requestSearchShipment();
    }
}
