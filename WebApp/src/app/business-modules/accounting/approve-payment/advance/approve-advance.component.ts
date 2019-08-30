import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { AdvancePaymentListRequestComponent } from '../../advance-payment/components/list-advance-payment-request/list-advance-payment-request.component';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute } from '@angular/router';
import { catchError, finalize } from 'rxjs/operators';
import { AdvancePayment } from 'src/app/shared/models';
import { AdvancePaymentFormCreateComponent } from '../../advance-payment/components/form-create-advance-payment/form-create-advance-payment.component';
import { ReportPreviewComponent } from 'src/app/shared/common';

@Component({
    selector: 'app-approve-advance',
    templateUrl: './approve-advance.component.html',
    styleUrls: ['./approve-advance.component.scss']
})

export class ApproveAdvancePaymentComponent extends AppPage {

    @ViewChild(AdvancePaymentFormCreateComponent, { static: false }) formCreateComponent: AdvancePaymentFormCreateComponent;
    @ViewChild(AdvancePaymentListRequestComponent, { static: true }) listRequestAdvancePaymentComponent: AdvancePaymentListRequestComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;

    progress: any[] = [1, 2, 3, 4, 5, 6];
    idAdvPayment: string = '';
    advancePayment: AdvancePayment;


    constructor(
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _activedRouter: ActivatedRoute
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this._activedRouter.params.subscribe((param: any) => {
            if (!!param && param.id) {
                this.idAdvPayment = param.id;
                this.getDetail(this.idAdvPayment);
            }
        });
    }

    getDetail(idAdvance: string) {
        this._progressRef.start();
        this._accoutingRepo.getDetailAdvancePayment(idAdvance)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
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

                        this.listRequestAdvancePaymentComponent.advanceNo = this.advancePayment.advanceNo;
                        console.log(this.advancePayment);
                    }
                },
                () => { },
                () => { },
            );
    }

}
