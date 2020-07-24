import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { CatChargeIncoterm, Charge, Unit, Currency } from '@models';
import { CommonEnum } from '@enums';
import { CatalogueRepo } from '@repositories';
import { map, takeUntil, shareReplay } from 'rxjs/operators';
import { forkJoin, Observable } from 'rxjs';

@Component({
    selector: 'list-charge-incoterm',
    templateUrl: './list-charge-incoterm.component.html',
    styleUrls: ['./list-charge-incoterm.component.scss']
})
export class CommercialListChargeIncotermComponent extends AppList implements OnInit {

    sellings: CatChargeIncoterm[] = [];
    buyings: CatChargeIncoterm[] = [];

    listCharges: Charge[] = [];
    listUnits: Observable<Unit[]>;
    listCurrency: Observable<Currency[]>;


    configChargeDisplayFields: CommonInterface.IComboGridDisplayField[] = [
        { field: 'chargeNameEn', label: 'Charge Code' },
        { field: 'code', label: 'Charge Name' },
    ];
    quantityHints: CommonInterface.IValueDisplay[] = [
        { displayName: 'G.W', value: CommonEnum.QUANTITY_TYPE.GW },
        { displayName: 'C.W', value: CommonEnum.QUANTITY_TYPE.CW },
        { displayName: 'CBM', value: CommonEnum.QUANTITY_TYPE.CBM },
        { displayName: 'P.K', value: CommonEnum.QUANTITY_TYPE.PACKAGE },
        { displayName: 'Cont', value: CommonEnum.QUANTITY_TYPE.CONT },
        { displayName: 'N.W', value: CommonEnum.QUANTITY_TYPE.NW },
    ];
    chargeTo: CommonInterface.IValueDisplay[] = [
        { displayName: 'Customer', value: CommonEnum.PartnerGroupEnum.CUSTOMER },
        { displayName: 'Agent', value: CommonEnum.PartnerGroupEnum.AGENT },
        { displayName: 'Shipper', value: CommonEnum.PartnerGroupEnum.SHIPPER },
        { displayName: 'Consignee', value: CommonEnum.PartnerGroupEnum.CONSIGNEE },
        { displayName: 'Carrier', value: CommonEnum.PartnerGroupEnum.CARRIER },
    ];
    feeTypes: CommonInterface.IValueDisplay[] = [
        { displayName: 'Handing', value: CommonEnum.FEE_TYPE.HANDLING },
        { displayName: 'Freight', value: CommonEnum.FEE_TYPE.FREIGHT },
        { displayName: 'Local Charge', value: CommonEnum.FEE_TYPE.LOCALCHARGE },
        { displayName: 'Logistics', value: CommonEnum.FEE_TYPE.LOGISTICS },
        { displayName: 'Fuel', value: CommonEnum.FEE_TYPE.FUEL },
        { displayName: 'Trucking', value: CommonEnum.FEE_TYPE.TRUCKING },
        { displayName: 'Other', value: CommonEnum.FEE_TYPE.OTHER },
    ];

    constructor(
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Charge Name', field: 'chargeId', sortable: true, required: true },
            { title: 'Hint Qty', field: 'chargeId', sortable: true, required: true },
            { title: 'Unit', field: 'chargeId', sortable: true, required: true },
            { title: 'Charge To', field: 'chargeId', sortable: true, required: true },
            { title: 'Currency', field: 'chargeId', sortable: true, required: true },
            { title: 'Fee Type', field: 'chargeId', sortable: true, required: true },
        ];
        this.getListCharge();
        this.listUnits = this._catalogueRepo.getUnit({ active: true }).pipe(shareReplay());
        this.listCurrency = this._catalogueRepo.getListCurrency().pipe(shareReplay());

    }

    addCharge(type: string) {
        if (type === CommonEnum.SurchargeTypeEnum.SELLING_RATE) {
            this.sellings.push(new CatChargeIncoterm({ type: CommonEnum.SurchargeTypeEnum.SELLING_RATE }));
            return;
        }
        this.sellings.push(new CatChargeIncoterm({ type: CommonEnum.SurchargeTypeEnum.BUYING_RATE }));
    }

    deleteCharge(index: number, type: string) {
    }

    getListCharge(serviceTypeId: string = null) {
        this._catalogueRepo.getListCharge(null, null, { active: true, serviceTypeId: serviceTypeId })
            .pipe(
                takeUntil(this.ngUnsubscribe)
            ).subscribe(
                (res: any[]) => {
                    this.listCharges = res;
                    console.log(this.listCharges);
                }
            );
    }

    onSelectDataTableInfo(data: any, chargeItem: CatChargeIncoterm, key: string, type: string) {
        console.log(data);
    }

    getUnitCurrency() {
        forkJoin([
            this._catalogueRepo.getUnit({ active: true }),
            this._catalogueRepo.getListCurrency(),
        ]).pipe(
            takeUntil(this.ngUnsubscribe)
        ).subscribe(
            ([units, currencies]) => {
                this.listUnits = units || [];
                this.listCurrency = currencies || [];
            }
        );

    }

}

