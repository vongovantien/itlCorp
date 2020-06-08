import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingRepo, ExportRepo } from '@repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { AccAccountingManagementResult } from 'src/app/shared/models/accouting/accounting-management';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from '@services';

@Component({
    selector: 'app-accounting-vat-invoice',
    templateUrl: './accounting-vat-invoice.component.html'
})

export class AccountingManagementVatInvoiceComponent extends AppList implements OnInit {

    invoices: AccAccountingManagementResult[] = [];

    constructor(
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _progressService: NgProgress,
        private _sortService: SortService,
        private _exportRepo: ExportRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.getListInvoice;
        this.requestSort = this.sortInvoice;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Voucher ID', field: 'voucherId', sortable: true },
            { title: 'Invoice No (Temp)', field: 'invoiceNoTempt', sortable: true },
            { title: 'Real Invoice No', field: 'invoiceNoReal', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Total Amount', field: 'totalAmount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice Date', field: 'date', sortable: true },
            { title: 'Issued Date', field: 'datetimeCreated', sortable: true },
            { title: 'Creator', field: 'creatorName', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
        ];
        this.dataSearch = {
            typeOfAcctManagement: 'Invoice'
        };
        this.getListInvoice();
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'cdi': {
                this._router.navigate([`home/accounting/management/cd-invoice`]);
                break;
            }
            case 'voucher': {
                this._router.navigate([`home/accounting/management/voucher`]);
                break;
            }
        }
    }

    getListInvoice() {
        this._progressRef.start();
        this.isLoading = true;
        this._accountingRepo.getListAcctMngt(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.isLoading = false;
                }),
                map((data: CommonInterface.IResponsePaging) => {
                    return {
                        data: (data.data || []).map((item: AccAccountingManagementResult) => new AccAccountingManagementResult(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.totalItems = res.totalItems || 0;
                    this.invoices = res.data;
                },
            );
    }

    onSearchInvoice($event: any) {
        this.page = 1;
        this.dataSearch = $event;
        this.getListInvoice();
    }

    sortInvoice(sortField: string) {
        this.invoices = this._sortService.sort(this.invoices, sortField, this.order);
    }

    exportInvoice() {
        this._progressRef.start();
        this._exportRepo.exportAccountingManagement("Invoice")
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'INVOICE - eFMS.xlsx');
                    } else {
                        this._toastService.warning('There is no data to export', '');
                    }
                },
            );
    }
}
