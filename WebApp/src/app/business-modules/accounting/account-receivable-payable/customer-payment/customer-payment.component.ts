import { Component, ViewChild } from "@angular/core";
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
import { IAppState } from "@store";
import { Store } from "@ngrx/store";
import { RegistTypeReceipt, ResetInvoiceList } from "./store/actions";
import { InjectViewContainerRefDirective } from "@directives";

enum PAYMENT_TAB {
    CUSTOMER = 'CUSTOMER',
    AGENCY = 'AGENCY',
    ARSUMMARY = 'ARSUMMARY',
    HISTORY = 'HISTORY'

}
@Component({
    selector: 'app-customer-payment',
    templateUrl: './customer-payment.component.html',
})
export class ARCustomerPaymentComponent extends AppList implements IPermissionBase {

    @ViewChild(ConfirmPopupComponent) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(Permission403PopupComponent) permissionPopup: Permission403PopupComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainer: InjectViewContainerRefDirective;


    CPs: ReceiptModel[] = [];

    selectedCPs: ReceiptModel = null;
    messageDelete: string = "";
    selectedTab: string = PAYMENT_TAB.CUSTOMER;

    dataSearch = {
        dateFrom: formatDate(new Date(new Date().setDate(new Date().getDate() - 29)), 'yyyy-MM-dd', 'en'),
        dateTo: formatDate(new Date(), 'yyyy-MM-dd', 'en')
    }

    constructor(
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        protected _store: Store<IAppState>
    ) {
        super();
        this.requestList = this.getCPs;
        this.requestSort = this.sortLocal;
    }

    ngOnInit() {

        this.headers = [
            { title: 'Receipt No', field: 'paymentRefNo', sortable: true },
            { title: 'Customer Name', field: 'customerName', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Payment Amount', field: 'paidAmount', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'Payment Method', field: 'paymentMethod', sortable: true },
            { title: 'Paid Date', field: 'paymentDate', sortable: true },
            { title: 'Billing Date', field: 'billingDate', sortable: true },
            { title: 'Last Sync', field: 'lastSyncDate', sortable: true },
            { title: 'Description', field: 'description', sortable: true },
            { title: 'Creator', field: 'userNameCreated', sortable: true },
            { title: 'Create Date', field: 'datetimeCreated', sortable: true },
            { title: 'Modifie Date', field: 'datetimeModiflied', sortable: true },

        ];
        this.getCPs(this.dataSearch);
    }

    checkAllowDetail(data: ReceiptModel) {
        this._accountingRepo
            .checkAllowGetDetailCPS(data.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this._store.dispatch(ResetInvoiceList());
                    this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/receipt/${data.id}`]);
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

    cancelReceipt(id: string) {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainer.viewContainerRef, {
            title: 'Cancel Receipt',
            body: 'This Receipt will be canceled. Are you sure you want to continue?',
            labelConfirm: 'Yes',
            classConfirmButton: 'btn-danger',
            iconConfirm: 'la la-times'
        }, () => {
            this._accountingRepo.cancelReceipt(id)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success('Cancel Receipt Success!');
                            this.getCPs(this.dataSearch);
                        } else {
                            this._toastService.warning(res.message);
                        }
                    },
                );
        })
    }
    getCPs(dataSearch) {
        this.isLoading = true;
        this._accountingRepo
            .getListCustomerPayment(
                this.page,
                this.pageSize,
                Object.assign({}, dataSearch)
            )
            .pipe(finalize(() => this.isLoading = false))
            .subscribe((res: any) => {
                this.CPs = (res.data || []).map((item: ReceiptModel) => new ReceiptModel(item));
                this.totalItems = res.totalItems || 0;
            });
    }

    onSearchCPs(data: any) {
        this.page = 1;
        this.dataSearch = data;
        this.getCPs(this.dataSearch);

    }

    sortCPsList(sortField: string, order: boolean) {
        this.CPs = this._sortService.sort(this.CPs, sortField, order);
    }

    onConfirmDeleteCP() {
        this._accountingRepo
            .deleteCusPayment(this.selectedCPs.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.confirmPopup.hide();
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

    onSelectTab(tabName: PAYMENT_TAB | string) {
        switch (tabName) {
            case 'AR':
                this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/summary`]);
                break;
            case 'HISTORY':
                this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/history-payment`]);
                break;

            default:
                break;
        }
        this.selectedTab = tabName;
    }

    gotoCreateReceipt(type: string) {
        this._store.dispatch(ResetInvoiceList());
        this._store.dispatch(RegistTypeReceipt({ data: type }));
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/receipt/new`]);
    }
}
