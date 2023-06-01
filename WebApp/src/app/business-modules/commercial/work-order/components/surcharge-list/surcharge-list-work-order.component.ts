import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Charge, Partner, Unit, WorkOrderModel, WorkOrderPriceModel, WorkOrderSurchargeModel, WorkOrderViewUpdateModel } from '@models';
import { ActionsSubject, Store } from '@ngrx/store';
import { CatalogueRepo } from '@repositories';
import cloneDeep from 'lodash-es/cloneDeep';
import { Observable } from 'rxjs';
import { filter, finalize, map, shareReplay, take, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { WorkOrderActionTypes, WorkOrderPriceItemUpdateModeState, workOrderDetailIsReadOnlyState, workOrderDetailState } from '../../store';
import { IWorkOrderDetailState } from '../../store/reducers/work-order-detail.reducer';
import { ToastrService } from 'ngx-toastr';
import { GetCatalogueUnitAction, getCatalogueUnitState } from '@store';

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
    workOrderPriceItem: WorkOrderPriceModel;
    workOrderPricePartnerId: string;
    workOrderPricePartnerName: string;
    workOrderDetail: WorkOrderViewUpdateModel;

    units: Unit[] = [];
    isValidPrimary: boolean = true;

    constructor(
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _store: Store<IWorkOrderDetailState>,
        private readonly _toast: ToastrService,
        private readonly _cd: ChangeDetectorRef,
        private readonly _actionStoreSubject: ActionsSubject,
    ) {
        super();
    }

    ngOnInit(): void {
        this.isCollapsed = !!this.surcharges.length;
        this.headers = [
            { title: 'Partner Type', field: '', required: true },
            { title: 'Partner', field: 'partnerName', required: true },
            { title: 'Charge', field: 'chargeName', required: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true, required: true },
            { title: 'Currency', field: 'currencyId', sortable: true, required: true, width: 50 },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Unit', field: 'unitId', sortable: true },
            { title: 'Primary', field: 'isPrimary', sortable: true, align: 'center', width: 50 },
        ];

        if (this.type === 'BUY') {
            this.headers.push({ title: 'KB', field: 'kickBack', sortable: true, align: 'center', width: 50 })
        }

        this.isReadonly = this._store.select(workOrderDetailIsReadOnlyState);

        this.isLoadingPartner = true;
        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL)
            .pipe(
                shareReplay(),
                finalize(() => this.isLoadingPartner = false)
            );
        
        // * listen WorkOrderPriceItemUpdateModeState from store.
        this._store.select(WorkOrderPriceItemUpdateModeState)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                filter(x => !!x)
            )
            .subscribe(
                (priceItem: WorkOrderPriceModel) => {
                    this.workOrderPriceItem = priceItem;
                    this.workOrderPricePartnerId = priceItem.partnerId;
                    this.workOrderPricePartnerName = priceItem.partnerName;
                }
            )

        // * listen workOrderDetailState from store.
        this._store.select(workOrderDetailState)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                filter(x => !!x.id)
            )
            .subscribe(
                (woDetail: any) => {
                    this.workOrderDetail = woDetail;
                }
            )

        // * listen SelectPartnerPriceItemWorkOrder dispatch event from store.
        this._actionStoreSubject
            .pipe(
                filter((x: { type: WorkOrderActionTypes, data: Partner }) => x.type === WorkOrderActionTypes.SELECT_PARTNER_PRICE_ITEM),
                map(d => d.data),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: Partner) => {
                    console.log("select partner price item", data);
                    this.workOrderPricePartnerId = data.id;
                    this.workOrderPricePartnerName = data.shortName;

                    if (!!this.surcharges.length) {
                        const surchargeCarriers = this.surcharges.filter(x => x.partnerType === 'Carrier');
                        if (surchargeCarriers.length) {
                            surchargeCarriers.forEach((x: WorkOrderSurchargeModel) => {
                                x.partnerId = data.id;
                                x.partnerName = data.shortName;
                            })
                        }
                    }
                }
            )

        // * listen ResetUpdatePriceItemWorkOrder dispatch event from store.
        this._actionStoreSubject
            .pipe(
                filter((x: { type: WorkOrderActionTypes }) => x.type === WorkOrderActionTypes.RESET_UPDATE_PRICE_ITEM),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                () => {
                    console.log("reset update price item");
                    this.workOrderPricePartnerId = null;
                    this.workOrderPricePartnerName = null;
                }
            )

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

    loadUnit() {
        if (!!this.units.length) return;

        this._store.dispatch(new GetCatalogueUnitAction());
        this._store.select(getCatalogueUnitState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (units: Unit[]) => {
                    this.units = units || [];
                    console.log(this.units);
                    this._cd.markForCheck();
                }
            )
    }

    duplicateCharge(index: number) {
        // this.isSubmitted = false;
        var newsurcharge = new WorkOrderSurchargeModel(this.surcharges[index]);
        newsurcharge.id = SystemConstants.EMPTY_GUID;
        newsurcharge.partnerName = this.surcharges[index].partnerName;
        newsurcharge.unitCode = this.surcharges[index].unitCode;

        this.surcharges.push(cloneDeep(newsurcharge));
    }

    deleteCharge(index: number) {
        const deletedIndexItem = this.surcharges[index];

        // this.isSubmitted = false;
        this.surcharges.splice(index, 1);

        this.cachedSurcharge.push(deletedIndexItem);
    }

    addCharge() {
        const partnerType: string = this.type === 'SELL' ? 'Customer' : 'Carrier';

        const newSurchargeItem: WorkOrderSurchargeModel = new WorkOrderSurchargeModel({
            chargeId: null,
            unitPrice: null,
            currencyId: this.transactionType.includes('A') ? 'USD' : 'VND',
            vatrate: null,
            partnerType: partnerType,
            type: this.type,
            kickBack: null,
            id: SystemConstants.EMPTY_GUID,
            partnerId: this.generatePartner(this.type, partnerType, 'id'),
            workOrderId: SystemConstants.EMPTY_GUID,
            workOrderPriceId: SystemConstants.EMPTY_GUID,
        });

        newSurchargeItem.partnerName = this.generatePartner(this.type, partnerType, 'name');

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
                surcharge.partnerId = this.generatePartner(this.type, data, 'id');
                surcharge.partnerName = this.generatePartner(this.type, data, 'name');
                if (!surcharge.partnerId || !surcharge.partnerName) {
                    this._toast.warning('No partner information found. Please try again.');
                }
                break;
            case 'unitId':
                surcharge.unitId = data.id || null;
                surcharge.unitCode = data.unitCode || null;
                break;
            default:
                break;
        }
    }

    generatePartner(chargeType: string, partnerType: string, key: string) {
        let partner = null;
        switch (partnerType) {
            case 'Customer':
                if (key === 'id') {
                    partner = this.workOrderDetail?.partnerId || null;
                } else {
                    partner = this.workOrderDetail?.partnerName || null;
                }

                break;
            case 'Agent':
                if (key === 'id') {
                    partner = this.workOrderDetail?.agentId || null;
                } else {
                    partner = this.workOrderDetail?.agentName || null;
                }
                break;
            case 'Carrier':
                if (key === 'id') {
                    partner = this.workOrderPricePartnerId || null;
                } else {
                    partner = this.workOrderPricePartnerName || null;
                }
                break
            default:
                break;
        }
        return partner;

    }

    handleChangePartnerCarrier() {

    }
}

