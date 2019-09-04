import { Component, ViewChild, TemplateRef } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { AdvancePaymentListRequestComponent } from '../../advance-payment/components/list-advance-payment-request/list-advance-payment-request.component';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize } from 'rxjs/operators';
import { AdvancePayment, Currency } from 'src/app/shared/models';
import { AdvancePaymentFormCreateComponent } from '../../advance-payment/components/form-create-advance-payment/form-create-advance-payment.component';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { BsModalRef, BsModalService } from 'ngx-bootstrap';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'app-approve-advance',
    templateUrl: './approve-advance.component.html',
    styleUrls: ['./approve-advance.component.scss']
})

export class ApproveAdvancePaymentComponent extends AppPage {

    @ViewChild(AdvancePaymentFormCreateComponent, { static: false }) formCreateComponent: AdvancePaymentFormCreateComponent;
    @ViewChild(AdvancePaymentListRequestComponent, { static: true }) listRequestAdvancePaymentComponent: AdvancePaymentListRequestComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDenyPopup: ConfirmPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmApprovePopup: ConfirmPopupComponent;

    idAdvPayment: string = '';
    advancePayment: AdvancePayment;
    approveInfo: any = {};
    dataReport: any = null;

    modalRef: BsModalRef;
    comment: string = '';

    constructor(
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _activedRouter: ActivatedRoute,
        private _modalService: BsModalService,
        private _router: Router
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

                        this.formCreateComponent.formCreate.disable();
                        this.formCreateComponent.isDisabled = true;

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

                        this.getInfoApprove(this.advancePayment.advanceNo);
                    }
                },
                () => { },
                () => { },
            );
    }

    getInfoApprove(advanceNo: string) {
        this._accoutingRepo.getInfoApprove(advanceNo)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: any) => {
                    this.approveInfo = res;
                },
            );
    }

    apporve() {
        this._progressRef.start();
        this._accoutingRepo.approveAdvPayment(this.idAdvPayment)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: any) => {
                    if (res.success) {
                        this._toastService.success(res.message, 'Approve Is Successfull');
                        this.getInfoApprove(this.advancePayment.advanceNo);
                    }
                },
            );
    }

    openModalDeny(template: TemplateRef<any>) {
        this.confirmDenyPopup.hide();
        this.modalRef = this._modalService.show(template, { backdrop: 'static' });
    }

    showDenyPopup() {
        this.confirmDenyPopup.show();
    }

    onSaveComment() {
        this._progressRef.start();
        this._accoutingRepo.deniedApprove(this.idAdvPayment, this.comment)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.modalRef.hide(); this._progressRef.complete(); })
            )
            .subscribe(
                (res: any) => {
                    if (res.success) {
                        this._toastService.success(res.message, ' Deny Is Successfull');
                        this.getInfoApprove(this.advancePayment.advanceNo);
                    } else {
                        this._toastService.error(res.message, '');
                    }
                },
            );
    }

    preview() {
        this._accoutingRepo.previewAdvancePayment(this.idAdvPayment)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    setTimeout(() => {
                        this.previewPopup.show();
                    }, 1000);

                },
            );
    }

    back() {
        this._router.navigate(['home/accounting/advance-payment']);
    }

    showModalApprove() {
        this.confirmApprovePopup.show();
    }

    onApprove() {
        this.confirmApprovePopup.hide();
        this.apporve();
    }

    onChangeCurrency(currency: Currency) {
        this.listRequestAdvancePaymentComponent.changeCurrency(currency);
        for (const item of this.listRequestAdvancePaymentComponent.listRequestAdvancePayment) {
            item.requestCurrency = currency.id;
        }
        this.listRequestAdvancePaymentComponent.currency = currency.id;
    }
}
