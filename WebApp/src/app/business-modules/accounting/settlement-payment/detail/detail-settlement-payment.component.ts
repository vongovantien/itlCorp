import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { SettlementListChargeComponent } from '../components/list-charge-settlement/list-charge-settlement.component';
import { SettlementFormCreateComponent } from '../components/form-create-settlement/form-create-settlement.component';
import { Currency, Surcharge } from 'src/app/shared/models';
import { ActivatedRoute, Router } from '@angular/router';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize } from 'rxjs/operators';
import { formatDate } from '@angular/common';
import { ReportPreviewComponent } from 'src/app/shared/common';

@Component({
    selector: 'app-settlement-payment-detail',
    templateUrl: './detail-settlement-payment.component.html'
})

export class SettlementPaymentDetailComponent extends AppPage {

    @ViewChild(SettlementListChargeComponent, { static: false }) requestSurchargeListComponent: SettlementListChargeComponent;
    @ViewChild(SettlementFormCreateComponent, { static: false }) formCreateSurcharge: SettlementFormCreateComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;


    settlementId: string = '';
    settlementCode: string = '';
    settlementPayment: ISettlementPaymentData;

    dataReport: any = null;

    constructor(
        private _activedRouter: ActivatedRoute,
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _progressService: NgProgress
    ) {
        super();

        this._progressRef = this._progressService.ref();

    }

    ngOnInit() {
        this._activedRouter.params.subscribe((param: any) => {
            if (!!param.id) {
                this.settlementId = param.id;
                this.getDetailSettlement(this.settlementId);
            }
        });
    }

    onChangeCurrency(currency: Currency) {
        this.requestSurchargeListComponent.changeCurrency(currency);
    }

    updateSettlement() {
        this._progressRef.start();
        const body: any = {
            settlement: {
                id: this.settlementPayment.settlement.id,
                settlementNo: this.formCreateSurcharge.settlementNo.value,
                requester: this.formCreateSurcharge.requester.value,
                requestDate: formatDate(this.formCreateSurcharge.requestDate.value.startDate || new Date(), 'yyyy-MM-dd', 'en'),
                paymentMethod: this.formCreateSurcharge.paymentMethod.value.value,
                settlementCurrency: this.formCreateSurcharge.currency.value.id,
                note: this.formCreateSurcharge.note.value,
                userCreated: this.settlementPayment.settlement.userCreated,
                datetimeCreated: this.settlementPayment.settlement.datetimeCreated,
                statusApproval: this.settlementPayment.settlement.statusApproval
            },
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };

        this._accoutingRepo.updateSettlementPayment(body)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.getDetailSettlement(this.settlementId);
                    }
                }
            );
    }

    getDetailSettlement(settlementId: string) {
        this._progressRef.start();
        this._accoutingRepo.getDetailSettlementPayment(settlementId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: any) => {
                    this.settlementPayment = res;
                    switch (this.settlementPayment.settlement.statusApproval) {
                        case 'New':
                        case 'Denied':
                            break;
                        default:
                            this.formCreateSurcharge.form.disable();
                            this.formCreateSurcharge.isDisabled = true;

                            this.requestSurchargeListComponent.STATE = 'READ';
                            break;
                    }

                    // * wait to currecy list api
                    setTimeout(() => {
                        this.formCreateSurcharge.form.setValue({
                            settlementNo: this.settlementPayment.settlement.settlementNo,
                            requester: this.settlementPayment.settlement.requester,
                            requestDate: new Date(this.settlementPayment.settlement.requestDate),
                            paymentMethod: this.formCreateSurcharge.methods.filter(method => method.value === this.settlementPayment.settlement.paymentMethod)[0],
                            note: this.settlementPayment.settlement.note,
                            amount: this.settlementPayment.chargeGrpSettlement.reduce((acc, curr) => acc + curr.totalAmount, 0),
                            currency: this.formCreateSurcharge.currencyList.filter(currency => currency.id === this.settlementPayment.settlement.settlementCurrency)[0]
                        });
                    }, 100);

                    this.requestSurchargeListComponent.surcharges = this.settlementPayment.chargeNoGrpSettlement;
                    this.requestSurchargeListComponent.groupShipments = this.settlementPayment.chargeGrpSettlement;

                    this.requestSurchargeListComponent.settlementCode = this.settlementPayment.settlement.settlementNo;

                    // *SWITCH UI TO GROUP LIST SHIPMENT
                    this.requestSurchargeListComponent.TYPE = 'GROUP'; // ? LIST

                    if (this.requestSurchargeListComponent.groupShipments.length) {
                        this.requestSurchargeListComponent.openAllCharge.next(true);
                    }

                },
            );
    }

    saveAndSendRequest() {
        this._progressRef.start();
        const body: any = {
            settlement: {
                id: this.settlementPayment.settlement.id,
                settlementNo: this.formCreateSurcharge.settlementNo.value,
                requester: this.formCreateSurcharge.requester.value,
                requestDate: formatDate(this.formCreateSurcharge.requestDate.value.startDate || new Date(), 'yyyy-MM-dd', 'en'),
                paymentMethod: this.formCreateSurcharge.paymentMethod.value.value,
                settlementCurrency: this.formCreateSurcharge.currency.value.id,
                note: this.formCreateSurcharge.note.value,
                userCreated: this.settlementPayment.settlement.userCreated,
                datetimeCreated: this.settlementPayment.settlement.datetimeCreated,
                statusApproval: this.settlementPayment.settlement.statusApproval
            },
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };

        this._accoutingRepo.saveAndSendRequestSettlemntPayment(body)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(`${res.data.settlement.settlementNo}`, ' Send request successfully');

                        this._router.navigate([`home/accounting/settlement-payment/${res.data.settlement.id}/approve`]);
                       
                    }
                }
            );
    }

    preview() {
        this._accoutingRepo.previewSettlementPayment(this.settlementPayment.settlement.settlementNo)
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
}

export interface ISettlementPaymentData {
    chargeGrpSettlement: any;
    chargeNoGrpSettlement: Surcharge[],
    settlement: any;
}

