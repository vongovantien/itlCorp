import { Component, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";

import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from "@common";
import { AccountingRepo } from "@repositories";
import { AppList, IPermissionBase } from "@app";
import { ReceiptModel } from "@models";
import { SortService } from "@services";
import { RoutingConstants } from "@constants";

import { catchError, finalize, map, takeUntil, withLatestFrom } from "rxjs/operators";
import { formatDate } from "@angular/common";
import { IAppState } from "@store";
import { Store } from "@ngrx/store";
import { LoadListCustomerPayment, RegistTypeReceipt, ResetInvoiceList } from "./store/actions";
import { InjectViewContainerRefDirective } from "@directives";
import { customerPaymentReceipListState, customerPaymentReceipPagingState, customerPaymentReceipSearchState, customerPaymentReceipLoadingState } from "./store/reducers";

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
        private readonly _sortService: SortService,
        private readonly _toastService: ToastrService,
        private readonly _router: Router,
        private readonly _accountingRepo: AccountingRepo,
        private readonly _store: Store<IAppState>
    ) {
        super();
        this.requestList = this.requestLoadListCustomerPayment;
        this.requestSort = this.sortLocal;
    }

    ngOnInit() {

        this.headers = [
            { title: 'Receipt No', field: 'paymentRefNo', sortable: true },
            { title: 'Paid Date', field: 'paymentDate', sortable: true },
            { title: 'Customer Name', field: 'customerName', sortable: true },
            { title: 'Collect Amount', field: '', sortable: true },
            { title: 'Payment Amount', field: 'paidAmount', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'Description', field: 'description', sortable: true },
            { title: 'Receipt Type', field: '', sortable: true },
            { title: 'Payment Method', field: 'paymentMethod', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Billing Date', field: 'billingDate', sortable: true },
            { title: 'Last Sync', field: 'lastSyncDate', sortable: true },
            { title: 'Creator', field: 'userNameCreated', sortable: true },
            { title: 'Create Date', field: 'datetimeCreated', sortable: true },
            { title: 'Modifie Date', field: 'datetimeModiflied', sortable: true },

        ];
        this.isLoading = this._store.select(customerPaymentReceipLoadingState)
        this.getCPs();


        this._store.select(customerPaymentReceipSearchState)
            .pipe(
                withLatestFrom(this._store.select(customerPaymentReceipPagingState)),
                map(([dataSearch, pagingData]) => ({ page: pagingData.page, pageSize: pagingData.pageSize, dataSearch: dataSearch })),
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe(
                (data) => {
                    if (!!data.dataSearch) {
                        this.dataSearch = data.dataSearch;
                    }

                    this.page = data.page;
                    this.pageSize = data.pageSize;

                    this.requestLoadListCustomerPayment();
                }
            );
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
                    this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainer.viewContainerRef, {
                        title: 'Delete Receipt',
                        body: this.messageDelete,
                        labelConfirm: 'Yes',
                        classConfirmButton: 'btn-danger',
                        iconConfirm: 'la la-trash'
                    }, () => this.onConfirmDeleteCP())
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
                            this.requestLoadListCustomerPayment();
                        } else {
                            this._toastService.warning(res.message);
                        }
                    },
                );
        })
    }

    getCPs() {
        // this.page = 1;
        this._store.select(customerPaymentReceipListState)
            .pipe(
                catchError(this.catchError),
                map((data: any) => {
                    return {
                        data: !!data.data ? data.data.map((item: any) => new ReceiptModel(item)) : [],
                        totalItems: data.totalItems,
                    };
                }),
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe(
                (res: any) => {
                    this.CPs = res.data || [];
                    this.totalItems = res.totalItems || 0;
                },
            );
    }

    sortCPsList(sortField: string, order: boolean) {
        this.CPs = this._sortService.sort(this.CPs, sortField, order);
    }

    onConfirmDeleteCP() {
        this._accountingRepo
            .deleteCusPayment(this.selectedCPs.id)
            .subscribe((res: any) => {
                this._toastService.success(res.message);
                // * search cps when success.

                this.requestLoadListCustomerPayment();
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
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/receipt/${type.toLowerCase()}/new`]);
    }

    requestLoadListCustomerPayment() {
        this._store.dispatch(LoadListCustomerPayment({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
    }

}

