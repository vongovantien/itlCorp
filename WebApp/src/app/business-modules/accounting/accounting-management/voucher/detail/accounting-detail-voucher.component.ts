import { Component, OnInit, ViewChild } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';

import { AccountingRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { IAccountingManagementState, UpdateChargeList } from '../../store';
import { AccAccountingManagementModel, ChargeOfAccountingManagementModel } from '@models';

import { AccountingManagementCreateVoucherComponent } from '../create/accounting-create-voucher.component';

import { tap, switchMap, catchError, finalize, concatMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { isUUID } from 'validator';
import _merge from 'lodash/merge';
import { formatDate } from '@angular/common';
import { ConfirmPopupComponent } from '@common';
import { BravoVoucher } from 'src/app/shared/models/partner-api/voucher-bravo';

@Component({
    selector: 'app-accounting-detail-voucher',
    templateUrl: './accounting-detail-voucher.component.html',
})
export class AccountingManagementDetailVoucherComponent extends AccountingManagementCreateVoucherComponent implements OnInit {
    @ViewChild('confirmVoucherPopup', { static: false }) confirmVoucherPopup: ConfirmPopupComponent;

    voucherId: string;
    accountingManagement: AccAccountingManagementModel = new AccAccountingManagementModel();
    confirmMessage: string = '';
    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _accountingRepo: AccountingRepo,
        protected _store: Store<IAccountingManagementState>,
        private _activedRoute: ActivatedRoute,
        private _ngProgressService: NgProgress,
    ) {
        super(_toastService, _accountingRepo, _store, _router);
        this._progressRef = this._ngProgressService.ref();
    }


    ngOnInit(): void {
        this._activedRoute.params.pipe(
            tap((param: Params) => {
                this.voucherId = !!param.voucherId ? param.voucherId : '';
            }),
            switchMap(() => of(this.voucherId)),
        )
            .subscribe(
                (voucherId: string) => {
                    if (isUUID(voucherId)) {
                        this.getDetailVoucher(voucherId);
                    } else {
                        this.gotoList();
                    }
                }
            );
    }

    getDetailVoucher(id: string) {
        this._accountingRepo.getDetailAcctMngt(id)
            .subscribe(
                (res: AccAccountingManagementModel) => {
                    this.accountingManagement = new AccAccountingManagementModel(res);
                    this.updateFormVoucher(res);
                    this.updateChargeList(res);

                    console.log(this.accountingManagement);

                    if (!!this.accountingManagement.lastSyncDate) {
                        this.formCreateComponent.isReadonly = true;
                        this.listChargeComponent.isReadOnly = true;
                    }
                }
            );
    }

    updateFormVoucher(res: AccAccountingManagementModel) {
        const formData: AccAccountingManagementModel | any = {
            date: !!res.date ? { startDate: new Date(res.date), endDate: new Date(res.date) } : null,
            paymentMethod: !!res.paymentMethod ? [{ id: res.paymentMethod, text: res.paymentMethod }] : null,
            currency: !!res.currency ? [{ id: res.currency, text: res.currency }] : null,
            voucherType: !!res.voucherType ? [{ id: res.voucherType, text: res.voucherType }] : null,

        };
        this.formCreateComponent.formGroup.patchValue(Object.assign(_merge(res, formData)));
    }

    updateChargeList(res: AccAccountingManagementModel) {
        // this.listChargeComponent.charges = res.charges;
        // this.listChargeComponent.updateTotalAmount();

        this._store.dispatch(UpdateChargeList({ charges: res.charges }));

    }

    onSubmitSaveVoucher() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        if (!this.listChargeComponent.charges.length) {
            this._toastService.warning("VAT Invoice don't have any charge in this period, Please check it again!");
            return;
        }

        if (!this.checkValidateExchangeRate()) {
            this._toastService.warning(this.invalidUpdateExchangeRate);
            return;
        }

        this._accountingRepo.checkVoucherIdExist(this.formCreateComponent.voucherId.value, this.voucherId)
            .pipe(
                switchMap(
                    (res: boolean) => {
                        if (res) {
                            this.formCreateComponent.voucherId.setErrors({ existed: true });
                            return of({ data: null, message: 'Voucher ID has been existed', status: false });
                        } else {
                            const modelAdd: AccAccountingManagementModel = this.onSubmitData();
                            modelAdd.charges = [...this.listChargeComponent.charges];

                            modelAdd.charges.forEach(c => {
                                if (!!c.invoiceDate) {
                                    const [day, month, year]: string[] = c.invoiceDate.split("/");
                                    c.invoiceDate = formatDate(new Date(+year, +month - 1, +day), 'yyyy-MM-dd', 'en');
                                } else {
                                    c.invoiceDate = null;
                                }
                            });
                            //  * Update field
                            modelAdd.id = this.voucherId;
                            modelAdd.status = this.accountingManagement.status;
                            modelAdd.type = this.accountingManagement.type;
                            modelAdd.companyId = this.accountingManagement.companyId;
                            modelAdd.officeId = this.accountingManagement.officeId;
                            modelAdd.departmentId = this.accountingManagement.departmentId;
                            modelAdd.groupId = this.accountingManagement.groupId;
                            modelAdd.userCreated = this.accountingManagement.userCreated;
                            modelAdd.datetimeCreated = this.accountingManagement.datetimeCreated;

                            this._progressRef.start();
                            return this._accountingRepo.updateAcctMngt(modelAdd)
                                .pipe(
                                    catchError(this.catchError),
                                    finalize(() => this._progressRef.complete()),
                                    concatMap((data: CommonInterface.IResult) => {
                                        if (data.status) {
                                            this._toastService.success(data.message);
                                            return this._accountingRepo.getDetailAcctMngt(this.voucherId);
                                        }
                                        return of({ data: null, message: 'Something getting error. Please check again!', status: false });
                                    })
                                );
                        }
                    }
                ),
            ).subscribe(
                (res: CommonInterface.IResult | AccAccountingManagementModel | any) => {
                    if (!!res && !res.status) {
                        this._toastService.error(res.message);
                    } else {
                        this.accountingManagement = new AccAccountingManagementModel(res);
                        this.updateFormVoucher((res as AccAccountingManagementModel));

                        // * Update list charge & total amount without dispatch action UpdateChargeList.
                        this.listChargeComponent.charges = this.formatInvoiceDate(res.charges);

                        this.listChargeComponent.updateTotalAmount();
                    }
                },
            );
    }

    saveVoucher(body: AccAccountingManagementModel) {
        this._progressRef.start();
        this._accountingRepo.updateAcctMngt(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                concatMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        return this._accountingRepo.getDetailAcctMngt(this.voucherId);
                    }
                    of(res);
                })
            )
            .subscribe(
                (res: CommonInterface.IResult | AccAccountingManagementModel | any) => {
                    if (!!res && res.status === false) {
                        this._toastService.error(res.message);
                    } else {
                        this.updateFormVoucher(res);
                    }
                }
            );
    }

    gotoList() {
        this._router.navigate(["home/accounting/management/voucher"]);
    }

    formatInvoiceDate(charges: ChargeOfAccountingManagementModel[]) {
        charges.forEach(c => {
            if (!!c.invoiceDate) {
                c.invoiceDate = formatDate(new Date(c.invoiceDate), 'dd/MM/yyyy', 'en');
            } else {
                c.invoiceDate = null;
            }
        });
        return charges;
    }

    confirmSync() {
        this._toastService.success("Tính năng đang phát triển");
        // this.confirmMessage = `Are you sure you want to sync data to accountant system?`;
        // this.confirmVoucherPopup.show();
    }

    onConfirmVoucher() {
        this.getDataVoucherToSync();
    }

    getDataVoucherToSync() {
        this.confirmVoucherPopup.hide();
        const voucherIds: string[] = [];
        voucherIds.push(this.voucherId);
        this._accountingRepo.getListVoucherToSync(voucherIds)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (res: BravoVoucher[]) => {
                    const data: BravoVoucher[] = res;
                    console.log(data);
                    this.syncToAccountant(data, voucherIds);
                },
            );
    }

    syncToAccountant(data: BravoVoucher[], ids: string[]) {
        // Gọi API Bravo

        // Sync Bravo success
        this.updateSyncStatusVoucher(ids);
    }

    updateSyncStatusVoucher(ids: string[]) {
        this._accountingRepo.syncVoucherToAccountant(ids)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success('Sync Data to Accountant System Successful!', '');
                        this.getDetailVoucher(this.voucherId);
                    } else {
                        this._toastService.error(res.message);
                    }
                },
            );
    }
}
