import { Component, OnInit, ViewChild } from '@angular/core';
import { formatDate } from '@angular/common';
import { ReceiptModel, ReceiptInvoiceModel } from '@models';
import { AppForm } from '@app';
import { Router } from '@angular/router';
import { RoutingConstants, SystemConstants, AccountingConstants } from '@constants';
import { InfoPopupComponent } from '@common';
import { AccountingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';

import { ARCustomerPaymentFormCreateReceiptComponent } from '../components/form-create-receipt/form-create-receipt.component';
import { ARCustomerPaymentReceiptPaymentListComponent } from '../components/receipt-payment-list/receipt-payment-list.component';
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

    invalidBalance: string = 'Total Paid Amount is not matched with Final Paid Amount, Please check it and Click Process Clear to update new value!';

    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _accountingRepo: AccountingRepo

    ) {
        super();
    }

    ngOnInit(): void { }

    saveReceipt(type: string) {
        this.formCreate.isSubmitted = true;
        this.listInvoice.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        if (!this.listInvoice.invoices.length) {
            this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");
            return;
        }
        if (this.listInvoice.invoices.some(x => x.paymentStatus === AccountingConstants.PAYMENT_STATUS.PAID)) {
            this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");

            return;
        }

        if (!this.checkValidateBalance(this.listInvoice.invoices, +this.listInvoice.finalPaidAmount.value, +this.listInvoice.balance.value)) {
            this._toastService.warning(this.invalidBalance, 'Warning');
            return;
        }
        const receiptModel: ReceiptModel = this.getDataForm();
        receiptModel.payments = this.listInvoice.invoices;

        if (receiptModel.payments.some(x => x.type === 'ADV')) {
            receiptModel.payments.forEach(inv => {
                inv.receiptExcPaidAmount = inv.paidAmount;
            });
        }

        this.onSaveDataReceipt(receiptModel, type);
    }

    getDataForm() {
        const dataForm: any = Object.assign({}, this.formCreate.formSearchInvoice.getRawValue(), this.listInvoice.form.getRawValue());

        const formMapValue: any = {
            fromDate: !!dataForm.date?.startDate ? formatDate(dataForm.date?.startDate, 'yyyy-MM-dd', 'en') : null,
            toDate: !!dataForm.date?.endDate ? formatDate(dataForm.date?.endDate, 'yyyy-MM-dd', 'en') : null,
            paymentDate: !!dataForm.paymentDate?.startDate ? formatDate(dataForm.paymentDate?.startDate, 'yyyy-MM-dd', 'en') : null,
            type: dataForm.type?.length ? dataForm.type.toString() : null,
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

    checkValidateBalance(invoices: ReceiptInvoiceModel[], finalPaid: number = 0, balance: number = 0) {
        let valid: boolean = true;
        const paidAmount = invoices.filter(x => x.type !== 'ADV').reduce((acc: number, curr: ReceiptInvoiceModel) => acc += (curr.paidAmount + curr.invoiceBalance), 0);
        if (+paidAmount + balance !== finalPaid) {
            valid = false;
        }

        return valid;
    }

    onSaveDataReceipt(model: ReceiptModel, actionString: string) {
        model.id = SystemConstants.EMPTY_GUID;
        this._accountingRepo.saveReceipt(model, SaveReceiptActionEnum.DRAFT_CREATE)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    console.log(res);
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/customer/receipt/${res.data.id}`]);
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

    gotoList() {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/customer`]);

    }
}
