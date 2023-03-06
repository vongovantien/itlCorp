import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Params } from '@angular/router';
import { PopupBase } from '@app';
import { ChargeConstants, JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, Unit } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
    AddPriceItemWorkOrderSuccess,
    IWorkOrderMngtState,
    UpdatePriceItemWorkOrderSuccess
}
    from '../../../store';
import { ISurchargeWorkOrder } from '../../surcharge-list/surcharge-list-work-order.component';

@Component({
    selector: 'price-item-work-order-popup',
    templateUrl: './price-item-work-order.component.html',
})
export class CommercialPriceItemWorkOrderPopupComponent extends PopupBase implements OnInit {

    @Output() onAdd: EventEmitter<IPriceWorkOrder> = new EventEmitter<IPriceWorkOrder>();
    @Output() onUpdate: EventEmitter<IPriceWorkOrder> = new EventEmitter<IPriceWorkOrder>();

    transactionType: string;

    partners: Observable<Partner[]>;
    selectedPartnerName: string;

    units: Observable<Unit[]>;
    selectedUnitCode: string;

    quantityRanges: CommonInterface.ICommonTitleValue[] = [
        {
            title: '< 20', value: { from: null, to: 19 }
        },
        {
            title: '20 - 40', value: { from: 20, to: 40 }
        },
        {
            title: '> 40', value: { from: 41, to: null }
        }
    ];

    form: FormGroup;
    partnerId: AbstractControl;
    unitId: AbstractControl;
    fromValue: AbstractControl;
    toValue: AbstractControl;
    quantityRange: AbstractControl;

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
    buyings: any[] = [];
    sellings: any[] = [];

    ACTION: CommonType.ACTION_FORM;

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
        this.units = this._catalogueRepo.getUnit({ active: true });

        this._activedRouter.params
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (params: Params) => {
                    this.transactionType = params?.transactionType;
                }
            )
    }

    initFreightCharge(transactionType: string) {
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
        });

        this.partnerId = this.form.controls['partnerId'];
        this.unitId = this.form.controls['unitId'];
        this.quantityRange = this.form.controls['quantityRange'];
        this.fromValue = this.form.controls['fromValue'];[]
        this.toValue = this.form.controls['toValue'];
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
        const priceItemWorkOrder: IPriceWorkOrder = {
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
        }
        console.log(priceItemWorkOrder);
        this.hide();
        if (this.ACTION === 'CREATE') {
            // this.onAdd.emit(priceItemWorkOrder);
            priceItemWorkOrder.id = SystemConstants.EMPTY_GUID;
            this._store.dispatch(AddPriceItemWorkOrderSuccess({ data: priceItemWorkOrder }));
            return;
        }
        this._store.dispatch(UpdatePriceItemWorkOrderSuccess({ data: priceItemWorkOrder }));


        // this.onUpdate.emit(priceItemWorkOrder);
    }
}

export interface IPriceWorkOrder {
    id?: string;
    partnerId: string;
    unitId: string;
    unitCode: string;
    partnerName?: string;
    quantityFromValue?: number;
    quantityToValue?: number;
    quantityFromRange: number;
    quantityToRange: number;
    mode: string;
    chargeIdBuying: string;
    chargeIdSelling: string;
    chargeCodeBuying: string;
    chargeCodeSelling: string;
    notes: string;
    currencyIdBuying: string;
    currencyIdSelling: string;
    unitPriceBuying: number;
    unitPriceSelling: number;
    surcharges: ISurchargeWorkOrder[];
    // freightCharges: IFreightCharge[];

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
