import { Component, ChangeDetectionStrategy, ViewChild } from '@angular/core';

import { AppList } from 'src/app/app.list';
import { ArrivalFreightCharge, Surcharge, Unit, Currency, Charge } from 'src/app/shared/models';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { forkJoin } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { Container } from '@angular/compiler/src/i18n/i18n_ast';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { Store } from '@ngrx/store';
import { getContainerSaveState, getHBLState } from '../../../../store';
import { formatDate } from '@angular/common';
import { NgxSpinnerService } from 'ngx-spinner';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { HBLArrivalNote } from 'src/app/shared/models/document/arrival-note-hbl';

@Component({
    selector: 'sea-fcl-import-hbl-arrival-note',
    templateUrl: './arrival-note.component.html',
    styleUrls: ['./arrival-note.component.scss'],
})

export class SeaFClImportArrivalNoteComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    hblArrivalNote: HBLArrivalNote = new HBLArrivalNote();

    headers: CommonInterface.IHeaderTable[];

    optionEditor: any = '';
    header: string = '';
    footer: string = '';

    freightCharges: ArrivalFreightCharge[] = new Array<ArrivalFreightCharge>();
    listCharges: Charge[] = [];
    listUnits: Unit[] = [];
    listCurrency: Currency[] = [];

    quantityTypes: CommonInterface.IValueDisplay[];
    containersShipment: Container[] = [];
    containersHBL: Container[] = [];

    configComboGridCharge: Partial<CommonInterface.IComboGirdConfig> = {};

    hblDetail: any; // TODO model.

    defaultExchangeRate: number;

    selectedIndexFreightCharge: number = -1;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _dataService: DataService,
        private _store: Store<any>,
        private _documentRepo: DocumentationRepo,
        private _spinner: NgxSpinnerService
    ) {
        super();
    }

    ngOnInit() {
        this.configData();
        this.getMasterData();

        // * Get container's shipment from Store.
        this._store.select(getContainerSaveState).subscribe(
            (res: Container[] | any[]) => {
                this.containersShipment = res || [];
                console.log("container shipment from store", this.containersShipment);
            }
        );

        // * Get Detail HBL from Store.
        this._store.select(getHBLState).subscribe(
            (res: any) => {
                this.containersHBL = res.data.csMawbcontainers || [];
                console.log("cotainer HBl from store", this.containersHBL);
            }
        );

    }

    configData() {
        this.configComboGridCharge = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'chargeNameEn', label: 'Name' },
                { field: 'unitPrice', label: 'Unit Price' },
                { field: 'unitId', label: 'Unit' },
                { field: 'code', label: 'Code' },
            ]
        }, { selectedDisplayFields: ['chargeNameEn'], });

        /* Config Text Editor. */
        this.optionEditor = {
            htmlExecuteScripts: false,
            heightMin: 150,
            charCounterCount: false,
            theme: 'royal',
            fontFamily: {
                "Roboto,sans-serif": 'Roboto',
                "Oswald,sans-serif": 'Oswald',
                "Montserrat,sans-serif": 'Montserrat',
                "'Open Sans Condensed',sans-serif": 'Open Sans Condensed',
                "'Arial',sans-serif,": 'Arial',
                "Time new Roman": 'Time new Roman',
                "Tahoma": 'Tahoma'
            },
            toolbarButtons: ['fullscreen', 'bold', 'italic', 'underline', 'strikeThrough', 'subscript', 'superscript', '|', 'fontFamily', 'fontSize', 'color', 'inlineClass', 'inlineStyle', 'paragraphStyle', 'lineHeight', '|', 'paragraphFormat', 'align', 'formatOL', 'formatUL', 'outdent', 'indent', 'quote', '-', 'insertLink', 'insertTable', '|', 'emoticons', 'fontAwesome', 'specialCharacters', 'insertHR', 'selectAll', 'clearFormatting', '|', 'spellChecker', 'help', 'html', '|', 'undo', 'redo'],
            quickInsertButtons: ['table', 'ul', 'ol', 'hr'],
            fontFamilySelection: true,
            language: 'vi',
        };

        this.headers = [
            { title: 'Charge', field: 'chargeCode', sortable: true, width: 300 },
            { title: 'Quantity', field: 'chargeCode', sortable: true, width: 200 },
            { title: 'Unit', field: 'chargeCode', sortable: true },
            { title: 'Unit Price', field: 'chargeCode', sortable: true },
            { title: 'Currency', field: 'chargeCode', sortable: true },
            { title: 'VAT', field: 'chargeCode', sortable: true },
            { title: 'Total Amount', field: 'chargeCode', sortable: true },
            { title: 'Exchange Rate', field: 'chargeCode', sortable: true },
            { title: 'Note', field: 'chargeCode', sortable: true },
            { title: 'Show', field: 'chargeCode', sortable: true },
            { title: 'Full', field: 'chargeCode', sortable: true },
            { title: 'Tick', field: 'chargeCode', sortable: true },
        ];

        this.quantityTypes = [
            { displayName: 'G.W', value: CommonEnum.QUANTITY_TYPE.GW },
            { displayName: 'C.W', value: CommonEnum.QUANTITY_TYPE.CW },
            { displayName: 'CBM', value: CommonEnum.QUANTITY_TYPE.CBM },
            { displayName: 'P.K', value: CommonEnum.QUANTITY_TYPE.PACKAGE },
            { displayName: 'Cont', value: CommonEnum.QUANTITY_TYPE.CONT },
            { displayName: 'N.W', value: CommonEnum.QUANTITY_TYPE.NW },
        ];
    }

    addCharge() {
        const newCharge: ArrivalFreightCharge = new ArrivalFreightCharge();
        newCharge.currencyId = "USD";

        // * get exchange rate.
        newCharge.exchangeRate = this.defaultExchangeRate;

        this.hblArrivalNote.csArrivalFrieghtCharges.push(newCharge);
        console.log(this.hblArrivalNote);
    }

    deleteFreightCharge(index: number) {
        this.selectedIndexFreightCharge = index;

        // TODO detect => Has Freight charge saved ?
        this.confirmDeletePopup.show();
    }

    onDeleteFreightCharge() {
        this.confirmDeletePopup.hide();
        this.hblArrivalNote.csArrivalFrieghtCharges.splice(this.selectedIndexFreightCharge, 1);
    }

    duplicateFreightCharge(indexFreightCharge: number) {
        const newCharge: ArrivalFreightCharge = new ArrivalFreightCharge(this.freightCharges[indexFreightCharge]);

        this.hblArrivalNote.csArrivalFrieghtCharges.push(newCharge);
    }

    getMasterData() {
        forkJoin([
            this._catalogueRepo.getCharges({ active: true }),
            this._catalogueRepo.getUnit({ active: true }),
            this._catalogueRepo.getCurrencyBy({ active: true }),
            this._catalogueRepo.convertExchangeRate(formatDate(new Date(), 'yyyy-MM-dd', 'en'), 'USD')
        ])
            .pipe(catchError(this.catchError))
            .subscribe(
                ([charges, units, currencies, exchangeRate]: any[] = [[], [], [], []]) => {
                    this.listCharges = charges;
                    this.listUnits = units;
                    this.listCurrency = currencies;

                    this.defaultExchangeRate = exchangeRate.rate;
                    console.log(exchangeRate);
                    this._dataService.setDataService(SystemConstants.CSTORAGE.CHARGE, this.listCharges);
                    this._dataService.setDataService(SystemConstants.CSTORAGE.CURRENCY, this.listCurrency);
                }
            );
    }

    onChangeCurrency(currencyId: any, charge: ArrivalFreightCharge) {
        this.updateExchangeRate(charge, currencyId);
    }

    updateExchangeRate(charge: ArrivalFreightCharge, currency: any) {
        this._spinner.show();
        this._catalogueRepo.convertExchangeRate(formatDate(new Date(), 'yyyy-MM-dd', 'en'), currency)
            .pipe(catchError(this.catchError), finalize(() => this._spinner.hide()))
            .subscribe(
                (res: IExchangeRate) => {
                    if (!!res) {
                        charge.exchangeRate = res.rate;
                    }
                }
            );
    }

    onChangeQuantity(quantity: number, chargeItem: ArrivalFreightCharge) {
        chargeItem.total = this.utility.calculateTotalAmountWithVat(chargeItem.vatrate, quantity, chargeItem.unitPrice);
    }

    onChangeVat(vat: number, chargeItem: ArrivalFreightCharge) {
        chargeItem.total = this.utility.calculateTotalAmountWithVat(vat, chargeItem.quantity, chargeItem.unitPrice);
    }

    onChangeUnitPrice(unitPrice: number, chargeItem: ArrivalFreightCharge) {
        chargeItem.total = this.utility.calculateTotalAmountWithVat(chargeItem.vatrate, chargeItem.quantity, unitPrice);
    }

    onChangeQuantityType(type: string, chargeItem: ArrivalFreightCharge) {
        switch (type) {
            case CommonEnum.QUANTITY_TYPE.GW:
                chargeItem.quantity = this.calculateContainer(this.containersHBL, CommonEnum.QUANTITY_TYPE.GW);
                break;
            case CommonEnum.QUANTITY_TYPE.NW:
                chargeItem.quantity = this.calculateContainer(this.containersHBL, CommonEnum.QUANTITY_TYPE.NW);
                break;
            case CommonEnum.QUANTITY_TYPE.CBM:
                chargeItem.quantity = this.calculateContainer(this.containersHBL, CommonEnum.QUANTITY_TYPE.CBM);
                break;
            case CommonEnum.QUANTITY_TYPE.CONT:
                chargeItem.quantity = this.calculateContainer(this.containersShipment, 'quantity');
                break;
            case CommonEnum.QUANTITY_TYPE.CW:
                chargeItem.quantity = this.calculateContainer(this.containersShipment, 'chargeAbleWeight');
                break;
            case CommonEnum.QUANTITY_TYPE.PACKAGE:
                chargeItem.quantity = this.calculateContainer(this.containersShipment, 'packageQuantity');
                break;
            default:
                break;
        }

        // * Update quantityType, total.
        chargeItem.quantityType = type;
        chargeItem.total = this.utility.calculateTotalAmountWithVat(chargeItem.vatrate, chargeItem.quantity, chargeItem.unitPrice);
    }

    onSelectCharge(data: Surcharge, chargeItem: ArrivalFreightCharge) {
        chargeItem.chargeId = data.id;

        // * Update unit Id.
        chargeItem.unitId = data.unitId;

    }

    calculateContainer(containers: Container[], key: string): number {
        let total: number = 0;
        total = containers.reduce((acc: any, curr: Container) => acc += curr[key], 0);
        return total;
    }

}

export interface IExchangeRate {
    id: number;
    currencyFromID: string;
    rate: number;
    currencyToID: string;
}

