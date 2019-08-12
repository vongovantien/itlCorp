import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { AdvancePaymentRequest } from 'src/app/shared/models';
import { Subject, Observable } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { SortService } from 'src/app/shared/services';
import { AdvancePaymentAddRequestPopupComponent } from '../popup/add-advance-payment-request/add-advance-payment-request.popup';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'adv-payment-list-request',
    templateUrl: './list-advance-payment-request.component.html',
    styleUrls: ['./list-advance-payment-request.component.scss']
})

export class AdvancePaymentListRequestComponent extends AppList {
    @ViewChild(AdvancePaymentAddRequestPopupComponent, { static: false }) addNewRequestPaymentPopup: AdvancePaymentAddRequestPopupComponent;

    headers: CommonInterface.IHeaderTable[];

    readonly $dataRequest: Subject<AdvancePaymentRequest> = new Subject<AdvancePaymentRequest>();
    $listRequestAdvancePayment: Observable<AdvancePaymentRequest> = this.$dataRequest.asObservable();
    listRequestAdvancePayment: AdvancePaymentRequest[] = [];

    selectedRequestAdvancePayment: AdvancePaymentRequest;

    totalAmount: number = 0;
    currency: string = 'VND';

    constructor(
        private _sortService: SortService,
        private _toastService: ToastrService
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
                (data: any) => {
                    this.listRequestAdvancePayment.push(data);
                    // * update total amount, Currency.
                    this.totalAmount = this.updateTotalAmount(this.listRequestAdvancePayment);
                    this.updateCurrencyForRequest(data);
                }
            );
    }

    sortRequestAdvancePament() {
        this.listRequestAdvancePayment = this._sortService.sort(this.listRequestAdvancePayment, this.sort, this.order);
    }

    copyRequestPayment(request: AdvancePaymentRequest) {

        this.selectedRequestAdvancePayment = request;
        this.selectedRequestAdvancePayment.uuid = Math.random();
        this.addNewRequestPaymentPopup.requestId = this.selectedRequestAdvancePayment.uuid;

        this.addNewRequestPaymentPopup.action = 'update';
        this.addNewRequestPaymentPopup.initFormUpdate(this.selectedRequestAdvancePayment);
        this.addNewRequestPaymentPopup.show({ backdrop: 'static' });

    }

    onUpdateRequestAdvancePayment(dataRequest: AdvancePaymentRequest) {
        const index: number = this.listRequestAdvancePayment.findIndex((item: AdvancePaymentRequest) => item.uuid === dataRequest.uuid);
        if (index !== -1) {
            this.listRequestAdvancePayment[index] = dataRequest;
            this.totalAmount = this.updateTotalAmount(this.listRequestAdvancePayment);
            this.updateCurrencyForRequest(dataRequest);
        }
    }

    updateCurrencyForRequest(request: AdvancePaymentRequest) {
        this.currency = request.requestCurrency;
        for (const item of this.listRequestAdvancePayment) {
            item.requestCurrency = request.requestCurrency;
        }
    }

    updateTotalAmount(listRequest: AdvancePaymentRequest[]) {
        try {
            return listRequest.reduce((acc, curr) => acc + curr.amount, 0) || 0;
        } catch (error) {
            this._toastService.error(error + '', 'Không lấy được amount');
        }
    }
}


