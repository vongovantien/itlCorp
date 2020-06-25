import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingPaymentModel } from 'src/app/shared/models/accouting/accounting-payment.model';
import { AccountingRepo } from '@repositories';
import { catchError } from 'rxjs/operators';
import { PaymentModel } from 'src/app/shared/models/accouting/payment.model';
import { SortService } from '@services';
import { AccountReceivablePayableUpdateExtendDayPopupComponent } from '../popup/update-extend-day/update-extend-day.popup';

@Component({
    selector: 'list-obh-account-receivable-payable',
    templateUrl: './list-obh-account-receivable-payable.component.html',
})
export class AccountReceivablePayableListOBHPaymentComponent extends AppList implements OnInit {
    @ViewChild(AccountReceivablePayableUpdateExtendDayPopupComponent, { static: false }) updateExtendDayPopup: AccountReceivablePayableUpdateExtendDayPopupComponent;
    refPaymens: AccountingPaymentModel[] = [];
    payments: PaymentModel[] = [];
    paymentHeaders: CommonInterface.IHeaderTable[];
    constructor(private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _sortService: SortService) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Reference No', field: 'referenceNo', sortable: true },
            { title: 'Partner Name', field: 'referenceNo', sortable: true },
            { title: 'OBH Amount', field: 'referenceNo', sortable: true },
            { title: 'Currency', field: 'referenceNo', sortable: true },
            { title: 'Issue Date', field: 'referenceNo', sortable: true },
            { title: 'Paid Amount', field: 'referenceNo', sortable: true },
            { title: 'Unpaid Amount', field: 'referenceNo', sortable: true },
            { title: 'Due Date', field: 'referenceNo', sortable: true },
            { title: 'Overdue Days', field: 'referenceNo', sortable: true },
            { title: 'Payment Status', field: 'referenceNo', sortable: true },
            { title: 'Extend days', field: 'referenceNo', sortable: true },
            { title: 'Notes', field: 'referenceNo', sortable: true },
        ];
    }

    import() {
        this._router.navigate(["home/accounting/account-receivable-payable/payment-import"], { queryParams: { type: 'OBH' } });
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

    sortPayment(sortField: string, order: boolean) {
        this.payments = this._sortService.sort(this.payments, sortField, order);
    }
    showConfirmDelete(item) { }
    showExtendDateModel(refId: string) {
        console.log(refId);
        this.updateExtendDayPopup.show();
    }
}

