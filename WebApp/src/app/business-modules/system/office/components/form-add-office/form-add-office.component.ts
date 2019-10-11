import { Component, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { SystemRepo } from 'src/app/shared/repositories';
import { Company } from 'src/app/shared/models';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
import { map } from 'rxjs/internal/operators/map';
import { Department } from 'src/app/shared/models/system/department';


@Component({
    selector: 'form-add-office',
    templateUrl: './form-add-office.component.html'
})
export class OfficeFormAddComponent extends AppForm {
    selectedDataOffice: any;
    isSubmited: boolean = false;
    isDetail: boolean = false;
    isCreate: boolean = false;
    configOffice: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    selectedCompany: Partial<CommonInterface.IComboGridData> = {};
    companies: Company[] = [];
    departments: Department[] = [];
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
    headers: CommonInterface.IHeaderTable[];


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
        this.initForm();
        this.getDataComboBox();
        this.headers = [
            { title: 'Department Code', field: 'code', sortable: true },
            { title: 'Name EN', field: 'deptNameEn', sortable: true },
            { title: 'Name Local', field: 'deptName', sortable: true },
            { title: 'Name Abbr', field: 'deptNameAbbr', sortable: true },
            { title: 'Office', field: 'officeName', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
        ];

    }
    onSelectDataFormInfo(data: any) {
        this.selectedCompany = { field: 'id', value: data.id };
        this.selectedDataOffice = data;
        console.log(this.selectedDataOffice);

    }

    getDepartment(data: any) {
        this.departments = data;
        console.log(this.departments);
    }

    update(formdata: any, data: any) {
        this.formGroup.patchValue(formdata);
        this.active.setValue(this.status.filter(i => i.value === data)[0]);
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
                    this.getCompanyData(this.companies);
                },
            );
    }
    getDataComboBox() {
        this.getCompanies();
    }

    getCompanyData(data: any) {
        this.configOffice.dataSource = data;
        this.configOffice.displayFields = [
            { field: 'code', label: 'Company Code' },
            { field: 'bunameEn', label: 'Name EN' },
            { field: 'bunameVn', label: 'Name Local' },
        ];
        this.configOffice.selectedDisplayFields = ['bunameEn'];

    }


    initForm() {
        this.formGroup = this._fb.group({
            code: ['',
                Validators.compose([
                    Validators.required
                ])],
            branchNameEn: ['',
                Validators.compose([
                    Validators.required
                ])],
            branchNameVn: ['',
                Validators.compose([
                    Validators.required
                ])],
            shortName: ['',
                Validators.compose([
                    Validators.required
                ])],
            addressEn: ['',
                Validators.compose([
                    Validators.required
                ])],
            addressVn: ['',
                Validators.compose([
                    Validators.required
                ])],
            taxcode: ['',
                Validators.compose([
                    Validators.required
                ])],
            tel: ['',
                Validators.compose([
                    Validators.required
                ])],
            email: ['',
                Validators.compose([
                    Validators.required
                ])],
            fax: ['',
                Validators.compose([
                    Validators.required
                ])],
            company: [],
            bankAccountVND: ['',
                Validators.compose([
                    Validators.required
                ])],
            bankAccountName: ['',
                Validators.compose([
                    Validators.required
                ])],
            swiftCode: ['',
                Validators.compose([
                    Validators.required
                ])],
            bankAddress: ['',
                Validators.compose([
                    Validators.required
                ])],
            bankName: ['',
                Validators.compose([
                    Validators.required
                ])],
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
    id: string;
    branchNameVn: string;
    bankAccountName: string;
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
    userCreated: string;
    datetimeCreated: string;
    userModified: string;
    datetimeModified: string;
    active: boolean;

}
