import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { ActivatedRoute, Router } from '@angular/router';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';
import { AdvancePayment, Currency } from 'src/app/shared/models';
import { AdvancePaymentFormCreateComponent } from '../components/form-create-advance-payment/form-create-advance-payment.component';
import { formatDate } from '@angular/common';
import { AdvancePaymentListRequestComponent } from '../components/list-advance-payment-request/list-advance-payment-request.component';
import { ToastrService } from 'ngx-toastr';
import { ReportPreviewComponent } from 'src/app/shared/common';

@Component({
    selector: 'app-advance-payment-detail',
    templateUrl: './detail-advance-payment.component.html',
})
export class AdvancePaymentDetailComponent extends AppPage {

    @ViewChild(AdvancePaymentFormCreateComponent, { static: false }) formCreateComponent: AdvancePaymentFormCreateComponent;
    @ViewChild(AdvancePaymentListRequestComponent, { static: true }) listRequestAdvancePaymentComponent: AdvancePaymentListRequestComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;

    progress: any[] = [];
    advancePayment: AdvancePayment;

    advId: string = '';

    dataReport: any = null;
    constructor(
        private _activedRouter: ActivatedRoute,
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _router: Router
    ) {
        super();
    }

    ngOnInit() {

        this._activedRouter.params.subscribe((param: any) => {
            if (!!param.id) {
                this.advId = param.id;
                this.getDetail(this.advId);
            }
        });
    }

    onChangeCurrency(currency: Currency) {
        this.listRequestAdvancePaymentComponent.changeCurrency(currency);
        for (const item of this.listRequestAdvancePaymentComponent.listRequestAdvancePayment) {
            item.requestCurrency = currency.id;
        }
        this.listRequestAdvancePaymentComponent.currency = currency.id;

    }

    getDetail(advanceId: string) {
        this._accoutingRepo.getDetailAdvancePayment(advanceId)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    this.advancePayment = new AdvancePayment(res);

                    // * wait to currecy list api
                    setTimeout(() => {
                        this.formCreateComponent.formCreate.setValue({
                            advanceNo: this.advancePayment.advanceNo,
                            requester: this.advancePayment.requester,
                            requestDate: { startDate: new Date(this.advancePayment.requestDate), endDate: new Date(this.advancePayment.requestDate) },
                            paymentMethod: this.formCreateComponent.methods.filter(method => method.value === this.advancePayment.paymentMethod)[0],
                            department: this.advancePayment.department,
                            deadLine: { startDate: new Date(this.advancePayment.deadlinePayment), endDate: new Date(this.advancePayment.deadlinePayment) },
                            note: this.advancePayment.advanceNote,
                            currency: this.formCreateComponent.currencyList.filter(currency => currency.id === this.advancePayment.advanceCurrency)[0]
                        });
                    }, 100);

                    this.listRequestAdvancePaymentComponent.listRequestAdvancePayment = this.advancePayment.advanceRequests;
                    this.listRequestAdvancePaymentComponent.totalAmount = this.listRequestAdvancePaymentComponent.updateTotalAmount(this.advancePayment.advanceRequests);
                },
                (errors: any) => { },
                () => { }
            );
    }

    updateAdvPayment() {
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
                requestDate: !!this.formCreateComponent.requestDate.value.startDate ? formatDate(this.formCreateComponent.requestDate.value.startDate, 'yyyy-MM-dd', 'vi') : null,
                deadlinePayment: !!this.formCreateComponent.deadLine.value.startDate ? formatDate(this.formCreateComponent.deadLine.value.startDate, 'yyyy-MM-dd', 'vi') : null,
                advanceNote: this.formCreateComponent.note.value || '',
                statusApproval: this.advancePayment.statusApproval,
                advanceNo: this.advancePayment.advanceNo,
                id: this.advancePayment.id,
                UserCreated: this.advancePayment.userCreated,
                DatetimeCreated: this.advancePayment.datetimeCreated
            };
            this._accoutingRepo.updateAdvPayment(body)
                .pipe(
                    catchError(this.catchError)
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(`${res.data.advanceNo + 'is update successfully'}`, 'Update Success !');
                        }
                    },
                    (errors: any) => { },
                    () => { }
                );
        }
    }

    back() {
        this._router.navigate(['home/accounting/advance-payment']);
    }


    previewAdvPayment() {
        this._accoutingRepo.previewAdvancePayment(this.advId)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    setTimeout(() => {
                        this.previewPopup.show();
                    }, 1000);
                 
                },
                (errors: any) => { },
                () => { },
            );
    }
}
