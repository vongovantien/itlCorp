import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { AppForm } from 'src/app/app.form';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'form-add-partner',
    templateUrl: './form-add-partner.component.html'
})

export class FormAddPartnerComponent extends AppForm {
    parentCustomers: any[] = [];
    partnerGroups: any[] = [];
    countries: any[] = [];
    shippingProvinces: any[] = [];
    billingProvinces: any[] = [];
    workPlaces: any[] = [];
    isExistedTaxcode: boolean = false;
    isShowSaleMan: boolean = false;
    partnerGroupActives: any[] = [];

    partnerForm: FormGroup;
    isSubmitted: boolean = false;
    partnerAccountNo: AbstractControl;
    internalReferenceNo: AbstractControl;
    nameENFull: AbstractControl;
    nameLocalFull: AbstractControl;
    shortName: AbstractControl;
    partnerAccountRef: AbstractControl;
    taxCode: AbstractControl;
    partnerGroup: AbstractControl;
    shippingCountry: AbstractControl;
    shippingProvince: AbstractControl;
    zipCodeShipping: AbstractControl;
    shippingAddressEN: AbstractControl;
    shippingAddressVN: AbstractControl;
    billingCountry: AbstractControl;
    billingProvince: AbstractControl;
    billingZipcode: AbstractControl;
    billingAddressEN: AbstractControl;
    billingAddressLocal: AbstractControl;
    partnerContactPerson: AbstractControl;
    partnerContactNumber: AbstractControl;
    partnerContactFaxNo: AbstractControl;
    employeeWorkPhone: AbstractControl;
    employeeEmail: AbstractControl;
    partnerWebsite: AbstractControl;
    partnerbankAccountNo: AbstractControl;
    partnerBankAccountName: AbstractControl;
    partnerBankAccountAddress: AbstractControl;
    partnerSwiftCode: AbstractControl;
    partnerWorkPlace: AbstractControl;
    active: AbstractControl;
    public: AbstractControl;

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }
    ngOnInit() {
        this.initForm();
    }

    onChange(event: any, type: string) {
        switch (type) {
            case 'shippingCountry':
                this.getShippingProvinces(event.id);
                break;
            case 'billingCountry':
                this.getBillingProvinces(event.id);
                break;
        }
    }
    removed(type: string) {
        switch (type) {
            case 'shippingCountry':
                this.getShippingProvinces();
                this.partnerForm.controls['shippingProvince'].setValue([]);
                break;
            case 'billingCountry':
                this.getBillingProvinces();
                this.partnerForm.controls['billingProvince'].setValue([]);
                break;
        }
    }
    getBillingProvinces(countryId?: number) {
        if (countryId) {
            this._catalogueRepo.getProvincesBycountry(countryId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.billingProvinces = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
                    }
                );
        } else {
            this._catalogueRepo.getProvinces()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.billingProvinces = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
                    }
                );
        }
    }
    getShippingProvinces(countryId?: number) {
        if (countryId) {
            this._catalogueRepo.getProvincesBycountry(countryId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.shippingProvinces = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
                    }
                );
        } else {
            this._catalogueRepo.getProvinces()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.shippingProvinces = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
                    }
                );
        }
    }
    initForm() {
        this.partnerForm = this._fb.group({
            partnerAccountNo: ['', Validators.compose([
                Validators.required,
            ])],
            internalReferenceNo: ['', Validators.compose([
                Validators.required
            ])],
            nameENFull: ['', Validators.compose([
                Validators.required,
            ])],
            nameLocalFull: ['', Validators.compose([
                Validators.required,
            ])],
            shortName: ['', Validators.compose([
                Validators.required
            ])],
            partnerAccountRef: [null],
            taxCode: ['', Validators.compose([
                Validators.required
            ])],
            partnerGroup: ['', Validators.compose([
                Validators.required
            ])],
            shippingCountry: [null, Validators.compose([
                Validators.required
            ])],
            shippingProvince: ['', Validators.compose([
                Validators.required
            ])],
            zipCodeShipping: [''],
            shippingAddressEN: ['', Validators.compose([
                Validators.required
            ])],
            shippingAddressVN: ['', Validators.compose([
                Validators.required
            ])],
            billingCountry: ['', Validators.compose([
                Validators.required
            ])],
            billingProvince: ['', Validators.compose([
                Validators.required
            ])],
            billingZipcode: ['', Validators.compose([
                Validators.required
            ])],
            billingAddressEN: ['', Validators.compose([
                Validators.required
            ])],
            billingAddressLocal: ['', Validators.compose([
                Validators.required
            ])],
            partnerContactPerson: [''],
            partnerContactNumber: [''],
            partnerContactFaxNo: [''],
            employeeWorkPhone: [{ value: '', disabled: true }],
            employeeEmail: [{ value: '', disabled: true }],
            partnerWebsite: [''],
            partnerbankAccountNo: [''],
            partnerBankAccountName: [''],
            partnerBankAccountAddress: [''],
            partnerSwiftCode: [''],
            partnerWorkPlace: [''],
            public: [false],
            active: [true]
        });
        this.partnerAccountNo = this.partnerForm.controls['partnerAccountNo'];
        this.internalReferenceNo = this.partnerForm.controls['internalReferenceNo'];
        this.nameENFull = this.partnerForm.controls['nameENFull'];
        this.nameLocalFull = this.partnerForm.controls['nameLocalFull'];
        this.shortName = this.partnerForm.controls['shortName'];
        this.partnerAccountRef = this.partnerForm.controls['partnerAccountRef'];
        this.taxCode = this.partnerForm.controls['taxCode'];
        this.partnerGroup = this.partnerForm.controls['partnerGroup'];
        this.shippingCountry = this.partnerForm.controls['shippingCountry'];
        this.shippingProvince = this.partnerForm.controls['shippingProvince'];
        this.zipCodeShipping = this.partnerForm.controls['zipCodeShipping'];
        this.shippingAddressEN = this.partnerForm.controls['shippingAddressEN'];
        this.shippingAddressVN = this.partnerForm.controls['shippingAddressVN'];
        this.billingCountry = this.partnerForm.controls['billingCountry'];
        this.billingProvince = this.partnerForm.controls['billingProvince'];
        this.billingZipcode = this.partnerForm.controls['billingZipcode'];
        this.billingAddressEN = this.partnerForm.controls['billingAddressEN'];
        this.billingAddressLocal = this.partnerForm.controls['billingAddressLocal'];
        this.partnerContactPerson = this.partnerForm.controls['partnerContactPerson'];
        this.partnerContactNumber = this.partnerForm.controls['partnerContactNumber'];
        this.partnerContactFaxNo = this.partnerForm.controls['partnerContactFaxNo'];
        this.employeeWorkPhone = this.partnerForm.controls['employeeWorkPhone'];
        this.employeeEmail = this.partnerForm.controls['employeeEmail'];
        this.partnerWebsite = this.partnerForm.controls['partnerWebsite'];
        this.partnerbankAccountNo = this.partnerForm.controls['partnerbankAccountNo'];
        this.partnerBankAccountName = this.partnerForm.controls['partnerBankAccountName'];
        this.partnerBankAccountAddress = this.partnerForm.controls['partnerBankAccountAddress'];
        this.partnerSwiftCode = this.partnerForm.controls['partnerSwiftCode'];
        this.partnerWorkPlace = this.partnerForm.controls['partnerWorkPlace'];
        this.active = this.partnerForm.controls['active'];
        this.public = this.partnerForm.controls['public'];
    }
    showPopupSaleman() { }
    onSubmit() { }
}
