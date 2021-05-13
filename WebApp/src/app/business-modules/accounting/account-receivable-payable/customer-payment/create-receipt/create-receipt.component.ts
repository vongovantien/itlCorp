import { Component, OnInit, ViewChild } from '@angular/core';
import { formatDate } from '@angular/common';
import { ReceiptModel, ReceiptInvoiceModel } from '@models';
import { AppForm } from '@app';
import { Router, ActivatedRoute } from '@angular/router';
import { RoutingConstants, SystemConstants } from '@constants';
import { InfoPopupComponent, ConfirmPopupComponent } from '@common';
import { AccountingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';

import { ARCustomerPaymentFormCreateReceiptComponent } from '../components/form-create-receipt/form-create-receipt.component';
import { ARCustomerPaymentReceiptPaymentListComponent } from '../components/receipt-payment-list/receipt-payment-list.component';
import { IAppState } from '@store';
import { Store } from '@ngrx/store';
import { ResetInvoiceList, RegistTypeReceipt } from '../store/actions';
import { combineLatest } from 'rxjs';
import { ReceiptCreditListState, ReceiptDebitListState, ReceiptTypeState } from '../store/reducers';
import { InjectViewContainerRefDirective } from '@directives';
import { HttpErrorResponse } from '@angular/common/http';

export enum SaveReceiptActionEnum {
    DRAFT_CREATE = 0,
    DRAFT_UPDATE = 1,
    DONE = 2,
    CANCEL = 3
}

@Component({
    selector: 'app-create-receipt',
    templateUrl: './create-receipt.component.html',
})
export class ARCustomerPaymentCreateReciptComponent extends AppForm implements OnInit {
    @ViewChild(ARCustomerPaymentFormCreateReceiptComponent) formCreate: ARCustomerPaymentFormCreateReceiptComponent;
    @ViewChild(ARCustomerPaymentReceiptPaymentListComponent) listInvoice: ARCustomerPaymentReceiptPaymentListComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;

    invalidBalance: string = 'Total Paid Amount is not matched with Final Paid Amount, Please check it and Click Process Clear to update new value!';
    type: string = null;
    paymentList: ReceiptInvoiceModel[] = [];

    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _accountingRepo: AccountingRepo,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<IAppState>,
    ) {
        super();
    }

    ngOnInit(): void {
        this.initSubmitClickSubscription((action: string) => { this.saveReceipt(action) });
        this._store.select(ReceiptTypeState)
            .pipe()
            .subscribe(x => this.type = x || 'Customer');
    }

    saveReceipt(actionString: string) {
        this.formCreate.isSubmitted = true;
        this.listInvoice.isSubmitted = true;
        this.listInvoice.receiptCreditList.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        if (!actionString) {
            return;
        }

        let action: number;
        switch (actionString) {
            case 'draft':
                action = SaveReceiptActionEnum.DRAFT_CREATE
                break;
            case 'update':
                action = SaveReceiptActionEnum.DRAFT_UPDATE
                break;
            case 'done':
                action = SaveReceiptActionEnum.DONE
                break;
            case 'cancel':
                action = SaveReceiptActionEnum.CANCEL
                break;
            default:
                break;
        }


        const receiptModel: ReceiptModel = this.getDataForm();

        this.paymentList = [];
        this.subscription = combineLatest([
            this._store.select(ReceiptDebitListState),
            this._store.select(ReceiptCreditListState)])
            .subscribe(x => {
                x.forEach((element: ReceiptInvoiceModel[]) => {
                    if (element.length > 0) {
                        element.map(item => this.paymentList.push(item))
                    }
                });
            }
            )
        if (this.paymentList.length === 0) {
            this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");
            return;
        }
        if (this.paymentList.filter((x: ReceiptInvoiceModel) => x.type === 'DEBIT' || x.type === 'OBH').length === 0) {
            this._toastService.warning("You can't save without debit in this period, Please check it again!");
            return;
        }
        if (this.paymentList.filter((x: ReceiptInvoiceModel) => x.type === 'CREDIT' && !x.invoiceNo).length > 0) {
            this._toastService.warning("Please select invoice no!");
            return;
        }

        receiptModel.payments = this.paymentList;
        this.onSaveDataReceipt(receiptModel, action);
    }

    getDataForm() {
        const dataForm: any = Object.assign({}, this.formCreate.formSearchInvoice.getRawValue(), this.listInvoice.form.getRawValue());

        const formMapValue: any = {
            fromDate: !!dataForm.date?.startDate ? formatDate(dataForm.date?.startDate, 'yyyy-MM-dd', 'en') : null,
            toDate: !!dataForm.date?.endDate ? formatDate(dataForm.date?.endDate, 'yyyy-MM-dd', 'en') : null,
            paymentDate: !!dataForm.paymentDate?.startDate ? formatDate(dataForm.paymentDate?.startDate, 'yyyy-MM-dd', 'en') : null,
            type: this.type || 'Customer',
        };

        const d = this.utility.mergeObject(dataForm, formMapValue);

        return d;
    }

    checkValidateForm() {
        let valid: boolean = true;
        if (!this.formCreate.formSearchInvoice.valid || !this.listInvoice.form.valid) {
            valid = false;
        }
        return valid;
    }
    onSaveDataReceipt(model: ReceiptModel, action: number) {
        model.id = SystemConstants.EMPTY_GUID;
        this._accountingRepo.saveReceipt(model, action)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    console.log(res);
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._store.dispatch(ResetInvoiceList());
                        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/receipt/${res.data.id}`]);
                        return;
                    }
                    this._toastService.error("Create data fail, Please check again!");
                },
                (res: HttpErrorResponse) => {
                    if (res.error.code === SystemConstants.HTTP_CODE.EXISTED) {
                        this.formCreate.paymentRefNo.setErrors({ existed: true });
                    }
                }
            )
    };

    caculateReceipt(onChange: boolean) {
        if (onChange) {
            this.listInvoice.caculateAmountFromDebitList();
        }
    }

    gotoList() {
        this._store.dispatch(ResetInvoiceList());
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}`]);

    }

    confirmCancel() {
        let dataList = [];
        combineLatest([
            this._store.select(ReceiptDebitListState),
            this._store.select(ReceiptCreditListState)])
            .subscribe(x => {
                x.forEach((element: ReceiptInvoiceModel[]) => {
                    if (element.length > 0) {
                        element.map(item => dataList.push(item))
                    }
                });
            });

        if (dataList.length > 0) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: 'Do you want to exit without saving?',
            }, () => {
                this.gotoList();
            })
        } else {
            this.gotoList();
        }
    }

    confirmDoneReceipt() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Noted: After you save the receipt, you can not edit. Are you sure do this action?',
            title: 'Alert',
            labelCancel: 'No',
            labelConfirm: 'Yes',
            iconConfirm: 'la la-save'
        }, () => {
            this.submitClick('done');
        })
    }
}
