import { Component, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';
import { Store } from '@ngrx/store';
import { RoutingConstants } from '@constants';
import { NgProgress } from '@ngx-progressbar/core';
import { NgxSpinnerService } from 'ngx-spinner';

import { AppList } from '@app';
import { AccountingRepo, ExportRepo, PartnerAPIRepo } from '@repositories';
import { SortService } from '@services';
import { AdvancePayment, AdvancePaymentRequest, User } from '@models';
import { AccountingConstants, SystemConstants } from '@constants';
import { IAppState, getMenuUserSpecialPermissionState } from '@store';
import { ConfirmPopupComponent, Permission403PopupComponent, InfoPopupComponent } from '@common';


import { UpdatePaymentVoucherPopupComponent } from './components/popup/update-payment-voucher/update-payment-voucher.popup';
import { AdvancePaymentFormsearchComponent } from './components/form-search-advance-payment/form-search-advance-payment.component';
import { AdvancePaymentsPopupComponent } from './components/popup/advance-payments/advance-payments.popup';

import { catchError, finalize, map } from 'rxjs/operators';

@Component({
    selector: 'app-advance-payment',
    templateUrl: './advance-payment.component.html',
})
export class AdvancePaymentComponent extends AppList {

    @ViewChild(AdvancePaymentFormsearchComponent, { static: false }) formSearch: AdvancePaymentFormsearchComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(Permission403PopupComponent, { static: false }) permissionPopup: Permission403PopupComponent;
    @ViewChild(UpdatePaymentVoucherPopupComponent, { static: false }) popupUpdateVoucher: UpdatePaymentVoucherPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild('confirmExistedVoucher', { static: false }) confirmExistedVoucher: ConfirmPopupComponent;
    @ViewChild('confirmRemoveSelectedVoucher', { static: false }) confirmRemoveSelectedVoucher: ConfirmPopupComponent;
    @ViewChild(AdvancePaymentsPopupComponent, { static: false }) advancePaymentsPopup: AdvancePaymentsPopupComponent;
    @ViewChild('confirmSyncAdvance', { static: false }) confirmSyncAdvancePopup: ConfirmPopupComponent;


    headers: CommonInterface.IHeaderTable[];
    headerGroupRequest: CommonInterface.IHeaderTable[];

    advancePayments: AdvancePayment[] = [];
    selectedAdv: AdvancePayment;

    groupRequest: AdvancePaymentRequest[] = [];
    userLogged: User;

    advancePaymentIds: string[] = [];

    paymentHasStatusDone = false;
    messageVoucherExisted: string = '';

    advanceSyncIds: any[] = [];

    constructor(
        private _accoutingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _exportRepo: ExportRepo,
        private _router: Router,
        private _store: Store<IAppState>,
        private _partnerAPI: PartnerAPIRepo,
        private _spinner: NgxSpinnerService,
    ) {
        super();
        this.requestList = this.getListAdvancePayment;
        this.requestSort = this.sortAdvancePayment;
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {

        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this.headers = [
            { title: 'Advance No', field: 'advanceNo', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'advanceCurrency', sortable: true },
            { title: 'Requester', field: 'requester', sortable: true },
            { title: 'Request Date', field: 'requestDate', sortable: true },
            { title: 'Deadline Date', field: 'deadlinePayment', sortable: true },
            { title: 'Modified Date', field: 'datetimeModified', sortable: true },
            { title: 'Status Approval', field: 'statusApprovalName', sortable: true },
            { title: 'Status Payment', field: 'statusApproval', sortable: true },
            { title: 'Payment Method', field: 'paymentMethod', sortable: true },
            { title: 'Description', field: 'advanceNote', sortable: true },
            { title: 'Voucher No', field: 'voucherNo', sortable: true },
            { title: 'Voucher Date', field: 'voucherDate', sortable: true },
            { title: 'Sync Date', field: 'lastSyncDate', sortable: true },
            { title: 'Sync Status', field: 'syncStatus', sortable: true },
            { title: 'User Modified', field: 'user', sortable: true },

        ];

        this.headerGroupRequest = [
            { title: 'JobId', field: 'jobId', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'requestCurrency', sortable: true },
            { title: 'Status Payment', field: 'statusPayment', sortable: true },
        ];

        this.getUserLogged();

    }

    onSearchAdvPayment(data: any) {
        this.page = 1;
        this.dataSearch = data; // Object.assign({}, data, { requester: this.userLogged.id });
        this.getListAdvancePayment();
    }

    getListAdvancePayment() {
        this.isLoading = true;
        this._progressRef.start();

        this._accoutingRepo.getListAdvancePayment(this.page, this.pageSize, this.dataSearch)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((data: any) => {
                    return {
                        data: !!data.data ? data.data.map((item: any) => new AdvancePayment(item)) : [],
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.advancePayments = res.data || [];
                    this.totalItems = res.totalItems || 0;
                    const objPayment = this.advancePayments.find(x => x.statusApproval === 'Done');
                    this.paymentHasStatusDone = !!objPayment ? true : false;
                },
            );
    }

    sortAdvancePayment(sort: string): void {
        if (!!sort) {
            this.advancePayments = this._sortService.sort(this.advancePayments, this.sort, this.order);
        }
    }

    sortByCollapse(sort: CommonInterface.ISortData): void {
        if (!!sort.sortField) {
            this.groupRequest = this._sortService.sort(this.groupRequest, sort.sortField, sort.order);
        }
    }

    onDeleteAdvPayment() {
        this._progressRef.start();
        this._accoutingRepo.deleteAdvPayment(this.selectedAdv.advanceNo)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.confirmDeletePopup.hide();
                    this._progressRef.complete();
                })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, 'Delete Success');
                        this.getListAdvancePayment();
                    }
                },
            );
    }

    prepareDeleteAdvance(selectedAdv: AdvancePayment) {
        this._accoutingRepo.checkAllowDeleteAdvance(selectedAdv.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this.selectedAdv = new AdvancePayment(selectedAdv);
                    this.confirmDeletePopup.show();
                } else {
                    this.permissionPopup.show();
                }
            });
    }

    getRequestAdvancePaymentGroup(advanceNo: string, index: number) {
        if (!!this.advancePayments[index].advanceRequests.length) {
            this.groupRequest = this.advancePayments[index].advanceRequests;
        } else {
            this.isLoading = true;
            this._progressRef.start();
            this._accoutingRepo.getGroupRequestAdvPayment(advanceNo)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this._progressRef.complete(); this.isLoading = false; })
                )
                .subscribe(
                    (res: any) => {
                        this.groupRequest = res;
                        this.advancePayments[index].advanceRequests = res;
                    },
                );
        }
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this.dataSearch = { requester: this.userLogged.id };
    }

    viewDetail(adv: AdvancePayment) {
        this._accoutingRepo.checkAllowGetDetailAdvance(adv.id)
            .subscribe((value: boolean) => {
                if (value) {
                    switch (adv.statusApproval) {
                        case 'New':
                        case 'Denied':
                            this._router.navigate([`${RoutingConstants.ACCOUNTING.ADVANCE_PAYMENT}/${adv.id}`]);
                            break;
                        default:
                            this._router.navigate([`${RoutingConstants.ACCOUNTING.ADVANCE_PAYMENT}/${adv.id}/approve`]);
                            break;
                    }
                } else {
                    this.permissionPopup.show();
                }
            });
    }

    export() {
        this._exportRepo.exportAdvancePaymentShipment(this.dataSearch)
            .subscribe(
                (res: Blob) => {
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'advance-payment.xlsx');
                }
            );
    }

    checkAllChange() {
        if (this.isCheckAll) {
            this.advancePayments.forEach(x => {
                if (x.statusApproval === 'Done') {
                    x.isChecked = true;
                }
            });
        } else {
            this.advancePayments.forEach(x => {
                x.isChecked = false;
            });
        }
    }

    removeAllChecked() {
        this.isCheckAll = false;
    }

    showPopupUpdateVoucher() {
        this.infoPopup.title = 'Cannot Update Voucher';
        this.infoPopup.body = 'Opps, Please check advance to update voucher !!';
        this.messageVoucherExisted = '';
        this.popupUpdateVoucher.voucherNo.setValue(null);
        this.popupUpdateVoucher.voucherDate.setValue(null);
        this.popupUpdateVoucher.isSubmitted = false;
        const objChecked = this.advancePayments.find(x => x.isChecked);
        if (!objChecked) {
            this.infoPopup.show();
            return;
        } else {
            const lstadvancePaymentsToCheck = this.advancePayments.filter(x => x.isChecked);
            this._accoutingRepo.checkExistedVoucherInAdvance(lstadvancePaymentsToCheck)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (!!res && res.lstVoucherData.length > 0) {
                            res.lstVoucherData.forEach(item => {
                                this.messageVoucherExisted += item.advanceNo + " has existed in " + item.voucherNo + "<br>";
                            });
                            this.messageVoucherExisted += "<br>" + " Would you like to keep updating?";
                            this.confirmExistedVoucher.show();
                        } else {
                            this.popupUpdateVoucher.show();
                        }
                    }
                );
        }
    }

    onSubmitUpdateVoucher() {
        this.confirmExistedVoucher.hide();
        this.popupUpdateVoucher.show();
    }

    removeSelectedVoucher() {
        const objChecked = this.advancePayments.find(x => x.isChecked);
        if (!objChecked) {
            this.infoPopup.title = 'Cannot Remove Voucher!';
            this.infoPopup.body = 'Opps, Please check advance to remove voucher';
            this.infoPopup.show();
            return;
        } else if (objChecked.voucherNo != null) {
            this.confirmRemoveSelectedVoucher.show();
        } else {
            this.infoPopup.title = 'Not Have Voucher!';
            this.infoPopup.body = 'Opps, Advance do not have voucher';
            this.infoPopup.show();
        }
    }

    onRemoveSelectedVoucher() {
        this.confirmRemoveSelectedVoucher.hide();
        this.advancePaymentIds = [];
        this.advancePayments.forEach(item => {
            if (item.isChecked) {
                this.advancePaymentIds.push(item.id);
            }
        });
        const body: any = {
            advancePaymentIds: this.advancePaymentIds,
            voucherNo: null,
            voucherDate: null
        };
        this.updateVoucherAdvancePayment(body);
    }

    applyVoucher($event: any) {
        const objVoucher = $event;
        this.advancePaymentIds = [];
        this.advancePayments.forEach(item => {
            if (item.isChecked) {
                this.advancePaymentIds.push(item.id);
            }
        });

        const body: any = {
            advancePaymentIds: this.advancePaymentIds,
            voucherNo: objVoucher.voucherNo,
            voucherDate: !!objVoucher.voucherDate && !!objVoucher.voucherDate.startDate ? formatDate(objVoucher.voucherDate.startDate, 'yyyy-MM-dd', 'en') : null
        };
        this.updateVoucherAdvancePayment(body);
    }

    updateVoucherAdvancePayment(body: any) {
        this._accoutingRepo.updateVoucherAdvancePayment(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.isCheckAll = false;
                        this.getListAdvancePayment();
                    } else {
                        this._toastService.error(res.message, '');
                    }
                }
            );
    }

    showPopupListAdvance() {
        this.advancePaymentsPopup.dataSearchList = this.dataSearch;
        this.advancePaymentsPopup.page = this.page;
        this.advancePaymentsPopup.pageSize = this.pageSize;
        this.advancePaymentsPopup.getListAdvancePayment();
        this.advancePaymentsPopup.show();
    }

    syncBravo() {
        const advanceSyncList = this.advancePayments.filter(x => x.isChecked && x.statusApproval === 'Done');
        if (!advanceSyncList.length) {
            this._toastService.warning("Please select advance to sync");
            return;
        }

        const hasSynced: boolean = advanceSyncList.some(x => x.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED);
        if (hasSynced) {
            const advanceHasSynced: string = advanceSyncList.filter(x => x.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED).map(a => a.advanceNo).toString();
            this._toastService.warning(`${advanceHasSynced} had synced, Please recheck!`);
            return;
        }

        this.advanceSyncIds = advanceSyncList.map((x: AdvancePayment) => {
            return <AccountingInterface.IRequestGuid>{
                Id: x.id,
                action: x.syncStatus === AccountingConstants.SYNC_STATUS.REJECTED ? 'UPDATE' : 'ADD'
            };
        });
        if (!this.advanceSyncIds.length) {
            return;
        }
        this.confirmSyncAdvancePopup.show();

    }

    onSyncBravo() {
        this.confirmSyncAdvancePopup.hide();
        this._spinner.show();
        this._accoutingRepo.syncAdvanceToAccountant(this.advanceSyncIds)
            .pipe(
                finalize(() => this._spinner.hide()),
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (((res as CommonInterface.IResult).status)) {
                        this._toastService.success("Sync Data to Accountant System Successful");

                        this.getListAdvancePayment();
                    } else {
                        this._toastService.error("Sync Data Fail");
                    }
                },
                (error) => {
                    console.log(error);
                }
            );
    }
}


