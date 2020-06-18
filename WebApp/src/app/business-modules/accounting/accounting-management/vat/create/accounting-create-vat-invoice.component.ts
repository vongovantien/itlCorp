import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AppForm } from 'src/app/app.form';
import { ToastrService } from 'ngx-toastr';
import { AccAccountingManagementModel } from '@models';
import { Store } from '@ngrx/store';
import { InfoPopupComponent } from '@common';
import { formatDate } from '@angular/common';
import { AccountingRepo } from '@repositories';

import { AccountingManagementFormCreateVATInvoiceComponent } from '../../components/form-create-vat-invoice/form-create-vat-invoice.component';
import { AccountingManagementListChargeComponent } from '../../components/list-charge/list-charge-accouting-management.component';
import { IAccountingManagementState, InitPartner } from '../../store';

import { catchError } from 'rxjs/operators';
import _merge from 'lodash/merge';
import { AccountingConstants } from '@constants';
import { Router } from '@angular/router';


@Component({
    selector: 'app-accounting-create-vat-invoice',
    templateUrl: './accounting-create-vat-invoice.component.html',
})
export class AccountingManagementCreateVATInvoiceComponent extends AppForm implements OnInit {

    @ViewChild(AccountingManagementFormCreateVATInvoiceComponent, { static: false }) formCreateComponent: AccountingManagementFormCreateVATInvoiceComponent;
    @ViewChild(AccountingManagementListChargeComponent, { static: false }) listChargeComponent: AccountingManagementListChargeComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;

    constructor(
        protected _toastService: ToastrService,
        protected _accountingRepo: AccountingRepo,
        protected _store: Store<IAccountingManagementState>,
        protected _router: Router,
    ) {
        super();
    }

    ngOnInit(): void {
    }

    onSubmitSaveInvoice() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        if (!this.listChargeComponent.charges.length) {
            this._toastService.warning("VAT Invoice don't have any charge in this period, Please check it again!");
            return;
        }

        const modelAdd: AccAccountingManagementModel = this.onSubmitData();
        modelAdd.type = AccountingConstants.ISSUE_TYPE.INVOICE;

        modelAdd.charges = [...this.listChargeComponent.charges];

        this.saveInvoice(modelAdd);
    }

    checkValidateForm() {
        this.setError(this.formCreateComponent.currency);
        this.setError(this.formCreateComponent.paymentMethod);

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
        };

        return new AccAccountingManagementModel(Object.assign(_merge(form, formData)));
    }

    saveInvoice(body: AccAccountingManagementModel) {
        this._accountingRepo.addNewAcctMgnt(body)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._router.navigate([`home/accounting/management/vat-invoice/${res.data.id}`]);
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

    gotoList() {
        this._router.navigate([`home/accounting/management/vat-invoice`]);
    }

    ngOnDestroy() {
        this._store.dispatch(InitPartner());
    }
}
