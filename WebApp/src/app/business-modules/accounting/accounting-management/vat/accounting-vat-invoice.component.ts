import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';

import { AccountingRepo, ExportRepo } from '@repositories';
import { SortService } from '@services';
import { AccAccountingManagementResult } from '@models';
import { AccountingConstants } from '@constants';
import { Permission403PopupComponent, ConfirmPopupComponent } from '@common';
import { IAppState, getMenuUserSpecialPermissionState } from '@store';

import { AppList } from 'src/app/app.list';

import { Observable } from 'rxjs';
import { catchError, finalize, map } from 'rxjs/operators';


@Component({
    selector: 'app-accounting-vat-invoice',
    templateUrl: './accounting-vat-invoice.component.html'
})

export class AccountingManagementVatInvoiceComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopupDelete: ConfirmPopupComponent;
    @ViewChild(Permission403PopupComponent, { static: false }) popup403: Permission403PopupComponent;

    invoices: AccAccountingManagementResult[] = [];
    selectedInvoice: AccAccountingManagementResult;

    confirmDeleteInvoiceText: string = '';
    menuSpecialPermission: Observable<any[]>;

    constructor(
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _progressService: NgProgress,
        private _sortService: SortService,
        private _exportRepo: ExportRepo,
        private _toastService: ToastrService,
        private _store: Store<IAppState>
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.getListInvoice;
        this.requestSort = this.sortInvoice;
    }

    ngOnInit() {
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
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
        this.dataSearch = {
            typeOfAcctManagement: AccountingConstants.ISSUE_TYPE.INVOICE,
            fromIssuedDate: formatDate(new Date(), 'yyyy-MM-dd', 'en'),
            toIssuedDate: formatDate(new Date(), 'yyyy-MM-dd', 'en'),
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
        this._exportRepo.exportAccountingManagement(this.dataSearch)
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

    prepareDeleteInvoice(invoice: AccAccountingManagementResult) {
        if (invoice.paymentStatus === AccountingConstants.PAYMENT_STATUS.PAID) {
            return;
        }
        this._accountingRepo.checkAllowDeleteAcctMngt(invoice.id)
            .subscribe(
                (res: boolean) => {
                    if (!res) {
                        this.popup403.show();
                        return;
                    }
                    this.selectedInvoice = invoice;

                    this.confirmDeleteInvoiceText = '';
                    const textConfirm: string = (this.confirmDeleteInvoiceText + '').slice();
                    this.confirmDeleteInvoiceText = textConfirm + `Do you want to delete ${invoice.invoiceNoTempt} ?`;
                    this.confirmPopupDelete.show();
                }
            );
    }

    onDeleteInvoice() {
        if (!!this.selectedInvoice) {
            this._accountingRepo.deleteAcctMngt(this.selectedInvoice.id)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.message) {
                            this._toastService.success(res.message);
                            this.confirmPopupDelete.hide();
                            this.onSearchInvoice(this.dataSearch);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
    }

    viewDetail(invoice: AccAccountingManagementResult): void {
        this._accountingRepo.checkDetailAcctMngtPermission(invoice.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this._router.navigate([`home/accounting/management/vat-invoice/${invoice.id}`]);
                } else {
                    this.popup403.show();
                }
            });
    }
}
