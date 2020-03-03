import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CatPartnerCharge, Charge, Unit, Currency } from '@models';
import { Observable } from 'rxjs';
import { GetCatalogueCurrencyAction, GetCatalogueUnitAction, getCatalogueCurrencyState, getCatalogueUnitState } from '@store';
import { Store } from '@ngrx/store';
import { IShareBussinessState } from '@share-bussiness';
import { finalize, catchError } from 'rxjs/operators';
import { CatalogueRepo } from '@repositories';
import { CommonEnum } from '@enums';
import { ToastrService } from 'ngx-toastr';
import cloneDeep from 'lodash/cloneDeep';

@Component({
    selector: 'partner-other-charge-popup',
    templateUrl: './partner-other-charge.popup.html',
    styleUrls: ['./partner-other-charge.popup.scss']
})
export class PartnerOtherChargePopupComponent extends PopupBase implements OnInit {


    charges: CatPartnerCharge[] = [];
    initCharges: CatPartnerCharge[] = [];

    listCharges: Charge[];
    listUnits: Unit[] = [];
    listCurrency: Observable<Currency[]>;

    quantityHints: CommonInterface.IValueDisplay[] = [
        { displayName: 'G.W', value: CommonEnum.QUANTITY_TYPE.GW },
        { displayName: 'C.W', value: CommonEnum.QUANTITY_TYPE.CW },
        { displayName: 'CBM', value: CommonEnum.QUANTITY_TYPE.CBM },
        { displayName: 'P.K', value: CommonEnum.QUANTITY_TYPE.PACKAGE },
        { displayName: 'Cont', value: CommonEnum.QUANTITY_TYPE.CONT },
        { displayName: 'N.W', value: CommonEnum.QUANTITY_TYPE.NW },
    ];
    configComboGridCharge: Partial<CommonInterface.IComboGirdConfig> = {};

    constructor(
        private _store: Store<IShareBussinessState>,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit(): void {
        this.configComboGridCharge = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'chargeNameEn', label: 'Name' },
                { field: 'unitPrice', label: 'Unit Price' },
                { field: 'unitId', label: 'Unit' },
                { field: 'code', label: 'Code' },
            ]
        }, { selectedDisplayFields: ['chargeNameEn'], });
        this.headers = [
            { title: 'Charge Name', field: 'chargeId', sortable: true, width: 200 },
            { title: 'Quantity', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unitId', sortable: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
        ];

        this._store.dispatch(new GetCatalogueCurrencyAction());
        this._store.dispatch(new GetCatalogueUnitAction());
        this.getCharge();

        this.listCurrency = this._store.select(getCatalogueCurrencyState);
        this._store.select(getCatalogueUnitState)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (units: Unit[]) => {
                    this.listUnits = units;
                }
            );

    }

    getCharge() {
        this._catalogueRepo.getCharges({ active: true, type: CommonEnum.CHARGE_TYPE.CREDIT })
            .subscribe(
                (charges: Charge[]) => {
                    this.listCharges = charges;
                }
            );
    }

    addCharge() {
        this.initCharges = [...this.initCharges, new CatPartnerCharge()];
    }

    deleteCharge(charge: CatPartnerCharge, index: number) {
        this.initCharges = [...this.initCharges.slice(0, index), ...this.initCharges.slice(index + 1)];
    }

    onSelectDataFormInfo(data: Charge, partnerCharge: CatPartnerCharge) {
        partnerCharge.chargeId = data.id;

    }

    onSavePartnerCharge() {
        if (this.charges.length) {
            this._toastService.warning("Please input charge");
        }
        if (!this.checkValidate()) {
            return;
        }
        this.charges = cloneDeep(this.initCharges);
        this.charges.forEach(c => {
            c.partnerId = '65794475-2cc8-48bd-a272-cf4a07c1131c';
        });
        console.log(this.charges);

        this._catalogueRepo.updatePartnerCharge(this.charges)
            .pipe()
            .subscribe((res: CommonInterface.IResult) => {
                if (res.status) {
                    this._toastService.success(res.message);
                } else {
                    this._toastService.error(res.message);
                }
                this.isSubmitted = false;
                this.hide();
                console.log(res);
            });
    }

    checkValidate() {
        let valid: boolean = true;
        for (const charge of this.initCharges) {
            if (
                !charge.chargeId
                || charge.vatrate > 100
            ) {
                valid = false;
                break;
            }
        }

        return valid;
    }

    closePopup() {
        this.isSubmitted = false;
        this.initCharges = cloneDeep(this.charges);
        this.hide();
    }
}
