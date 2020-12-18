import { Component, ViewChild } from "@angular/core";
import { NgProgress } from "@ngx-progressbar/core";
import { Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";

import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from "@common";
import { AccountingRepo } from "@repositories";
import { AppList, IPermissionBase } from "@app";
import { ReceiptModel } from "@models";
import { SortService } from "@services";
import { RoutingConstants } from "@constants";


import { catchError, finalize } from "rxjs/operators";
import { formatDate } from "@angular/common";
@Component({
    selector: 'app-customer-payment',
    templateUrl: './customer-payment.component.html',
})
export class ARCustomerPaymentComponent extends AppList implements IPermissionBase {

    @ViewChild(ConfirmPopupComponent) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(Permission403PopupComponent) permissionPopup: Permission403PopupComponent;

    CPs: ReceiptModel[] = [];

    selectedCPs: ReceiptModel = null;
    messageDelete: string = "";

    dataSearch = {
        dateFrom: formatDate(new Date(new Date().setDate(new Date().getDate() - 29)), 'yyyy-MM-dd', 'en'),
        dateTo: formatDate(new Date(), 'yyyy-MM-dd', 'en')
    }

    constructor(
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _progressService: NgProgress,
        private _router: Router,
        private _accountingRepo: AccountingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.sortCPsList;
        this.requestSort = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Payment Ref No', field: 'paymentRefNo', sortable: true },
            { title: 'Customer Name', field: 'customerName', sortable: true },
            { title: 'Payment Amount', field: 'paidAmount', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'Paid Date', field: 'paymentDate', sortable: true },
            { title: 'Billing Date', field: 'billingDate', sortable: true },
            { title: 'Sync Status', field: 'syncStatus', sortable: true },
            { title: 'Last Sync', field: 'lastSyncDate', sortable: true },
            { title: 'Description', field: 'description', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'Creator', field: 'userNameCreated', sortable: true },
            { title: 'Create Date', field: 'datetimeCreated', sortable: true },
            { title: 'Modifie Date', field: 'datetimeModiflied', sortable: true },

        ];
        this.getCPs();
    }

    checkAllowDetail(data: ReceiptModel) {
        this._accountingRepo
            .checkAllowGetDetailCPS(data.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/customer/receipt/${data.id}`]);
                } else {
                    this.permissionPopup.show();
                }
            });
    }

    checkAllowDelete(data: ReceiptModel) {
        this._accountingRepo
            .checkAllowDeleteCusPayment(data.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this.selectedCPs = new ReceiptModel(data);
                    this.messageDelete = `Do you want to delete Receipt ${data.paymentRefNo} ? `;
                    this.confirmPopup.show();
                } else {
                    this.permissionPopup.show();
                }
            });
    }

    getCPs() {
        this.isLoading = true;
        this._progressRef.start();
        this._accountingRepo
            .getListCustomerPayment(
                this.page,
                this.pageSize,
                Object.assign({}, this.dataSearch)
            )
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.isLoading = false;
                    this._progressRef.complete();
                })
            )
            .subscribe((res: any) => {
                this.CPs = (res.data || []).map((item: ReceiptModel) => new ReceiptModel(item));
                this.totalItems = res.totalItems || 0;
            });
    }
    onSearchCPs(data: any) {
        this.page = 1;
        this.dataSearch = data;
        this.getCPs();

    }
    sortCPsList(sortField: string, order: boolean) {
        this.CPs = this._sortService.sort(this.CPs, sortField, order);
    }

    onConfirmDeleteCP() {
        this._progressRef.start();
        this._accountingRepo
            .deleteCusPayment(this.selectedCPs.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.confirmPopup.hide();
                    this._progressRef.complete();
                })
            )
            .subscribe((res: any) => {
                this._toastService.success(res.message);
                // * search cps when success.
                this.onSearchCPs(this.dataSearch);
            });
    }

    sortLocal(sort: string): void {
        this.CPs = this._sortService.sort(this.CPs, sort, this.order);
    }

    onSelectTab(tab: string) {
        switch (tab) {
            case 'agency':
                this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/agency`]);
                break;
            case 'ar':
                this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/summary`]);
                break;
            case 'history':
                this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/history-payment`]);
                break;
            default:
                break;
        }
    }
}
