import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { PopupBase } from '@app';
import { ChargeConstants, JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, Unit, WorkOrderPriceModel, WorkOrderSurchargeModel } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo } from '@repositories';
import { GetCatalogueUnitAction, getCatalogueUnitState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { Observable, merge } from 'rxjs';
import { filter, pluck, takeUntil } from 'rxjs/operators';
import {
    AddPriceItemWorkOrder,
    IWorkOrderMngtState,
    ResetUpdatePriceItemWorkOrder,
    UpdatePriceItemWorkOrder,
    workOrderDetailIsReadOnlyState,
    workOrderDetailTransationTypeState,
    WorkOrderPriceItemUpdateModeState
}
    from '../../../store';

@Component({
    selector: 'price-item-work-order-popup',
    templateUrl: './price-item-work-order.component.html',
})
export class CommercialPriceItemWorkOrderPopupComponent extends PopupBase implements OnInit {
    transactionType: string;

    partners: Observable<Partner[]>;
    selectedPartnerName: string;

    units: Observable<Unit[]>;
    selectedUnitCode: string;
    types: CommonInterface.ICommonTitleValue[] = [
        { title: 'AIR', value: 'AIR' },
        { title: 'FCL', value: 'FCL' },
        { title: 'LCL', value: 'LCL' },
        { title: 'Other', value: 'OTHER' }
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

    form: FormGroup;
    partnerId: AbstractControl;
    unitId: AbstractControl;
    fromValue: AbstractControl;
    toValue: AbstractControl;
    quantityRange: AbstractControl;
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

    id: string;
    workOrderId: string;
    title = 'New Price List';
    constructor(
        private readonly _fb: FormBuilder,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _toastService: ToastrService,
        private readonly _activedRouter: ActivatedRoute,
        private readonly _store: Store<IWorkOrderMngtState>

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

        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CARRIER);
        this._store.dispatch(new GetCatalogueUnitAction());
        this.units = this._store.select(getCatalogueUnitState);

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
            (transactionType: string) => {
                this.transactionType = transactionType;
            }
        )

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

                        this.form.patchValue({
                            partnerId: res.partnerId,
                            unitId: res.unitId,
                            quantityRange: { from: res.quantityFromRange, to: res.quantityToRange },
                            type: res.type
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

        this.isReadonly = this._store.select(workOrderDetailIsReadOnlyState);


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

    getFreightChargeName(transactionType: string) {
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
            quantityRange: [null, Validators.required],
            fromValue: [{ value: null, disabled: true }],
            toValue: [{ value: null, disabled: true }],
            type: [null, Validators.required]
        });

        this.partnerId = this.form.controls['partnerId'];
        this.unitId = this.form.controls['unitId'];
        this.quantityRange = this.form.controls['quantityRange'];
        this.fromValue = this.form.controls['fromValue'];[]
        this.toValue = this.form.controls['toValue'];
        this.type = this.form.controls['type'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                const partner: Partner = data as Partner;
                this.partnerId.setValue(partner.id);
                this.selectedPartnerName = partner.shortName;
                break;
            case 'unit':
                this.selectedUnitCode = data.code;
                break;
            default:
                break;
        }
    }

    onSubmit() {
        this.isSubmitted = true;
        if (!this.form.valid || this.frieghtCharges.some(x => !x.unitPriceBuying || !x.unitPriceSelling)) {
            this._toastService.warning("It looks like you missed something, Please recheck the highlighted field below.", "Opps");
            return;
        };

        if (!!this.buyings.length) {
            if (this.buyings.some(x => !x.chargeId || !x.unitPrice)) {
                this._toastService.warning("It looks like you missed something, Please recheck the highlighted field below.", "Opps");
                return;
            }
        }

        if (!!this.sellings.length) {
            if (this.sellings.some(x => !x.chargeId || !x.unitPrice)) {
                this._toastService.warning("It looks like you missed something, Please recheck the highlighted field below.", "Opps");
                return;
            }
        }
        const formValue = this.form.getRawValue();

        const freightCharge = this.frieghtCharges[0];
        const priceItemWorkOrder: WorkOrderPriceModel = {
            partnerId: formValue.partnerId,
            partnerName: this.selectedPartnerName,
            unitId: formValue.unitId,
            quantityFromRange: formValue.quantityRange?.from,
            quantityToRange: formValue.quantityRange?.to,
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
        this.hide();
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
        return item.value.from === selectedItem.from && item.value.to === selectedItem.to;
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
