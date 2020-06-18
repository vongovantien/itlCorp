import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { SystemRepo, CatalogueRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { User, Company, Office } from '@models';
import { JobConstants } from '@constants';

@Component({
    selector: 'form-contract-popup',
    templateUrl: 'form-contract.popup.html'
})

export class FormContractPopupComponent extends PopupBase {

    formContract: FormGroup;

    isUpdate: boolean = false;

    salesmanId: AbstractControl;
    companyId: AbstractControl;
    officeId: AbstractControl;

    users: User[] = [];
    companies: Company[] = [];
    offices: Office[] = [];

    contractTypes: CommonInterface.INg2Select[] = [
        { id: "Trial", text: "Trial" },
        { id: "Official", text: "Official" },
        { id: "Guarantee", text: "Guarantee" },
        { id: "Cash", text: "Cash" }
    ];
    serviceTypes: CommonInterface.INg2Select[] = [
        { id: "All", text: "All" },
        { id: "Air Import", text: "Air Import" },
        { id: "Air Export", text: "Air Export" },
        { id: "Sea FCL Export", text: "Sea FCL Export" },
        { id: "Sea LCL Export", text: "Sea LCL Export" },
        { id: "Sea FCL Import", text: "Sea FCL Import" },
        { id: "Sea LCL Import", text: "Sea LCL Import" },
        { id: "Custom Logistic", text: "Custom Logistic" },
        { id: "Trucking", text: "Trucking" }
    ];
    paymentMethods: CommonInterface.INg2Select[] = [
        { id: "All", text: "All" },
        { id: "Prepaid", text: "Prepaid" },
        { id: "Collect", text: "Collect" }
    ];
    vaslst: CommonInterface.INg2Select[] = this.serviceTypes;
    isCollapsed: boolean = false;


    constructor(
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo

    ) {
        super();
    }
    ngOnInit() {
        this.initDataForm();
        this.formContract = this._fb.group({
            salesman: [null, Validators.required],
            companyId: [null, Validators.required],
            officeId: [],
            contractNo: [],
            effectiveDate: [null, Validators.required],
            expiriedDate: [],
            contractType: [[{ id: 'Trial', text: 'Trial' }], Validators.required],
            saleService: [null, Validators.required],
            paymentMethod: [[{ id: 'All', text: 'All' }]],
            vas: [],
            trialEffectDate: [],
            trialExpiredDate: [],
            trialCreditLimit: [],
            trialCreditDays: [],
            paymentTerm: [],
            creditLimit: [],
            creditLimitRate: [],
            creditAmount: [],
            billingAmount: [],
            paidAmount: [],
            unpaidAmount: [],
            customerAmount: [],
            creditRate: [],
            description: []
        });
        this.salesmanId = this.formContract.controls['salesman'];
        this.companyId = this.formContract.controls['companyId'];
        this.officeId = this.formContract.controls['officeId'];



    }

    initDataForm() {
        this.getUsers();
        this.getCompanies();
        this.getOffices();
    }

    getUsers() {
        this._systemRepo.getSystemUsers({ active: true })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: User[]) => {
                    if (!!res) {
                        this.users = res;
                    }
                },
            );
    }

    getCompanies() {
        this._systemRepo.getListCompany()
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: Company[]) => {
                    this.companies = res;
                },
            );
    }

    getOffices() {
        this._systemRepo.getAllOffice()
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: Office[]) => {
                    this.offices = res;
                },
            );
    }

    onSelectedDataFormInfo($event, type: string) {
        if (type === 'salesman') {
            this.salesmanId.setValue($event);
        }
    }

    resetFormControl(type: string) {
        if (type === 'salesman') {
            this.salesmanId.setValue(null);
        } else if (type === 'company') {
            this.companyId.setValue(null);
        } else if (type === 'office') {
            this.officeId.setValue(null);

        }
    }


}