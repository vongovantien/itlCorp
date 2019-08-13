import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services';
import { AdvancePaymentFormsearchComponent } from './components/form-search-advance-payment/form-search-advance-payment.component';
import { AdvancePaymentRequest } from 'src/app/shared/models';

@Component({
    selector: 'app-advance-payment',
    templateUrl: './advance-payment.component.html',
    styleUrls: ['./advance-payment.component.sass']
})
export class AdvancePaymentComponent extends AppList {
    @ViewChild(AdvancePaymentFormsearchComponent, { static: false }) formSearch: AdvancePaymentFormsearchComponent;

    headers: CommonInterface.IHeaderTable[];
    advancePayments: AdvancePaymentRequest[] = [];

    constructor(
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _sortService: SortService
    ) {
        super();
        this.requestList = this.getListAdvancePayment;

    }

    ngOnInit() {
        this.headers = [
            { title: 'Advance No', field: 'advanceNo', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'Job ID', field: 'JobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'Description', field: 'description', sortable: true },
            { title: 'Advance Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'requestCurrency', sortable: true },
            { title: 'Request Date', field: 'requestDate', sortable: true },
            { title: 'DeadLine Date', field: 'deadlinePayment', sortable: true },
            { title: 'Modified Date', field: 'advanceDatetimeModified', sortable: true },
            { title: 'Status Approval', field: 'statusApproval', sortable: true },
            { title: 'Status Payment', field: 'statusPayment', sortable: true },
            { title: 'Payment Method', field: 'paymentMethod', sortable: true },
        ];
        this.getListAdvancePayment();
    }

    getListAdvancePayment(dataSearch?: any) {
        this.isLoading = true;
        this._accoutingRepo.getListAdvancePayment(this.page, this.pageSize, dataSearch)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
                map((data: any) => {
                    return {
                        data: data.data.map((item: any) => new AdvancePaymentRequest(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.advancePayments = res.data;
                    this.totalItems = res.totalItems || 0;
                },
                (errors: any) => {
                    this.handleError(errors);
                },
                () => { }

            );
    }

    setSortBy(sort?: string, order?: boolean): void {
        this.sort = sort ? sort : 'code';
        this.order = order;
    }

    sortClass(sort: string): string {
        if (!!sort) {
            let classes = 'sortable ';
            if (this.sort === sort) {
                classes += ('sort-' + (this.order ? 'asc' : 'desc') + ' ');
            }

            return classes;
        }
        return '';
    }

    sortBy(sort: string): void {
        if (!!sort) {
            this.setSortBy(sort, this.sort !== sort ? true : !this.order);
            if (!!this.advancePayments.length) {
                this.advancePayments = this._sortService.sort(this.advancePayments, sort, this.order);
            }
        }
    }

    deleteAdvancePayment() {

    }

    copyAdvancePayment() {

    }

    handleError(errors: any) {
        let message: string = 'Has Error Please Check Again !';
        let title: string = '';
        if (errors instanceof HttpErrorResponse) {
            message = errors.message;
            title = errors.statusText;
        }
        this._toastService.error(message, title, { positionClass: 'toast-bottom-right' });
    }

}


