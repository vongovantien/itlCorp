import { Component, ViewChild } from "@angular/core";
import {
    ConfirmPopupComponent,
    InfoPopupComponent,
    Permission403PopupComponent
} from "src/app/shared/common/popup";
import { AccountingRepo } from "src/app/shared/repositories";
import { catchError, finalize } from "rxjs/operators";
import { AppList, IPermissionBase } from "src/app/app.list";
import { ReceiptModel, User } from "src/app/shared/models";
import { ToastrService } from "ngx-toastr";
import { SortService } from "src/app/shared/services";
import { NgProgress } from "@ngx-progressbar/core";
import { Router } from "@angular/router";
import { RoutingConstants, SystemConstants } from "@constants";


@Component({
    selector: 'app-customer-payment',
    templateUrl: './customer-payment.component.html',
})
export class ARCustomerPaymentComponent extends AppList implements IPermissionBase {
    @ViewChild(ConfirmPopupComponent) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(Permission403PopupComponent) permissionPopup: Permission403PopupComponent;
    headers: CommonInterface.IHeaderTable[];
    CPs: ReceiptModel[] = [];
    selectedCPs: ReceiptModel = null;
    messageDelete: string = "";
    constructor(
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _progressService: NgProgress,
        private _router: Router,
        private _accountingRepo: AccountingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortCPsList;
        this.requestSort = this.sortLocal;
    }
    checkAllowDetail(T: any): void {
        throw new Error("Method not implemented.");
    }
    checkAllowDelete(T: any): void {
        throw new Error("Method not implemented.");
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
            { title: 'Create Date', field: 'datetimeCreate', sortable: true },
            { title: 'Modifie Date', field: 'datetimeModiflied', sortable: true },

        ];
        this.getCPs();
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
                console.log(res);
                this.CPs = (res.data || []).map((item: ReceiptModel) => new ReceiptModel(item));
                console.log(this.CPs);
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
    prepareDeleteCP(cpItem: ReceiptModel) {
        this._accountingRepo
            .checkAllowDeleteCusPayment(cpItem.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this.selectedCPs = new ReceiptModel(cpItem);
                    this.messageDelete = `Do you want to delete Receipt ${cpItem.paymentRefNo} ? `;
                    this.confirmPopup.show();
                } else {
                    this.permissionPopup.show();
                }
            });
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
                this._toastService.success(res.message, "", {
                    positionClass: "toast-bottom-right"
                });
                // * search cps when success.
                this.onSearchCPs(this.dataSearch);
            });
    }
    sortLocal(sort: string): void {
        this.CPs = this._sortService.sort(this.CPs, sort, this.order);
    }

    viewDetail(id: string) {
        this._accountingRepo
            .checkAllowGetDetailCPS(id)
            .subscribe((value: boolean) => {
                if (value) {
                    this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/customer/receipt/${id}`]);
                } else {
                    this.permissionPopup.show();
                }
            });
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
