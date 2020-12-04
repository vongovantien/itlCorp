import { Component, ViewChild, TemplateRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';

import { AppPage } from 'src/app/app.base';
import { AccountingRepo, ExportRepo } from '@repositories';
import { AdvancePayment } from '@models';
import { ReportPreviewComponent, ConfirmPopupComponent } from '@common';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ICrystalReport } from 'src/app/shared/interfaces/report-interface';
import { delayTime } from '@decorators';

import { AdvancePaymentListRequestComponent } from '../../advance-payment/components/list-advance-payment-request/list-advance-payment-request.component';
import { AdvancePaymentFormCreateComponent } from '../../advance-payment/components/form-create-advance-payment/form-create-advance-payment.component';

import { HistoryDeniedPopupComponent } from '../components/popup/history-denied/history-denied.popup';
import { RoutingConstants } from '@constants';

import { catchError, concatMap, finalize, takeUntil } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
    selector: 'app-approve-advance',
    templateUrl: './approve-advance.component.html',
})

export class ApproveAdvancePaymentComponent extends AppPage implements ICrystalReport {

    @ViewChild(AdvancePaymentFormCreateComponent, { static: true }) formCreateComponent: AdvancePaymentFormCreateComponent;
    @ViewChild(AdvancePaymentListRequestComponent, { static: true }) listRequestAdvancePaymentComponent: AdvancePaymentListRequestComponent;
    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;
    @ViewChild('confirmDenyPopup') confirmDenyPopup: ConfirmPopupComponent;
    @ViewChild('confirmApprovePopup') confirmApprovePopup: ConfirmPopupComponent;
    @ViewChild(HistoryDeniedPopupComponent) historyDeniedPopup: HistoryDeniedPopupComponent;

    idAdvPayment: string = '';
    advancePayment: AdvancePayment;
    approveInfo: any = null;

    modalRef: BsModalRef;
    comment: string = '';
    paymentTerm: number;

    constructor(
        private _accoutingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _activedRouter: ActivatedRoute,
        private _modalService: BsModalService,
        private _router: Router,
        private _exportRepo: ExportRepo,
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this._activedRouter.params
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((param: any) => {
                if (!!param && param.id) {
                    this.idAdvPayment = param.id;
                    this.getDetail(this.idAdvPayment);
                }
            });
    }

    @delayTime(1000)
    showReport(): void {
        this.previewPopup.frm.nativeElement.submit();
        this.previewPopup.show();
    }
    getDetail(idAdvance: string) {
        this._progressRef.start();
        this.listRequestAdvancePaymentComponent.isLoading = true;

        this._accoutingRepo.getDetailAdvancePayment(idAdvance)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.advancePayment = new AdvancePayment(res);
                        console.log(this.advancePayment);

                        // * wait to currecy list api
                        this.formCreateComponent.formCreate.patchValue({
                            advanceNo: this.advancePayment.advanceNo,
                            requester: this.advancePayment.requester,
                            requestDate: { startDate: new Date(this.advancePayment.requestDate), endDate: new Date(this.advancePayment.requestDate) },
                            paymentMethod: this.formCreateComponent.methods.filter(method => method.value === this.advancePayment.paymentMethod)[0],
                            statusApproval: this.advancePayment.statusApproval,
                            deadLine: { startDate: new Date(this.advancePayment.deadlinePayment), endDate: new Date(this.advancePayment.deadlinePayment) },
                            note: this.advancePayment.advanceNote,
                            currency: this.advancePayment.advanceCurrency,
                            paymentTerm: this.advancePayment.paymentTerm,
                            bankAccountNo: this.advancePayment.bankAccountNo,
                            bankAccountName: this.advancePayment.bankAccountName,
                            bankName: this.advancePayment.bankName
                        });

                        this.formCreateComponent.formCreate.disable();
                        this.formCreateComponent.isDisabled = true;

                        this.listRequestAdvancePaymentComponent.listRequestAdvancePayment = this.advancePayment.advanceRequests;
                        this.listRequestAdvancePaymentComponent.totalAmount = this.listRequestAdvancePaymentComponent.updateTotalAmount(this.advancePayment.advanceRequests);

                        this.listRequestAdvancePaymentComponent.advanceNo = this.advancePayment.advanceNo;

                        this.paymentTerm = this.advancePayment.paymentTerm;

                        this.getInfoApprove(this.advancePayment.advanceNo);
                    } else {
                        this._toastService.warning('Advance Payment not found');
                        this.back();
                        return;
                    }
                },
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
                (res: CommonInterface.IResult) => {
                    console.log(res);
                    if (res.status) {
                        this._toastService.success(res.message, 'Approve Is Successfull');
                        this.getDetail(this.advancePayment.id);
                    } else {
                        this._toastService.error(res.message, '');
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
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, ' Deny Is Successfull');
                        this.getDetail(this.advancePayment.id);
                    } else {
                        this._toastService.error(res.message, '');
                    }
                },
            );
    }

    previewAdvPayment() {
        this._progressRef.start();
        this._accoutingRepo.previewAdvancePayment(this.idAdvPayment)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    this.showReport();
                },
            );
    }

    back() {
        if (!this.approveInfo.requesterAprDate) {
            this._router.navigate([`${RoutingConstants.ACCOUNTING.ADVANCE_PAYMENT}/${this.idAdvPayment}`]);
        } else {
            window.history.back();
        }
    }

    showModalApprove() {
        this.confirmApprovePopup.show();
    }

    onApprove() {
        this.confirmApprovePopup.hide();
        this.apporve();
    }

    onChangeCurrency(currency: string) {
        this.listRequestAdvancePaymentComponent.changeCurrency(currency);
        for (const item of this.listRequestAdvancePaymentComponent.listRequestAdvancePayment) {
            item.requestCurrency = currency;
        }
        this.listRequestAdvancePaymentComponent.currency = currency;
    }

    exportAdvPayment(lang: string) {
        this._progressRef.start();
        this._exportRepo.exportAdvancePaymentDetail(this.idAdvPayment, lang)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    this.downLoadFile(response, "application/ms-excel", 'Advance Form - eFMS.xlsx');
                },
            );
    }

    recall() {
        this._progressRef.start();
        this._accoutingRepo.recallRequest(this.idAdvPayment)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    console.log(res);
                    if (res.status) {
                        this._toastService.success(res.message, 'Recall Is Successfull');
                        this.getDetail(this.advancePayment.id);
                    } else {
                        this._toastService.error(res.message, '');
                    }
                },
            );
    }

    showInfoDenied() {
        this.historyDeniedPopup.getDeniedComment('Advance', this.advancePayment.advanceNo);
        this.historyDeniedPopup.show();
    }

    updatePaymentTerm(days: number) {
        this._progressRef.start();
        this._accoutingRepo.updatePaymentTerm(this.idAdvPayment, days)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                concatMap(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            return this._accoutingRepo.getDetailAdvancePayment(this.idAdvPayment);
                        }
                        return of(false);
                    }
                )
            )
            .subscribe(
                (response: AdvancePayment | boolean) => {
                    if (response === false) {
                        this._toastService.error("Update payment term fail");
                    } else {
                        console.log(response);
                        this._toastService.success("Update data success");

                        this.advancePayment.datetimeModified = (response as AdvancePayment).datetimeModified;
                        this.advancePayment.userNameModified = (response as AdvancePayment).userNameModified;
                        this.formCreateComponent.formCreate.patchValue({
                            deadLine: { startDate: new Date((response as AdvancePayment).deadlinePayment), endDate: new Date((response as AdvancePayment).deadlinePayment) },
                        });
                    }
                },
            );
    }
}
