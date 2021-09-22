import { LoadListSettlePayment } from './components/store/actions/settlement-payment.action';
import { takeUntil, withLatestFrom } from 'rxjs/operators';
import { getSettlementPaymentListState, getSettlementPaymentSearchParamsState, getSettlementPaymentListPagingState, getSettlementPaymentListLoadingState } from './components/store/reducers/index';
import { InjectViewContainerRefDirective } from './../../../shared/directives/inject-view-container-ref.directive';
import { Component, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';
import { Router } from '@angular/router';

import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';

import { IAppState, getMenuUserSpecialPermissionState } from '@store';
import { RoutingConstants, AccountingConstants, SystemConstants } from '@constants';
import { AppList } from '@app';
import { AccountingRepo, ExportRepo } from '@repositories';
import { SortService } from '@services';
import { User, SettlementPayment, PartnerOfAcctManagementResult, SettleRequestsPayment } from '@models';
import {
    ConfirmPopupComponent,
    Permission403PopupComponent,
    InfoPopupComponent,
    ReportPreviewComponent
} from '@common';
import { delayTime } from '@decorators';
import { ICrystalReport } from '@interfaces';

import { SelectRequester } from '../accounting-management/store';
import { ShareAccountingManagementSelectRequesterPopupComponent } from '../components/select-requester/select-requester.popup';
import { SettlementPaymentsPopupComponent } from './components/popup/settlement-payments/settlement-payments.popup';

import { catchError, finalize, map, } from 'rxjs/operators';
@Component({
    selector: 'app-settlement-payment',
    templateUrl: './settlement-payment.component.html',
})
export class SettlementPaymentComponent extends AppList implements ICrystalReport {

    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;
    @ViewChild(Permission403PopupComponent) permissionPopup: Permission403PopupComponent;
    @ViewChild(ShareAccountingManagementSelectRequesterPopupComponent) selectRequesterPopup: ShareAccountingManagementSelectRequesterPopupComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(SettlementPaymentsPopupComponent) settlementPaymentsPopup: SettlementPaymentsPopupComponent;

    @ViewChild(InjectViewContainerRefDirective) confirmPopupContainerRef: InjectViewContainerRefDirective;


    settlements: SettlementPayment[] = [];
    selectedSettlement: SettlementPayment;

    shipments: SettleRequestsPayment[] = [];
    headerCustomClearance: CommonInterface.IHeaderTable[];

    userLogged: User;

    settleSyncIds: any[] = [];

    constructor(
        private _accoutingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _router: Router,
        private _exportRepo: ExportRepo,
        private _store: Store<IAppState>,
    ) {
        super();
        this._progressRef = this._progressService.ref();

        this.requestList = this.requestSettlePaymentList;
        this.requestSort = this.sortSettlementPayment;
    }

    @delayTime(1000)
    showReport(): void {
        this.previewPopup.frm.nativeElement.submit();
        this.previewPopup.show();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Settlement No', field: 'settlementNo', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'chargeCurrency', sortable: true },
            { title: 'Requester', field: 'requester', sortable: true },
            { title: 'Department', field: 'departmentName', sortable: true },
            { title: 'Payee', field: 'payeeName', sortable: true },
            { title: 'Request Date', field: 'requestDate', sortable: true },
            { title: 'Status Approval', field: 'statusApproval', sortable: true },
            { title: 'Payment method', field: 'paymentMethod', sortable: true },
            { title: 'Voucher No', field: 'voucherNo', sortable: true },
            { title: 'Voucher Date', field: 'voucherDate', sortable: true },
            { title: 'General Reason', field: 'note', sortable: true },
            { title: 'Sync Date', field: 'lastSyncDate', sortable: true },
            { title: 'Sync Status', field: 'syncStatus', sortable: true },
        ];

        this.headerCustomClearance = [
            { title: 'JobID', field: 'jobId', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'chargeCurrency', sortable: true }
        ];
        this.getUserLogged();

        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);

        this.getListSettlePayment();


        this._store.select(getSettlementPaymentSearchParamsState)
            .pipe(
                withLatestFrom(this._store.select(getSettlementPaymentListPagingState)),
                takeUntil(this.ngUnsubscribe),
                map(([dataSearch, pagingData]) => ({ page: pagingData.page, pageSize: pagingData.pageSize, dataSearch: dataSearch }))
            )
            .subscribe(
                (data) => {
                    if (!!data.dataSearch) {
                        this.dataSearch = data.dataSearch;
                    }

                    this.page = data.page;
                    this.pageSize = data.pageSize;

                    this.requestSettlePaymentList();
                }
            );

        this.isLoading = this._store.select(getSettlementPaymentListLoadingState);

    }

    requestSettlePaymentList() {
        this._store.dispatch(LoadListSettlePayment({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this.dataSearch = { requester: this.userLogged.id };
    }

    showSurcharge(settlementNo: string, indexsSettle: number) {
        if (!!this.settlements[indexsSettle].settleRequests.length) {
            this.shipments = this.settlements[indexsSettle].settleRequests;
        } else {
            this._progressRef.start();
            this._accoutingRepo.getShipmentOfSettlements(settlementNo)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                ).subscribe(
                    (res: any[]) => {
                        if (!!res) {
                            this.shipments = res;
                            this.settlements[indexsSettle].settleRequests = res;
                        }
                    },
                );
        }

    }

    sortByCustomClearance(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.shipments = this._sortService.sort(this.shipments, sortData.sortField, sortData.order);
        }
    }

    getListSettlePayment() {
        this._store.select(getSettlementPaymentListState)
            .pipe(
                catchError(this.catchError),
                takeUntil(this.ngUnsubscribe),
                map((data: any) => {
                    return {
                        data: !!data.data ? data.data.map((item: any) => new SettlementPayment(item)) : [],
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.settlements = res.data || [];
                },
            );
    }

    prepareDeleteSettle(settlement: SettlementPayment) {
        this._accoutingRepo.checkAllowDeleteSettlement(settlement.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this.selectedSettlement = settlement;

                    this.showPopupDynamicRender<ConfirmPopupComponent>(
                        ConfirmPopupComponent,
                        this.confirmPopupContainerRef.viewContainerRef, {
                        body: 'Do you want to delete ?',
                        labelConfirm: 'Yes',
                        labelCancel: 'No'
                    }, () => {
                        this.deleteSettlement(this.selectedSettlement.settlementNo);
                    })
                } else {
                    this.permissionPopup.show();
                }
            });
    }

    deleteSettlement(settlementNo: string) {
        this._accoutingRepo.deleteSettlement(settlementNo)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.requestSettlePaymentList();
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }

    sortSettlementPayment(sort: string): void {
        this.settlements = this._sortService.sort(this.settlements, sort, this.order);
    }

    viewDetail(settlement: SettlementPayment) {
        this._accoutingRepo.checkAllowGetDetailSettlement(settlement.id)
            .subscribe((value: boolean) => {
                if (value) {
                    switch (settlement.statusApproval) {
                        case 'New':
                        case 'Denied':
                            this._router.navigate([`${RoutingConstants.ACCOUNTING.SETTLEMENT_PAYMENT}/${settlement.id}`]);
                            break;
                        default:
                            this._router.navigate([`${RoutingConstants.ACCOUNTING.SETTLEMENT_PAYMENT}/${settlement.id}/approve`]);
                            break;
                    }
                } else {
                    this.permissionPopup.show();
                }
            });
    }

    printSettlement(settlementNo: string) {
        this._accoutingRepo.previewSettlementPayment(settlementNo)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: any) => {
                    if (res != null) {
                        this.dataReport = res;
                        this.showReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    export() {
        this._exportRepo.exportSettlementPaymentShipment(this.dataSearch)
            .subscribe(
                (res: Blob) => {
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'settlement-payment.xlsx');
                }
            );
    }

    accountingeExport() {
        this._exportRepo.exportSettlementPaymentShipmentDetail(this.dataSearch)
            .subscribe(
                (res: Blob) => {
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'Settlement-Detail Template.xlsx');
                }
            );
    }

    issueVoucher() {
        const settlementCodes = this.settlements.filter(x => x.isSelected && x.statusApproval === 'Done');
        if (!!settlementCodes.length) {
            this.searchRef(settlementCodes.map(x => x.settlementNo));
        } else {
            this._toastService.warning("Please select settlement payment");
        }
    }

    searchRef(settlementCodes: string[]) {
        const body: AccountingInterface.IPartnerOfAccountingManagementRef = {
            cdNotes: null,
            soaNos: null,
            jobNos: null,
            hbls: null,
            mbls: null,
            settlementCodes: settlementCodes
        };

        this._accoutingRepo.getChargeForVoucherByCriteria(body)
            .subscribe(
                (res: PartnerOfAcctManagementResult[]) => {
                    if (!!res && !!res.length) {
                        if (res.length === 1) {
                            this._store.dispatch(SelectRequester(res[0]));
                            this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNTING_MANAGEMENT}/voucher/new`]);
                        } else {// * Ưu tiên settlement có partnerId
                            if (res.some(x => !!x.partnerId)) {
                                this.selectRequesterPopup.isPayee = true;
                                this.selectRequesterPopup.listRequesters = res.filter(x => !!x.partnerId);

                                if (this.selectRequesterPopup.listRequesters.length === 1) {
                                    this._store.dispatch(SelectRequester(this.selectRequesterPopup.listRequesters[0]));
                                    this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNTING_MANAGEMENT}/voucher/new`]);
                                    return;
                                }

                                this.selectRequesterPopup.selectedRequester = null;
                                this.selectRequesterPopup.show();
                            } else {
                                this.selectRequesterPopup.isPayee = false;
                                this.selectRequesterPopup.listRequesters = res;
                                this.selectRequesterPopup.selectedRequester = null;
                                this.selectRequesterPopup.show();
                            }
                        }
                    } else {
                        this._toastService.warning("Not found data charge");
                    }
                }
            );
    }

    hidePopupRequester() {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNTING_MANAGEMENT}/voucher/new`]);
        this.selectRequesterPopup.hide();
    }

    checkAllSettlement() {
        if (this.isCheckAll) {
            this.settlements.forEach(x => {
                if (x.statusApproval === 'Done' && x.syncStatus !== AccountingConstants.SYNC_STATUS.SYNCED) {
                    x.isSelected = true;
                }
            });
        } else {
            this.settlements.forEach(x => {
                x.isSelected = false;
            });
        }
    }

    onChangeSelectedSettlement() {
        this.isCheckAll = this.settlements.filter(x => x.statusApproval === 'Done' && !x.voucherNo).every(x => x.isSelected === true);
    }

    showPopupList() {
        this.settlementPaymentsPopup.dataSearchList = this.dataSearch;
        this.settlementPaymentsPopup.page = this.page;
        this.settlementPaymentsPopup.pageSize = this.pageSize;
        this.settlementPaymentsPopup.getListSettlePayment();
        this.settlementPaymentsPopup.show();
    }

    syncBravo() {
        const settlementSyncList = this.settlements.filter(x => x.isSelected && x.statusApproval === 'Done');
        if (!settlementSyncList.length) {
            this._toastService.warning("Please select settlement payments to sync");
            return;
        }

        const hasSynced: boolean = settlementSyncList.some(x => x.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED);
        if (hasSynced) {
            const settlementHasSynced = settlementSyncList.filter(x => x.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED).map(a => a.settlementNo).toString();
            this._toastService.warning(`${settlementHasSynced} had synced, Please recheck!`);
            return;
        }

        this.settleSyncIds = settlementSyncList.map((x: SettlementPayment) => {
            return <AccountingInterface.IRequestGuid>{
                Id: x.id,
                action: x.syncStatus === AccountingConstants.SYNC_STATUS.REJECTED ? 'UPDATE' : 'ADD'
            };
        });
        if (!this.settleSyncIds.length) {
            return;
        }

        this.showPopupDynamicRender<ConfirmPopupComponent>(
            ConfirmPopupComponent,
            this.confirmPopupContainerRef.viewContainerRef,    // ? View ContainerRef chứa UI popup khi render 
            {
                body: 'Are you sure you want to sync data to accountant system ?',   // ? Config confirm popup
                iconConfirm: 'la la-cloud-upload',
                labelConfirm: 'Yes'
            },
            (v: boolean) => {                                   // ? Hàm Callback khi sumit
                this.onSyncBravo(this.settleSyncIds);
            });

    }

    onSyncBravo(Ids: AccountingInterface.IRequestGuid[]) {
        this._accoutingRepo.syncSettleToAccountant(Ids)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (((res as CommonInterface.IResult).status)) {
                        this._toastService.success("Sync Data to Accountant System Successful");

                        this.requestSettlePaymentList();
                    } else {
                        this._toastService.error("Sync Data Fail");
                    }
                },
                (error) => {
                    console.log(error);
                }
            );
    }

    denySettle() {
        const settlesDenyList = this.settlements.filter(x => x.isSelected && x.statusApproval !== AccountingConstants.STATUS_APPROVAL.NEW
            && x.syncStatus !== AccountingConstants.SYNC_STATUS.SYNCED);
        if (!settlesDenyList.length) {
            this._toastService.warning("Please select settle payment was rejected to deny");
            return;
        }

        const hasDenied: boolean = settlesDenyList.some(x => x.statusApproval === 'Denied');
        if (hasDenied) {
            const advanceHasDenied: string = settlesDenyList.filter(x => x.statusApproval === 'Denied').map(a => a.settlementNo).toString();
            this._toastService.warning(`${advanceHasDenied} had denied, Please recheck!`);
            return;
        }

        const settleIds: string[] = settlesDenyList.map((x: SettlementPayment) => x.id);
        if (!settleIds.length) {
            return;
        }

        this.showPopupDynamicRender<ConfirmPopupComponent>(
            ConfirmPopupComponent,
            this.confirmPopupContainerRef.viewContainerRef,
            { body: 'Are you sure you want to deny settle payments ?' },
            (v: boolean) => {
                this.onDenySettlePayments(settleIds);
            });
    }

    onDenySettlePayments(settleIds: string[]) {
        this._accoutingRepo.denySettlePayments(settleIds)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.requestSettlePaymentList();
                    }
                },
                (error) => {
                    console.log(error);
                }
            )
    }
}