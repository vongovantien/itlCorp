import { formatDate } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ISettlementPaymentState, getListEdocState } from '../components/store/reducers/index';
import { SettlementPayment } from './../../../../shared/models/accouting/settlement-payment';

import { AppPage } from '@app';
import { InfoPopupComponent, ReportPreviewComponent } from '@common';
import { RoutingConstants } from '@constants';
import { delayTime } from '@decorators';
import { InjectViewContainerRefDirective } from '@directives';
import { ICrystalReport } from '@interfaces';
import { Surcharge } from '@models';
import { AccountingRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { DataService } from '@services';

import { SettlementFormCreateComponent } from '../components/form-create-settlement/form-create-settlement.component';
import { SettlementListChargeComponent } from '../components/list-charge-settlement/list-charge-settlement.component';

import { Store } from '@ngrx/store';
import { getCurrentUserState } from '@store';
import { EMPTY, Observable, of } from 'rxjs';
import { catchError, concatMap, finalize, pluck, takeUntil } from 'rxjs/operators';
import { ShareBussinessAttachFileV2Component } from 'src/app/business-modules/share-business/components/edoc/files-attach-v2/files-attach-v2.component';
import isUUID from 'validator/lib/isUUID';
import { LoadDetailSettlePayment, LoadDetailSettlePaymentFail, LoadDetailSettlePaymentSuccess } from '../components/store';
@Component({
    selector: 'app-settlement-payment-detail',
    templateUrl: './detail-settlement-payment.component.html',
})

export class SettlementPaymentDetailComponent extends AppPage implements ICrystalReport {

    @ViewChild(SettlementListChargeComponent) requestSurchargeListComponent: SettlementListChargeComponent;
    @ViewChild(SettlementFormCreateComponent, { static: true }) formCreateSurcharge: SettlementFormCreateComponent;
    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;
    @ViewChild(InjectViewContainerRefDirective) public reportContainerRef: InjectViewContainerRefDirective;
    @ViewChild(ShareBussinessAttachFileV2Component) public attachRef: ShareBussinessAttachFileV2Component;

    settlementId: string = '';
    settlementCode: string = '';
    settlementPayment: ISettlementPaymentData;
    totalEDoc: number = 0;

    attachFiles: any[] = [];
    folderModuleName: string = 'Settlement';
    userLogged$: Observable<Partial<SystemInterface.IClaimUser>>;
    advAmount: number = 0;
    //isAttach: boolean = true;

    constructor(
        private _activedRouter: ActivatedRoute,
        private _accoutingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _exportRepo: ExportRepo,
        private _dataService: DataService,
        private _systemFileRepo: SystemFileManageRepo,
        private _store: Store<ISettlementPaymentState>
    ) {
        super();
    }


    ngOnInit() {
        this.subscription = this._activedRouter.params
            .pipe(pluck('id'))
            .subscribe((id: string) => {
                if (!!id && isUUID(id)) {
                    this.settlementId = id;
                    this.getDetailSettlement(this.settlementId, 'LIST');
                } else {
                    this.back();
                }
            });
        this.userLogged$ = this._store.select(getCurrentUserState);
        this._store.select(getListEdocState).pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: any) => {
                    this.totalEDoc = res.length;
                }
            );

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
            settlementType: this.requestSurchargeListComponent.isDirectSettlement ? 'DIRECT' : (this.requestSurchargeListComponent.isExistingSettlement ? 'EXISTING' : null),
            payee: this.formCreateSurcharge.payee.value,
            bankName: !this.formCreateSurcharge.bankNameDescription.value ? this.formCreateSurcharge.bankNameDescription.value : this.formCreateSurcharge.bankNameDescription.value.normalize("NFD").replace(/[\u0300-\u036f]/g, ""),
            bankAccountName: this.formCreateSurcharge.beneficiaryName.value,
            bankAccountNo: this.formCreateSurcharge.bankAccountNo.value,
            bankCode: this.formCreateSurcharge.bankCode.value,
            dueDate: !!this.formCreateSurcharge.dueDate.value && !!this.formCreateSurcharge.dueDate.value.startDate ? formatDate(this.formCreateSurcharge.dueDate.value.startDate, 'yyyy-MM-dd', 'en') : null
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
        if (!this.checkValidSettle()) {
            return;
        }

        this.formatInvoiceDateSurcharge();
        const body: any = {
            settlement: this.getBodySettlement(),
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };
        this._accoutingRepo.checkIfInvalidFeeShipmentSettle(body)
            .pipe(catchError(this.catchError), finalize(() => this.isLoading = false),
                concatMap((res: CommonInterface.IResult) => {
                    if (!res.status) {
                        this.showPopupDynamicRender(InfoPopupComponent, this.reportContainerRef.viewContainerRef, {
                            title: 'Warning',
                            body: "<b>You Can't Create Advance/Settlement For These Shipments!</b> because the following shipments unprofitable:</br>" + res.message,
                            class: 'bg-danger'
                        });
                        return EMPTY;
                    } else {
                        return this._accoutingRepo.updateSettlementPayment(body);
                    }
                }))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.getDetailSettlement(this.settlementId, 'LIST');
                        this.attachRef.getDocumentType('Settlement');
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
        this._store.dispatch(LoadDetailSettlePayment({ id: settlementId }));
        this._accoutingRepo.getDetailSettlementPayment(settlementId, typeCharge)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: ISettlementPaymentData) => {
                    if (!res.settlement) {
                        this.back();
                        this._toastService.warning("Settlement not found");
                        return;
                    }
                    this.settlementPayment = res
                    // * Update store.
                    this._store.dispatch(LoadDetailSettlePaymentSuccess(this.settlementPayment));

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
                        payee: this.settlementPayment.settlement.payee,
                        bankName: this.settlementPayment.settlement.bankName,
                        bankNameDescription: this.settlementPayment.settlement.bankName,
                        beneficiaryName: this.settlementPayment.settlement.bankAccountName,
                        bankAccountNo: this.settlementPayment.settlement.bankAccountNo,
                        advanceAmount: this.settlementPayment.settlement.advanceAmount,
                        balanceAmount: this.settlementPayment.settlement.balanceAmount,
                        bankCode: this.settlementPayment.settlement.bankCode,
                        dueDate: !!this.settlementPayment.settlement.dueDate ? { startDate: new Date(this.settlementPayment.settlement.dueDate), endDate: new Date(this.settlementPayment.settlement.dueDate) } : null
                    });
                    this.requestSurchargeListComponent.surcharges = this.settlementPayment.chargeNoGrpSettlement;
                    this.requestSurchargeListComponent.groupShipments = this.settlementPayment.chargeGrpSettlement;

                    this.requestSurchargeListComponent.settlementCode = this.settlementPayment.settlement.settlementNo;
                    this.settlementCode = this.settlementPayment.settlement.settlementNo;

                    this.requestSurchargeListComponent.requester = this.settlementPayment.settlement.requester;

                    // *SWITCH UI TO GROUP LIST SHIPMENT
                    this.requestSurchargeListComponent.TYPE = typeCharge; // ? GROUP/LIST
                    this.requestSurchargeListComponent.STATE = 'WRITE'; //  ? READ/WRITE
                    this.requestSurchargeListComponent.isShowButtonCopyCharge = false;
                    if (this.settlementPayment.settlement.settlementType === 'DIRECT') {
                        this.requestSurchargeListComponent.isDirectSettlement = true;
                    }
                    if (this.settlementPayment.settlement.settlementType === 'EXISTING') {
                        this.requestSurchargeListComponent.isExistingSettlement = true;
                    }
                },
                () => {
                    this._store.dispatch(LoadDetailSettlePaymentFail());
                }
            );
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
        if ((!this.formCreateSurcharge.dueDate.value || !this.formCreateSurcharge.dueDate.value.startDate) || (!['New', 'Denied'].includes(this.formCreateSurcharge.statusApproval.value) && !this.formCreateSurcharge.form.valid)) {
            return false;
        }
        return true;
    }


    saveAndSendRequest() {
        if (!this.checkValidSettle()) {
            return;
        }

        this.formatInvoiceDateSurcharge();
        const body: any = {
            settlement: this.getBodySettlement(),
            shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        };

        // if (!this.checkValidAttachEdoc(body.settlement.id)) {
        //     this._toastService.error("Please check your Document Type !");
        //     return;
        // }

        let settlementResult: any = {};
        this._accoutingRepo.checkValidToSendRequestSettle(body)
            .pipe(catchError(this.catchError), finalize(() => this.isLoading = false),
                concatMap((res: CommonInterface.IResult) => {
                    if (!res.status && !!res.message) {
                        return of(res);
                    }
                    return this._accoutingRepo.checkIfInvalidFeeShipmentSettle(body);
                }
                ),
                concatMap((res: CommonInterface.IResult) => {
                    if (!res.status) {
                        res.data = 1;
                        return of(res);
                    }
                    return this._systemFileRepo.CheckAllowSettleEdocSendRequest(body.settlement.id);
                },
                ),
                concatMap(
                    (v) => {
                        if (!v) {
                            let data: CommonInterface.IResult = ({
                                data: null,
                                message: 'Please check your Document Type !',
                                status: false
                            })
                            return of(data);
                        }
                        return this._accoutingRepo.saveAndSendRequestSettlemntPayment(body);
                    }
                ),
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
                        return this._accoutingRepo.updateAndSendMailApprovalSettlement(approve);
                    }
                }),
                catchError(this.catchError),
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.data === 1) {
                        this.showPopupDynamicRender(InfoPopupComponent, this.reportContainerRef.viewContainerRef, {
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

    previewSettlementPayment() {
        this._accoutingRepo.previewSettlementPayment(this.settlementPayment.settlement.settlementNo)
            .pipe(catchError(this.catchError))
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

    exportSettlementPayment(language: string, typeExp: string) {

        this._exportRepo.exportSettlementPaymentDetail(this.settlementPayment.settlement.id, language)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe((response: any) => {
                if (response && response.data) {
                    if (typeExp === 'preview') {
                        this._exportRepo.previewExport(response.data);
                    } else {
                        this._exportRepo.downloadExport(response.data);
                    }
                }
            });
    }

    exportSettlementPaymentTemplate(language: string, typeExp: string) {

        this._exportRepo.exportSettlementPaymentDetailTemplate(this.settlementPayment.settlement.id, language)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe((response: any) => {
                if (response && response.data) {
                    if (typeExp === 'preview') {
                        this._exportRepo.previewExport(response.data);
                    } else {
                        this._exportRepo.downloadExport(response.data);
                    }
                }
            });
    }


    exportGeneralPreview(typeExp: string) {

        this._exportRepo.exportGeneralSettlementPayment(this.settlementPayment.settlement.id)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe((response: any) => {
                if (response && response.data) {
                    if (typeExp === 'preview') {
                        this._exportRepo.previewExport(response.data);
                    } else {
                        this._exportRepo.downloadExport(response.data);
                    }
                }
            });
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.ShowWithDelay(); // Gọi method có delay này để ViewChild Popup nó get đc
    }
}

export interface ISettlementPaymentData {
    chargeGrpSettlement: any;
    chargeNoGrpSettlement: Surcharge[];
    settlement: SettlementPayment;
}

