import { formatDate } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

import { AppPage } from '@app';
import { RoutingConstants } from '@constants';
import { Surcharge } from '@models';
import { AccountingRepo, SystemFileManageRepo } from '@repositories';
import { DataService } from '@services';
import { ToastrService } from 'ngx-toastr';

import { SettlementFormCreateComponent } from '../components/form-create-settlement/form-create-settlement.component';
import { SettlementListChargeComponent } from '../components/list-charge-settlement/list-charge-settlement.component';

import { InfoPopupComponent } from '@common';
import { InjectViewContainerRefDirective } from '@directives';
import { EMPTY, of } from 'rxjs';
import { catchError, concatMap, finalize, takeUntil } from 'rxjs/operators';
@Component({
    selector: 'app-settle-payment-new',
    templateUrl: './add-settle-payment.component.html'
})

export class SettlementPaymentAddNewComponent extends AppPage {

    @ViewChild(SettlementListChargeComponent) requestSurchargeListComponent: SettlementListChargeComponent;
    @ViewChild(SettlementFormCreateComponent) formCreateSurcharge: SettlementFormCreateComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;

    constructor(
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private cdRef: ChangeDetectorRef,
        private _dataService: DataService,
        private _systemFileRepo: SystemFileManageRepo
    ) {
        super();
    }

    ngOnInit() {
    }

    ngAfterViewInit() {
        this.requestSurchargeListComponent.isShowButtonCopyCharge = true;
        this.cdRef.detectChanges(); // * Force to update view
        if (!!this.formCreateSurcharge) {
            this.requestSurchargeListComponent.requester = this.formCreateSurcharge.requester.value;
        }
    }

    onChangeCurrency(currency: string) {
        if (!!this.requestSurchargeListComponent) {
            this.requestSurchargeListComponent.changeCurrency(currency);
        }
    }

    getPayeeInfo(event: any) {
        if (event === true) {
            if (!!this.requestSurchargeListComponent.surcharges) {
                const partnerId = this.requestSurchargeListComponent.surcharges[0].type === 'OBH' ? this.requestSurchargeListComponent.surcharges[0].payerId
                    : this.requestSurchargeListComponent.surcharges[0].paymentObjectId;
                this.formCreateSurcharge.payee.setValue(partnerId);
                this.formCreateSurcharge.getBeneficiaryInfo();
            }
        }
    }

    checkValidSettle() {
        if (this.formCreateSurcharge.checkStaffPartner()) {
            this._toastService.warning('Payment Method "Net Off Shipment" not use for Staff, Please check again!');
            return false;
        }

        this.formCreateSurcharge.isSubmitted = true;
        if (this.requestSurchargeListComponent.surcharges.length === 0) {
            this._toastService.error("Settlement Payment don't have any charge in this period, Please check it again!");
            return false;
        }
        if (!this.formCreateSurcharge.dueDate.value || !this.formCreateSurcharge.dueDate.value.startDate) {
            return false;
        }
        return true;
    }

    saveSettlement() {
        // if (!this.requestSurchargeListComponent.surcharges.length) {
        //     this._toastService.warning(`Settlement payment don't have any surcharge in this period, Please check it again! `, '');
        //     return;
        // }

        // this.requestSurchargeListComponent.surcharges.forEach(s => {
        //     if (!!s.invoiceDate && typeof s.invoiceDate !== 'string') {
        //         s.invoiceDate = formatDate(s.invoiceDate, 'yyyy-MM-dd', 'en');
        //     }
        // });

        if (!this.checkValidSettle()) {
            return;
        }

        this.formatInvoiceDateSurcharge();

        const body: IDataSettlement = {
            settlement: this.getDataForm(),
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };

        this._accountingRepo.checkIfInvalidFeeShipmentSettle(body)
            .pipe(catchError(this.catchError), finalize(() => this.isLoading = false),
                concatMap((res: CommonInterface.IResult) => {
                    if (!res.status) {
                        this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                            title: 'Warning',
                            body: "<b>You Can't Create Advance/Settlement For These Shipments!</b> because the following shipments unprofitable:</br>" + res.message,
                            class: 'bg-danger'
                        });
                        return EMPTY;
                    } else {
                        return this._accountingRepo.addNewSettlement(body);
                    }
                }))
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
        if (!this.checkValidSettle()) {
            return;
        }
        this.formatInvoiceDateSurcharge();
        const body: IDataSettlement = {
            settlement: this.getDataForm(),
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };

        //this.checkValidAttachEdoc(body.settlement.id);


        let settlementResult: any = {};

        this._accountingRepo.checkValidToSendRequestSettle(body)
            .pipe(catchError(this.catchError), finalize(() => this.isLoading = false),
                concatMap((res: CommonInterface.IResult) => {
                    if (!res.status && !!res.message) {
                        return of(res);
                    }
                    return this._accountingRepo.checkIfInvalidFeeShipmentSettle(body);
                }),
                concatMap((res: CommonInterface.IResult) => {
                    if (!res.status) {
                        res.data = 1;
                        return of(res);
                    }
                    return this._systemFileRepo.CheckAllowSettleEdocSendRequest(body.settlement.id);
                }),
                concatMap((v) => {
                    if (!v) {
                        let data: CommonInterface.IResult = ({
                            data: null,
                            message: 'Please check your Document Type !',
                            status: false
                        })
                        return of(data);
                    }
                    return this._accountingRepo.saveAndSendRequestSettlemntPayment(body);
                }),
                concatMap((res: CommonInterface.IResult) => {
                    if (!res.status) {
                        this.requestSurchargeListComponent.selectedIndexSurcharge = null;
                        return of(res);
                    } else {
                        settlementResult = res.data.settlement;
                        let approve: any = {
                            settlementNo: settlementResult.settlementNo,
                            requester: settlementResult.requester
                        }
                        return this._accountingRepo.updateAndSendMailApprovalSettlement(approve);
                    }
                }),
                catchError(this.catchError),
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.data === 1) {
                        this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                            title: 'Warning',
                            body: "<b>You Can't Create Advance/Settlement For These Shipments!</b> because the following shipments unprofitable:</br>" + res.message,
                            class: 'bg-danger'
                        });
                        return;
                    }
                    if (!res.status) {
                        this._toastService.warning(res.message, '', { enableHtml: true });
                    }
                    else {
                        this._toastService.success(`${settlementResult.settlementNo}`, ' Send request successfully');
                        this._router.navigate([`${RoutingConstants.ACCOUNTING.SETTLEMENT_PAYMENT}/${settlementResult.id}/approve`]);
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
            settlementType: this.requestSurchargeListComponent.isDirectSettlement ? 'DIRECT' : (this.requestSurchargeListComponent.isExistingSettlement ? 'EXISTING' : null),
            bankAccountNo: this.formCreateSurcharge.bankAccountNo.value,
            bankAccountName: this.formCreateSurcharge.beneficiaryName.value,
            bankName: !this.formCreateSurcharge.bankNameDescription.value ? this.formCreateSurcharge.bankNameDescription.value : this.formCreateSurcharge.bankNameDescription.value.normalize("NFD").replace(/[\u0300-\u036f]/g, ""),
            bankCode: this.formCreateSurcharge.bankCode.value,
            dueDate: !!this.formCreateSurcharge.dueDate.value && !!this.formCreateSurcharge.dueDate.value.startDate ? formatDate(this.formCreateSurcharge.dueDate.value.startDate, 'yyyy-MM-dd', 'en') : null
        };

        return dataSettle;
    }

    back() {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.SETTLEMENT_PAYMENT}`]);
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
}
interface IDataSettlement {
    settlement: any;
    shipmentCharge: Surcharge[];
}

