import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

import { AppPage } from '@app';
import { Surcharge } from '@models';
import { AccountingRepo, ExportRepo } from '@repositories';
import { ReportPreviewComponent } from '@common';
import { InjectViewContainerRefDirective } from '@directives';
import { RoutingConstants } from '@constants';
import { ICrystalReport } from '@interfaces';
import { delayTime } from '@decorators';
import { DataService } from '@services';

import { SettlementListChargeComponent } from '../components/list-charge-settlement/list-charge-settlement.component';
import { SettlementFormCreateComponent } from '../components/form-create-settlement/form-create-settlement.component';

import { catchError, finalize, pluck } from 'rxjs/operators';
import isUUID from 'validator/lib/isUUID';
@Component({
    selector: 'app-settlement-payment-detail',
    templateUrl: './detail-settlement-payment.component.html',
})

export class SettlementPaymentDetailComponent extends AppPage implements ICrystalReport {

    @ViewChild(SettlementListChargeComponent) requestSurchargeListComponent: SettlementListChargeComponent;
    @ViewChild(SettlementFormCreateComponent, { static: true }) formCreateSurcharge: SettlementFormCreateComponent;
    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;
    @ViewChild(InjectViewContainerRefDirective) public reportContainerRef: InjectViewContainerRefDirective;

    settlementId: string = '';
    settlementCode: string = '';
    settlementPayment: ISettlementPaymentData;

    constructor(
        private _activedRouter: ActivatedRoute,
        private _accoutingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _progressService: NgProgress,
        private _exportRepo: ExportRepo,
        private _dataService: DataService
    ) {
        super();

        this._progressRef = this._progressService.ref();
    }


    ngOnInit() {
        this._activedRouter.params
            .pipe(pluck('id'))
            .subscribe((id: string) => {
                if (!!id && isUUID(id)) {
                    this.settlementId = id;
                    this.getDetailSettlement(this.settlementId, 'LIST');
                } else {
                    this.back();
                }
            });
    }

    onChangeCurrency(currency: string) {
        if (!!this.requestSurchargeListComponent) {
            this.requestSurchargeListComponent.changeCurrency(currency);
        }
    }

    getBodySettlement() {
        const settlement = {
            id: this.settlementPayment.settlement.id,
            settlementNo: this.formCreateSurcharge.settlementNo.value,
            requester: this.formCreateSurcharge.requester.value,
            requestDate: formatDate(this.formCreateSurcharge.requestDate.value.startDate || new Date(), 'yyyy-MM-dd', 'en'),
            paymentMethod: this.formCreateSurcharge.paymentMethod.value.value,
            settlementCurrency: this.formCreateSurcharge.currency.value,
            note: this.formCreateSurcharge.note.value,
            userCreated: this.settlementPayment.settlement.userCreated,
            datetimeCreated: this.settlementPayment.settlement.datetimeCreated,
            statusApproval: this.settlementPayment.settlement.statusApproval,
            payee: this.formCreateSurcharge.payee.value
        };

        return settlement;
    }

    formatInvoiceDateSurcharge() {
        this.requestSurchargeListComponent.surcharges.forEach(s => {
            if (!!s.invoiceDate && typeof s.invoiceDate !== 'string') {
                if (Object.prototype.toString.call(s.invoiceDate) === '[object Date]') {
                    s.invoiceDate = formatDate(s.invoiceDate, 'yyyy-MM-dd', 'en');
                }
            }
        });
    }

    updateSettlement() {
        if (!this.requestSurchargeListComponent.surcharges.length) {
            this._toastService.warning(`Settlement payment don't have any surcharge in this period, Please check it again! `, '');
            return;
        }
        this.formatInvoiceDateSurcharge();
        this._progressRef.start();
        const body: any = {
            settlement: this.getBodySettlement(),
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };

        this._accoutingRepo.updateSettlementPayment(body)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.getDetailSettlement(this.settlementId, 'GROUP');
                    } else {
                        this._toastService.warning(res.message, '', { enableHtml: true });
                    }
                },
                (error) => {
                    if (error instanceof HttpErrorResponse) {
                        if (error.error?.data) {
                            this._dataService.setData('duplicateChargeSettlement', error.error);
                        }
                    }
                }
            );
    }

    getDetailSettlement(settlementId: string, typeCharge: string) {
        this._progressRef.start();
        this._accoutingRepo.getDetailSettlementPayment(settlementId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: ISettlementPaymentData) => {
                    if (!res.settlement) {
                        this.back();
                        this._toastService.warning("Settlement not found");
                        return;
                    }
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
                    this.formCreateSurcharge.form.patchValue({
                        settlementNo: this.settlementPayment.settlement.settlementNo,
                        requester: this.settlementPayment.settlement.requester,
                        requestDate: { startDate: new Date(this.settlementPayment.settlement.requestDate), endDate: new Date(this.settlementPayment.settlement.requestDate) },
                        paymentMethod: this.formCreateSurcharge.methods.filter(method => method.value === this.settlementPayment.settlement.paymentMethod)[0],
                        note: this.settlementPayment.settlement.note,
                        statusApproval: this.settlementPayment.settlement.statusApproval,
                        amount: this.settlementPayment.settlement.amount,
                        currency: this.settlementPayment.settlement.settlementCurrency,
                        payee: this.settlementPayment.settlement.payee
                    });

                    this.requestSurchargeListComponent.surcharges = this.settlementPayment.chargeNoGrpSettlement;
                    this.requestSurchargeListComponent.groupShipments = this.settlementPayment.chargeGrpSettlement;

                    this.requestSurchargeListComponent.settlementCode = this.settlementPayment.settlement.settlementNo;
                    this.requestSurchargeListComponent.requester = this.settlementPayment.settlement.requester;

                    // *SWITCH UI TO GROUP LIST SHIPMENT
                    this.requestSurchargeListComponent.TYPE = typeCharge; // ? GROUP/LIST
                    this.requestSurchargeListComponent.STATE = 'WRITE'; //  ? READ/WRITE
                    this.requestSurchargeListComponent.isShowButtonCopyCharge = false;

                    // if (this.requestSurchargeListComponent.groupShipments.length) {
                    //     this.requestSurchargeListComponent.openAllCharge.next(true);
                    // }
                },
            );
    }

    saveAndSendRequest() {
        if (!this.requestSurchargeListComponent.surcharges.length) {
            this._toastService.warning(`Settlement payment don't have any surcharge in this period, Please check it again! `, '');
            return;
        }
        this.formatInvoiceDateSurcharge();
        this._progressRef.start();
        const body: any = {
            settlement: this.getBodySettlement(),
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };

        this._accoutingRepo.saveAndSendRequestSettlemntPayment(body)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(`${res.data.settlement.settlementNo}`, ' Send request successfully');

                        this._router.navigate([`${RoutingConstants.ACCOUNTING.SETTLEMENT_PAYMENT}/${res.data.settlement.id}/approve`]);
                    } else {
                        this._toastService.warning(res.message, '', { enableHtml: true });
                    }
                },
                (error) => {
                    if (error instanceof HttpErrorResponse) {
                        if (error.error?.data) {
                            this._dataService.setData('duplicateChargeSettlement', error.error);
                        }
                    }
                }
            );
    }

    previewSettlementPayment() {
        if (!this.requestSurchargeListComponent.surcharges.length) {
            this._toastService.warning(`Settlement payment don't have any surcharge in this period, Please check it again! `, '');
            return;
        }

        this._progressRef.start();
        this._accoutingRepo.previewSettlementPayment(this.settlementPayment.settlement.settlementNo)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: any) => {
                    this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.reportContainerRef.viewContainerRef);
                    (this.componentRef.instance as ReportPreviewComponent).data = res;

                    this.showReport();

                    this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
                        (v: any) => {
                            this.subscription.unsubscribe();
                            this.reportContainerRef.viewContainerRef.clear();
                        });

                },
            );
    }

    back() {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.SETTLEMENT_PAYMENT}`]);
    }

    exportSettlementPayment(language: string) {
        if (!this.requestSurchargeListComponent.surcharges.length) {
            this._toastService.warning(`Settlement payment don't have any surcharge in this period, Please check it again! `, '');
            return;
        }

        this._progressRef.start();
        this._exportRepo.exportSettlementPaymentDetail(this.settlementPayment.settlement.id, language)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    this.downLoadFile(response, "application/ms-excel", 'Settlement Form - eFMS.xlsx');
                },
            );
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }
}

export interface ISettlementPaymentData {
    chargeGrpSettlement: any;
    chargeNoGrpSettlement: Surcharge[];
    settlement: any;
}

