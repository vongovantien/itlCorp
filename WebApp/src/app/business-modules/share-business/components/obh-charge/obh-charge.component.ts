import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Input } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { ShareBussinessBuyingChargeComponent } from '../buying-charge/buying-charge.component';
import { CatalogueRepo, DocumentationRepo, AccountingRepo } from '@repositories';
import { SortService } from '@services';
import { CommonEnum } from 'src/app/shared/enums/common.enum';

import { takeUntil, catchError, finalize, switchMap } from 'rxjs/operators';
import { CsShipmentSurcharge, Partner, Charge, Unit } from '@models';

import * as fromStore from './../../store';
import { NgxSpinnerService } from 'ngx-spinner';
import { ActivatedRoute } from '@angular/router';
import { getCatalogueCurrencyState, getCatalogueUnitState } from '@store';

@Component({
    selector: 'obh-charge',
    templateUrl: './obh-charge.component.html',
    styleUrls: ['./../buying-charge/buying-charge.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush

})

export class ShareBussinessOBHChargeComponent extends ShareBussinessBuyingChargeComponent {
    TYPE: any = CommonEnum.SurchargeTypeEnum.OBH;

    messageCreditRate: string = '';

    @Input() allowSaving: boolean = true; // * not allow to save or add Charges without saving the job

    listPartnerPayee: any[] = [];

    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _store: Store<fromStore.IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _sortService: SortService,
        protected _ngProgressService: NgProgress,
        protected _spinner: NgxSpinnerService,
        protected _accountingRepo: AccountingRepo,
        protected _activedRoute: ActivatedRoute,
        protected _cd: ChangeDetectorRef


    ) {
        super(
            _catalogueRepo,
            _store,
            _documentRepo,
            _toastService,
            _sortService,
            _ngProgressService,
            _spinner,
            _accountingRepo,
            _activedRoute,
            _cd);
        this._progressRef = this._ngProgressService.ref();
    }

    getPartner() {
        this._spinner.show(this.spinnerpartner);

        this._store.select(fromStore.getTransactionDetailCsTransactionState)
            .pipe(
                switchMap(
                    (data) => {
                        if (!!data?.officeId) {
                            return this._catalogueRepo.getPartnerByGroups(
                                [CommonEnum.PartnerGroupEnum.AGENT, CommonEnum.PartnerGroupEnum.CUSTOMER],
                                true,
                                this.serviceTypeId,
                                data?.officeId
                            ).pipe(
                                finalize(() => {
                                    this._spinner.hide(this.spinnerpartner);
                                    this.isShowLoadingPartner = false;
                                }),
                            );
                        }
                    }
                ),
                catchError(this.catchError),
                takeUntil(this.ngUnsubscribe),
            ).subscribe(
                (partners: any[]) => {
                    this.listPartner = partners;
                }
            );

        this._catalogueRepo.getListPartner(null, null, { active: true })
            .subscribe(
                (partners: Partner[]) => {
                    console.log(partners)
                    this.listPartnerPayee = partners;
                }
            );
    }

    getCurrency() {
        this.listCurrency = this._store.select(getCatalogueCurrencyState);
    }

    getUnits() {
        this._store.select(getCatalogueUnitState)
            .pipe(catchError(this.catchError))
            .subscribe(
                (units: Unit[]) => {
                    this.listUnits = units;
                }
            );
    }

    getSurcharge() {
        this._store.select(fromStore.getOBHSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                    this._cd.markForCheck();
                }
            );
    }

    configHeader() {
        this.headers = [
            { title: 'Payee', field: 'receiverName', required: true, sortable: true, width: 150 },
            { title: 'OBH Partner', field: 'payerName', required: true, sortable: true, width: 200 },
            { title: 'Charge', field: 'chargeId', required: true, sortable: true, width: 250 },
            { title: 'Quantity', field: 'quantity', required: true, sortable: true, width: 150 },
            { title: 'Unit', field: 'unitId', required: true, sortable: true },
            { title: 'Unit Price', field: 'unitPrice', required: true, sortable: true },
            { title: 'Currency', field: 'currencyId', required: true, sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Total', field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Series No', field: 'seriesNo', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'VAT Partner', field: 'vatPartnerId', sortable: true, width: 200 },
            { title: 'SOA', field: 'soano', sortable: true, width: 170 },
            { title: 'Credit/Debit Note', field: 'cdno', sortable: true, width: 230 },
            { title: 'Settle Payment', field: 'settlementCode', sortable: true },
            { title: 'Exchange Rate Date', field: 'exchangeDate', sortable: true },
            { title: 'Final Exchange Rate', field: 'finalExchangeRate', sortable: true },
            { title: 'Voucher ID', field: 'voucherId', sortable: true, width: 210 },
            { title: 'Voucher ID Date', field: 'voucherIddate', sortable: true, width: 160 },
            { title: 'Net Amount', field: 'netAmount', sortable: true },
        ];
    }

    getCharge() {
        this._catalogueRepo.getCharges({ active: true, serviceTypeId: this.serviceTypeId, type: CommonEnum.CHARGE_TYPE.OBH }).subscribe(
            (charges: Charge[]) => {
                this.listCharges = charges;
            }
        );
    }

    selectPartnerTypes(partnerType: CommonInterface.IValueDisplay, chargeItem: CsShipmentSurcharge, type: string) {
        chargeItem.objectBePaid = null;
        switch (type) {
            case 'receiver':
                switch (partnerType.value) {
                    case CommonEnum.PartnerGroupEnum.CUSTOMER:
                        chargeItem.receiverName = this.hbl.customerName;
                        if (!chargeItem.receiverName) {
                            chargeItem.receiverName = this.listPartner.find(p => p.id === this.hbl.customerId).partnerNameEn;
                        }
                        chargeItem.paymentObjectId = this.hbl.customerId;
                        break;
                    case CommonEnum.PartnerGroupEnum.CARRIER:
                        chargeItem.receiverName = this.shipment.supplierName;
                        chargeItem.paymentObjectId = this.shipment.coloaderId;
                        break;
                    case CommonEnum.PartnerGroupEnum.AGENT:
                        chargeItem.receiverName = this.shipment.agentName;
                        chargeItem.paymentObjectId = this.shipment.agentId;
                        break;
                    default:
                        break;
                }
                break;
            case 'payer':
                switch (partnerType.value) {
                    case CommonEnum.PartnerGroupEnum.CUSTOMER:
                        console.log(this.hbl);
                        chargeItem.payerName = this.hbl.customerName;
                        chargeItem.payerShortName = this.listPartnerPayee.find(p => p.id === this.hbl.customerId).shortName;
                        chargeItem.payerId = this.hbl.customerId;

                        if (chargeItem.invoiceNo) {
                            chargeItem.vatPartnerId = chargeItem.payerId;
                        }
                        break;
                    case CommonEnum.PartnerGroupEnum.CARRIER:
                        chargeItem.payerName = this.shipment.supplierName;
                        chargeItem.payerId = this.shipment.coloaderId;
                        chargeItem.payerShortName = this.listPartnerPayee.find(p => p.id === this.shipment.coloaderId).shortName;
                        break;
                    case CommonEnum.PartnerGroupEnum.AGENT:
                        chargeItem.payerShortName = this.listPartnerPayee.find(p => p.id === this.shipment.agentId).shortName;
                        chargeItem.payerId = this.shipment.agentId;
                        chargeItem.payerName = this.shipment.agentName;

                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    onSelectPayerReceiver(partnerData: Partner, chargeItem: CsShipmentSurcharge, type: string) {
        chargeItem.objectBePaid = null;
        switch (type) {
            case 'receiver':
                if (!!partnerData) {
                    chargeItem.receiverName = partnerData.partnerNameEn;
                    chargeItem.paymentObjectId = partnerData.id;
                    chargeItem.receiverShortName = partnerData.shortName;
                }
                break;
            case 'payer':
                if (!!partnerData) {
                    chargeItem.payerName = partnerData.partnerNameEn;
                    chargeItem.payerId = partnerData.id;
                    chargeItem.payerShortName = partnerData.shortName;

                    if (chargeItem.invoiceNo) {
                        chargeItem.vatPartnerId = chargeItem.payerId;
                    }
                }
                break;
            default:
                break;
        }
    }

    onSelectPartner(partnerData: Partner, chargeItem: CsShipmentSurcharge) {
        if (!!partnerData) {
            chargeItem.receiverName = partnerData.shortName;
            chargeItem.paymentObjectId = partnerData.id;
        }
    }

    isWhiteSpace(input: any){
        if(input.trim().length===0||input===null){
            return true;
        }
        return false;
    }

    saveOBHSurCharge() {
        if (!this.charges.length) {
            this._toastService.warning("Please add charge");
            return;
        }
        for (const charge of this.charges) {
            if(!this.isWhiteSpace(charge.invoiceNo) &&this.isWhiteSpace(charge.seriesNo)){
                this._toastService.warning("Series No Must be fill in");
                return;
            }
            if(this.isWhiteSpace(charge.invoiceNo) && !this.isWhiteSpace(charge.seriesNo)){
                this._toastService.warning("Invoice No Must be fill in");
                return;
            }
        }
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }
        if (!this.checkDuplicate()) {
            return;
        }
        this.updateSurchargeField(CommonEnum.SurchargeTypeEnum.OBH);
        this._documentRepo.addShipmentSurcharges(this.charges)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (result: CommonInterface.IResult) => {
                    if (result.status) {
                        this._toastService.success(result.message);

                        // Tính công nợ
                        // this.calculatorReceivable(this.charges);

                        this.getProfit();
                        this.getSurcharges(CommonEnum.SurchargeTypeEnum.OBH);

                    } else {
                        this._toastService.error(result.message);
                    }
                }
            );
    }

    checkValidate() {
        let valid: boolean = true;
        for (const charge of this.charges) {
            if (this.checkSpecialCaseCharge(charge)) {
                valid = true;
                continue;
            }
            if (
                !charge.paymentObjectId
                || !charge.payerId
                || !charge.unitId
                || !charge.chargeId
                || !charge.chargeCode
                || !charge.receiverName
                || !charge.payerName
                || charge.unitPrice === null
                || charge.quantity === null
                // || charge.unitPrice < 0 // ? Cho phép nhập âm nh8
                || +charge.unitPrice === 0
                || charge.quantity < 0
                || charge.vatrate > 100
                || charge.paymentObjectId === charge.payerId
                || (charge.invoiceNo !==null && charge.seriesNo===null)
            ) {
                valid = false;
                break;
            }
        }

        return valid;
    }

    onChangeInvoiceNo(chargeItem: CsShipmentSurcharge, invNo: string) {
        if (invNo) {
            chargeItem.vatPartnerId = chargeItem.payerId;
        }
    }
}
