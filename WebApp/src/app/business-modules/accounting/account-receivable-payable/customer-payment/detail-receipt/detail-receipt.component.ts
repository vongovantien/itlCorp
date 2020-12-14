import { Component, OnInit, ViewChild } from '@angular/core';
import { ARCustomerPaymentCreateReciptComponent, SaveReceiptActionEnum } from '../create-receipt/create-receipt.component';
import { ToastrService } from 'ngx-toastr';
import { AccountingRepo } from '@repositories';
import { Router, ActivatedRoute } from '@angular/router';
import { ReceiptModel } from '@models';
import { SystemConstants } from '@constants';
import { HttpErrorResponse } from '@angular/common/http';
import { pluck, switchMap, tap, switchMapTo } from 'rxjs/operators';
import { ARCustomerPaymentReceiptSummaryComponent } from '../components/receipt-summary/receipt-summary.component';

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
                tap((id: string) => { console.log(id); this.receiptId = id }),
                switchMap((receiptId: string) => this._accountingRepo.getDetailReceipt(this.receiptId))
            )
            .subscribe(
                (res: ReceiptModel) => {
                    if (!!res) {
                        this.receiptDetail = res;
                        this.updateFormCreate(this.receiptDetail);
                        this.updateListInvoice(this.receiptDetail);
                        this.updateSummary(this.receiptDetail);
                    } else {
                        this.gotoList();
                    }

                },
                (err) => {
                    this.gotoList();
                }
            );
    }

    updateFormCreate(res: ReceiptModel) {
        const formMapping = {
            date: !!res.fromDate && !!res.toDate ? { startDate: new Date(res.fromDate), endDate: new Date(res.toDate) } : null,
            customerId: res.customerId,
            agreementId: res.agreementId,
            paymentRefNo: res.paymentRefNo
        };

        this.formCreate.formSearchInvoice.patchValue(formMapping);
    }

    updateListInvoice(res: ReceiptModel) {
        const formMapping = {
            type: res.type?.split(","),
            paymentDate: !!res.paymentDate ? { startDate: new Date(res.paymentDate), endDate: new Date(res.paymentDate) } : null,
        };

        this.listInvoice.form.patchValue(this.utility.mergeObject({ ...res }, formMapping));

        this.listInvoice.invoices = res.payments || [];

    }

    updateSummary(res: ReceiptModel) {
        this.summary.invoices = res.payments;
        console.log(res.payments);
        this.summary.calculateInfodataInvoice(res.payments);
    }


    onSaveDataReceipt(model: ReceiptModel) {
        model.id = SystemConstants.EMPTY_GUID;

        const action = SaveReceiptActionEnum.DRAFT_CREATE;
        this._accountingRepo.saveReceipt(model, action)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    console.log(res);
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error("Create data fail, Please check again!");
                    }
                },
                (res: HttpErrorResponse) => {
                    if (res.error.code === SystemConstants.HTTP_CODE.EXISTED) {
                        this.formCreate.paymentRefNo.setErrors({ existed: true });
                    }
                }
            )
    };

}
