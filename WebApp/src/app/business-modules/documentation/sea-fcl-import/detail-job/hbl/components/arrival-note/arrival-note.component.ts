import { Component, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';
import { formatDate } from '@angular/common';

import { AppList } from 'src/app/app.list';
import { ArrivalFreightCharge, Surcharge, Unit, Currency, Charge, User } from 'src/app/shared/models';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { DataService, SortService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { HBLArrivalNote } from 'src/app/shared/models/document/arrival-note-hbl';
import { Container } from 'src/app/shared/models/document/container.model';

import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize, takeUntil, tap, switchMap } from 'rxjs/operators';

import { getContainerSaveState, getHBLState } from '../../../../store';


@Component({
    selector: 'sea-fcl-import-hbl-arrival-note',
    templateUrl: './arrival-note.component.html',
    styleUrls: ['./arrival-note.component.scss'],
})

export class SeaFClImportArrivalNoteComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    hblArrivalNote: HBLArrivalNote = new HBLArrivalNote();

    headers: CommonInterface.IHeaderTable[];

    userLogged: User = new User();

    freightCharges: ArrivalFreightCharge[] = new Array<ArrivalFreightCharge>();
    listCharges: Charge[] = [];
    listUnits: Unit[] = [];
    listCurrency: Currency[] = [];
    quantityTypes: CommonInterface.IValueDisplay[];
    containersShipment: Container[] = [];
    containersHBL: Container[] = [];

    configComboGridCharge: Partial<CommonInterface.IComboGirdConfig> = {};

    defaultExchangeRate: number;
    selectedIndexFreightCharge: number = -1;

    isSubmitted: boolean = false;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _dataService: DataService,
        private _store: Store<any>,
        private _documentRepo: DocumentationRepo,
        private _sortService: SortService,
        private _ngProgress: NgProgress,
        private _toastService: ToastrService
    ) {
        super();

        this._progressRef = this._ngProgress.ref();

        this.requestSort = this.sortFreightCharge;

        this.userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));

    }

    ngOnInit() {
        this.configData();

        this.getCharge();
        this.getCurrency();
        this.getUnit();

        // * Get default exchange rate.
        this.getExchangeRate('USD', formatDate(new Date(), 'yyyy-MM-dd', 'en')).then(
            (exchangeRate: IExchangeRate) => {
                this.defaultExchangeRate = exchangeRate.rate;
            }
        );


        // * Get container's shipment from Store.
        this._store.select(getContainerSaveState).subscribe(
            (res: Container[] | any[]) => {
                this.containersShipment = res || [];
            }
        );

        this._store.select(getHBLState)
            .pipe(
                catchError(this.catchError),
                takeUntil(this.ngUnsubscribe),
                tap((res: any) => {
                    this.hblArrivalNote.hblid = res.data.id || SystemConstants.EMPTY_GUID;
                    this.containersHBL = res.data.csMawbcontainers || []; // * Get container from HBL detail.
                }),
                switchMap(() => this._documentRepo.getArrivalInfo(this.hblArrivalNote.hblid, CommonEnum.TransactionTypeEnum.SeaFCLImport)) // * Get arrival info.
            )
            .subscribe(
                (res: HBLArrivalNote) => {
                    if (!!res) {
                        this.hblArrivalNote = res;
                        this.hblArrivalNote.arrivalFirstNotice = !!res.arrivalFirstNotice ? { startDate: new Date(res.arrivalFirstNotice), endDate: new Date(res.arrivalSecondNotice) } : null;
                        this.hblArrivalNote.arrivalSecondNotice = !!res.arrivalSecondNotice ? { startDate: new Date(res.arrivalSecondNotice), endDate: new Date(res.arrivalSecondNotice) } : null;
                    }
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

        this.headers = [
            { title: 'Charge', field: 'chargeId', sortable: true, width: 300 },
            { title: 'Quantity', field: 'quantity', sortable: true, width: 200 },
            { title: 'Unit', field: 'unitId', sortable: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Total Amount', field: 'total', sortable: true },
            { title: 'Exchange Rate', field: 'exchangeRate', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'Show', field: 'isShow', sortable: true },
            { title: 'Full', field: 'isFull', sortable: true },
            { title: 'Tick', field: 'isTick', sortable: true },
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

        // * Update field.
        newCharge.exchangeRate = this.defaultExchangeRate;
        newCharge.hblid = this.hblArrivalNote.hblid;

        this.hblArrivalNote.csArrivalFrieghtCharges.push(newCharge);
    }

    deleteFreightCharge(index: number, chargeItem: ArrivalFreightCharge) {
        this.selectedIndexFreightCharge = index;

        // * detect => Has Freight charge saved ?
        if (chargeItem.id === SystemConstants.EMPTY_GUID) {
            this.hblArrivalNote.csArrivalFrieghtCharges.splice(index, 1);
        } else {
            this.confirmDeletePopup.show();
        }
    }

    sortFreightCharge() {
        this.hblArrivalNote.csArrivalFrieghtCharges = this._sortService.sort(this.hblArrivalNote.csArrivalFrieghtCharges, this.sort, this.order);
    }

    onDeleteFreightCharge() {
        this.confirmDeletePopup.hide();
        this.hblArrivalNote.csArrivalFrieghtCharges.splice(this.selectedIndexFreightCharge, 1);
    }

    duplicateFreightCharge(indexFreightCharge: number) {
        const newCharge: ArrivalFreightCharge = new ArrivalFreightCharge(this.freightCharges[indexFreightCharge]);

        this.hblArrivalNote.csArrivalFrieghtCharges.push(newCharge);
    }

    getCharge() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CHARGE)) {
            this.listCharges = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CHARGE);
        } else {
            this._catalogueRepo.getCharges({ active: true }).pipe(
                catchError(this.catchError)
            ).subscribe(
                (charges: Charge[]) => {
                    this.listCharges = charges;
                    if (!!charges.length) {
                        this._dataService.setDataService(SystemConstants.CSTORAGE.CHARGE, this.listCharges);
                    }
                }
            );
        }
    }

    getCurrency() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY)) {
            this.listCurrency = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY);
        } else {
            this._catalogueRepo.getCurrencyBy({ active: true }).pipe(
                catchError(this.catchError)
            ).subscribe(
                (currencies: Currency[]) => {
                    this.listCurrency = currencies;
                    if (!!currencies.length) {
                        this._dataService.setDataService(SystemConstants.CSTORAGE.CURRENCY, this.listCurrency);
                    }
                }
            );
        }
    }

    getUnit() {
        this._catalogueRepo.getUnit({ active: true }).pipe(
            catchError(this.catchError)
        ).subscribe(
            (units: Unit[]) => {
                this.listUnits = units;
            }
        );
    }

    async getExchangeRate(currency: any, date: string) {
        try {
            this._progressRef.start();
            const exchangeRate: IExchangeRate = await this._catalogueRepo.convertExchangeRate(date, currency).toPromise();
            if (!!exchangeRate) {
                return exchangeRate;
            }
        } catch (error) {

        } finally {
            this._progressRef.complete();
        }
    }

    onChangeCurrency(currencyId: any, charge: ArrivalFreightCharge) {
        this.getExchangeRate(currencyId, formatDate(new Date(), 'yyyy-MM-dd', 'en')).then(
            (exchangeRate: IExchangeRate) => {
                if (!!exchangeRate) {
                    charge.exchangeRate = exchangeRate.rate;
                } else {
                    charge.exchangeRate = 0;
                }
            }
        );
    }

    saveArrivalNote() {
        this._progressRef.start();

        const dateNotice = {
            arrivalFirstNotice: !!this.hblArrivalNote.arrivalFirstNotice && !!this.hblArrivalNote.arrivalFirstNotice.startDate ? formatDate(this.hblArrivalNote.arrivalFirstNotice.startDate, 'yyyy-MM-dd', 'en') : null,
            arrivalSecondNotice: !!this.hblArrivalNote.arrivalSecondNotice && <any>!!this.hblArrivalNote.arrivalSecondNotice.startDate ? formatDate(this.hblArrivalNote.arrivalSecondNotice.startDate, 'yyyy-MM-dd', 'en') : null,
        };
        this._documentRepo.updateArrivalInfo(Object.assign({}, this.hblArrivalNote, dateNotice))
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                    }
                    console.log(res);
                }
            );
    }

    setDefaultHeadeFooter() {
        const body: IArrivalDefault = {
            transactionType: CommonEnum.TransactionTypeEnum.SeaFCLImport,
            userDefault: this.userLogged.id,
            arrivalFooter: this.hblArrivalNote.arrivalFooter,
            arrivalHeader: this.hblArrivalNote.arrivalHeader
        };

        this._progressRef.start();

        this._documentRepo.updateArrivalInfo(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    setDefaultFreightCharge() {
        const body: IArrivalFreightChargeDefault = {
            userDefault: this.userLogged.id,
            transactionType: CommonEnum.TransactionTypeEnum.SeaFCLImport,
            csArrivalFrieghtChargeDefaults: this.hblArrivalNote.csArrivalFrieghtCharges
        };

        this._progressRef.start();

        this._documentRepo.setArrivalFreightChargeDefault(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error(res.message);
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

    checkValidate() {
        let valid: boolean = true;
        for (const charge of this.hblArrivalNote.csArrivalFrieghtCharges) {
            if (
                !charge.chargeId
                || charge.unitPrice < 0
                || charge.vatrate > 100
            ) {
                valid = false;
                break;
            }
        }

        return valid;
    }


}

interface IExchangeRate {
    id: number;
    currencyFromID: string;
    rate: number;
    currencyToID: string;
}
interface IArrivalDefault {
    transactionType: number;
    userDefault: string;
    arrivalHeader: string;
    arrivalFooter: string;
}

interface IArrivalFreightChargeDefault {
    transactionType: number;
    userDefault: string;
    csArrivalFrieghtChargeDefaults: ArrivalFreightCharge[];
}
