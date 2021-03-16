import { Router } from '@angular/router';
import { Component, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

import { AppPage } from '@app';
import { AccountingRepo } from '@repositories';
import { RoutingConstants } from '@constants';

import { AdvancePaymentListRequestComponent } from '../components/list-advance-payment-request/list-advance-payment-request.component';
import { AdvancePaymentFormCreateComponent } from '../components/form-create-advance-payment/form-create-advance-payment.component';
@Component({
    selector: 'app-advance-payment-new',
    templateUrl: './add-new-advance-payment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class AdvancePaymentAddNewComponent extends AppPage {

    @ViewChild(AdvancePaymentListRequestComponent, { static: true }) listRequestAdvancePaymentComponent: AdvancePaymentListRequestComponent;
    @ViewChild(AdvancePaymentFormCreateComponent, { static: true }) formCreateComponent: AdvancePaymentFormCreateComponent;

    constructor(
        private _toastService: ToastrService,
        private _accoutingRepo: AccountingRepo,
        private _router: Router,

    ) {
        super();

    }

    ngOnInit() { }

    onChangeCurrency(currency: string) {
        this.listRequestAdvancePaymentComponent.changeCurrency(currency);
        for (const item of this.listRequestAdvancePaymentComponent.listRequestAdvancePayment) {
            item.requestCurrency = currency;
        }
        this.listRequestAdvancePaymentComponent.currency = currency;
    }

    saveAdvancePayment() {
        if (!this.listRequestAdvancePaymentComponent.listRequestAdvancePayment.length) {
            this._toastService.warning(`Advance Payment don't have any request in this period, Please check it again! `, '');
            return;
        }
        if (!this.checkValidateAmountAdvance()) {
            this._toastService.warning(`Total Advance Amount by cash is not exceed 100.000.000 VND `, '');
            return;
        }

        const body = this.getFormData();
        this._accoutingRepo.addNewAdvancePayment(body)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(`${res.data.advanceNo + ' is added successfully'}`, 'Save Success !');

                        //  * go to detail page
                        this._router.navigate([`home/accounting/advance-payment/${res.data.id}`]);
                    } else {
                        this.handleError(null, (data: any) => {
                            this._toastService.error(data.message, data.title);
                        });
                    }
                },
            );
    }

    sendRequest() {
        if (!this.listRequestAdvancePaymentComponent.listRequestAdvancePayment.length) {
            this._toastService.warning(`Advance Payment don't have any request in this period, Please check it again! `, '');
            return;
        }
        if (!this.checkValidateAmountAdvance()) {
            this._toastService.warning(`Total Advance Amount by cash is not exceed 100.000.000 VND `, '');
            return;
        }
        const body = this.getFormData();
        this._accoutingRepo.sendRequestAdvPayment(body)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(`${res.data.advanceNo + 'Save and Send Request successfully'}`, 'Save Success !');
                        this._router.navigate([`${RoutingConstants.ACCOUNTING.ADVANCE_PAYMENT}/${res.data.id}/approve`]);
                    } else {
                        this.handleError(null, (data: any) => {
                            this._toastService.error(data.message, data.title);
                        });
                    }
                },
            );

    }

    getFormData() {
        const body = {
            advanceRequests: this.listRequestAdvancePaymentComponent.listRequestAdvancePayment,
            requester: this.formCreateComponent.requester.value || 'Admin',
            // statusApproval: this.formCreateComponent.statusApproval.value || '',
            paymentMethod: this.formCreateComponent.paymentMethod.value,
            advanceCurrency: this.formCreateComponent.currency.value || 'VND',
            requestDate: formatDate(this.formCreateComponent.requestDate.value.startDate || new Date(), 'yyyy-MM-dd', 'en'),
            deadlinePayment: formatDate(this.formCreateComponent.deadlinePayment.value.startDate || new Date(), 'yyyy-MM-dd', 'en'),
            advanceNote: this.formCreateComponent.note.value || '',
            paymentTerm: this.formCreateComponent.paymentTerm.value || 9,
            bankAccountNo: this.formCreateComponent.bankAccountNo.value,
            bankAccountName: this.formCreateComponent.bankAccountName.value,
            bankName: this.formCreateComponent.bankName.value
        };
        return body;
    }

    checkValidateAmountAdvance(): boolean {
        if (this.listRequestAdvancePaymentComponent.totalAmount > 100000000 && this.formCreateComponent.paymentMethod.value === 'Cash') {
            return false;
        }
        return true;
    }

    back() {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ADVANCE_PAYMENT}`]);
    }

}


