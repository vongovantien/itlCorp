import { Component, Input } from '@angular/core';

import { CatalogueRepo } from 'src/app/shared/repositories';
import { Charge, Unit, CsShipmentSurcharge, Currency, Partner } from 'src/app/shared/models';
import { Container } from 'src/app/shared/models/document/container.model';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { AppList } from 'src/app/app.list';


import { forkJoin } from 'rxjs';
import { catchError } from 'rxjs/operators';


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

    @Input() containers: Container[] = []; // * House bill was selected.

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
    constructor(
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Partner Name', field: 's', required: true, sortable: true, width: 200 },
            { title: 'Charge Name', field: 's', required: true, sortable: true, width: 200 },
            { title: 'Quantity', field: 's', required: true, sortable: true, width: 200 },
            { title: 'Unit', field: 's', required: true, sortable: true },
            { title: 'Unit Price', field: '', required: true, sortable: true },
            { title: 'Currency', field: '', required: true, sortable: true },
            { title: 'VAT', field: '', required: true, sortable: true },
            { title: 'Total', field: '', sortable: true },
            { title: 'Note', field: '', sortable: true },
            { title: 'Invoice No', field: '', sortable: true },
            { title: 'Series No', field: '', sortable: true },
            { title: 'Invoice Date', field: '', sortable: true },
            { title: 'Exchange Rate Date', field: '', sortable: true },
            { title: 'KB', field: '', sortable: true },
            { title: 'SOA', field: '', sortable: true },
            { title: 'Credit/Debit Note', field: '', sortable: true },
            { title: 'Settle Payment', field: '', sortable: true },
            { title: 'Voucher ID', field: '', sortable: true },
            { title: 'Voucher ID Date', field: '', sortable: true },
            { title: 'Voucher IDRE', field: '', sortable: true },
            { title: 'Voucher IDRE Date', field: '', sortable: true },
            { title: 'Final Exchange Rate', field: '', sortable: true },
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
            { displayName: 'Package', value: QUANTITY_TYPE.PACKAGE },
            { displayName: 'Cont', value: QUANTITY_TYPE.CONT },
            { displayName: 'N.W', value: QUANTITY_TYPE.NW },
        ];

        this.partnerType = [
            { displayName: 'Customer', value: CommonEnum.PartnerGroupEnum.CUSTOMER, fieldName: 'CUSTOMER' },
            { displayName: 'Carrier', value: CommonEnum.PartnerGroupEnum.CARRIER, fieldName: 'CARRIER' },
            { displayName: 'Agent', value: CommonEnum.PartnerGroupEnum.AGENT, fieldName: 'AGENT' },
        ];

        this.getMasterData();
        this.addCharge();
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

    onSelectDataFormInfo(data: Charge | any, type: string, chargeItem: CsShipmentSurcharge) {
        switch (type) {
            case 'charge':
                chargeItem.chargeId = data.id;
                chargeItem.chargeCode = data.code;

                // * Unit, Unit Price had value
                if (!chargeItem.unitId || chargeItem.unitPrice == null) {
                    chargeItem.unitId = this.listUnits.filter(unit => unit.id === data.unitId)[0].id;
                    chargeItem.unitPrice = data.unitPrice;
                }

                console.log(data);
                break;

            default:
                break;
        }
    }

    addCharge() {
        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge();
        newSurCharge.currencyId = "USD"; // * Set default.
        newSurCharge.quantity = 0;
        newSurCharge.quantityType = null;
        newSurCharge.exchangeDate = { startDate: new Date(), endDate: new Date() };

        this.charges.push(newSurCharge);

        console.log(this.charges);
    }

    deleteCharge(index: number) {
        this.charges.splice(index, 1);
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

    saveSurcharge() {
        console.log(this.charges);
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

        // * Update 
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
            chargeItem.paymentObjectId = partnerData.id,
                chargeItem.objectBePaid = null;  // nếu chọn customer/agent/carrier

        }
    }

    selectPartnerType(partnerType: CommonInterface.IValueDisplay, chargeItem: CsShipmentSurcharge) {
        chargeItem.objectBePaid = partnerType.fieldName;
        switch (partnerType.value) {
            case CommonEnum.PartnerGroupEnum.CUSTOMER:

                break;

            default:
                break;
        }
        console.log(partnerType);
    }
}
