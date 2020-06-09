import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';

import { InfoPopupComponent } from '@common';
import { ToastrService } from 'ngx-toastr';
import { AccountingRepo } from '@repositories';
import { Store } from '@ngrx/store';
import { IAccountingManagementState, InitPartner } from '../../store';
import { AccAccountingManagementModel } from '@models';
import { AccountingConstants } from '@constants';
import { formatDate } from '@angular/common';

import { AccountingManagementFormCreateVoucherComponent } from '../../components/form-create-voucher/form-create-voucher.component';
import { AccountingManagementListChargeComponent } from '../../components/list-charge/list-charge-accouting-management.component';

import _merge from 'lodash/merge';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'app-accounting-create-voucher',
    templateUrl: './accounting-create-voucher.component.html',
})
export class AccountingManagementCreateVoucherComponent extends AppForm implements OnInit {

    @ViewChild(AccountingManagementFormCreateVoucherComponent, { static: false }) formCreateComponent: AccountingManagementFormCreateVoucherComponent;
    @ViewChild(AccountingManagementListChargeComponent, { static: false }) listChargeComponent: AccountingManagementListChargeComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;

    constructor(
        protected _toastService: ToastrService,
        protected _accountingRepo: AccountingRepo,
        protected _store: Store<IAccountingManagementState>

    ) {
        super();
    }

    ngOnInit(): void {
    }

    onSubmitSaveVoucher() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        if (!this.listChargeComponent.charges.length) {
            this._toastService.warning("Voucher don't have any charge in this period, Please check it again!");
            return;
        }

        const modelAdd: AccAccountingManagementModel = this.onSubmitData();
        modelAdd.type = AccountingConstants.ISSUE_TYPE.VOUCHER;

        modelAdd.charges = [...this.listChargeComponent.charges];

        this.saveVoucher(modelAdd);
    }

    checkValidateForm() {
        this.setError(this.formCreateComponent.currency);
        this.setError(this.formCreateComponent.paymentMethod);
        this.setError(this.formCreateComponent.voucherType);

        let valid: boolean = true;
        if (!this.formCreateComponent.formGroup.valid) {
            valid = false;
        }
        return valid;
    }

    onSubmitData(): AccAccountingManagementModel {
        const form: { [name: string]: any } = this.formCreateComponent.formGroup.getRawValue();
        const formData = {
            date: !!form.date && !!form.date.startDate ? formatDate(form.date.startDate, 'yyyy-MM-dd', 'en') : null,
            paymentMethod: !!form.paymentMethod && !!form.paymentMethod.length ? form.paymentMethod[0].id : null,
            currency: !!form.currency && !!form.currency.length ? form.currency[0].id : null,
            voucherType: !!form.voucherType && !!form.voucherType.length ? form.voucherType[0].id : null,
        };

        return new AccAccountingManagementModel(Object.assign(_merge(form, formData)));
    }

    saveVoucher(body: AccAccountingManagementModel) {
        this._accountingRepo.addNewAcctMgnt(body)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error(res.message);
                    }
                },
                (res: HttpErrorResponse) => {
                    if (res.error.message === 'Voucher ID has been existed') {
                        this.formCreateComponent.voucherId.setErrors({ existed: true });
                    }
                }
            );
    }

    ngOnDestroy() {
        this._store.dispatch(InitPartner());
    }
}
