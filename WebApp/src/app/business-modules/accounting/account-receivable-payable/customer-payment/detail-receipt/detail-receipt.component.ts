import { Component, OnInit, ViewChild } from '@angular/core';
import { ARCustomerPaymentCreateReciptComponent, SaveReceiptActionEnum } from '../create-receipt/create-receipt.component';
import { ToastrService } from 'ngx-toastr';
import { AccountingRepo } from '@repositories';
import { Router, ActivatedRoute } from '@angular/router';
import { ReceiptModel, ReceiptInvoiceModel } from '@models';
import { SystemConstants, AccountingConstants } from '@constants';
import { HttpErrorResponse } from '@angular/common/http';
import { pluck, switchMap, tap, concatMap } from 'rxjs/operators';
import { ARCustomerPaymentReceiptSummaryComponent } from '../components/receipt-summary/receipt-summary.component';
import { of } from 'rxjs';

@Component({
    selector: 'app-detail-receipt',
    templateUrl: './detail-receipt.component.html',
})
export class ARCustomerPaymentDetailReceiptComponent extends ARCustomerPaymentCreateReciptComponent implements OnInit {
    @ViewChild(ARCustomerPaymentReceiptSummaryComponent) summary: ARCustomerPaymentReceiptSummaryComponent;

    receiptId: string;
    receiptDetail: ReceiptModel;

    constructor(
        protected _router: Router,
        protected _toast: ToastrService,
        protected _accountingRepo: AccountingRepo,
        protected _activedRoute: ActivatedRoute
    ) {
        super(_router, _toast, _accountingRepo);
    }

    ngOnInit() {
        this._activedRoute.params
            .pipe(
                pluck('id'),
                tap((id: string) => { this.receiptId = id }),
                switchMap((receiptId: string) => this._accountingRepo.getDetailReceipt(this.receiptId))
            )
            .subscribe(
                (res: ReceiptModel) => {
                    if (!!res) {
                        if (res.id === SystemConstants.EMPTY_GUID) {
                            this.gotoList();
                            return;
                        }
                        this.updateDetailForm(res);

                    } else {
                        this.gotoList();
                    }

                },
                (err) => {
                    this.gotoList();
                }
            );
    }

    updateDetailForm(res: ReceiptModel) {
        this.receiptDetail = res;
        console.log(this.receiptDetail);

        this.updateFormCreate(this.receiptDetail);
        this.updateListInvoice(this.receiptDetail);
        this.updateSummary(this.receiptDetail);
    }

    updateFormCreate(res: ReceiptModel) {
        const formMapping = {
            date: !!res.fromDate && !!res.toDate ? { startDate: new Date(res.fromDate), endDate: new Date(res.toDate) } : null,
            customerId: res.customerId,
            agreementId: res.agreementId,
            paymentRefNo: res.paymentRefNo
        };

        this.formCreate.formSearchInvoice.patchValue(formMapping);

        this.formCreate.isReadonly = true;

    }

    updateListInvoice(res: ReceiptModel) {
        const formMapping = {
            type: res.type?.split(","),
            paymentDate: !!res.paymentDate ? { startDate: new Date(res.paymentDate), endDate: new Date(res.paymentDate) } : null,
        };

        this.listInvoice.form.patchValue(this.utility.mergeObject({ ...res }, formMapping));

        this.listInvoice.invoices = res.payments || [];
        (this.listInvoice.customerInfo as any) = { id: res.customerId };

        if (res.status === AccountingConstants.RECEIPT_STATUS.DONE || res.status === AccountingConstants.RECEIPT_STATUS.CANCEL) {
            this.listInvoice.isReadonly = true;
        }
    }

    updateSummary(res: ReceiptModel) {
        this.summary.invoices = [...(res.payments || [])];
        this.summary.calculateInfodataInvoice([...res.payments] || []);
    }

    checkValidateBalance(invoices: ReceiptInvoiceModel[], finalPaid: number = 0, balance: number = 0) {
        let valid: boolean = true;
        if (this.receiptDetail.status !== AccountingConstants.RECEIPT_STATUS.DRAFT) {
            return true;
        }
        const paidAmount = invoices.filter(x => x.type !== 'ADV').reduce((acc: number, curr: ReceiptInvoiceModel) => acc += (curr.paidAmount + curr.invoiceBalance), 0);
        if (+paidAmount + balance !== finalPaid) {
            valid = false;
        }

        return valid;
    }


    onSaveDataReceipt(model: ReceiptModel, actionString: string) {
        model.id = this.receiptDetail.id;
        model.userCreated = this.receiptDetail.userCreated;
        model.userModified = this.receiptDetail.userModified;
        model.datetimeCreated = this.receiptDetail.datetimeCreated;
        model.datetimeModified = this.receiptDetail.datetimeModified;
        model.status = this.receiptDetail.status;
        model.syncStatus = this.receiptDetail.syncStatus;
        model.lastSyncDate = this.receiptDetail.lastSyncDate;

        if (!actionString) {
            return;
        }
        let action: number;
        switch (actionString) {
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
        if (!action) { return; };
        this._accountingRepo.saveReceipt(model, action)
            .pipe(
                concatMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        return this._accountingRepo.getDetailReceipt(this.receiptId);
                    }
                    of(res);
                })
            )
            .subscribe(
                (res: any) => {
                    if (!!res && res.status === false) {
                        this._toastService.error(res.message);
                        return;
                    }
                    this.updateDetailForm(res);
                },
                (res: HttpErrorResponse) => {
                    if (res.error.code === SystemConstants.HTTP_CODE.EXISTED) {
                        this.formCreate.paymentRefNo.setErrors({ existed: true });
                    }
                }
            )
    };

    onSyncBravo() { }

}
