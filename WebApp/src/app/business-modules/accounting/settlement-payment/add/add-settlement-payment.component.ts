import { HttpErrorResponse } from '@angular/common/http';
import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { formatDate } from '@angular/common';
import { NgProgress } from '@ngx-progressbar/core';

import { AppPage } from '@app';
import { Surcharge } from '@models';
import { AccountingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { RoutingConstants } from '@constants';
import { DataService } from '@services';

import { SettlementListChargeComponent } from '../components/list-charge-settlement/list-charge-settlement.component';
import { SettlementFormCreateComponent } from '../components/form-create-settlement/form-create-settlement.component';

import { catchError, finalize } from 'rxjs/operators';
@Component({
    selector: 'app-settle-payment-new',
    templateUrl: './add-settle-payment.component.html'
})

export class SettlementPaymentAddNewComponent extends AppPage {

    @ViewChild(SettlementListChargeComponent) requestSurchargeListComponent: SettlementListChargeComponent;
    @ViewChild(SettlementFormCreateComponent) formCreateSurcharge: SettlementFormCreateComponent;

    constructor(
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _progressService: NgProgress,
        private cdRef: ChangeDetectorRef,
        private _dataService: DataService
    ) {
        super();

        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {

    }

    ngAfterViewInit() {
        this.requestSurchargeListComponent.isShowButtonCopyCharge = true;
        this.cdRef.detectChanges(); // * Force to update view
    }

    onChangeCurrency(currency: string) {
        if (!!this.requestSurchargeListComponent) {
            this.requestSurchargeListComponent.changeCurrency(currency);
        }
    }

    saveSettlement() {
        if (!this.requestSurchargeListComponent.surcharges.length) {
            this._toastService.warning(`Settlement payment don't have any surcharge in this period, Please check it again! `, '');
            return;
        }
        this.requestSurchargeListComponent.surcharges.forEach(s => {
            if (!!s.invoiceDate && typeof s.invoiceDate !== 'string') {
                s.invoiceDate = formatDate(s.invoiceDate, 'yyyy-MM-dd', 'en');
            }
        });

        this._progressRef.start();
        const body: IDataSettlement = {
            settlement: this.getDataForm(),
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };

        this._accountingRepo.addNewSettlement(body)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        this._router.navigate([`${RoutingConstants.ACCOUNTING.SETTLEMENT_PAYMENT}/${res.data.settlement.id}`]);
                    } else {
                        this._toastService.warning(res.message, '', { enableHtml: true });
                    }
                    this.requestSurchargeListComponent.selectedIndexSurcharge = null;
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

    saveAndSendRequest() {
        if (!this.requestSurchargeListComponent.surcharges.length) {
            this._toastService.warning(`Settlement payment don't have any surcharge in this period, Please check it again! `, '');
            return;
        }
        this._progressRef.start();
        const body: IDataSettlement = {
            settlement: this.getDataForm(),
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };

        this._accountingRepo.saveAndSendRequestSettlemntPayment(body)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(`${res.data.settlement.settlementNo}`, ' Send request successfully');

                        this._router.navigate([`${RoutingConstants.ACCOUNTING.SETTLEMENT_PAYMENT}/${res.data.settlement.id}/approve`]);
                    } else {
                        this._toastService.warning(res.message, '', { enableHtml: true });
                    }
                    this.requestSurchargeListComponent.selectedIndexSurcharge = null;

                }
            );
    }

    getDataForm() {
        const dataSettle = {
            id: "00000000-0000-0000-0000-000000000000",
            settlementNo: this.formCreateSurcharge.settlementNo.value,
            requester: this.formCreateSurcharge.requester.value,
            requestDate: formatDate(this.formCreateSurcharge.requestDate.value.startDate || new Date(), 'yyyy-MM-dd', 'en'),
            paymentMethod: this.formCreateSurcharge.paymentMethod.value.value,
            settlementCurrency: this.formCreateSurcharge.currency.value,
            note: this.formCreateSurcharge.note.value,
            payee: this.formCreateSurcharge.payee.value,
        };

        return dataSettle;
    }

    back() {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.SETTLEMENT_PAYMENT}`]);
    }



}
interface IDataSettlement {
    settlement: any;
    shipmentCharge: Surcharge[];
}

