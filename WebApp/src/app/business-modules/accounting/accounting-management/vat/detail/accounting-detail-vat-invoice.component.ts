import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { AccountingManagementCreateVATInvoiceComponent } from '../create/accounting-create-vat-invoice.component';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { AccountingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { Store } from '@ngrx/store';
import { AccAccountingManagementModel } from '@models';

import { IAccountingManagementState } from '../../store';

import { tap, switchMap, catchError, finalize, concatMap } from 'rxjs/operators';
import { of } from 'rxjs';
import isUUID from 'validator/lib/isUUID';
import _merge from 'lodash/merge';

@Component({
    selector: 'app-accounting-detail-vat-invoice',
    templateUrl: './accounting-detail-vat-invoice.component.html',
})
export class AccountingManagementDetailVatInvoiceComponent extends AccountingManagementCreateVATInvoiceComponent implements OnInit {

    vatInvoiceId: string;

    accountingManagement: AccAccountingManagementModel = new AccAccountingManagementModel();


    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _accountingRepo: AccountingRepo,
        protected _store: Store<IAccountingManagementState>,
        private _activedRoute: ActivatedRoute,
        private _ngProgressService: NgProgress,
        private _cd: ChangeDetectorRef
    ) {
        super(_toastService, _accountingRepo, _store);
        this._progressRef = this._ngProgressService.ref();
    }


    ngOnInit(): void {
        this._activedRoute.params.pipe(
            tap((param: Params) => {
                this.vatInvoiceId = !!param.vatInvoiceId ? param.vatInvoiceId : '';
            }),
            switchMap(() => of(this.vatInvoiceId)),
        )
            .subscribe(
                (vatInvoiceId: string) => {
                    if (isUUID(vatInvoiceId)) {
                        this.getDetailVatInvoice(vatInvoiceId);
                    } else {
                        this.gotoList();
                    }
                }
            );
    }

    getDetailVatInvoice(id: string) {
        this._accountingRepo.getDetailAcctMngt(id)
            .subscribe(
                (res: AccAccountingManagementModel) => {
                    this.accountingManagement = new AccAccountingManagementModel(res);
                    this.updateFormInvoice(res);
                    this.updateChargeList(res);

                }
            );
    }

    updateFormInvoice(res: AccAccountingManagementModel) {
        const formData: AccAccountingManagementModel | any = {
            date: !!res.date ? { startDate: new Date(res.date), endDate: new Date(res.date) } : null,
            paymentMethod: !!res.paymentMethod ? [{ id: res.paymentMethod, text: res.paymentMethod }] : null,
            currency: !!res.currency ? [{ id: res.currency, text: res.currency }] : null,

        };
        this.formCreateComponent.formGroup.patchValue(Object.assign(_merge(res, formData)));
    }

    updateChargeList(res: AccAccountingManagementModel) {
        this.listChargeComponent.charges = res.charges;
        this.listChargeComponent.updateTotalAmount();
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
        modelAdd.charges = [...this.listChargeComponent.charges];

        //  * Update field
        modelAdd.id = this.vatInvoiceId;
        modelAdd.status = this.accountingManagement.status;
        modelAdd.type = this.accountingManagement.type;
        modelAdd.companyId = this.accountingManagement.companyId;
        modelAdd.officeId = this.accountingManagement.officeId;
        modelAdd.departmentId = this.accountingManagement.departmentId;
        modelAdd.groupId = this.accountingManagement.groupId;
        modelAdd.userCreated = this.accountingManagement.userCreated;
        modelAdd.datetimeCreated = this.accountingManagement.datetimeCreated;

        this.saveInvoice(modelAdd);
    }

    saveInvoice(body: AccAccountingManagementModel) {
        this._progressRef.start();
        this._accountingRepo.updateAcctMngt(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                concatMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        return this._accountingRepo.getDetailAcctMngt(this.vatInvoiceId);
                    }
                    of(res);
                })
            )
            .subscribe(
                (res: CommonInterface.IResult | AccAccountingManagementModel | any) => {
                    if (!!res && res.status === false) {
                        this._toastService.error(res.message);
                    } else {
                        this.updateFormInvoice(res);
                    }
                }
            );
    }


    gotoList() {
        this._router.navigate(["home/accounting/management/vat-invoice"]);
    }
}
