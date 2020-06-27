import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccAccountingManagementResult } from 'src/app/shared/models/accouting/accounting-management';
import { AccountingRepo, ExportRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize, map } from 'rxjs/operators';
import { ConfirmPopupComponent, Permission403PopupComponent } from '@common';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';
import { IAppState, getMenuUserSpecialPermissionState } from '@store';

@Component({
    selector: 'app-accounting-voucher',
    templateUrl: './accounting-voucher.component.html'
})

export class AccountingManagementVoucherComponent extends AppList implements OnInit {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopupDelete: ConfirmPopupComponent;
    @ViewChild(Permission403PopupComponent, { static: false }) popup403: Permission403PopupComponent;

    vouchers: AccAccountingManagementResult[] = [];
    confirmDeleteVoucherText: string;
    selectedVoucher: AccAccountingManagementResult;
    menuSpecialPermission: Observable<any[]>;

    constructor(
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _progressService: NgProgress,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _exportRepo: ExportRepo,
        private _store: Store<IAppState>
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.getListVoucher;
        this.requestSort = this.sortVoucher;
    }

    ngOnInit() {
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this.headers = [
            { title: 'Voucher ID', field: 'voucherId', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Total Amount', field: 'totalAmount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Voucher Date', field: 'date', sortable: true },
            { title: 'Issued Date', field: 'datetimeCreated', sortable: true },
            { title: 'Creator', field: 'creatorName', sortable: true },
        ];
        this.dataSearch = {
            typeOfAcctManagement: 'Voucher'
        };
        this.getListVoucher();
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'vat': {
                this._router.navigate([`home/accounting/management/vat-invoice`]);
                break;
            }
            case 'cdi': {
                this._router.navigate([`home/accounting/management/cd-invoice`]);
                break;
            }
        }
    }

    getListVoucher() {
        this._progressRef.start();
        this._accountingRepo.getListAcctMngt(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
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
                    this.vouchers = res.data;
                },
            );
    }

    onSearchVoucher($event: any) {
        this.page = 1;
        this.dataSearch = $event;
        this.getListVoucher();
    }

    sortVoucher(sortField: string) {
        this.vouchers = this._sortService.sort(this.vouchers, sortField, this.order);
    }

    exportVoucher() {
        this._progressRef.start();
        this._exportRepo.exportAccountingManagement(this.dataSearch)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'VOUCHER - eFMS.xlsx');
                    } else {
                        this._toastService.warning('There is no data to export', '');
                    }
                },
            );
    }

    prepareDeleteVoucher(voucher: AccAccountingManagementResult) {
        this._accountingRepo.checkAllowDeleteAcctMngt(voucher.id)
            .subscribe(
                (res: boolean) => {
                    if (!res) {
                        this.popup403.show();
                        return;
                    }
                    this.selectedVoucher = voucher;

                    this.confirmDeleteVoucherText = '';
                    const textConfirm: string = (this.confirmDeleteVoucherText + '').slice();
                    this.confirmDeleteVoucherText = textConfirm + `Do you want to delete ${voucher.voucherId} ?`;
                    this.confirmPopupDelete.show();
                }
            );
    }

    onDeleteVoucher() {
        if (!!this.selectedVoucher) {
            this._accountingRepo.deleteAcctMngt(this.selectedVoucher.id)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.message) {
                            this._toastService.success(res.message);
                            this.confirmPopupDelete.hide();
                            this.onSearchVoucher(this.dataSearch);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
    }

    viewDetail(voucher: AccAccountingManagementResult): void {
        this._accountingRepo.checkDetailAcctMngtPermission(voucher.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this._router.navigate([`home/accounting/management/voucher/${voucher.id}`]);
                } else {
                    this.popup403.show();
                }
            });
    }
}