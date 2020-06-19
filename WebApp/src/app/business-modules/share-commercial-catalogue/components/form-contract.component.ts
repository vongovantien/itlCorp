import { Component, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { SystemRepo, CatalogueRepo } from '@repositories';
import { catchError, finalize, distinctUntilChanged, map, tap, switchMap } from 'rxjs/operators';
import { User, Company, Office } from '@models';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { JobConstants } from '@constants';
import { AppForm } from 'src/app/app.form';
import { Router, ActivatedRoute, Params } from '@angular/router';
import moment from 'moment';


@Component({
    selector: 'form-contract',
    templateUrl: 'form-contract.component.html'
})

export class FormContractComponent extends AppForm {

    formContract: FormGroup;

    isUpdate: boolean = false;
    isRequiredContractNo: boolean = false;

    salesmanId: AbstractControl;
    companyId: AbstractControl;
    officeId: AbstractControl;
    effectiveDate: AbstractControl;
    expiredDate: AbstractControl;
    contractType: AbstractControl;
    saleService: AbstractControl;
    paymentMethod: AbstractControl;
    vas: AbstractControl;
    trialEffectDate: AbstractControl;
    trialExpiredDate: AbstractControl;

    minDateEffective: any = null;
    minDateExpired: any = null;

    partnerId: string;

    users: User[] = [];
    companies: Company[] = [];
    offices: Office[] = [];

    contract: Contract = new Contract();

    fileToUpload: File = null;
    fileList: FileList[] = null;
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
        ...JobConstants.COMMON_DATA.FREIGHTTERMS
    ];
    vaslst: CommonInterface.INg2Select[] = this.serviceTypes;
    isCollapsed: boolean = false;


    constructor(
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService,
        protected _router: Router,
        private _activedRoute: ActivatedRoute,

    ) {
        super();
    }

    ngOnInit() {
        this._activedRoute.params
            .subscribe((param: Params) => {
                if (param.partnerId) {
                    this.partnerId = param.partnerId;
                    console.log(this.partnerId);

                } else {
                    this.gotoList();
                }
            });
        this.initForm();
        this.initDataForm();
        if (!this.isUpdate) {
            const userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
            this.companyId.setValue(userLogged.companyId);
            this.formContract.controls['paymentTerm'].setValue(30);
            this.formContract.controls['creditLimitRate'].setValue(120);
        }

    }

    initForm() {
        this.formContract = this._fb.group({
            salesman: [null, Validators.required],
            companyId: [null, Validators.required],
            officeId: [],
            contractNo: [],
            effectiveDate: [null, Validators.required],
            expiredDate: [],
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
        this.effectiveDate = this.formContract.controls['effectiveDate'];
        this.expiredDate = this.formContract.controls['expiredDate'];
        this.contractType = this.formContract.controls['contractType'];
        this.saleService = this.formContract.controls['saleService'];
        this.paymentMethod = this.formContract.controls['paymentMethod'];
        this.vas = this.formContract.controls['vas'];
        this.trialEffectDate = this.formContract.controls['trialEffectDate'];
        this.trialExpiredDate = this.formContract.controls['trialExpiredDate'];
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
            this.salesmanId.setValue($event.id);
        } else if (type === 'company') {
            this.companyId.setValue($event.id);
        } else if (type === 'office') {
            this.officeId.setValue($event.id);

        }
    }

    onSubmit() {
        this.setError(this.vas);
        this.setError(this.paymentMethod);
        this.isSubmitted = true;
        if (!!this.contractType.value && this.contractType.value.length > 0) {
            if (this.contractType.value[0].id === this.contractTypes[1].id) {
                this.isRequiredContractNo = true;
                return;
            } else {
                this.isRequiredContractNo = false;
            }
        }
        if (this.formContract.valid) {
            this.asignValueToModel();
            console.log(this.contract);
            if (!this.isUpdate) {
                this._catalogueRepo.createContract(this.contract)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);
                                if (!!this.fileList) {
                                    this.uploadFileContract();
                                } else {
                                    this.gotoList();
                                }
                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
            }

        }
    }

    uploadFileContract() {
        this._catalogueRepo.uploadFileContract(this.partnerId, this.fileList)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.gotoList();
                    }
                }
            );
    }

    asignValueToModel() {
        this.contract.saleManId = this.salesmanId.value;
        this.contract.companyId = this.companyId.value;
        this.contract.officeId = this.officeId.value;
        this.contract.contractNo = this.formContract.controls['contractNo'].value;
        this.contract.effectiveDate = this.effectiveDate.value ? (this.effectiveDate.value.startDate !== null ? formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null;
        this.contract.expiredDate = !!this.expiredDate.value && !!this.expiredDate.value.startDate ? formatDate(this.expiredDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.contract.contractType = !!this.contractType.value ? this.contractType.value[0].id : null;
        this.contract.saleService = !!this.saleService.value ? this.saleService.value[0].id : null;
        this.contract.paymentMethod = !!this.paymentMethod.value ? this.paymentMethod.value[0].id : null;
        this.contract.vas = !!this.vas.value ? this.vas.value[0].id : null;
        this.contract.trialCreditLimit = this.formContract.controls['trialCreditLimit'].value;
        this.contract.trialCreditDays = this.formContract.controls['trialCreditDays'].value;
        this.contract.trialEffectDate = this.trialEffectDate.value;
        this.contract.trialExpiredDate = this.trialExpiredDate.value;
        this.contract.paymentTerm = this.formContract.controls['paymentTerm'].value;
        this.contract.creditLimit = this.formContract.controls['creditLimit'].value;
        this.contract.creditLimitRate = this.formContract.controls['creditLimitRate'].value;
        this.contract.creditAmount = this.formContract.controls['creditAmount'].value;
        this.contract.billingAmount = this.formContract.controls['billingAmount'].value;
        this.contract.paidAmount = this.formContract.controls['paidAmount'].value;
        this.contract.unpaidAmount = this.formContract.controls['unpaidAmount'].value;
        this.contract.customerAdvanceAmount = this.formContract.controls['customerAmount'].value;
        this.contract.creditRate = this.formContract.controls['creditRate'].value;
        this.contract.description = this.formContract.controls['description'].value;
        this.contract.partnerId = this.partnerId;
    }


    handleFileInput(event: any) {
        this.fileList = event.target['files'];
        console.log(this.fileList);
    }

    gotoList() {
        this._router.navigate([`home/commercial/customer//${this.partnerId}`]);
    }

    onChangeTrialDate() {


    }

}