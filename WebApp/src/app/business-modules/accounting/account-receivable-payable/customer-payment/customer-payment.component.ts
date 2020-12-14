// import { Component, OnInit, ViewChild } from '@angular/core';
// import { Router } from '@angular/router';
// import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from '@common';
// import { RoutingConstants } from '@constants';

// import { NgProgress } from '@ngx-progressbar/core';
// import { AccountingRepo } from '@repositories';
// import { SortService } from '@services';
// import { ToastrService } from 'ngx-toastr';
// import { catchError, finalize } from 'rxjs/operators';
// import { AppList } from 'src/app/app.list';
// import { Receipt } from 'src/app/shared/models/accouting/receipt.model';
import { Component, ViewChild } from "@angular/core";
import {
    ConfirmPopupComponent,
    InfoPopupComponent,
    Permission403PopupComponent
} from "src/app/shared/common/popup";
import { AccountingRepo } from "src/app/shared/repositories";
import { catchError, finalize } from "rxjs/operators";
import { AppList } from "src/app/app.list";
import { Receipt, User } from "src/app/shared/models";
import { ToastrService } from "ngx-toastr";
import { SortService } from "src/app/shared/services";
import { NgProgress } from "@ngx-progressbar/core";
import { Router } from "@angular/router";
import { RoutingConstants, SystemConstants } from "@constants";


@Component({
    selector: 'app-customer-payment',
    templateUrl: './customer-payment.component.html',
})
export class ARCustomerPaymentComponent extends AppList {
    @ViewChild(ConfirmPopupComponent) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(Permission403PopupComponent) permissionPopup: Permission403PopupComponent;
    headers: CommonInterface.IHeaderTable[];
    CPs: Receipt[] = [];
    selectedCPs: Receipt = null;
    messageDelete: string = "";
    userLogged: User;
    // trialOfficialList: TrialOfficialOtherModel[] = [];

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
        // this.dataSearch = {
        //     currency: "VND"
        // };
        this.getUserLogged();
        this.getCPs();


    }
    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this.dataSearch = { userCreated: this.userLogged.id };
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
                this.CPs = (res.data || []).map((item: Receipt) => new Receipt(item));
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


    prepareDeleteCP(cpItem: Receipt) {
        this._accountingRepo
            .checkAllowDeleteCusPayment(cpItem.paymentRefNo)
            .subscribe((value: boolean) => {
                if (value) {
                    this.selectedCPs = new Receipt(cpItem);
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
            .deleteCusPayment(this.selectedCPs.paymentRefNo)
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

                // * search SOA when success.
                this.onSearchCPs(this.dataSearch);
            });
    }
    sortLocal(sort: string): void {
        this.CPs = this._sortService.sort(this.CPs, sort, this.order);
    }
    // getPagingList() {
    //     this._progressRef.start();
    //     this.isLoading = true;

    //     this._accountingRepo.receivablePaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
    //         .pipe(
    //             catchError(this.catchError),
    //             finalize(() => {
    //                 this._progressRef.complete();
    //                 this.isLoading = false;
    //             })
    //         ).subscribe(
    //             (res: CommonInterface.IResponsePaging) => {
    //                 this.CPs = (res.data || []).map((item: Receipt) => new Receipt(item));
    //                 this.totalItems = res.totalItems;
    //             },
    //         );
    // }

    viewDetail(paymentrefno: string, currency: string) {
        // this._accountingRepo
        //     .checkAllowGetDetailSOA(soano)
        //     .subscribe((value: boolean) => {
        //         if (value) {
        //             this._router.navigate([`${RoutingConstants.ACCOUNTING.STATEMENT_OF_ACCOUNT}/detail/`], {
        //                 queryParams: { no: soano, currency: currency }
        //             });
        //         } else {
        //             this.permissionPopup.show();
        //         }
        //     });
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
