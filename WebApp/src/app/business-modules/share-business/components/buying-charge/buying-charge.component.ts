import { Component, Input, ChangeDetectorRef } from '@angular/core';

import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { Charge, Unit, CsShipmentSurcharge, Currency, Partner, CsTransactionDetail } from 'src/app/shared/models';
import { Container } from 'src/app/shared/models/document/container.model';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { AppList } from 'src/app/app.list';


import { forkJoin } from 'rxjs';
import { catchError, takeUntil } from 'rxjs/operators';

import * as fromStore from './../../store';
import { Store } from '@ngrx/store';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';

enum QUANTITY_TYPE {
    GW = 'gw',
    NW = 'nw',
    CW = 'cw',
    CBM = 'cbm',
    PACKAGE = 'package',
    CONT = 'cont'
}
@Component({
    selector: 'buying-charge',
    templateUrl: './buying-charge.component.html',
    styleUrls: ['./buying-charge.component.scss'],

})
export class ShareBussinessBuyingChargeComponent extends AppList {

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
    selectedCharge: Partial<CommonInterface.IComboGridData> = {};

    quantityHints: CommonInterface.IValueDisplay[];
    selectedQuantityHint: CommonInterface.IValueDisplay = null;

    partnerType: CommonInterface.IValueDisplay[];

    TYPE: CommonEnum.SurchargeTypeEnum.BUYING_RATE = CommonEnum.SurchargeTypeEnum.BUYING_RATE;

    isSubmitted: boolean = false;
    isDuplicateChargeCode: boolean = false;
    isDuplicateInvoice: boolean = false;


    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _store: Store<fromStore.IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _sortService: SortService,
    ) {
        super();
        this.requestSort = this.sortSurcharge;
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Partner Name', field: 'partnerName', required: true, sortable: true, width: 200 },
            { title: 'Charge Name', field: 'chargeId', required: true, sortable: true, width: 400 },
            { title: 'Quantity', field: 'quantity', required: true, sortable: true, width: 200 },
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
            { displayName: 'G.W', value: QUANTITY_TYPE.GW },
            { displayName: 'C.W', value: QUANTITY_TYPE.CW },
            { displayName: 'CBM', value: QUANTITY_TYPE.CBM },
            { displayName: 'P.K', value: QUANTITY_TYPE.PACKAGE },
            { displayName: 'Cont', value: QUANTITY_TYPE.CONT },
            { displayName: 'N.W', value: QUANTITY_TYPE.NW },
        ];

        this.partnerType = [
            { displayName: 'Customer', value: CommonEnum.PartnerGroupEnum.CUSTOMER, fieldName: 'CUSTOMER' },
            { displayName: 'Carrier', value: CommonEnum.PartnerGroupEnum.CARRIER, fieldName: 'CARRIER' },
            { displayName: 'Agent', value: CommonEnum.PartnerGroupEnum.AGENT, fieldName: 'AGENT' },
        ];

        this.getMasterData();

        this._store.select(fromStore.getBuyingSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                    console.log("get buying charge from store", this.charges);
                }
            );
    }

    getMasterData() {
        forkJoin([
            this._catalogueRepo.getCharges({ active: true }),
            this._catalogueRepo.getUnit({ active: true }),
            this._catalogueRepo.getCurrencyBy({ active: true }),
            this._catalogueRepo.getListPartner(null, null, { active: true })
        ])
            .pipe(catchError(this.catchError))
            .subscribe(
                ([charges, units, currencies, partners]: any[] = [[], [], [], []]) => {
                    this.listCharges = charges;
                    this.listUnits = units;
                    this.listCurrency = currencies;
                    this.listPartner = partners;
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

    addCharge() {
        this.isSubmitted = false;
        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge();
        newSurCharge.currencyId = "USD"; // * Set default.
        newSurCharge.quantity = 0;
        newSurCharge.quantityType = null;
        newSurCharge.exchangeDate = { startDate: new Date(), endDate: new Date() };
        newSurCharge.invoiceDate = null;
        // this.charges.push(newSurCharge);

        this._store.dispatch(new fromStore.AddBuyingSurchargeAction(newSurCharge));
    }

    duplicate(index: number) {
        this.isSubmitted = false;
        const newCharge = this.charges[index];
        newCharge.id = SystemConstants.EMPTY_GUID;
        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge(newCharge);

        this._store.dispatch(new fromStore.AddBuyingSurchargeAction(newSurCharge));

    }

    deleteCharge(index: number) {
        this._store.dispatch(new fromStore.DeleteBuyingSurchargeAction(index));
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

    saveBuyingCharge() {
        // * Update data 
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }
        if (this.utility.checkDuplicateInObject("chargeId", this.charges)) {
            this.isDuplicateChargeCode = true;
            return;
        } else {
            this.isDuplicateChargeCode = false;
        }
        if (this.utility.checkDuplicateInObject("invoiceNo", this.charges)) {
            this.isDuplicateInvoice = true;
            return;
        } else {
            this.isDuplicateInvoice = false;
        }

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
            charge.type = this.TYPE;
        }

        this._documentRepo.addShipmentSurcharges(this.charges)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        // this._store.dispatch(new fromStore.SaveBuyingSurchargeAction(this.charges));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onChangeQuantityHint(hintType: string, chargeItem: CsShipmentSurcharge) {
        switch (hintType) {
            case QUANTITY_TYPE.GW:
                chargeItem.quantity = this.calculateContainer(this.containers, QUANTITY_TYPE.GW);
                break;
            case QUANTITY_TYPE.NW:
                chargeItem.quantity = this.calculateContainer(this.containers, QUANTITY_TYPE.NW);
                break;
            case QUANTITY_TYPE.CBM:
                chargeItem.quantity = this.calculateContainer(this.containers, QUANTITY_TYPE.CBM);
                break;
            case QUANTITY_TYPE.CW:
                chargeItem.quantity = this.calculateContainer(this.containers, 'chargeAbleWeight');
                break;
            case QUANTITY_TYPE.PACKAGE:
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


}
