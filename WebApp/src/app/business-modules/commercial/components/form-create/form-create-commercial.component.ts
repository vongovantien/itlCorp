import { Component, OnInit, Input } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';

import { AppForm } from 'src/app/app.form';
import { CatalogueRepo } from '@repositories';
import { CountryModel, ProviceModel, Partner, Customer } from '@models';
import { JobConstants } from '@constants';
import { CommonEnum } from '@enums';

import { Observable } from 'rxjs';
import { shareReplay, catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'form-create-commercial',
    templateUrl: './form-create-commercial.component.html',
})
export class CommercialFormCreateComponent extends AppForm implements OnInit {

    formGroup: FormGroup;
    partnerNameEn: AbstractControl;
    shortName: AbstractControl;
    partnerNameVn: AbstractControl;
    internalReferenceNo: AbstractControl;
    taxCode: AbstractControl;
    inter: AbstractControl;
    countryShippingId: AbstractControl;
    countryId: AbstractControl; // * Billing country
    countryName: AbstractControl;
    provinceShippingId: AbstractControl;
    provinceId: AbstractControl; // * Billing Province
    parentId: AbstractControl; // * A/C
    countryShippingName: AbstractControl;
    provinceShippingName: AbstractControl;
    provinceName: AbstractControl;
    addressShippingEn: AbstractControl;
    addressShippingVn: AbstractControl;
    addressEn: AbstractControl;
    addressVn: AbstractControl;

    countries: Observable<CountryModel[]>;
    cities: Observable<ProviceModel[]>;
    acRefCustomers: Observable<Partner[]>;

    shippingProvinces: ProviceModel[];
    initShippingProvinces: ProviceModel[];
    billingProvinces: ProviceModel[];
    initbillingProvinces: ProviceModel[];
    billingsProvinces: ProviceModel[] = [];
    shipingsProvinces: ProviceModel[] = [];

    displayFieldCountry: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_COUNTRY;
    displayFieldCity: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_CITY_PROVINCE;
    displayFieldCustomer: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    isExistedTaxcode: boolean = false;
    @Input() isUpdate: boolean = false;


    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit(): void {
        this.countries = this._catalogueRepo.getCountry().pipe(shareReplay());

        this.acRefCustomers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);
        this.getProvinces();
        if (this.isUpdate) {
            this.getShippingProvinces();
            this.getBillingProvinces();
        }

        this.initForm();
    }

    getProvinces() {
        this._catalogueRepo.getProvinces()
            .subscribe(
                (provinces: ProviceModel[]) => {
                    this.billingProvinces = this.shippingProvinces = this.initShippingProvinces = this.initbillingProvinces = provinces;
                }
            );
    }

    initForm() {
        this.formGroup = this._fb.group({
            accountNo: [{ value: null, disabled: true }],
            partnerNameEn: [null, Validators.required],
            partnerNameVn: [null, Validators.required],
            shortName: [null, Validators.required],
            taxCode: [null, Validators.compose([
                Validators.maxLength(14),
                Validators.minLength(8),
                Validators.required
            ])],
            internalReferenceNo: [null, Validators.compose([
                Validators.maxLength(10),
                Validators.minLength(3)
            ])],
            addressShippingEn: [null, Validators.required],
            addressShippingVn: [null, Validators.required],
            addressVn: [null, Validators.required],
            addressEn: [null, Validators.required],
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

            countryShippingId: [null, Validators.required],
            provinceShippingId: [],
            countryId: [null, Validators.required],
            provinceId: [],
            parentId: [],
        });

        this.partnerNameEn = this.formGroup.controls["partnerNameEn"];
        this.partnerNameVn = this.formGroup.controls["partnerNameVn"];
        this.shortName = this.formGroup.controls["shortName"];
        this.taxCode = this.formGroup.controls["taxCode"];
        this.internalReferenceNo = this.formGroup.controls["internalReferenceNo"];
        this.countryShippingId = this.formGroup.controls["countryShippingId"];
        this.provinceShippingId = this.formGroup.controls["provinceShippingId"];
        this.countryId = this.formGroup.controls["countryId"];
        this.countryName = this.formGroup.controls["countryName"];
        this.countryShippingName = this.formGroup.controls["countryShippingName"];
        this.provinceShippingName = this.formGroup.controls["provinceShippingName"];
        this.provinceName = this.formGroup.controls["provinceName"];
        this.provinceId = this.formGroup.controls["provinceId"];
        this.parentId = this.formGroup.controls["parentId"];
        this.addressShippingEn = this.formGroup.controls["addressShippingEn"];
        this.addressShippingVn = this.formGroup.controls["addressShippingVn"];
        this.addressEn = this.formGroup.controls["addressEn"];
        this.addressVn = this.formGroup.controls["addressVn"];
    }

    onSelectDataFormInfo(data: any, type: string) {
        console.log(data);
        switch (type) {
            case 'acRef':
                this.parentId.setValue(data.id);
                break;
            case 'shippping-country':
                this.countryShippingId.setValue(data.id);

                this.getShippingProvinces(data.id, !!this.provinceShippingId.value ? this.provinceShippingId.value : null);
                break;
            case 'shippping-city':
                this.provinceShippingId.setValue(data.id);
                break;
            case 'billing-country':
                this.countryId.setValue(data.id);

                // this.getBillingProvince(data.id);
                this.getBillingProvinces(data.id, !!this.provinceId.value ? this.provinceId.value : null);
                break;
            case 'billing-city':
                this.provinceId.setValue(data.id);

                break;
            default:
                break;
        }
    }

    getBillingProvince(countryId: number) {
        this.provinceId.setValue(null);
        this.provinceName.setValue(null);

        this.billingProvinces = [...this.initbillingProvinces.filter(x => x.countryID === countryId)];
        if (this.billingProvinces.length === 1) {
            // this.provinceId.setValue(this.billingProvinces[0].id);
        } else {
            this.provinceId.setValue(null);
        }
    }

    copyInfoShipping(event: Event) {
        event.preventDefault();
        event.stopImmediatePropagation();
        event.stopPropagation();

        this.countryId.setValue(this.countryShippingId.value);
        this.getBillingProvince(this.countryShippingId.value);

        this.provinceId.setValue(this.provinceShippingId.value);

        this.formGroup.controls['zipCode'].setValue(this.formGroup.controls['zipCodeShipping'].value);
        this.formGroup.controls['addressVn'].setValue(this.formGroup.controls['addressShippingVn'].value);
        this.formGroup.controls['addressEn'].setValue(this.formGroup.controls['addressShippingEn'].value);

    }

    getShippingProvinces(countryId?: number, provinceId: string = null) {
        if (countryId) {
            this._catalogueRepo.getProvincesBycountry(countryId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.shipingsProvinces = res;
                        console.log(this.shipingsProvinces);
                        if (!!provinceId) {
                            const obj = this.shipingsProvinces.find(x => x.id === provinceId);
                            if (obj === undefined) {
                                this.provinceShippingId.setValue(null);
                            }
                        }
                    }
                );
        } else {
            this._catalogueRepo.getProvinces()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.shipingsProvinces = res;
                    }
                );
        }
    }

    getBillingProvinces(countryId?: number, provinceId: string = null) {
        if (countryId) {
            this._catalogueRepo.getProvincesBycountry(countryId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.billingsProvinces = res;
                        console.log(this.billingsProvinces);
                        if (!!provinceId) {
                            const obj = this.billingsProvinces.find(x => x.id === provinceId);
                            if (obj === undefined) {
                                this.provinceId.setValue(null);
                            }
                        }

                    }
                );
        } else {
            this._catalogueRepo.getProvinces()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.billingsProvinces = res;
                    }
                );
        }
    }
}
