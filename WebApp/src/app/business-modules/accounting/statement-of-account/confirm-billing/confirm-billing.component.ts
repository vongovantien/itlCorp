import { Component, OnInit, ViewChild } from '@angular/core';
import { AccountingRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { AppList } from '@app';
import { catchError, finalize, map } from 'rxjs/operators';
@Component({
    selector: 'confirm-billing',
    templateUrl: './confirm-billing.component.html',
    styleUrls: ['./confirm-billing.component.scss']
})
export class ConfirmBillingComponent extends AppList implements OnInit {
    invoices: [] = [];
    constructor(
        private _accountingRepo: AccountingRepo,
        private _progressService: NgProgress,
        private _sortService: SortService,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Reference No', field: 'invoiceNoReal', sortable: true },
            { title: 'Partner ID', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Amount', field: 'totalAmount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Date', field: 'date', sortable: true },
            { title: 'Payment Term', field: 'paymentTerm', sortable: true },
            { title: 'Confirm Billing', field: 'confirmBillingDate', sortable: true },
            { title: 'Due Date', field: 'dueDate', sortable: true },
            { title: 'Payment Status', field: 'paymentStatus', sortable: true },
        ];

        this.getListInvoice();
    }

    getListInvoice() {
        this._progressRef.start();
        this._accountingRepo.getListConfirmBilling(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                }),
                map((data: any) => {
                    return {
                        data: data.data,
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.invoices = res.data;
                },
            );
    }
}
