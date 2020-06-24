import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { AccountReceivablePayableListInvoicePaymentComponent } from './components/list-invoice-payment/list-invoice-account-receivable-payable.component';

@Component({
    selector: 'app-account-receivable-payable',
    templateUrl: './account-receivable-payable.component.html',
})
export class AccountReceivablePayableComponent extends AppList implements OnInit {
    @ViewChild(AccountReceivablePayableListInvoicePaymentComponent, { static: false }) invoiceListComponent: AccountReceivablePayableListInvoicePaymentComponent;
    selectedTab: string = "INVOICE";

    constructor(private _ngProgessSerice: NgProgress,
        private _accountingRepo: AccountingRepo) {
        super();
        this._progressRef = this._ngProgessSerice.ref();
    }

    ngOnInit() {
        this.requestSearchShipment();
    }
    onSelectTabLocation(tabname) {
        this.selectedTab = tabname;
    }
    onSearchPayment(event) {
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
                        this.invoiceListComponent.refPaymens = res.data;
                        console.log(this.invoiceListComponent.refPaymens);
                    }
                },
            );
    }
}
