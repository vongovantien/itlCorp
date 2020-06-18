import { Component, OnInit } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';

import { AppForm } from 'src/app/app.form';
import { CatalogueRepo } from '@repositories';
import { CountryModel, ProviceModel, Partner } from '@models';
import { JobConstants } from '@constants';

import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { CommonEnum } from '@enums';

@Component({
    selector: 'form-create-commercial',
    templateUrl: './form-create-commercial.component.html',
})
export class CommercialFormCreateComponent extends AppForm implements OnInit {

    formGroup: FormGroup;
    shippingCountry: AbstractControl;
    countryId: AbstractControl; // * Billing country
    countryName: AbstractControl;
    provinceShippingId: AbstractControl;
    provinceId: AbstractControl; // * Billing Province
    parentId: AbstractControl; // * A/C
    countryShippingName: AbstractControl;
    provinceShippingName: AbstractControl;
    provinceName: AbstractControl;

    countries: Observable<CountryModel[]>;
    cities: Observable<ProviceModel[]>;
    acRefCustomers: Observable<Partner[]>;


    displayFieldCountry: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_COUNTRY;
    displayFieldCity: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_CITY_PROVINCE;
    displayFieldCustomer: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit(): void {
        this.countries = this._catalogueRepo.getCountry().pipe(shareReplay());
        this.cities = this._catalogueRepo.getProvinces().pipe(shareReplay());
        this.acRefCustomers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);

        this.initForm();
    }

    initForm() {
        this.formGroup = this._fb.group({
            accountNo: [{ value: null, disabled: true }],
            partnerNameEn: [],
            partnerNameVn: [],
            shortName: [],
            taxCode: [],
            internalReferenceNo: [],
            addressShippingEn: [],
            addressShippingVn: [],
            addressVn: [],
            addressEn: [],
            zipCode: [],
            zipCodeShipping: [],
            contactPerson: [],
            tel: [],
            fax: [],
            workPhoneEx: [],
            email: [],
            billingEmail: [],
            billingPhone: [],
            countryName: [],
            countryShippingName: [],
            provinceShippingName: [],
            provinceName: [],

            shippingCountry: [],
            provinceShippingId: [],
            countryId: [],
            provinceId: [],
            parentId: [],
        });

        this.shippingCountry = this.formGroup.controls["shippingCountry"];
        this.provinceShippingId = this.formGroup.controls["provinceShippingId"];
        this.countryId = this.formGroup.controls["countryId"];
        this.countryName = this.formGroup.controls["countryName"];
        this.countryShippingName = this.formGroup.controls["countryShippingName"];
        this.provinceShippingName = this.formGroup.controls["provinceShippingName"];
        this.provinceName = this.formGroup.controls["provinceName"];

        this.provinceId = this.formGroup.controls["provinceId"];
        this.parentId = this.formGroup.controls["parentId"];
    }



    onSelectDataFormInfo(data: any, type: string) {

    }

}
