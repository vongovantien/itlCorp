import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';

@Component({
    selector: 'form-add-company',
    templateUrl: './form-add-company.component.html',
    styleUrls: ['./../../company-information.component.scss']
})
export class CompanyInformationFormAddComponent extends AppForm {

    formGroup: FormGroup;
    code: AbstractControl;
    bunameVn: AbstractControl;
    bunameEn: AbstractControl;
    bunameAbbr: AbstractControl;
    website: AbstractControl;
    inactive: AbstractControl;

    types: CommonInterface.ICommonTitleValue[] = [
        { title: 'Active', value: false },
        { title: 'Inactive', value: true },
    ];
    photoUrl: string = '';

    constructor(
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit(): void {
        this.initForm();
    }

    initForm() {
        this.formGroup = this._fb.group({
            code: [],
            bunameVn: [],
            bunameEn: [],
            bunameAbbr: [],
            website: [],
            inactive: [this.types[0]],
        });

        this.code = this.formGroup.controls['code'];
        this.bunameVn = this.formGroup.controls['bunameVn'];
        this.bunameEn = this.formGroup.controls['bunameEn'];
        this.bunameAbbr = this.formGroup.controls['bunameAbbr'];
        this.website = this.formGroup.controls['website'];
        this.inactive = this.formGroup.controls['inactive'];
    }
}

export interface IFormAddCompany {
    code: string;
    bunameVn: string;
    bunameEn: string;
    bunameAbbr: string;
    website: string;
    inactive: boolean;
}
