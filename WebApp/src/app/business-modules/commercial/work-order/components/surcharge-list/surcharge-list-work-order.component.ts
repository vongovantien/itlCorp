import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Charge } from '@models';
import { CatalogueRepo } from '@repositories';
import cloneDeep from 'lodash/cloneDeep';
import { Observable } from 'rxjs';
import { shareReplay, switchMap, switchMapTo, takeUntil, tap } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'surcharge-list-work-order',
    templateUrl: './surcharge-list-work-order.component.html',
})
export class CommercialSurchargeListWorkOrderComponent extends AppList implements OnInit {

    @Input() type: string;
    @Input() transactionType: string;
    @Input() set submitted(isSubmit: boolean) {
        this.isSubmitted = isSubmit;
    }

    get submitted() {
        return this.isSubmitted;
    }

    @Input() surcharges: ISurchargeWorkOrder[] = [];
    @Output() surchargesChange: EventEmitter<ISurchargeWorkOrder[]> = new EventEmitter<ISurchargeWorkOrder[]>();

    private isSubmitted: boolean;
    isLoading = false;
    partnerTypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Customer', value: 'Customer' },
        { title: 'Agent', value: 'Agent' },
        { title: 'Carrier', value: 'Carrier' },
    ];
    charges: Observable<Charge[]>;

    configComboGridCharge = [
        { field: 'chargeNameEn', label: 'Name En' },
        { field: 'code', label: 'Code' },
    ]

    constructor(
        private readonly _catalogueRepo: CatalogueRepo,
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Partner Type', field: '', required: true },
            { title: 'Charge Name', field: '', required: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true, required: true },
            { title: 'Currency', field: 'currencyId', sortable: true, required: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
        ];

        if (this.type === 'BUY') {
            this.headers.push({ title: 'KB', field: 'kickBack', sortable: true, align: 'center' })
        }

        this.charges = this._catalogueRepo.getCharges({
            active: true,
            serviceTypeId: this.transactionType,
            type: this.utility.getChargeType(this.type),
        }).pipe(
            shareReplay(),
            takeUntil(this.ngUnsubscribe),
        )

    }

    duplicateCharge(index: number) {
        this.isSubmitted = false;
        this.surcharges.push(cloneDeep(this.surcharges[index]));
    }

    deleteCharge(index: number) {
        this.isSubmitted = false;
        this.surcharges.splice(index, 1);
    }

    addCharge() {
        console.log(this.type);
        console.log(this.surcharges);
        this.surcharges.push({
            chargeId: null,
            unitPrice: null,
            currencyId: this.transactionType.includes('A') ? 'USD' : 'VND',
            vatrate: null,
            partnerType: 'Customer',
            type: this.type,
            kickBack: null
        });
    }

    onSelectDataTableInfo(data: any, surcharge: ISurchargeWorkOrder, type: string) {
        switch (type) {
            case 'chargeId':
                surcharge.chargeId = data;
                break;

            default:
                break;
        }
    }
}

export interface ISurchargeWorkOrder {
    chargeId: string;
    unitPrice: number;
    currencyId: string;
    type: string;
    vatrate: number;
    partnerType: string;
    kickBack: boolean;
}
