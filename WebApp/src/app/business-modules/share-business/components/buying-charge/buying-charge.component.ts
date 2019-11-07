import { Component, ViewEncapsulation } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { forkJoin } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Surcharge, Charge, Unit, CsShipmentSurcharge, Currency } from 'src/app/shared/models';

@Component({
    selector: 'buying-charge',
    templateUrl: './buying-charge.component.html',
    styleUrls: ['./buying-charge.component.scss'],

})
export class ShareBussinessBuyingChargeComponent extends AppList {

    headers: CommonInterface.IHeaderTable[] = [];
    charges: CsShipmentSurcharge[] = new Array<CsShipmentSurcharge>();

    listCharges: Charge[] = new Array<Charge>();
    listUnits: Unit[] = new Array<Unit>();
    listCurrency: Currency[] = new Array<Currency>();

    configComboGridCharge: Partial<CommonInterface.IComboGirdConfig> = {};
    selectedCharge: Partial<CommonInterface.IComboGridData> = {};

    constructor(
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Partner Name', field: 's', required: true, sortable: true },
            { title: 'Charge Name', field: 's', required: true, sortable: true },
            { title: 'Quantity', field: 's', required: true, sortable: true },
            { title: 'Unit', field: 's', required: true, sortable: true },
            { title: 'Unit Price', field: '', required: true, sortable: true },
            { title: 'Currency', field: '', required: true, sortable: true },
            { title: 'VAT', field: '', required: true, sortable: true },
            { title: 'Total', field: '', sortable: true },
            { title: 'Note', field: '', sortable: true },
            { title: 'Invoice No', field: '', sortable: true },
            { title: 'Series No', field: '', sortable: true },
            { title: 'Invoice Date', field: '', sortable: true },
            { title: 'SOA', field: '', sortable: true },
            { title: 'Credit/Debit Note', field: '', sortable: true },
            { title: 'Settle Payment', field: '', sortable: true },
            { title: 'Exchange Rate Date', field: '', sortable: true },
            { title: 'Voucher ID', field: '', sortable: true },
            { title: 'Voucher ID Date', field: '', sortable: true },
            { title: 'Voucher IDRE', field: '', sortable: true },
            { title: 'Voucher IDRE Date', field: '', sortable: true },
            { title: 'Final Exchange Rate', field: '', sortable: true },
            { title: 'KB', field: '', sortable: true },
        ];

        this.configComboGridCharge = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'chargeNameEn', label: 'Name' },
                { field: 'unitPrice', label: 'Unit Price' },
                { field: 'unitId', label: 'Unit' },
                { field: 'code', label: 'Code' },
            ]
        }, { selectedDisplayFields: ['chargeNameEn'], })
        this.getMasterData();
    }

    getMasterData() {
        forkJoin([
            this._catalogueRepo.getCharges({ active: true }),
            this._catalogueRepo.getUnit({ active: true }),
            this._catalogueRepo.getCurrencyBy({ active: true })
        ])
            .pipe(catchError(this.catchError))
            .subscribe(
                ([charges, units, currencies]: any[] = [[], []]) => {
                    this.listCharges = charges;
                    this.listUnits = units;
                    this.listCurrency = currencies;
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
        newSurCharge.exchangeDate = { startDate: new Date(), endDate: new Date() };

        this.charges.push(newSurCharge);

        console.log(this.charges);
    }

    onChangeVat(vat: number, chargeItem: CsShipmentSurcharge) {
        console.log(vat, chargeItem);
        chargeItem.total = this.calculateTotalAmount(vat, chargeItem.quantity, chargeItem.unitPrice);
    }

    onChangeUnitPrice(unitPrice: number, chargeItem: CsShipmentSurcharge) {
        chargeItem.total = this.calculateTotalAmount(chargeItem.vatrate, chargeItem.quantity, unitPrice);
    }

    calculateTotalAmount(vat: number, quantity: number, unitPrice: number): number {
        return this.utility.calculateTotalAmountWithVat(vat, quantity, unitPrice);
    }

    saveSurcharge() {
        console.log(this.charges);
    }
}
