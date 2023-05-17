import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Charge, Partner, WorkOrderSurchargeModel } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo } from '@repositories';
import cloneDeep from 'lodash-es/cloneDeep';
import { Observable } from 'rxjs';
import { finalize, shareReplay, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { workOrderDetailIsReadOnlyState } from '../../store';
import { IWorkOrderDetailState } from '../../store/reducers/work-order-detail.reducer';

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

    @Input() surcharges: WorkOrderSurchargeModel[] = [];
    @Output() surchargesChange: EventEmitter<WorkOrderSurchargeModel[]> = new EventEmitter<WorkOrderSurchargeModel[]>();

    private isSubmitted: boolean;
    isLoadingPartner = false;
    partnerTypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Customer', value: 'Customer' },
        { title: 'Agent', value: 'Agent' },
        { title: 'Carrier', value: 'Carrier' },
        { title: 'Other', value: 'Other' },
    ];
    charges: Observable<Charge[]>;
    partners: Observable<Partner[]>;

    configComboGridCharge = [
        { field: 'chargeNameEn', label: 'Name En' },
        { field: 'code', label: 'Code' },
    ];
    displayFieldsPartner = JobConstants.CONFIG.COMBOGRID_PARTNER;


    cachedSurcharge: WorkOrderSurchargeModel[] = [];

    constructor(
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _store: Store<IWorkOrderDetailState>,
    ) {
        super();
    }

    ngOnInit(): void {
        this.isCollapsed = !!this.surcharges.length;
        this.headers = [
            { title: 'Partner Type', field: '', required: true },
            { title: 'Partner Name', field: '' },
            { title: 'Charge Name', field: '', required: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true, required: true },
            { title: 'Currency', field: 'currencyId', sortable: true, required: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
        ];

        if (this.type === 'BUY') {
            this.headers.push({ title: 'KB', field: 'kickBack', sortable: true, align: 'center' })
        }

        this.isReadonly = this._store.select(workOrderDetailIsReadOnlyState);

        this.isLoadingPartner = true;
        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL)
            .pipe(
                shareReplay(),
                finalize(() => this.isLoadingPartner = false)
            );
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes.hasOwnProperty("transactionType") && !!changes.transactionType.currentValue) {
            this.transactionType = changes.transactionType.currentValue;

            this.charges = this._catalogueRepo.getCharges({
                active: true,
                serviceTypeId: this.transactionType,
                type: this.utility.getChargeType(this.type),
            }).pipe(
                shareReplay(),
                takeUntil(this.ngUnsubscribe),
            )
        }
    }

    duplicateCharge(index: number) {
        // this.isSubmitted = false;
        var newsurcharge = new WorkOrderSurchargeModel(this.surcharges[index]);
        newsurcharge.id = SystemConstants.EMPTY_GUID;
        this.surcharges.push(cloneDeep(newsurcharge));
    }

    deleteCharge(index: number) {
        const deletedIndexItem = this.surcharges[index];

        // this.isSubmitted = false;
        this.surcharges.splice(index, 1);

        this.cachedSurcharge.push(deletedIndexItem);
    }

    addCharge() {
        const newSurchargeItem: WorkOrderSurchargeModel = new WorkOrderSurchargeModel({
            chargeId: null,
            unitPrice: null,
            currencyId: this.transactionType.includes('A') ? 'USD' : 'VND',
            vatrate: null,
            partnerType: this.type === 'SELL' ? 'Customer' : 'Carrier',
            type: this.type,
            kickBack: null,
            id: SystemConstants.EMPTY_GUID,
            partnerId: null,
            workOrderId: SystemConstants.EMPTY_GUID,
            workOrderPriceId: SystemConstants.EMPTY_GUID,
            partnerName: null
        });

        if (!!this.cachedSurcharge.length) {
            // * replace id of new item with id of cached item.
            newSurchargeItem.id = this.cachedSurcharge[0].id;
            newSurchargeItem.workOrderId = this.cachedSurcharge[0].workOrderId;
            newSurchargeItem.workOrderPriceId = this.cachedSurcharge[0].workOrderPriceId;
            newSurchargeItem.datetimeCreated = this.cachedSurcharge[0].datetimeCreated;
            newSurchargeItem.userCreated = this.cachedSurcharge[0].userCreated;

            // * remove item from cache
            this.cachedSurcharge.splice(0, 1);
        }

        this.surcharges.push(newSurchargeItem);

    }

    onSelectDataTableInfo(data: any, surcharge: WorkOrderSurchargeModel, type: string) {
        surcharge[type] = data;
        switch (type) {
            case 'chargeId':
                surcharge.chargeId = data.id || null;
                surcharge.unitPrice = data.unitPrice || null;
                surcharge.vatRate = data.vatrate || null;
                if (data.chargeGroupName === 'Com') {
                    surcharge.kickBack = true;
                } else {
                    surcharge.kickBack = false;
                }
                break;
            case 'partnerType':
                if (data !== 'Other') {
                    surcharge.partnerId = null;
                    surcharge.partnerName = null;
                }
                break;
            default:
                break;
        }
    }
}

