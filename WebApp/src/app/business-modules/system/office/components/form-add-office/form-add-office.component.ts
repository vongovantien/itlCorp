import { Component, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { SystemRepo } from 'src/app/shared/repositories';
import { Company } from 'src/app/shared/models';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
import { map } from 'rxjs/internal/operators/map';

@Component({
    selector: 'form-add-office',
    templateUrl: './form-add-office.component.html'
})
export class OfficeFormAddComponent extends AppForm {

    companies: Company[] = [];
    formGroup: FormGroup;
    code: AbstractControl;
    branchNameEn: AbstractControl;
    branchNameVn: AbstractControl;
    shortName: AbstractControl;
    addressEn: AbstractControl;
    addressVn: AbstractControl;
    taxcode: AbstractControl;
    tel: AbstractControl;
    email: AbstractControl;
    fax: AbstractControl;
    company: AbstractControl;
    bankAccountVND: AbstractControl;
    bankAccountName: AbstractControl;
    swiftCode: AbstractControl;
    bankAddress: AbstractControl;
    active: AbstractControl;
    bankName: AbstractControl;

    status: CommonInterface.ICommonTitleValue[] = [
        { title: 'Active', value: true },
        { title: 'Inactive', value: false },
    ];
    photoUrl: string = '';

    constructor(
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this.getDataComboBox();
        this.initForm();

    }

    getCompanies() {
        this._systemRepo.getListCompany()
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.companies = res;
                    console.log(this.companies);
                },
            );
    }
    getDataComboBox() {
        this.getCompanies();
    }

    initForm() {
        this.formGroup = this._fb.group({
            code: [],
            branchNameEn: [],
            branchNameVn: [],
            shortName: [],
            addressEn: [],
            addressVn: [],
            taxcode: [],
            tel: [],
            email: [],
            fax: [],
            company: [],
            bankAccountVND: [],
            bankAccountName: [],
            swiftCode: [],
            bankAddress: [],
            bankName: [],
            active: [this.status[0]],
        });

        this.code = this.formGroup.controls['code'];
        this.branchNameEn = this.formGroup.controls['branchNameEn'];
        this.shortName = this.formGroup.controls['shortName'];
        this.addressEn = this.formGroup.controls['addressEn'];
        this.taxcode = this.formGroup.controls['taxcode'];
        this.tel = this.formGroup.controls['tel'];
        this.email = this.formGroup.controls['email'];
        this.fax = this.formGroup.controls['fax'];
        this.company = this.formGroup.controls['company'];
        this.bankAccountName = this.formGroup.controls['bankAccountName'];
        this.swiftCode = this.formGroup.controls['swiftCode'];
        this.bankAddress = this.formGroup.controls['bankAddress'];
        this.active = this.formGroup.controls['active'];
        this.bankName = this.formGroup.controls['bankName'];
        this.addressVn = this.formGroup.controls['addressVn'];
        this.bankAccountVND = this.formGroup.controls['bankAccountVND'];
        this.branchNameVn = this.formGroup.controls['branchNameVn'];

    }
}

export interface IFormAddOffice {
    branchNameVn: string;
    branchNameEn: string;
    buid: string;
    addressVn: string;
    addressEn: string;
    tel: string;
    fax: string;
    email: string;
    taxcode: string;
    bankAccountVND: string;
    bankAccountUSD: string;
    bankName: string;
    bankAddress: string;
    code: string;
    swiftCode: string;
    shortName: string;
    active: boolean;
}
