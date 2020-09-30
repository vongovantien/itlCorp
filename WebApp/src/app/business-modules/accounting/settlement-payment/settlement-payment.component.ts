import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { AccountingRepo, ExportRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services';
import { NgProgress } from '@ngx-progressbar/core';
import { Router } from '@angular/router';
import { catchError, finalize, map, } from 'rxjs/operators';
import { User, SettlementPayment, PartnerOfAcctManagementResult } from 'src/app/shared/models';
import { ConfirmPopupComponent, Permission403PopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { SystemConstants } from 'src/constants/system.const';
import { ShareAccountingManagementSelectRequesterPopupComponent } from '../components/select-requester/select-requester.popup';
import { IAppState, getMenuUserSpecialPermissionState } from '@store';
import { Store } from '@ngrx/store';
import { SelectRequester } from '../accounting-management/store';

@Component({
    selector: 'app-settlement-payment',
    templateUrl: './settlement-payment.component.html',
})
export class SettlementPaymentComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;
    @ViewChild(Permission403PopupComponent, { static: false }) permissionPopup: Permission403PopupComponent;
    @ViewChild(ShareAccountingManagementSelectRequesterPopupComponent, { static: false }) selectRequesterPopup: ShareAccountingManagementSelectRequesterPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;

    headers: CommonInterface.IHeaderTable[];
    settlements: SettlementPayment[] = [];
    selectedSettlement: SettlementPayment;

    customClearances: any[] = [];
    headerCustomClearance: CommonInterface.IHeaderTable[];

    userLogged: User;
    dataReport: any = null;

    constructor(
        private _accoutingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _router: Router,
        private _exportRepo: ExportRepo,
        private _store: Store<IAppState>
    ) {
        super();
        this._progressRef = this._progressService.ref();

        this.requestList = this.getListSettlePayment;
        this.requestSort = this.sortSettlementPayment;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Settlement No', field: 'settlementNo', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'chargeCurrency', sortable: true },
            { title: 'Requester', field: 'requester', sortable: true },
            { title: 'Request Date', field: 'requestDate', sortable: true },
            { title: 'Status Approval', field: 'statusApproval', sortable: true },
            { title: 'Payment method', field: 'paymentMethod', sortable: true },
            { title: 'Voucher No', field: 'voucherNo', sortable: true },
            { title: 'Voucher Date', field: 'voucherDate', sortable: true },
            { title: 'Description', field: 'note', sortable: true },
        ];

        this.headerCustomClearance = [
            { title: 'JobID', field: 'jobId', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'chargeCurrency', sortable: true }
        ];
        this.getUserLogged();
        this.getListSettlePayment();

        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);

    }

    showSurcharge(settlementNo: string, indexsSettle: number) {
        if (!!this.settlements[indexsSettle].settleRequests.length) {
            this.customClearances = this.settlements[indexsSettle].settleRequests;
        } else {
            this._progressRef.start();
            this._accoutingRepo.getShipmentOfSettlements(settlementNo)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                ).subscribe(
                    (res: any[]) => {
                        if (!!res) {
                            this.customClearances = res;
                            this.settlements[indexsSettle].settleRequests = res;
                        }
                    },
                );
        }

    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this.dataSearch = { requester: this.userLogged.id };
    }

    onSearchSettlement(data: any) {
        this.page = 1;
        this.dataSearch = data; // Object.assign({}, data, { requester: this.userLogged.id });
        this.getListSettlePayment();
    }


    sortByCustomClearance(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.customClearances = this._sortService.sort(this.customClearances, sortData.sortField, sortData.order);
        }
    }

    getListSettlePayment() {
        this.isLoading = true;
        this._progressRef.start();
        this._accoutingRepo.getListSettlementPayment(this.page, this.pageSize, this.dataSearch)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
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

    prepareDeleteAdvance(settlement: SettlementPayment) {
        this._accoutingRepo.checkAllowDeleteSettlement(settlement.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this.selectedSettlement = settlement;
                    this.confirmDeletePopup.show();
                } else {
                    this.permissionPopup.show();
                }
            });
    }

    onDeleteSettlemenPayment() {
        this.confirmDeletePopup.hide();
        this.deleteSettlement(this.selectedSettlement.settlementNo);
    }

    deleteSettlement(settlementNo: string) {
        this.isLoading = true;
        this._progressRef.start();
        this._accoutingRepo.deleteSettlement(settlementNo)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.getListSettlePayment();
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
                            this._router.navigate([`home/accounting/settlement-payment/${settlement.id}`]);
                            break;
                        default:
                            this._router.navigate([`home/accounting/settlement-payment/${settlement.id}/approve`]);
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
                        setTimeout(() => {
                            this.previewPopup.frm.nativeElement.submit();
                            this.previewPopup.show();
                        }, 1000);
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


    issueVoucher() {
        const settlementCodes = this.settlements.filter(x => x.isSelected && x.statusApproval === 'Done');
        if (!!settlementCodes.length) {
            this.searchRef(settlementCodes.map(x => x.settlementNo));
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
                            this._router.navigate(["home/accounting/management/voucher/new"]);
                        } else {
                            this.selectRequesterPopup.listRequesters = res;
                            this.selectRequesterPopup.selectedRequester = null;
                            this.selectRequesterPopup.show();
                        }
                    } else {
                        this._toastService.warning("Not found data charge");
                    }
                }
            );
    }

    hidePopupRequester() {
        this._router.navigate(["home/accounting/management/voucher/new"]);
        this.selectRequesterPopup.hide();
    }

    checkAllSettlement() {
        if (this.isCheckAll) {
            this.settlements.forEach(x => {
                if (x.statusApproval === 'Done') {
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

    printMultiple() {
        const objChecked = this.settlements.find(x => x.isSelected);
        const settlementNos = [];
        if (!objChecked) {
            this.infoPopup.title = 'Cannot Print Multiple Settlement!';
            this.infoPopup.body = 'Opps, Please check settlement to print';
            this.infoPopup.show();
            return;
        } else {
            this.settlements.forEach(item => {
                if (item.isSelected) {
                    settlementNos.push(item.settlementNo);
                }
            });

            this.previewMultiple(settlementNos);
        }
    }

    previewMultiple(settlementNos: string[]) {
        this._progressRef.start();
        this._accoutingRepo.previewSettlementPaymentMultiple(settlementNos)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: any) => {
                    if (res != null) {
                        this.dataReport = res;
                        setTimeout(() => {
                            this.previewPopup.frm.nativeElement.submit();
                            this.previewPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }
}
