import { Component, ViewChild, Input } from '@angular/core';
import { Store } from '@ngrx/store';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { Charge, Unit, CsShipmentSurcharge, Currency, Partner, HouseBill, CsTransaction, CatPartnerCharge, Container } from '@models';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { GetBuyingSurchargeAction, GetOBHSurchargeAction, GetSellingSurchargeAction } from './../../store';
import { CommonEnum } from 'src/app/shared/enums/common.enum';

import { Observable } from 'rxjs';
import { catchError, takeUntil, finalize } from 'rxjs/operators';

import * as fromStore from './../../store';
import * as fromRoot from 'src/app/store';

import { getCatalogueCurrencyState, GetCatalogueCurrencyAction, getCatalogueUnitState, GetCatalogueUnitAction } from 'src/app/store';
import cloneDeep from 'lodash/cloneDeep';
import { ChargeConstants } from 'src/constants/charge.const';

@Component({
    selector: 'buying-charge',
    templateUrl: './buying-charge.component.html',
    styleUrls: ['./buying-charge.component.scss'],

})
export class ShareBussinessBuyingChargeComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @Input() service: string = 'sea';
    @Input() showSyncOtherCharge: boolean = false; // * Hiển thị sync other charge ở service air.

    serviceTypeId: string;
    containers: Container[] = [];
    shipment: CsTransaction;
    hbl: HouseBill;

    headers: CommonInterface.IHeaderTable[] = [];
    headerPartner: CommonInterface.IHeaderTable[] = [];
    charges: CsShipmentSurcharge[] = new Array<CsShipmentSurcharge>();

    listCharges: Charge[];
    listUnits: Unit[] = [];
    listCurrency: Observable<Currency[]>;
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

    selectedIndexCharge: number = -1;


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

        this._store.select(fromRoot.getDataRouterState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (dataParam: CommonInterface.IDataParam) => {
                    this.serviceTypeId = dataParam.serviceId;
                }
            );

        this.isLocked = this._store.select(fromStore.getTransactionLocked);
        this.permissionShipments = this._store.select(fromStore.getTransactionPermission);
    }

    getSurcharge() {
        this._store.select(fromStore.getBuyingSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
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

        this._store.dispatch(new GetCatalogueCurrencyAction());
        this._store.dispatch(new GetCatalogueUnitAction());

        this.listCurrency = this._store.select(getCatalogueCurrencyState);

        this.getUnits();
        this.getPartner();
        this.getCharge();
        this.getShipmentContainer();
        this.getShipmentDetail();
        this.getDetailHBL();

    }

    configHeader() {
        this.headers = [
            { title: 'Partner Name', field: 'partnerShortName', required: true, sortable: true, width: 150 },
            { title: 'Charge Name', field: 'chargeId', required: true, sortable: true, width: 250 },
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
            { title: 'Exchange Rate Date', field: 'exchangeDate', sortable: true },
            { title: 'Final Exchange Rate', field: 'finalExchangeRate', sortable: true },
            { title: 'KB', field: 'kickBack', sortable: true },
            { title: 'SOA', field: 'soano', sortable: true },
            { title: 'Credit/Debit Note', field: 'cdno', sortable: true },
            { title: 'Settle Payment', field: 'settlementCode', sortable: true },
            { title: 'Voucher ID', field: 'voucherId', sortable: true },
            { title: 'Voucher ID Date', field: 'voucherIddate', sortable: true },
            { title: 'Voucher IDRE', field: 'voucherIdre', sortable: true },
            { title: 'Voucher IDRE Date', field: 'voucherIdredate', sortable: true },
        ];
    }

    getUnits() {
        this._progressRef.start();
        this._store.select(getCatalogueUnitState)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (units: Unit[]) => {
                    this.listUnits = units;
                }
            );
    }

    getPartner() {
        this._progressRef.start();
        this._catalogueRepo.getListPartner(null, null, { active: true })
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (partners: Partner[]) => {
                    this.listPartner = partners;
                }
            );
    }

    getShipmentContainer() {
        this._store.select(fromStore.getHBLContainersState)
            .pipe(catchError(this.catchError), takeUntil(this.ngUnsubscribe))
            .subscribe(
                (containers: any[]) => {
                    this.containers = containers;
                }
            );
    }

    getShipmentDetail() {
        this._store.select(fromStore.getTransactionDetailCsTransactionState)
            .pipe(catchError(this.catchError), takeUntil(this.ngUnsubscribe))
            .subscribe(
                (shipment: any) => {
                    this.shipment = shipment;
                }
            );
    }

    getDetailHBL() {
        this._store.select(fromStore.getDetailHBlState)
            .pipe(catchError(this.catchError), takeUntil(this.ngUnsubscribe))
            .subscribe(
                (hbl: any) => {
                    this.hbl = hbl;
                }
            );
    }

    getCharge() {
        this._catalogueRepo.getCharges({ active: true, serviceTypeId: this.serviceTypeId, type: CommonEnum.CHARGE_TYPE.CREDIT })
            .subscribe(
                (charges: Charge[]) => {
                    this.listCharges = charges;
                }
            );
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
                    chargeItem.unitId = this.listUnits.find((u: Unit) => u.id === data.unitId).id;
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
        newSurCharge.hblno = this.hbl.hwbno || null;
        newSurCharge.mblno = this.shipment.mawb || null;
        newSurCharge.jobNo = this.shipment.jobNo || null;


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
        newSurCharge.invoiceDate = null;

        this.addSurcharges(type, newSurCharge);
    }

    deleteCharge(charge: CsShipmentSurcharge, index: number, type: CommonEnum.SurchargeTypeEnum) {
        this.isSubmitted = false;
        this.selectedIndexCharge = index;

        if (charge.id === SystemConstants.EMPTY_GUID) {
            this.deleteChargeWithType(type, this.selectedIndexCharge);
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

    onDeleteShipmentSurcharge(type: CommonEnum.SurchargeTypeEnum) {
        this.confirmDeletePopup.hide();

        if (!!this.selectedSurcharge && this.selectedSurcharge.id !== SystemConstants.EMPTY_GUID) {
            this._progressRef.start();
            this._documentRepo.deleteShipmentSurcharge(this.selectedSurcharge.id)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            if (this.selectedIndexCharge > -1) {
                                // this.charges = [...this.charges.slice(0, this.selectedIndexCharge), ...this.charges.slice(this.selectedIndexCharge + 1)];
                                // this._store.dispatch(new fromStore.DeleteBuyingSurchargeAction(this.selectedIndexCharge));

                                this.deleteChargeWithType(type, this.selectedIndexCharge);
                            }
                            // this.getSurcharges(type);
                            this.getProfit();
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
    }

    deleteChargeWithType(type: CommonEnum.SurchargeTypeEnum, index: number) {
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
    }

    onChangeVat(vat: number, chargeItem: CsShipmentSurcharge) {
        chargeItem.total = this.utility.calculateTotalAmountWithVat(vat, chargeItem.quantity, chargeItem.unitPrice);
    }

    onChangeUnitPrice(unitPrice: number, chargeItem: CsShipmentSurcharge) {
        chargeItem.total = this.utility.calculateTotalAmountWithVat(chargeItem.vatrate || 0, chargeItem.quantity, unitPrice);
    }

    onChangeQuantity(quantity: number, chargeItem: CsShipmentSurcharge) {
        chargeItem.total = this.utility.calculateTotalAmountWithVat(chargeItem.vatrate || 0, quantity, chargeItem.unitPrice);
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

            charge.unitPrice = +charge.unitPrice;

            // Update HBL ID,Type
            if (!!this.hbl && !!this.hbl.id) {
                charge.hblid = this.hbl.id;
            } else {
                this._toastService.warning("HBL was not found");
                return;
            }
            charge.type = type;
        }
    }

    saveBuyingCharge(type: CommonEnum.SurchargeTypeEnum | string) {
        if (!this.charges.length) {
            this._toastService.warning("Please add charge");
            return;
        }
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

                        this.getProfit();
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
                if (this.TYPE === CommonEnum.SurchargeTypeEnum.BUYING_RATE && this.service !== 'sea') {
                    chargeItem.quantity = this.hbl.grossWeight || 0;
                } else {
                    chargeItem.quantity = this.calculateContainer(this.containers, CommonEnum.QUANTITY_TYPE.GW);
                }
                break;
            case CommonEnum.QUANTITY_TYPE.NW:
                if (this.TYPE === CommonEnum.SurchargeTypeEnum.BUYING_RATE && this.service !== 'sea') {
                    chargeItem.quantity = this.hbl.netWeight || 0;
                } else {
                    chargeItem.quantity = this.calculateContainer(this.containers, CommonEnum.QUANTITY_TYPE.NW);
                }
                break;
            case CommonEnum.QUANTITY_TYPE.CBM:
                if (this.TYPE === CommonEnum.SurchargeTypeEnum.BUYING_RATE && this.service !== 'sea') {
                    chargeItem.quantity = this.hbl.cbm || 0;
                } else {
                    chargeItem.quantity = this.calculateContainer(this.containers, CommonEnum.QUANTITY_TYPE.CBM);
                }
                break;
            case CommonEnum.QUANTITY_TYPE.CONT:
                chargeItem.quantity = this.calculateContainer(this.containers, 'quantity');
                break;
            case CommonEnum.QUANTITY_TYPE.CW:
                if (this.TYPE === CommonEnum.SurchargeTypeEnum.BUYING_RATE && this.service !== 'sea') {
                    chargeItem.quantity = this.hbl.chargeWeight || 0;
                } else {
                    chargeItem.quantity = this.calculateContainer(this.containers, 'chargeAbleWeight');
                }
                break;
            case CommonEnum.QUANTITY_TYPE.PACKAGE:
                if (this.TYPE === CommonEnum.SurchargeTypeEnum.BUYING_RATE && this.service !== 'sea') {
                    chargeItem.quantity = this.hbl.packageQty || 0;
                } else {
                    chargeItem.quantity = this.calculateContainer(this.containers, 'packageQuantity');
                }
                break;
            default:
                break;
        }

        // * Update quantityType, total.
        chargeItem.quantityType = hintType;
        chargeItem.total = this.utility.calculateTotalAmountWithVat(chargeItem.vatrate || 0, chargeItem.quantity, chargeItem.unitPrice);
    }

    calculateContainer(containers: Container[], key: string): number {
        let total: number = 0;
        total = containers.reduce((acc: any, curr: Container) => acc += curr[key], 0);
        return total;
    }

    onSelectPartner(partnerData: Partner, chargeItem: CsShipmentSurcharge) {
        if (!!partnerData) {
            chargeItem.partnerShortName = partnerData.shortName;
            chargeItem.paymentObjectId = partnerData.id;
            chargeItem.objectBePaid = null;  // nếu chọn customer/agent/carrier
        }
    }

    selectPartnerType(partnerType: CommonInterface.IValueDisplay, chargeItem: CsShipmentSurcharge) {
        chargeItem.objectBePaid = partnerType.fieldName;
        switch (partnerType.value) {
            case CommonEnum.PartnerGroupEnum.CUSTOMER:
                chargeItem.partnerShortName = this.hbl.customerName;
                if (!chargeItem.partnerShortName) {
                    chargeItem.partnerShortName = this.listPartner.find(p => p.id === this.hbl.customerId).partnerNameEn;
                }
                chargeItem.paymentObjectId = this.hbl.customerId;
                break;
            case CommonEnum.PartnerGroupEnum.CARRIER:
                chargeItem.partnerShortName = this.shipment.supplierName;
                chargeItem.paymentObjectId = this.shipment.coloaderId;
                break;
            case CommonEnum.PartnerGroupEnum.AGENT:
                chargeItem.partnerShortName = this.shipment.agentName;
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
                || charge.quantity === null
                || !charge.unitId
                || !charge.partnerShortName
                || charge.unitPrice === null
                || charge.quantity < 0
                || charge.unitPrice < 0
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

    getProfit() {
        this._store.dispatch(new fromStore.GetProfitHBLAction(this.hbl.id));
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

    getStandardCharge() {
        const chargeCodes: string[] = this.detectDefaultCode(this.serviceTypeId);

        this._catalogueRepo.getChargeByCodes(chargeCodes)
            .pipe(catchError(this.catchError))
            .subscribe((charges: Charge[]) => {
                if (charges && !!charges.length) {
                    const csShipmentSurcharge: CsShipmentSurcharge[] = this.updateDefaultChargeCode(charges, this.serviceTypeId);

                    if (!!csShipmentSurcharge.length) {
                        csShipmentSurcharge.forEach((c: CsShipmentSurcharge) => {
                            this._store.dispatch(new fromStore.AddBuyingSurchargeAction(c));
                        });
                    }
                } else {
                    this._toastService.warning("Not found default charge");
                }
            });
    }

    detectDefaultCode(serviceTypeId: string) {
        switch (serviceTypeId) {
            case ChargeConstants.AE_CODE:
            case ChargeConstants.AI_CODE:
                return ChargeConstants.DEFAULT_AIR;
            case ChargeConstants.SFE_CODE:
                return ChargeConstants.DEFAULT_FCL_EXPORT;
            case ChargeConstants.SLE_CODE:
                return ChargeConstants.DEFAULT_LCL_EXPORT;
            case ChargeConstants.SFI_CODE:
                return ChargeConstants.DEFAULT_FCL_IMPORT;
            case ChargeConstants.SLI_CODE:
                return ChargeConstants.DEFAULT_LCL_IMPORT;
            default:
                return [];
        }
    }

    updateDefaultChargeCode(charges: Charge[], serviceTypeId: string): CsShipmentSurcharge[] {
        const shipmentSurcharges: CsShipmentSurcharge[] = this.mapChargeToShipmentSurCharge(charges);
        switch (serviceTypeId) {
            case ChargeConstants.AE_CODE:
            case ChargeConstants.AI_CODE:
                shipmentSurcharges.forEach((c: CsShipmentSurcharge) => {
                    if (c.chargeCode === ChargeConstants.DEFAULT_AIR[0]) {
                        const unit: Unit = this.listUnits.find(u => u.code === 'kgs' || u.code === 'KGS');
                        c.unitId = !!unit ? unit.id : null;

                        c.quantityType = CommonEnum.QUANTITY_TYPE.CW;
                        c.quantity = this.shipment.chargeWeight;
                    }
                    if (c.chargeCode === ChargeConstants.DEFAULT_AIR[1]) {
                        const unit: Unit = this.listUnits.find(u => u.code === 'kgs' || u.code === 'KGS');
                        c.unitId = !!unit ? unit.id : null;

                        c.quantityType = CommonEnum.QUANTITY_TYPE.GW;
                        c.quantity = this.shipment.grossWeight;
                    }
                    if (c.chargeCode === ChargeConstants.DEFAULT_AIR[2]) {
                        const unit: Unit = this.listUnits.find(u => u.code === 'SET' || u.code === 'set');
                        c.unitId = !!unit ? unit.id : null;

                        c.quantity = 1;
                    }

                });
                break;
            case ChargeConstants.SFE_CODE:
            case ChargeConstants.SLE_CODE:
            case ChargeConstants.SFI_CODE:
            case ChargeConstants.SLI_CODE:
            default:
                return [];

        }

        return shipmentSurcharges;
    }

    mapChargeToShipmentSurCharge(charges: Charge[]): CsShipmentSurcharge[] {
        const newCsShipmentSurcharge: CsShipmentSurcharge[] = charges.map((c: Charge) => new CsShipmentSurcharge(
            Object.assign({}, c, {
                chargeCode: c.code,
                type: CommonEnum.SurchargeTypeEnum.BUYING_RATE,
                chargeId: c.id,
                id: SystemConstants.EMPTY_GUID,
                exchangeDate: { startDate: new Date, endDate: new Date() }
            })));
        return newCsShipmentSurcharge || [];
    }

    getOtherCharge() {
        if (!!this.shipment) {
            if (!!this.shipment.coloaderId) {
                this._catalogueRepo.getPartnerCharge(this.shipment.coloaderId)
                    .pipe(catchError(this.catchError))
                    .subscribe((partnerCharges: CatPartnerCharge[]) => {
                        if (!partnerCharges || !partnerCharges.length) {
                            this._toastService.warning("Not found charges of AIRLINE/COLOADER");
                        } else {
                            const buyingCharges: CsShipmentSurcharge[] = partnerCharges.map(x => new CsShipmentSurcharge(Object.assign({}, x, { paymentObjectId: x.partnerId })));
                            // * update CsShipmentSurcharge charge to chargeId
                            buyingCharges.forEach(c => {
                                c.id = SystemConstants.EMPTY_GUID;
                                c.type = CommonEnum.SurchargeTypeEnum.BUYING_RATE;
                                c.exchangeDate = { startDate: new Date, endDate: new Date() };

                                this._store.dispatch(new fromStore.AddBuyingSurchargeAction(c));
                            });
                        }
                    });
            } else {
                this._toastService.warning("Not found Airline/Coloader of shipment");
            }
        }
    }

}
