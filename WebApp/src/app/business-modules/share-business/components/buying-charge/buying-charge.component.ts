import { Component, Input, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { Charge, Unit, CsShipmentSurcharge, Currency, Partner, CsTransactionDetail } from 'src/app/shared/models';
import { Container } from 'src/app/shared/models/document/container.model';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { GetBuyingSurchargeAction, GetOBHSurchargeAction, GetSellingSurchargeAction } from './../../store';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { ChargeConstants } from 'src/constants/charge.const';

import { forkJoin } from 'rxjs';
import { catchError, takeUntil, finalize } from 'rxjs/operators';

import * as fromStore from './../../store';
@Component({
    selector: 'buying-charge',
    templateUrl: './buying-charge.component.html',
    styleUrls: ['./buying-charge.component.scss'],

})
export class ShareBussinessBuyingChargeComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    @Input() containers: Container[] = [];
    @Input() shipment: any;
    @Input() hbl: CsTransactionDetail;

    headers: CommonInterface.IHeaderTable[] = [];
    headerPartner: CommonInterface.IHeaderTable[] = [];
    charges: CsShipmentSurcharge[] = new Array<CsShipmentSurcharge>();

    listCharges: Charge[] = new Array<Charge>();
    listUnits: Unit[] = new Array<Unit>();
    listCurrency: Currency[] = new Array<Currency>();
    listPartner: Partner[] = new Array<Partner>();

    configComboGridCharge: Partial<CommonInterface.IComboGirdConfig> = {};

    quantityHints: CommonInterface.IValueDisplay[];
    selectedQuantityHint: CommonInterface.IValueDisplay = null;

    partnerType: CommonInterface.IValueDisplay[];

    TYPE: CommonEnum.SurchargeTypeEnum.BUYING_RATE = CommonEnum.SurchargeTypeEnum.BUYING_RATE;

    isSubmitted: boolean = false;
    isDuplicateChargeCode: boolean = false;
    isDuplicateInvoice: boolean = false;

    selectedSurcharge: CsShipmentSurcharge;
    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _store: Store<fromStore.IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _sortService: SortService,
        protected _ngProgressService: NgProgress

    ) {
        super();
        this.requestSort = this.sortSurcharge;
        this._progressRef = this._ngProgressService.ref();

        this.getSurcharge();
    }

    getSurcharge() {
        this._store.select(fromStore.getBuyingSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                    console.log("get buying charge from store", this.charges);
                }
            );
    }

    ngOnInit(): void {
        this.configHeader();

        this.headerPartner = [
            { title: 'Name', field: 'partnerNameEn' },
            { title: 'Partner Code', field: 'taxCode' },
        ];

        this.configComboGridCharge = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'chargeNameEn', label: 'Name' },
                { field: 'unitPrice', label: 'Unit Price' },
                { field: 'unitId', label: 'Unit' },
                { field: 'code', label: 'Code' },
            ]
        }, { selectedDisplayFields: ['chargeNameEn'], });

        this.quantityHints = [
            { displayName: 'G.W', value: CommonEnum.QUANTITY_TYPE.GW },
            { displayName: 'C.W', value: CommonEnum.QUANTITY_TYPE.CW },
            { displayName: 'CBM', value: CommonEnum.QUANTITY_TYPE.CBM },
            { displayName: 'P.K', value: CommonEnum.QUANTITY_TYPE.PACKAGE },
            { displayName: 'Cont', value: CommonEnum.QUANTITY_TYPE.CONT },
            { displayName: 'N.W', value: CommonEnum.QUANTITY_TYPE.NW },
        ];

        this.partnerType = [
            { displayName: 'Customer', value: CommonEnum.PartnerGroupEnum.CUSTOMER, fieldName: 'CUSTOMER' },
            { displayName: 'Carrier', value: CommonEnum.PartnerGroupEnum.CARRIER, fieldName: 'CARRIER' },
            { displayName: 'Agent', value: CommonEnum.PartnerGroupEnum.AGENT, fieldName: 'AGENT' },
        ];

        this.getMasterData();
    }

    configHeader() {
        this.headers = [
            { title: 'Partner Name', field: 'partnerName', required: true, sortable: true, width: 150 },
            { title: 'Charge Name', field: 'chargeId', required: true, sortable: true, width: 250 },
            { title: 'Quantity', field: 'quantity', required: true, sortable: true },
            { title: 'Unit', field: 'unitId', required: true, sortable: true },
            { title: 'Unit Price', field: 'unitPrice', required: true, sortable: true },
            { title: 'Currency', field: 'currencyId', required: true, sortable: true },
            { title: 'VAT', field: 'vatrate', required: true, sortable: true },
            { title: 'Total', field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Series No', field: 'seriesNo', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Exchange Rate Date', field: 'exchangeDate', sortable: true },
            { title: 'KB', field: 'kickBack', sortable: true },
            { title: 'SOA', field: 'soano', sortable: true },
            { title: 'Credit/Debit Note', field: 'cdno', sortable: true },
            { title: 'Settle Payment', field: 'settlementCode', sortable: true },
            { title: 'Voucher ID', field: 'voucherId', sortable: true },
            { title: 'Voucher ID Date', field: 'voucherIddate', sortable: true },
            { title: 'Voucher IDRE', field: 'voucherIdre', sortable: true },
            { title: 'Voucher IDRE Date', field: 'voucherIdredate', sortable: true },
            { title: 'Final Exchange Rate', field: 'finalExchangeRate', sortable: true },
        ];
    }

    getMasterData() {
        this._progressRef.start();
        forkJoin([
            this.getCharge(),
            this._catalogueRepo.getUnit({ active: true }),
            this._catalogueRepo.getCurrencyBy({ active: true }),
            this._catalogueRepo.getListPartner(null, null, { active: true })
        ])
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                ([charges, units, currencies, partners]: any[] = [[], [], [], []]) => {
                    this.listCharges = charges;
                    this.listUnits = units;
                    this.listCurrency = currencies;
                    this.listPartner = partners;
                }
            );
    }

    getCharge() {
        return this._catalogueRepo.getCharges({ active: true, serviceTypeId: ChargeConstants.SFI_CODE, type: CommonEnum.CHARGE_TYPE.DEBIT });
    }

    sortSurcharge() {
        this.charges = this._sortService.sort(this.charges, this.sort, this.order);
    }

    onSelectDataFormInfo(data: Charge | any, type: string, chargeItem: CsShipmentSurcharge) {
        [this.isDuplicateChargeCode, this.isDuplicateInvoice] = [false, false];

        switch (type) {
            case 'charge':
                chargeItem.chargeId = data.id;
                chargeItem.chargeCode = data.code;
                chargeItem.chargeNameEn = data.chargeNameEn;
                // * Unit, Unit Price had value
                if (!chargeItem.unitId || chargeItem.unitPrice == null) {
                    chargeItem.unitId = this.listUnits.filter(unit => unit.id === data.unitId)[0].id;
                    chargeItem.unitPrice = data.unitPrice;
                }
                break;

            default:
                break;
        }
    }

    addCharge(type: CommonEnum.SurchargeTypeEnum | string) {
        this.isSubmitted = false;

        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge();
        newSurCharge.currencyId = "USD"; // * Set default.
        newSurCharge.exchangeDate = { startDate: new Date(), endDate: new Date() };
        newSurCharge.quantity = 0;
        newSurCharge.quantityType = null;
        newSurCharge.invoiceDate = null;
        newSurCharge.creditNo = null;
        newSurCharge.debitNo = null;
        newSurCharge.settlementCode = null;
        newSurCharge.voucherId = null;
        newSurCharge.voucherIddate = null;
        newSurCharge.voucherIdre = null;
        newSurCharge.voucherIdredate = null;
        newSurCharge.isFromShipment = true;

        this.addSurcharges(type, newSurCharge);
    }

    duplicate(index: number, type: CommonEnum.SurchargeTypeEnum | string) {
        this.isSubmitted = false;

        const newCharge = this.charges[index];
        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge(newCharge);

        newSurCharge.id = SystemConstants.EMPTY_GUID;
        newSurCharge.creditNo = null;
        newSurCharge.debitNo = null;
        newSurCharge.settlementCode = null;
        newSurCharge.voucherId = null;
        newSurCharge.voucherIddate = null;
        newSurCharge.voucherIdre = null;
        newSurCharge.voucherIdredate = null;
        newSurCharge.isFromShipment = true;

        this.addSurcharges(type, newSurCharge);
    }

    deleteCharge(charge: CsShipmentSurcharge, index: number, type: CommonEnum.SurchargeTypeEnum | string) {
        this.isSubmitted = false;
        if (charge.id === SystemConstants.EMPTY_GUID) {
            switch (type) {
                case CommonEnum.SurchargeTypeEnum.BUYING_RATE:
                    this._store.dispatch(new fromStore.DeleteBuyingSurchargeAction(index));
                    break;
                case CommonEnum.SurchargeTypeEnum.SELLING_RATE:
                    this._store.dispatch(new fromStore.DeleteSellingSurchargeAction(index));
                    break;
                case CommonEnum.SurchargeTypeEnum.OBH:
                    this._store.dispatch(new fromStore.DeleteOBHSurchargeAction(index));
                    break;
                default:
                    break;
            }
        } else if (
            !!charge.soano
            || !!charge.creditNo
            || !!charge.debitNo
            || !!charge.settlementCode
            || !!charge.voucherId
            || !!charge.voucherIddate
            || !!charge.voucherIdre
        ) { return; } else {
            this.selectedSurcharge = new CsShipmentSurcharge(charge);
            this.confirmDeletePopup.show();
        }
    }

    onDeleteShipmentSurcharge(type: CommonEnum.SurchargeTypeEnum | string) {
        this.confirmDeletePopup.hide();

        if (!!this.selectedSurcharge && this.selectedSurcharge.id !== SystemConstants.EMPTY_GUID) {
            this._progressRef.start();
            this._documentRepo.deleteShipmentSurcharge(this.selectedSurcharge.id)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);

                            console.log(type);
                            this.getSurcharges(type);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
    }

    onChangeVat(vat: number, chargeItem: CsShipmentSurcharge) {
        chargeItem.total = this.utility.calculateTotalAmountWithVat(vat, chargeItem.quantity, chargeItem.unitPrice);
    }

    onChangeUnitPrice(unitPrice: number, chargeItem: CsShipmentSurcharge) {
        chargeItem.total = this.utility.calculateTotalAmountWithVat(chargeItem.vatrate, chargeItem.quantity, unitPrice);
    }

    onChangeQuantity(quantity: number, chargeItem: CsShipmentSurcharge) {
        chargeItem.total = this.utility.calculateTotalAmountWithVat(chargeItem.vatrate, quantity, chargeItem.unitPrice);
    }

    updateSurchargeField(type: CommonEnum.SurchargeTypeEnum) {
        for (const charge of this.charges) {
            if (!!charge.exchangeDate && !!charge.exchangeDate.startDate) {
                charge.exchangeDate = formatDate(charge.exchangeDate.startDate, 'yyyy-MM-dd', 'en');
            }

            if (!!charge.invoiceDate && !!charge.invoiceDate.startDate) {
                charge.invoiceDate = formatDate(charge.invoiceDate.startDate, 'yyyy-MM-dd', 'en');
            } else {
                charge.invoiceDate = null;
            }

            // Update HBL ID,Type
            charge.hblid = this.hbl.id;
            charge.type = type;
        }

    }

    saveBuyingCharge(type: CommonEnum.SurchargeTypeEnum | string) {
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }
        if (!this.checkDuplicate()) {
            return;
        }

        this.updateSurchargeField(CommonEnum.SurchargeTypeEnum.BUYING_RATE);
        this._progressRef.start();
        this._documentRepo.addShipmentSurcharges(this.charges)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * Get Profit
                        this._store.dispatch(new fromStore.GetProfitAction(this.hbl.id));

                        this.getSurcharges(type);

                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onChangeQuantityHint(hintType: string, chargeItem: CsShipmentSurcharge) {
        switch (hintType) {
            case CommonEnum.QUANTITY_TYPE.GW:
                chargeItem.quantity = this.calculateContainer(this.containers, CommonEnum.QUANTITY_TYPE.GW);
                break;
            case CommonEnum.QUANTITY_TYPE.NW:
                chargeItem.quantity = this.calculateContainer(this.containers, CommonEnum.QUANTITY_TYPE.NW);
                break;
            case CommonEnum.QUANTITY_TYPE.CBM:
                chargeItem.quantity = this.calculateContainer(this.containers, CommonEnum.QUANTITY_TYPE.CBM);
                break;
            case CommonEnum.QUANTITY_TYPE.CONT:
                chargeItem.quantity = this.calculateContainer(this.containers, 'quantity');
                break;
            case CommonEnum.QUANTITY_TYPE.CW:
                chargeItem.quantity = this.calculateContainer(this.containers, 'chargeAbleWeight');
                break;
            case CommonEnum.QUANTITY_TYPE.PACKAGE:
                chargeItem.quantity = this.calculateContainer(this.containers, 'packageQuantity');
                break;
            default:
                break;

        }

        // * Update quantityType, total.
        chargeItem.quantityType = hintType;
        chargeItem.total = this.utility.calculateTotalAmountWithVat(chargeItem.vatrate, chargeItem.quantity, chargeItem.unitPrice);
    }

    calculateContainer(containers: Container[], key: string): number {
        let total: number = 0;
        total = containers.reduce((acc: any, curr: Container) => acc += curr[key], 0);
        return total;
    }

    onSelectPartner(partnerData: Partner, chargeItem: CsShipmentSurcharge) {
        if (!!partnerData) {
            chargeItem.partnerName = partnerData.shortName;
            chargeItem.paymentObjectId = partnerData.id;
            chargeItem.objectBePaid = null;  // nếu chọn customer/agent/carrier

        }
    }

    selectPartnerType(partnerType: CommonInterface.IValueDisplay, chargeItem: CsShipmentSurcharge) {
        chargeItem.objectBePaid = partnerType.fieldName;
        switch (partnerType.value) {
            case CommonEnum.PartnerGroupEnum.CUSTOMER:
                chargeItem.partnerName = this.hbl.customerName;
                chargeItem.paymentObjectId = this.hbl.customerId;
                break;
            case CommonEnum.PartnerGroupEnum.CARRIER:
                chargeItem.partnerName = this.shipment.supplierName;
                chargeItem.paymentObjectId = this.shipment.coloaderId;
                break;
            case CommonEnum.PartnerGroupEnum.AGENT:
                chargeItem.partnerName = this.shipment.agentName;
                chargeItem.paymentObjectId = this.shipment.agentId;
                break;
            default:
                break;
        }
    }

    checkValidate() {
        let valid: boolean = true;
        for (const charge of this.charges) {
            if (
                !charge.paymentObjectId
                || !charge.chargeId
                || !charge.chargeCode
                || !charge.unitId
                || !charge.partnerName
                || charge.unitPrice === null
                || charge.unitPrice < 0
                || charge.vatrate === null
                || charge.vatrate > 100
            ) {
                valid = false;
                break;
            }
        }

        return valid;
    }

    checkDuplicate() {

        let valid: boolean = true;
        if (this.utility.checkDuplicateInObject("chargeId", this.charges) && this.utility.checkDuplicateInObject("invoiceNo", this.charges)) {
            this.isDuplicateChargeCode = true;
            this.isDuplicateInvoice = true;
            valid = false;
            this._toastService.warning("The Charge code and InvoiceNo is duplicated");
            return;
        } else {
            valid = true;
            this.isDuplicateChargeCode = false;
            this.isDuplicateInvoice = false;
        }
        return valid;
    }

    getSurcharges(type: string) {
        switch (type) {
            case CommonEnum.SurchargeTypeEnum.BUYING_RATE:
                this._store.dispatch(new GetBuyingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.BUYING_RATE, hblId: this.hbl.id }));
                break;
            case CommonEnum.SurchargeTypeEnum.SELLING_RATE:
                this._store.dispatch(new GetSellingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.SELLING_RATE, hblId: this.hbl.id }));
                break;
            case CommonEnum.SurchargeTypeEnum.OBH:
                this._store.dispatch(new GetOBHSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.OBH, hblId: this.hbl.id }));
                break;
            default:
                break;
        }
    }

    addSurcharges(type: string, newcharge: CsShipmentSurcharge) {
        switch (type) {
            case CommonEnum.SurchargeTypeEnum.BUYING_RATE:
                this._store.dispatch(new fromStore.AddBuyingSurchargeAction(newcharge));
                break;
            case CommonEnum.SurchargeTypeEnum.SELLING_RATE:
                this._store.dispatch(new fromStore.AddSellingSurchargeAction(newcharge));
                break;
            case CommonEnum.SurchargeTypeEnum.OBH:
                this._store.dispatch(new fromStore.AddOBHSurchargeAction(newcharge));
                break;
            default:
                break;
        }

    }

}
