import { Component, OnInit, ViewChild } from '@angular/core';
import { AccountingRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { AppList } from '@app';
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
            { title: 'Voucher ID', field: 'voucherId', sortable: true },
            { title: 'Invoice No (Temp)', field: 'invoiceNoTempt', sortable: true },
            { title: 'Real Invoice No', field: 'invoiceNoReal', sortable: true },
            { title: 'Serie No', field: 'serie', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Total Amount', field: 'totalAmount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice Date', field: 'date', sortable: true },
            { title: 'Issued Date', field: 'datetimeCreated', sortable: true },
            { title: 'Creator', field: 'creatorName', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'Payment Status', field: 'paymentStatus', sortable: true },

        ];

    }
}
