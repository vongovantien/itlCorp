import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from '@common';
import { RoutingConstants } from '@constants';
import { TrialOfficialOtherModel } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-customer-payment',
    templateUrl: './customer-payment.component.html',
})
export class ARCustomerPaymentComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(Permission403PopupComponent) permissionPopup: Permission403PopupComponent;

    CPs: any[] = [];
    selectedCPs: any = null;
    messageDelete: string = "";

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
        this.requestList = this.getPagingList;
    }
    ngOnInit() {

        this.headers = [
            { title: 'Payment Ref No', field: 'paymentrefno', sortable: true },
            { title: 'Customer Name', field: 'customername', sortable: true },
            { title: 'Payment Amount', field: 'paymentamount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Paid Date', field: 'paiddate', sortable: true },
            { title: 'Billing Date', field: 'billingdate', sortable: true },
            { title: 'Sync Status', field: 'syncstatus', sortable: true },
            { title: 'Last Sync', field: 'lastsync', sortable: true },
            { title: 'Description', field: 'description', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'Creator', field: 'creator', sortable: true },
            { title: 'Create Date', field: 'createdate', sortable: true },
            { title: 'Modifie Date', field: 'modeifiedate', sortable: true },

        ];
        this.dataSearch = {
            CurrencyLocal: "VND"
        };
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
                this.CPs = (res.data || []).map((item: any) => new (item)); // any
                this.totalItems = res.totalItems || 0;
            });
    }

    onSearchCPs(data: any) {
        this.page = 1;
        this.dataSearch = data;
        this.getCPs();
        console.log();
    }
    sortCPsList(sortField: string, order: boolean) {
        this.CPs = this._sortService.sort(this.CPs, sortField, order);
    }


    prepareDeleteCP(cpItem: any) {
        this._accountingRepo
            .checkAllowDeleteCusPayment(cpItem.paymentrefno)
            .subscribe((value: boolean) => {
                if (value) {
                    this.selectedCPs = new (cpItem); /// any
                    this.messageDelete = `Do you want to delete SOA ${cpItem.paymentrefno} ? `;
                    this.confirmPopup.show();
                } else {
                    this.permissionPopup.show();
                }
            });
    }

    onConfirmDeleteCP() {
        this._progressRef.start();
        this._accountingRepo
            .deleteCusPayment(this.selectedCPs.paymentrefno)
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

    getPagingList() {
        this._progressRef.start();
        this.isLoading = true;

        this._accountingRepo.receivablePaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.isLoading = false;
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.CPs = (res.data || []).map((item: TrialOfficialOtherModel) => new TrialOfficialOtherModel(item));
                    this.totalItems = res.totalItems;
                },
            );
    }

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
