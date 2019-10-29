import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';

@Component({
    selector: 'app-add-province',
    templateUrl: './add-province.component.html'
})
export class AddProvinceComponent extends PopupBase implements OnInit {
    formProvince: FormGroup;
    provinceCityToAdd = new CatPlaceModel();
    resetNg2SelectCountry = true;
    ngSelectDataCountries: any = [];
    isSubmitted = false;

    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;
    country: AbstractControl;

    constructor(private _fb: FormBuilder) {
        super();
    }

    ngOnInit() {
        this.initForm();
    }
    initForm() {
        this.formProvince = this._fb.group({
            code: ['', Validators.compose([
                Validators.required
            ])],
            nameEn: ['', Validators.compose([
                Validators.required
            ])],
            nameVn: ['', Validators.compose([
                Validators.required
            ])],
            country: ['', Validators.compose([
                Validators.required
            ])]
        });
        this.code = this.formProvince.controls['code'];
        this.nameEn = this.formProvince.controls['nameEn'];
        this.nameVn = this.formProvince.controls['nameVn'];
        this.country = this.formProvince.controls['country'];

        console.log(typeof this.code)
    }
    saveProvince() {
        this.isSubmitted = true;
        if (this.formProvince.valid) {
            const body: any = {
                code: this.code.value,
                nameEn: this.nameEn.value,
                nameVn: this.nameVn.value,
                countryId: this.country.value.id
            };
        }
    }
    cancelAdd() {
        this.hide();
        this.isSubmitted = false;
        this.formProvince.reset();
    }
    refreshValue(event) { }
    typed(event) { }
    removed(event) { }
    selectedCountry(event, s: string, k: string) { }
}
