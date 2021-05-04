import { Component, OnInit, ViewChild } from '@angular/core';
import { formatDate } from '@angular/common';
import { ReceiptModel, ReceiptInvoiceModel } from '@models';
import { AppForm } from '@app';
import { Router, ActivatedRoute } from '@angular/router';
import { RoutingConstants, SystemConstants } from '@constants';
import { InfoPopupComponent } from '@common';
import { AccountingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';

import { ARCustomerPaymentFormCreateReceiptComponent } from '../components/form-create-receipt/form-create-receipt.component';
import { ARCustomerPaymentReceiptPaymentListComponent } from '../components/receipt-payment-list/receipt-payment-list.component';
import { IAppState } from '@store';
import { Store } from '@ngrx/store';
import { ResetInvoiceList } from '../store/actions';
import { combineLatest, empty } from 'rxjs';
import { ReceiptCreditListState, ReceiptDebitListState } from '../store/reducers';

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
    type: string = null;
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
        this.initSubmitClickSubscription(() => { this.saveReceipt('draft') });
        this._activedRoute.queryParams.subscribe((param: any) => {
            if (!!param) {
                this.type = param.type;
            }
        })
    }

    saveReceipt(type: string) {
        this.formCreate.isSubmitted = true;
        this.listInvoice.isSubmitted = true;
        this.listInvoice.receiptCreditList.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        // if (!this.listInvoice.invoices.length) {
        //     this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");
        //     return;
        // }
        // if (this.listInvoice.invoices.some(x => x.paymentStatus === AccountingConstants.PAYMENT_STATUS.PAID)) {
        //     this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");

        //     return;
        // }

        // if (!this.checkValidateBalance(this.listInvoice.invoices, +this.listInvoice.finalPaidAmount.value, +this.listInvoice.balance.value)) {
        //     this._toastService.warning(this.invalidBalance, 'Warning');
        //     return;
        // }
        const receiptModel: ReceiptModel = this.getDataForm();

        let paymentList = [];
       combineLatest([
            this._store.select(ReceiptDebitListState),
            this._store.select(ReceiptCreditListState)])
            .subscribe(x=> {
                x.forEach((element: ReceiptInvoiceModel[]) => {
                    if(element.length > 0){
                        console.log('element', element)
                        element.map(item => paymentList.push(item))
                    }
                });
            }
                )
        console.log('list', paymentList)
        if (paymentList.length === 0) {
            this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");
            return;
        }
        if (paymentList.filter((x: ReceiptInvoiceModel) => x.type === 'Debit' || x.type === 'OBH').length === 0) {
            this._toastService.warning("You can't save without debit in this period, Please check it again!");
            return;
        }
        if (paymentList.filter((x: ReceiptInvoiceModel) => x.type === 'Credit' && !x.invoiceNo).length > 0) {
            this._toastService.warning("Please select invoice no!");
            return;
        }
        receiptModel.payments = paymentList;
        // if (receiptModel.payments.some(x => x.type === 'ADV')) {
        //     receiptModel.payments.forEach(inv => {
        //         inv.receiptExcPaidAmount = inv.paidAmount;
        //     });
        // }

        // this.onSaveDataReceipt(receiptModel, type);
    }
    
    getDataForm() {
        const dataForm: any = Object.assign({}, this.formCreate.formSearchInvoice.getRawValue(), this.listInvoice.form.getRawValue());

        const formMapValue: any = {
            fromDate: !!dataForm.date?.startDate ? formatDate(dataForm.date?.startDate, 'yyyy-MM-dd', 'en') : null,
            toDate: !!dataForm.date?.endDate ? formatDate(dataForm.date?.endDate, 'yyyy-MM-dd', 'en') : null,
            paymentDate: !!dataForm.paymentDate?.startDate ? formatDate(dataForm.paymentDate?.startDate, 'yyyy-MM-dd', 'en') : null,
            type: this.type,
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
        // const paidAmount = invoices.filter(x => x.type !== 'ADV').reduce((acc: number, curr: ReceiptInvoiceModel) => acc += (curr.paidAmount + curr.invoiceBalance), 0);
        // if (+paidAmount + balance !== finalPaid) {
        //     valid = false;
        // }

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
                        this._store.dispatch(ResetInvoiceList());
                        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/receipt/${res.data.id}`]);
                        return;
                    }
                    this._toastService.error("Create data fail, Please check again!");
                },
                // (res: HttpErrorResponse) => {
                //     if (res.error.code === SystemConstants.HTTP_CODE.EXISTED) {
                //         this.formCreate.paymentRefNo.setErrors({ existed: true });
                //     }
                // }
            )
    };

    changeDebitList(event){
        if(event){
            this.listInvoice.caculatorAmountFromDebitList();
        }
    }

    gotoList() {
        this._store.dispatch(ResetInvoiceList());
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}`]);

    }
}
