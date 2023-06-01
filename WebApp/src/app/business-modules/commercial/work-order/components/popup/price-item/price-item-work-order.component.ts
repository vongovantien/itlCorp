import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { PopupBase } from '@app';
import { ChargeConstants, JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, Unit, WorkOrderPriceModel, WorkOrderSurchargeModel } from '@models';
import { ActionsSubject, Store } from '@ngrx/store';
import { CatalogueRepo } from '@repositories';
import { GetCatalogueUnitAction, getCatalogueUnitState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, Observable, merge } from 'rxjs';
import { filter, finalize, map, pluck, takeUntil, tap, withLatestFrom } from 'rxjs/operators';
import {
    AddPriceItemWorkOrder,
    AddPriceItemWorkOrderSuccess,
    IWorkOrderMngtState,
    ResetUpdatePriceItemWorkOrder,
    SelectPartnerPriceItemWorkOrder,
    UpdatePriceItemWorkOrder,
    UpdatePriceItemWorkOrderSuccess,
    WorkOrderActionTypes,
    workOrderDetailIsReadOnlyState,
    workOrderDetailTransationTypeState,
    WorkOrderListPricestate,
    WorkOrderPriceItemUpdateModeState
}
    from '../../../store';

enum QUANTITYPE {
    FLAT = 'FLAT',
    RANGE = 'RANGE',
    ULD = 'ULD',
}

enum PRICETYPE {
    AIR = 'AIR',
    FCL = 'FCL',
    LCL = 'LCL',
    OTHER = 'OTHER',
}
@Component({
    selector: 'price-item-work-order-popup',
    templateUrl: './price-item-work-order.component.html',
})
export class CommercialPriceItemWorkOrderPopupComponent extends PopupBase implements OnInit {
    transactionType: string;

    partners: Partner[];
    selectedPartnerName: string;

    units: Unit[];
    selectedUnitCode: string;
    types: CommonInterface.ICommonTitleValue[] = [
        { title: 'AIR', value: PRICETYPE.AIR },
        { title: 'FCL', value: PRICETYPE.FCL },
        { title: 'LCL', value: PRICETYPE.LCL },
        { title: 'Other', value: PRICETYPE.OTHER }
    ];
    quantityRanges: CommonInterface.ICommonTitleValue[] = [
        {
            title: '< 20', value: { from: null, to: 20 }
        },
        {
            title: '20 - 40', value: { from: 20, to: 40 }
        },
        {
            title: '> 40', value: { from: 40, to: null }
        }
    ];

    quantityTypes: CommonInterface.ICommonTitleValue[] = [
        {
            title: 'FLAT', value: QUANTITYPE.FLAT,
        },
        {
            title: 'RANGE', value: QUANTITYPE.RANGE
        },
        {
            title: 'ULD', value: QUANTITYPE.ULD
        }
    ];

    form: FormGroup;
    partnerId: AbstractControl;
    unitId: AbstractControl;
    fromValue: AbstractControl;
    toValue: AbstractControl;
    quantityType: AbstractControl;
    type: AbstractControl;

    displayFieldsPartner = JobConstants.CONFIG.COMBOGRID_PARTNER;

    frieghtCharges: IFreightCharge[] = [];
    freightChargeMappingDefaults = new Map([
        [ChargeConstants.AE_CODE, ['BA_A_F_Air', 'SA_A_F_Air']],
        [ChargeConstants.AI_CODE, ['BA_A_F_Air', 'SA_A_F_Air']],
        [ChargeConstants.SFE_CODE, ['BS_OCF_Sea', 'SS_OCF_Sea']],
        [ChargeConstants.SFI_CODE, ['BS_OCF_Sea', 'SS_OCF_Sea']],
        [ChargeConstants.SLE_CODE, ['BS_OCF_Sea', 'SS_OCF_Sea']],
        [ChargeConstants.SLI_CODE, ['BS_OCF_Sea', 'SS_OCF_Sea']],
        [ChargeConstants.SCE_CODE, ['BS_OCF_Sea', 'SS_OCF_Sea']],
        [ChargeConstants.SCI_CODE, ['BS_OCF_Sea', 'SS_OCF_Sea']],
        [ChargeConstants.CL_CODE, ['BO_COF_OPS', 'SO_COF_OPS']],

    ]);
    buyings: WorkOrderSurchargeModel[] = [];
    sellings: WorkOrderSurchargeModel[] = [];

    ACTION: CommonType.ACTION_FORM;
    actionDispatchType: WorkOrderActionTypes;

    id: string;
    workOrderId: string;
    title = 'New Price List';

    popupClosing$: BehaviorSubject<boolean> = new BehaviorSubject(false);

    constructor(
        private readonly _fb: FormBuilder,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _toastService: ToastrService,
        private readonly _activedRouter: ActivatedRoute,
        private readonly _store: Store<IWorkOrderMngtState>,
        private readonly _actionStoreSubject: ActionsSubject,
        private _cd: ChangeDetectorRef
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Charge Name', width: 150, required: true, field: '' },
            { title: 'Unit Price Buying', field: '', required: true, },
            { title: 'Unit Price Selling', field: '', required: true },
            { title: 'VAT Buying', field: '' },
            { title: 'VAT Selling', field: '' },
            { title: 'Note', field: '' },
        ];

        this.initForm();
        this.isReadonly = this._store.select(workOrderDetailIsReadOnlyState);

        /*
            ? Subsribe to get transctionType:
            * from Param for Creating page
            * from Selector from Detail page
        */
        merge(
            this._activedRouter.params.pipe(pluck('transactionType')),
            this._store.select(workOrderDetailTransationTypeState)
        ).pipe(
            filter(x => !!x),
            takeUntil(this.ngUnsubscribe)
        ).subscribe(
            (transactionType: string) => { this.transactionType = transactionType; })

        // * Listen Update Price Item.
        this._store.select(WorkOrderPriceItemUpdateModeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: WorkOrderPriceModel) => {
                    if (!!res) {
                        console.log(res);
                        this.selectedUnitCode = res.unitCode;
                        this.selectedPartnerName = res.partnerName;
                        this.id = res.id;
                        this.workOrderId = res.workOrderId;

                        this.units = [{ id: res.unitId, code: res.unitCode }] as any[];
                        this.form.patchValue({
                            partnerId: res.partnerId,
                            unitId: res.unitId,
                            quantityType: res.quantityType,
                            type: res.type,
                            fromValue: res.quantityFromValue,
                            toValue: res.quantityToValue,
                        });

                        this.frieghtCharges.length = 0;
                        this.frieghtCharges.push({
                            chargeName: `${this.getFreightChargeName(this.transactionType)}`,
                            chargeCodeBuying: this.freightChargeMappingDefaults.get(this.transactionType)[0],
                            chargeCodeSelling: this.freightChargeMappingDefaults.get(this.transactionType)[1],
                            unitPriceSelling: res.unitPriceSelling,
                            unitPriceBuying: res.unitPriceBuying,
                            vatSelling: res.vatrateSelling,
                            vatBuying: res.vatrateBuying,
                            currencyIdBuying: res.currencyIdBuying,
                            currencyIdSelling: res.currencyIdSelling,
                            notes: res.notes,
                            chargeIdbuying: res.chargeIdBuying,
                            chargeIdSelling: res.chargeIdSelling,
                        });

                        this.buyings = (res.surcharges || []).filter(x => x.type === "BUY");
                        this.sellings = (res.surcharges || []).filter(x => x.type === "SELL");
                    }
                }
            )

        // * Listen event dispatch data and check duplicate
        this._actionStoreSubject
            .pipe(
                filter((x: { type: WorkOrderActionTypes, data: WorkOrderPriceModel }) => x.type === WorkOrderActionTypes.ADD_PRICE_ITEM || x.type === WorkOrderActionTypes.UPDATE_PRICE_ITEM),
                tap((x: { type: WorkOrderActionTypes, data: WorkOrderPriceModel }) => { this.actionDispatchType = x.type; }),
                map(d => d.data),
                withLatestFrom(this._store.select(WorkOrderListPricestate)),
                takeUntil(this.ngUnsubscribe)
            ).subscribe(([priceUpdate, currentPrices]: [WorkOrderPriceModel, WorkOrderPriceModel[]]) => {
                switch (this.actionDispatchType) {
                    case WorkOrderActionTypes.ADD_PRICE_ITEM:
                        if (currentPrices.length === 0) {
                            this._store.dispatch(AddPriceItemWorkOrderSuccess({ data: priceUpdate }));
                            this.popupClosing$.next(true);
                            return;
                        }

                        // * check exist quantity type
                        const isValidPriceAdd = this.checkValidatePriceItem(priceUpdate, currentPrices);
                        if (!isValidPriceAdd) return;

                        this._store.dispatch(AddPriceItemWorkOrderSuccess({ data: priceUpdate }));
                        this.popupClosing$.next(true);
                        break;
                    case WorkOrderActionTypes.UPDATE_PRICE_ITEM:
                        if (currentPrices.length === 1) {
                            this._store.dispatch(UpdatePriceItemWorkOrderSuccess({ data: priceUpdate }));
                            this.popupClosing$.next(true);
                            return;
                        }
                        // * check exist quantity type
                        const isValidPriceUpdate = this.checkValidatePriceItem(priceUpdate, currentPrices, 'UPDATE');
                        if (!isValidPriceUpdate) return;

                        this._store.dispatch(UpdatePriceItemWorkOrderSuccess({ data: priceUpdate }));
                        this.popupClosing$.next(true);
                        break;
                    default:
                        break;
                }
            });

        // * listen event to close popup
        this.popupClosing$.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((isClose: boolean) => {
                if (!!isClose) { this.hide(); }
            });
    }

    checkValidatePriceItem(priceUpdate: WorkOrderPriceModel, currentPrices: WorkOrderPriceModel[], action: string = 'ADD') {
        const currentPriceType = currentPrices.map((x: WorkOrderPriceModel) => x.quantityType);

        if (!currentPriceType.includes(priceUpdate.quantityType)) {
            this._toastService.warning('Quantity type must be the same type as ' + currentPriceType[0]);
            return false;
        }

        let duplicatePriceItem: WorkOrderPriceModel | undefined;
        let duplicatePriceItems: WorkOrderPriceModel[] = [];

        if (priceUpdate.quantityType === QUANTITYPE.RANGE) {
            duplicatePriceItems = this.getDuplicateRangeItems(priceUpdate, currentPrices, action);
            duplicatePriceItem = action === 'ADD' ? duplicatePriceItems[0] : duplicatePriceItems.find(x => x.id !== priceUpdate.id);

            if (duplicatePriceItem) {
                this._toastService.warning(`Quantity value's ${priceUpdate.partnerName} has overlapped`);
                return false;
            }
        } else {
            if (priceUpdate.type === PRICETYPE.AIR) {
                duplicatePriceItems = this.getDuplicateAirItems(priceUpdate, currentPrices, action);
                duplicatePriceItem = action === 'ADD' ? duplicatePriceItems[0] : duplicatePriceItems.find(x => x.id !== priceUpdate.id);

                if (duplicatePriceItem) {
                    this._toastService.warning(`Fee type's ${priceUpdate.partnerName} ${priceUpdate.type} is already existed`);
                    return false;
                }
            } else {
                duplicatePriceItems = this.getDuplicateStandardItems(priceUpdate, currentPrices, action);
                duplicatePriceItem = action === 'ADD' ? duplicatePriceItems[0] : duplicatePriceItems.find(x => x.id !== priceUpdate.id);

                if (duplicatePriceItem) {
                    this._toastService.warning(`Fee type's ${priceUpdate.partnerName} ${priceUpdate.type}-${priceUpdate.unitCode} is already existed`);
                    return false;
                }
            }
        }

        return true;
    }

    private getDuplicateRangeItems(priceUpdate: WorkOrderPriceModel, currentPrices: WorkOrderPriceModel[], action: string): WorkOrderPriceModel[] {
        return currentPrices.filter((x: WorkOrderPriceModel) =>
            x.quantityType === QUANTITYPE.RANGE
            && x.partnerId === priceUpdate.partnerId
            && (action === 'ADD' ? x.id === x.id : x.id !== priceUpdate.id)
            && this.utility.checkRangeOverlap([x.quantityFromValue, x.quantityToValue], [priceUpdate.quantityFromValue, priceUpdate.quantityToValue])
        );
    }

    private getDuplicateAirItems(priceUpdate: WorkOrderPriceModel, currentPrices: WorkOrderPriceModel[], action: string): WorkOrderPriceModel[] {
        return currentPrices.filter((x: WorkOrderPriceModel) =>
            x.partnerId === priceUpdate.partnerId
            && x.type === PRICETYPE.AIR
            && (action === 'ADD' ? x.id === x.id : x.id !== priceUpdate.id)
        );
    }

    private getDuplicateStandardItems(priceUpdate: WorkOrderPriceModel, currentPrices: WorkOrderPriceModel[], action: string): WorkOrderPriceModel[] {
        return currentPrices.filter((x: WorkOrderPriceModel) =>
            x.partnerId === priceUpdate.partnerId
            && x.unitId === priceUpdate.unitId
            && x.type === priceUpdate.type
            && (action === 'ADD' ? x.id === x.id : x.id !== priceUpdate.id))
    }

    loadPartner() {
        if (!!this.partners?.length) return;
        this.isLoading = true;
        this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CARRIER)
            .pipe(finalize(() => { this.isLoading = false }))
            .subscribe(
                (partners) => {
                    this.partners = partners || [];
                    this._cd.markForCheck();
                }
            )
    }

    loadUnit() {
        this._store.dispatch(new GetCatalogueUnitAction());
        this._store.select(getCatalogueUnitState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (units: Unit[]) => {
                    this.units = units || [];
                    if (!this.units.length) return;

                    const AIR_OTHER_TYPE = [PRICETYPE.AIR, PRICETYPE.OTHER];
                    const FCL_LCL_TYPE = [PRICETYPE.FCL, PRICETYPE.LCL];

                    if (!!this.type.value) {
                        if (AIR_OTHER_TYPE.includes(this.type.value)) {
                            this.units = units.filter(x => x.unitType !== CommonEnum.UnitType.CONTAINER);
                        }
                        if (FCL_LCL_TYPE.includes(this.type.value)) {
                            this.units = units.filter(x => x.unitType === CommonEnum.UnitType.CONTAINER);
                        }
                    }
                    console.log(this.units);
                    this._cd.markForCheck();
                }
            )
    }

    initFreightCharge(transactionType: string) {
        this.selectedPartnerName = null;
        this.frieghtCharges.length = 0;
        this.frieghtCharges.push({
            chargeName: `${this.getFreightChargeName(transactionType)}`,
            chargeCodeBuying: this.freightChargeMappingDefaults.get(transactionType)[0],
            chargeCodeSelling: this.freightChargeMappingDefaults.get(transactionType)[1],
            unitPriceSelling: 100,
            unitPriceBuying: 100,
            vatSelling: 10,
            vatBuying: 10,
            currencyIdBuying: this.transactionType.includes('A') ? 'USD' : 'VND',
            currencyIdSelling: this.transactionType.includes('A') ? 'USD' : 'VND',
            notes: null,
            chargeIdbuying: null,
            chargeIdSelling: null,
        })
    }

    private getFreightChargeName(transactionType: string) {
        switch (transactionType) {
            case 'AE':
            case 'AI':
                return 'Air Freight'
            case 'CL':
            case 'TK':
                return 'Custom Fee'
            default:
                return 'Ocean Freight'
        }
    }

    initForm() {
        this.form = this._fb.group({
            partnerId: [null, Validators.required],
            unitId: [null, Validators.required],
            quantityType: [null, Validators.required],
            fromValue: [null, Validators.compose([
                Validators.required,
                Validators.min(0),
                Validators.max(999999)
            ])],
            toValue: [null, Validators.compose([
                Validators.required,
                Validators.min(0),
                Validators.max(999999)
            ])],
            type: [null, Validators.required]
        });

        this.partnerId = this.form.controls['partnerId'];
        this.unitId = this.form.controls['unitId'];
        this.quantityType = this.form.controls['quantityType'];
        this.fromValue = this.form.controls['fromValue'];
        this.toValue = this.form.controls['toValue'];
        this.type = this.form.controls['type'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        console.log(data);
        this.isSubmitted = false;
        switch (type) {
            case 'partner':
                const partner: Partner = data as Partner;
                this.partnerId.setValue(partner.id);
                this.selectedPartnerName = partner.shortName;

                this._store.dispatch(SelectPartnerPriceItemWorkOrder({ data: partner }));
                break;
            case 'unit':
                this.selectedUnitCode = data.code;
                break;
            case 'type':
                this.selectedUnitCode = null;
                this.unitId.reset();
                break;
            case 'quantityType':
                if (
                    [this.quantityTypes[0].value, this.quantityTypes[2].value].includes(data.value)) {
                    this.quantityType.setValue(data.value);
                    this.fromValue.setValue(1);
                    this.toValue.setValue(1);

                    this.fromValue.disable();
                    this.toValue.disable();
                    this.fromValue.updateValueAndValidity();
                    this.toValue.updateValueAndValidity();
                } else {
                    this.fromValue.enable();
                    this.fromValue.reset();
                    this.toValue.reset();
                    this.toValue.enable();
                }
                break;
            default:
                break;
        }
    }

    onSubmit() {
        this.isSubmitted = true;
        let valid = true;
        let errCode = 1;
        if (!this.form.valid || (this.fromValue?.value > this.toValue?.value) || (this.toValue?.value < this.fromValue?.value)) {
            valid = false;
        }
        if (this.frieghtCharges.some(x => !x.unitPriceBuying || !x.unitPriceSelling)) {
            valid = false;
        };

        if (!!this.buyings.length) {
            if (this.buyings.some(x => !x.chargeId || !x.unitPrice))  {
                valid = false;
            }
            if((this.buyings.filter(x => x.isPrimary).length > 1)) {
                valid = false;
                errCode = 2;
            }
        }

        if (!!this.sellings.length) {
            if (this.sellings.some(x => !x.chargeId || !x.unitPrice)) {
                valid = false;
            }
            if((this.sellings.filter(x => x.isPrimary).length > 1)) {
                valid = false;
                errCode = 2;
            }
        }

        if (!valid) {
            if(errCode === 1) {
                this._toastService.warning("It looks like you missed something, Please recheck the highlighted field below.", "Opps");
            } else {
                this._toastService.warning("Please select only one primary surcharge", "Opps");
            }
            return;
        }

        const formValue = this.form.getRawValue();

        const freightCharge = this.frieghtCharges[0];
        const priceItemWorkOrder: WorkOrderPriceModel = {
            partnerId: formValue.partnerId,
            partnerName: this.selectedPartnerName,
            unitId: formValue.unitId,
            quantityType: formValue.quantityType,
            // quantityFromRange: formValue.quantity?.from,
            // quantityToRange: formValue.quantity?.to,
            surcharges: [...this.buyings, ...this.sellings],
            chargeIdBuying: freightCharge.chargeIdbuying,
            chargeIdSelling: freightCharge.chargeIdSelling,
            chargeCodeBuying: freightCharge.chargeCodeBuying,
            chargeCodeSelling: freightCharge.chargeCodeSelling,
            mode: null,
            unitCode: this.selectedUnitCode,
            notes: freightCharge.notes,
            currencyIdBuying: freightCharge.currencyIdBuying,
            currencyIdSelling: freightCharge.currencyIdSelling,
            quantityFromValue: formValue.fromValue,
            quantityToValue: formValue.toValue,
            id: null,
            unitPriceBuying: freightCharge.unitPriceBuying,
            unitPriceSelling: freightCharge.unitPriceSelling,
            transactionType: null,
            vatrateBuying: freightCharge.vatBuying,
            vatrateSelling: freightCharge.vatSelling,
            workOrderId: null,
            type: formValue.type,
        }
        // this.hide();
        if (this.ACTION === 'CREATE') {
            priceItemWorkOrder.id = SystemConstants.EMPTY_GUID;
            priceItemWorkOrder.workOrderId = SystemConstants.EMPTY_GUID;

            this._store.dispatch(AddPriceItemWorkOrder({ data: priceItemWorkOrder }));
            return;
        }
        priceItemWorkOrder.id = this.id;
        priceItemWorkOrder.workOrderId = this.workOrderId;
        this._store.dispatch(UpdatePriceItemWorkOrder({ data: priceItemWorkOrder }));
    }

    compareNgSelectQuantityRange(item: CommonInterface.ICommonTitleValue, selectedItem: any): boolean {
        return item.value === selectedItem.value && item.value === selectedItem.value;
    }

    hidePriceItemPopup() {
        this.onHidePriceItemPopup();
    }

    onHidePriceItemPopup() {
        this.hide();
        this._store.dispatch(ResetUpdatePriceItemWorkOrder());
    }
}

interface IFreightCharge {
    chargeName?: string;
    chargeIdbuying: string;
    chargeIdSelling: string;
    chargeCodeBuying: string;
    chargeCodeSelling: string;
    unitPriceSelling: number,
    unitPriceBuying: number,
    vatSelling: number,
    vatBuying: number,
    currencyIdSelling: string;
    currencyIdBuying: string;
    notes: string;
}
