import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { AdvancePaymentListRequestComponent } from '../components/list-advance-payment-request/list-advance-payment-request.component';
import { Currency } from 'src/app/shared/models';
import { AdvancePaymentFormCreateComponent } from '../components/form-create-advance-payment/form-create-advance-payment.component';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';

@Component({
    selector: 'app-advance-payment-new',
    templateUrl: './add-new-advance-payment.component.html',
})

export class AdvancePaymentAddNewComponent extends AppPage {

    @ViewChild(AdvancePaymentListRequestComponent, { static: true }) listRequestAdvancePaymentComponent: AdvancePaymentListRequestComponent;
    @ViewChild(AdvancePaymentFormCreateComponent, { static: true }) formCreateComponent: AdvancePaymentFormCreateComponent;

    constructor(
        private _toastService: ToastrService,
        private _accoutingRepo: AccoutingRepo
    ) {
        super();
    }

    ngOnInit() { }

    onChangeCurrency(currency: Currency) {
        this.listRequestAdvancePaymentComponent.changeCurrency(currency);
    }

    saveAdvancePayment() {
        if (this.listRequestAdvancePaymentComponent.totalAmount > 100000000 && this.formCreateComponent.paymentMethod.value.value === 'Cash') {
            this._toastService.warning(`Total Advance Amount by cash is not exceed 100.000.000 VND `, '', { positionClass: 'toast-bottom-right' });
            return;
        }
        if (!this.listRequestAdvancePaymentComponent.listRequestAdvancePayment.length) {
            this._toastService.warning(`Advance Payment don't have any request in this period, Please check it again! `, '', { positionClass: 'toast-bottom-right' });
            return;
        } else {
            const body = {
                advanceRequests: this.listRequestAdvancePaymentComponent.listRequestAdvancePayment,
                requester: this.formCreateComponent.requester.value || 'Admin',
                department: this.formCreateComponent.department.value || '',
                paymentMethod: this.formCreateComponent.paymentMethod.value.value,
                advanceCurrency: this.formCreateComponent.currency.value.id || 'VND',
                requestDate: formatDate(this.formCreateComponent.requestDate.value || new Date(), 'yyyy-MM-dd', 'vi'),
                deadlinePayment: formatDate(this.formCreateComponent.deadLine.value.startDate || new Date(), 'yyyy-MM-dd', 'vi'),
                advanceNote: this.formCreateComponent.note.value || '',
            };
            this._accoutingRepo.addNewAdvancePayment(body)
                .pipe(
                    catchError(this.catchError)
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(`${res.data.advanceNo + 'is added successfully'}`, 'Save Success !', { positionClass: 'toast-bottom-right' });
                        }
                    },
                    (errors: any) => { },
                    () => { }
                );
        }
    }


}



