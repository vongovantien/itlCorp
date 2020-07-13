import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';

import { AccountingRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { IAccountingManagementState, UpdateChargeList } from '../../store';
import { AccAccountingManagementModel } from '@models';

import { AccountingManagementCreateVoucherComponent } from '../create/accounting-create-voucher.component';

import { tap, switchMap, catchError, finalize, concatMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { isUUID } from 'validator';
import _merge from 'lodash/merge';

@Component({
    selector: 'app-accounting-detail-voucher',
    templateUrl: './accounting-detail-voucher.component.html',
})
export class AccountingManagementDetailVoucherComponent extends AccountingManagementCreateVoucherComponent implements OnInit {

    voucherId: string;
    accountingManagement: AccAccountingManagementModel = new AccAccountingManagementModel();

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
}
