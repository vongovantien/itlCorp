import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { ChargeConstants } from 'src/constants/charge.const';
import { CatChargeToAddOrUpdate } from 'src/app/shared/models/catalogue/catChargeToAddOrUpdate.model';
import { forkJoin } from 'rxjs';

@Component({
    selector: 'form-add-charge',
    templateUrl: 'form-add-charge.component.html'
})

export class FormAddChargeComponent extends AppForm {
    formGroup: FormGroup;
    isSubmitted: boolean = false;
    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;
    unit: AbstractControl;
    unitPrice: AbstractControl;
    currency: AbstractControl;
    vat: AbstractControl;
    type: AbstractControl;
    service: AbstractControl;
    ngDataUnit: any = [];
    ngDataCurrentcyUnit: any = [];
    activeServices: any = [];
    Charge: CatChargeToAddOrUpdate = null;
    serviceTypeId: string = '';
    requiredService: boolean = false;

    ngDataType = [
        { id: "CREDIT", text: "CREDIT" },
        { id: "DEBIT", text: "DEBIT" },
        { id: "OBH", text: "OBH" }
    ];
    ngDataService = [
        { text: ChargeConstants.IT_DES, id: ChargeConstants.IT_CODE },
        { text: ChargeConstants.AI_DES, id: ChargeConstants.AI_CODE },
        { text: ChargeConstants.AE_DES, id: ChargeConstants.AE_CODE },
        { text: ChargeConstants.SFE_DES, id: ChargeConstants.SFE_CODE },
        { text: ChargeConstants.SFI_DES, id: ChargeConstants.SFI_CODE },
        { text: ChargeConstants.SLE_DES, id: ChargeConstants.SLE_CODE },
        { text: ChargeConstants.SLI_DES, id: ChargeConstants.SLI_CODE },
        { text: ChargeConstants.SCE_DES, id: ChargeConstants.SCE_CODE },
        { text: ChargeConstants.SCI_DES, id: ChargeConstants.SCI_CODE },
        { text: ChargeConstants.CL_DES, id: ChargeConstants.CL_CODE }
    ];

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }

    ngOnInit() {
        this.getUnit();
        this.getCurrency();
        this.initForm();
    }

    initForm() {
        this.formGroup = this._fb.group({
            code: [null, Validators.required],
            nameEn: [null, Validators.required],
            nameVn: [null, Validators.required],
            unit: [null, Validators.required],
            unitPrice: [null, Validators.required],
            currency: [null, Validators.required],
            vat: [null, Validators.required],
            type: [null, Validators.required],
            service: [null, Validators.required]
        });
        this.code = this.formGroup.controls["code"];
        this.nameEn = this.formGroup.controls["nameEn"];
        this.nameVn = this.formGroup.controls["nameVn"];
        this.unit = this.formGroup.controls["unit"];
        this.unitPrice = this.formGroup.controls["unitPrice"];
        this.currency = this.formGroup.controls["currency"];
        this.vat = this.formGroup.controls["vat"];
        this.type = this.formGroup.controls["type"];
        this.service = this.formGroup.controls["service"];
    }


    checkValidateForm() {
        let valid: boolean = true;
        this.setError(this.service);
        if (!this.formGroup.valid) {
            valid = false;
        }

        return valid;
    }


    addCharge() {
        this.isSubmitted = true;
    }

    getUnit() {
        this._catalogueRepo.getUnit().subscribe((res: any) => {
            if (!!res) {
                const units = res;
                this.ngDataUnit = units.map(x => ({ text: x.code, id: x.id }));
            }

        });
    }

    getCurrency() {
        this._catalogueRepo.getCurrency().subscribe((res: any) => {
            if (!!res) {
                const currencies = res;
                this.ngDataCurrentcyUnit = currencies.map(x => ({ text: x.id + " - " + x.currencyName, id: x.id }));
            }
        });
    }

    getCurrentActiveService(ChargeService: any) {
        const listService = ChargeService.split(";");
        const activeServiceList: any = [];
        listService.forEach(item => {
            const element = this.ngDataService.find(x => x.id === item);
            if (element !== undefined) {
                const activeService = element;
                activeServiceList.push(activeService);
            }
        });
        return activeServiceList;
    }

    selected(value: any) {
        if (value !== undefined) {
            this.requiredService = false;
        }


    }

    updateDataToForm(res: CatChargeToAddOrUpdate) {
        this.serviceTypeId = res.charge.serviceTypeId;
        this.formGroup.patchValue({
            code: res.charge.code,
            nameEn: res.charge.chargeNameEn,
            nameVn: res.charge.chargeNameVn,

            unitPrice: res.charge.unitPrice,
            currency: [<CommonInterface.INg2Select>{ id: res.charge.currencyId, text: res.charge.currencyId }],
            vat: res.charge.vatrate,
            type: [<CommonInterface.INg2Select>{ id: res.charge.type, text: res.charge.type }],
            service: [<CommonInterface.INg2Select>{ id: res.charge.serviceTypeId, text: '' }]
        });
        const itemUnit = this.ngDataUnit.find(x => x.id === res.charge.unitId);
        this.unit.setValue([<CommonInterface.INg2Select>{ id: res.charge.unitId, text: itemUnit.text }]);
        const itemCurrency = this.ngDataCurrentcyUnit.find(x => x.id === res.charge.currencyId);
        this.currency.setValue([<CommonInterface.INg2Select>{ id: res.charge.currencyId, text: itemCurrency.text }]);




    }

}