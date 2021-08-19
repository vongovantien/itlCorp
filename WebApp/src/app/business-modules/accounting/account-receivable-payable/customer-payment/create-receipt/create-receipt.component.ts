import { Component, OnInit, ViewChild } from '@angular/core';
import { formatDate } from '@angular/common';
import { ReceiptModel, ReceiptInvoiceModel } from '@models';
import { AppForm } from '@app';
import { Router, ActivatedRoute } from '@angular/router';
import { RoutingConstants, SystemConstants, AccountingConstants } from '@constants';
import { InfoPopupComponent, ConfirmPopupComponent } from '@common';
import { AccountingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { IAppState } from '@store';
import { Store } from '@ngrx/store';
import { InjectViewContainerRefDirective } from '@directives';
import { HttpErrorResponse } from '@angular/common/http';

import { ARCustomerPaymentFormCreateReceiptComponent } from '../components/form-create-receipt/form-create-receipt.component';
import { ARCustomerPaymentReceiptPaymentListComponent } from '../components/receipt-payment-list/receipt-payment-list.component';
import { ResetInvoiceList, RegistTypeReceipt, GetInvoiceListSuccess } from '../store/actions';
import { ReceiptCreditListState, ReceiptDebitListState, ReceiptTypeState } from '../store/reducers';

import { takeUntil, pluck, tap, switchMap } from 'rxjs/operators';
import { combineLatest, EMPTY } from 'rxjs';

export enum SaveReceiptActionEnum {
    DRAFT_CREATE = 0,
    DRAFT_UPDATE = 1,
    DONE = 2,
    CANCEL = 3,
    BANK_CREATE = 4,
    BANK_DONE = 5,
}

@Component({
    selector: 'app-create-receipt',
    templateUrl: './create-receipt.component.html',
})
export class ARCustomerPaymentCreateReciptComponent extends AppForm implements OnInit {
    @ViewChild(ARCustomerPaymentFormCreateReceiptComponent) formCreate: ARCustomerPaymentFormCreateReceiptComponent;
    @ViewChild(ARCustomerPaymentReceiptPaymentListComponent) listInvoice: ARCustomerPaymentReceiptPaymentListComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;

    invalidBalance: string = 'Total Paid Amount is not matched with Final Paid Amount, Please check it and Click Process Clear to update new value!';
    type: string = null;
    paymentList: ReceiptInvoiceModel[] = [];

    receiptRefId: string = null;
    receiptRefDetail: ReceiptModel;
    titleReceipt: string;
    actionReceiptFromParams: string;

    constructor(
        protected readonly _router: Router,
        protected readonly _toastService: ToastrService,
        protected readonly _accountingRepo: AccountingRepo,
        protected readonly _activedRoute: ActivatedRoute,
        protected readonly _store: Store<IAppState>,

    ) {
        super();
    }

    ngOnInit(): void {
        this.initSubmitClickSubscription((action: string) => { this.saveReceipt(action) });

        this._store.select(ReceiptTypeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(x => {
                this.type = x || 'Customer'
                this.titleReceipt = `Create ${this.type} Receipt`;
            });

        // * Listen Bank Fee/Other/Clear Debit Receipt     
        this._activedRoute.queryParams
            .pipe(
                tap((params: { id: string, action: string }) => {
                    this.receiptRefId = params.id;
                    this.actionReceiptFromParams = params['action'];
                }),
                switchMap((params: { id: string, action: string }) => {
                    if (!!params['id']) {
                        return this._accountingRepo.getDetailReceipt(params['id']);
                    }
                    return EMPTY;
                }),
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.titleReceipt = this.actionReceiptFromParams === 'debit' ? 'Create Clear Debit' : "Create Bank Fee/Other";
                        this.setFormReceiptDefault(res);
                    }
                },
                (err) => {
                    console.log(err);
                }
            );
    }

    saveReceipt(actionString: string) {
        this.formCreate.isSubmitted = true;
        this.listInvoice.isSubmitted = true;
        this.listInvoice.receiptDebitList.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: this.invalidFormText
            })
            return;
        }

        if (!actionString) {
            return;
        }

        let action: number;
        switch (actionString) {
            case 'draft':
                action = SaveReceiptActionEnum.DRAFT_CREATE;
                break;
            case 'update':
                action = SaveReceiptActionEnum.DRAFT_UPDATE;
                break;
            case 'done':
                action = SaveReceiptActionEnum.DONE;
                break;
            case 'cancel':
                action = SaveReceiptActionEnum.CANCEL;
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
                        element.map(item => {
                            this.paymentList.push(item);
                        })
                    }
                });
            })

        if (this.paymentList.length === 0) {
            this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");
            return;
        }
        if (this.paymentList.some(x => (!x.paidAmountVnd || !x.paidAmountUsd))) {
            this._toastService.warning("Paid amount is required");
            return;
        }

        if (this.formCreate.class.value === AccountingConstants.RECEIPT_CLASS.CLEAR_DEBIT &&
            this.paymentList.filter((x: ReceiptInvoiceModel) => x.type === 'DEBIT' || x.type === 'OBH').length === 0
        ) {
            this._toastService.warning("Receipt Type is Wrong, Please You correct it!");
            return;
        }
        if (this.paymentList.filter(x => x.type == 'CREDIT').length) {
            const isCreditHaveInvoice = this.paymentList.filter(x => x.type === "CREDIT").some(x => !x.invoiceNo);
            if (isCreditHaveInvoice) {
                this._toastService.warning("Some credit do not have net off invoice");
                return;
            }
        }

        if (this.paymentList.some(x => x.paymentType !== 'OTHER' && x.isChangeValue == true)) {
            this._toastService.warning('Please you do Process Clear firstly!');
            return;
        }
        if (this.paymentList.some(x => x.totalPaidVnd > 0 && x.type == "DEBIT" && !x.creditNo && (x.totalPaidVnd > x.unpaidAmountVnd || x.totalPaidUsd > x.unpaidAmountUsd))) {
            this._toastService.warning("Total Paid must <= Unpaid");
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
            notifyDepartment: !!dataForm.notifyDepartment ? dataForm.notifyDepartment.toString() : null
        };

        const d = this.utility.mergeObject(dataForm, formMapValue);
        return d;
    }

    checkValidateForm() {
        let valid: boolean = true;
        if (!this.formCreate.formSearchInvoice.valid || !this.listInvoice.form.valid) {
            valid = false;
        }
        if (this.formCreate.class.value?.includes('OBH') ||
            (
                this.formCreate.class.value === AccountingConstants.RECEIPT_CLASS.CLEAR_DEBIT
                && this.listInvoice.paymentMethod.value === AccountingConstants.RECEIPT_PAYMENT_METHOD.INTERNAL
            )) {
            if (!this.listInvoice.obhpartnerId.value) {
                this.listInvoice.obhpartnerId.setErrors({ required: true });
                valid = false;
            } else {
                this.listInvoice.obhpartnerId.setErrors({ required: null });
                valid = true;
            }
        }

        return valid;
    }
    onSaveDataReceipt(model: ReceiptModel, action: number) {
        model.id = SystemConstants.EMPTY_GUID;
        model.referenceId = this.receiptRefId; // * Set Id cho phiếu ngân hàng.
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

    confirmCancelReceipt() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Cancel Receipt',
            body: 'This Receipt will be canceled. Are you sure you want to continue?',
            labelConfirm: 'Yes',
            classConfirmButton: 'btn-danger',
            iconConfirm: 'la la-times'
        }, () => {
            this.submitClick('cancel');
        })
    }

    setFormReceiptDefault(receiptModel: ReceiptModel) {
        this.receiptRefDetail = receiptModel;
        this._store.dispatch(RegistTypeReceipt({ data: receiptModel.type.toUpperCase(), partnerId: receiptModel.customerId }));

        this.setFormCreateDefault(this.receiptRefDetail);
        this.setPaymentListFormDefault(this.receiptRefDetail);
    }

    setFormCreateDefault(res: ReceiptModel) {
        const formMapping = {
            date: !!res.fromDate && !!res.toDate ? { startDate: new Date(res.fromDate), endDate: new Date(res.toDate) } : null,
            customerId: res.customerId,
            agreementId: res.agreementId,
            class: this.actionReceiptFromParams === 'debit' ? AccountingConstants.RECEIPT_CLASS.CLEAR_DEBIT : res.class,
        };

        this.formCreate.formSearchInvoice.patchValue(formMapping);

        this.formCreate.customerName = res.customerName;
        this.formCreate.getContract();

        if (this.actionReceiptFromParams !== 'debit') {
            this.formCreate.isReadonly = true;
        }

        if (this.actionReceiptFromParams === 'bank') {
            const formMappingBank = {
                paymentRefNo: res.paymentRefNo + '_BANK',
            }
            this.formCreate.formSearchInvoice.patchValue(formMappingBank);
            this.formCreate.receiptReference = res.paymentRefNo + res.class;

        } else if (this.actionReceiptFromParams === 'other') {
            this.formCreate.formSearchInvoice.patchValue({ paymentRefNo: res.paymentRefNo + '_OTH001' });
            this.formCreate.receiptReference = res.paymentRefNo + '_' + res.class;
        }
    }

    setPaymentListFormDefault(res: ReceiptModel) {
        if (this.actionReceiptFromParams === 'debit') {
            this.setPaymentListFormForClearDebit(res);
            return;
        } else if (this.actionReceiptFromParams === 'bank') {
            this.setPaymentListDefaultForBankFee(res);
            return;
        }
        this.setListInvoiceDefaultForOther(res);
    }

    setPaymentListDefaultForBankFee(res: ReceiptModel) {
        const formMapping = {
            type: res.type?.split(","),
            paymentDate: !!res.paymentDate ? { startDate: new Date(res.paymentDate), endDate: new Date(res.paymentDate) } : null,
            cusAdvanceAmount: 0,
            creditAmountUsd: 0,
            creditAmountVnd: 0,
            paidAmountVnd: 0,
            paidAmountUsd: 0,
            finalPaidAmountVnd: 0,
            finalPaidAmountUsd: 0,
            paymentMethod: AccountingConstants.RECEIPT_PAYMENT_METHOD.MANAGEMENT_FEE,
        };
        this.listInvoice.form.patchValue(this.utility.mergeObject({ ...res }, formMapping));

        this._store.dispatch(GetInvoiceListSuccess({ invoices: this.generateDefaultDebitList(res) }));

        (this.listInvoice.partnerId as any) = { id: res.customerId };
    }

    setPaymentListFormForClearDebit(res: ReceiptModel) {
        const formMapping = {
            type: res.type?.split(","),
            paymentDate: !!res.paymentDate ? { startDate: new Date(res.paymentDate), endDate: new Date(res.paymentDate) } : null,
            creditAmountUsd: 0,
            creditAmountVnd: 0,
            paidAmountVnd: res.finalPaidAmountVnd,
            paidAmountUsd: res.finalPaidAmountUsd,
            cusAdvanceAmountVnd: res.class === AccountingConstants.RECEIPT_CLASS.ADVANCE ? res.finalPaidAmountVnd : 0,
            cusAdvanceAmountIsd: res.class === AccountingConstants.RECEIPT_CLASS.ADVANCE ? res.finalPaidAmountUsd : 0,

            paymentMethod: res.class?.includes('OBH') ? AccountingConstants.RECEIPT_PAYMENT_METHOD.INTERNAL : AccountingConstants.RECEIPT_PAYMENT_METHOD.OTHER
        };

        this.listInvoice.form.patchValue(this.utility.mergeObject({ ...res }, formMapping));

        this._store.dispatch(GetInvoiceListSuccess({ invoices: [] }));
        (this.listInvoice.partnerId as any) = { id: res.customerId };
    }

    setListInvoiceDefaultForOther(res: ReceiptModel) {
        const formMappingFormOther = {
            type: res.type?.split(","),
            paymentDate: !!res.paymentDate ? { startDate: new Date(res.paymentDate), endDate: new Date(res.paymentDate) } : null,
            cusAdvanceAmount: 0,
            creditAmountUsd: 0,
            creditAmountVnd: 0,
            paidAmountVnd: 0,
            paidAmountUsd: 0,
            finalPaidAmountVnd: 0,
            finalPaidAmountUsd: 0,
            paymentMethod: AccountingConstants.RECEIPT_PAYMENT_METHOD.OTHER_FEE
        };

        this.listInvoice.form.patchValue(this.utility.mergeObject({ ...res }, formMappingFormOther));

        this._store.dispatch(GetInvoiceListSuccess({ invoices: this.generateDefaultDebitList(res) }));

        (this.listInvoice.partnerId as any) = { id: res.customerId };
    }

    generateDefaultDebitList(res: ReceiptModel) {
        const listPaymentWithUnpaid = res.payments.filter(x => (x.type === "DEBIT" || x.type === "OBH")
            && x.paymentStatus === AccountingConstants.PAYMENT_STATUS.PAID_A_PART);
        if (!listPaymentWithUnpaid.length) {
            return;
        }
        listPaymentWithUnpaid.forEach((c: ReceiptInvoiceModel) => {
            if (c.currencyId === 'VND') {
                c.unpaidAmount = c.unpaidAmountVnd - c.paidAmountVnd;
            } else {
                c.unpaidAmount = c.unpaidAmountUsd - c.paidAmountUsd;
            }
            c.unpaidAmountVnd = c.paidAmountVnd = c.totalPaidVnd = c.unpaidAmountVnd - c.paidAmountVnd;
            c.unpaidAmountUsd = c.paidAmountUsd = c.totalPaidUsd = c.unpaidAmountUsd - c.paidAmountUsd;
        })
        listPaymentWithUnpaid.forEach((x: ReceiptInvoiceModel) => {
            x.paidAmountVnd = x.unpaidAmountVnd;
            x.paidAmountUsd = x.unpaidAmountUsd;
            x.notes = "Bank Fee/Other Receipt";
            x.id = SystemConstants.EMPTY_GUID; // ? Reset ID Trường hợp phiếu ngân hàng.
        })

        return listPaymentWithUnpaid;
    }
}
