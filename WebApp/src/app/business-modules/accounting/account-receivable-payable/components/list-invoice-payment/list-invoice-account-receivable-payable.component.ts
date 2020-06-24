import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { SortService } from '@services';
import { PaymentModel } from 'src/app/shared/models/accouting/payment.model';
import { AccountingPaymentModel } from 'src/app/shared/models/accouting/accounting-payment.model';

@Component({
    selector: 'list-invoice-account-receivable-payable',
    templateUrl: './list-invoice-account-receivable-payable.component.html',
})
export class AccountReceivablePayableListInvoicePaymentComponent extends AppList implements OnInit {
    refPaymens: AccountingPaymentModel[] = [];
    payments: PaymentModel[] = [];
    paymentHeaders: CommonInterface.IHeaderTable[];
    constructor(
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _sortService: SortService) {
        super();
        // this.requestList = this.requestSearchShipment;
        this.requestSort = this.sortAccPayment;
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Reference No', field: 'invoiceNoReal', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Invoice Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice Date', field: 'issuedDate', sortable: true },
            { title: 'Serie No', field: 'serie', sortable: true },
            { title: 'Paid Amount', field: 'paidAmount', sortable: true },
            { title: 'Unpaid Amount', field: 'unpaidAmount', sortable: true },
            { title: 'Due Date', field: 'dueDate', sortable: true },
            { title: 'Overdue Days', field: 'overdueDays', sortable: true },
            { title: 'Payment Status', field: 'status', sortable: true },
            { title: 'Extends date', field: 'extendDays', sortable: true },
            { title: 'Notes', field: 'extendNote', sortable: true },
        ];
        this.paymentHeaders = [
            { title: 'Payment No', field: 'paymentNo', sortable: true },
            { title: 'Payment Amount', field: 'paymentAmount', sortable: true },
            { title: 'Balance', field: 'balance', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'Paid Date', field: 'paidDate', sortable: true },
            { title: 'Payment Type', field: 'paymentType', sortable: true },
            { title: 'Update Person', field: 'userModifiedName', sortable: true },
            { title: 'Update Date', field: 'datetimeModified', sortable: true }
        ];
    }

    ngAfterViewInit() {
        // this.updateExtendDayPopup.show();

    }
    import() {
        this._router.navigate(["home/accounting/account-receivable-payable/payment-import"], { queryParams: { type: 'Invoice' } });
    }
    getPayments(refId: string) {
        this._accountingRepo.getPaymentByrefId(refId)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (res: []) => {
                    this.payments = res;
                    console.log(this.payments);
                },
            );
    }
    sortAccPayment(sortField: string, order: boolean) {
        this.refPaymens = this._sortService.sort(this.refPaymens, sortField, order);
    }
    sortPayment(sortField: string, order: boolean) {
        this.payments = this._sortService.sort(this.payments, sortField, order);
    }
    showConfirmDelete(item) { }
}

