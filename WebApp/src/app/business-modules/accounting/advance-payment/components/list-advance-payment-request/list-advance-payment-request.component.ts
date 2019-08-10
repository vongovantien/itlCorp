import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { AdvancePaymentRequest } from 'src/app/shared/models';
import { Subject } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { SortService } from 'src/app/shared/services';

@Component({
    selector: 'adv-payment-list-request',
    templateUrl: './list-advance-payment-request.component.html',
    styleUrls: ['./list-advance-payment-request.component.scss']
})

export class AdvancePaymentListRequestComponent extends AppList {

    headers: CommonInterface.IHeaderTable[];
    $listRequestAdvancePayment: Subject<AdvancePaymentRequest[]> = new Subject<AdvancePaymentRequest[]>();

    listRequestAdvancePayment: AdvancePaymentRequest[] = [];

    totalAmount: number = 0;
    currency: string = 'VND';

    constructor(
        private _sortService: SortService
    ) {
        super();
        this.requestList = this.sortRequestAdvancePament;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Description', field: 'description', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'JobID', field: 'JobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];
        this.getRequestAdvancePayment();
    }

    getRequestAdvancePayment() {
        this.$listRequestAdvancePayment
            .pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError),
            )
            .subscribe(
                (data: AdvancePaymentRequest) => {
                    this.listRequestAdvancePayment.push(data);

                    // * update total amount, Currency.
                    this.totalAmount = this.listRequestAdvancePayment.reduce((acc, curr) => acc + curr.amount, 0);
                    this.currency = data.requestCurrency;

                    for (const request of this.listRequestAdvancePayment) {
                        request.requestCurrency = data.requestCurrency;
                    }
                }
            );
    }

    sortRequestAdvancePament() {
        this.listRequestAdvancePayment = this._sortService.sort(this.listRequestAdvancePayment, this.sort, this.order);
    }
}
