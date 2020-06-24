import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { finalize, catchError } from 'rxjs/operators';
import { Office, Company, User } from '@models';
import { Validators, FormBuilder, FormGroup, AbstractControl } from '@angular/forms';
import { JobConstants } from '@constants';
import { SystemRepo, CatalogueRepo } from '@repositories';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { Store } from '@ngrx/store';
import { IAppState, getMenuUserSpecialPermissionState } from '@store';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';
import { Observable } from 'rxjs';
import { formatDate } from '@angular/common';

@Component({
    selector: 'popup-form-contract-commercial-catalogue',
    templateUrl: 'form-contract-commercial-catalogue.popup.html'
})

export class FormContractCommercialPopupComponent extends PopupBase {
    formGroup: FormGroup;

    isUpdate: boolean = false;
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();

    isRequiredContractNo: boolean = false;
    isCreateNewCommercial: boolean = false;
    isDuplicateContract: boolean = false;

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
    contracts: Contract[] = [];

    selectedContract: Contract = new Contract();

    indexDetailContract: number = null;


    fileToUpload: File = null;
    fileList: any[] = null;

    files: any = {};

    menuSpecialPermission: Observable<any[]>;

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
        protected _router: Router,
        protected _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _store: Store<IAppState>

    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this.initForm();
        this.initDataForm();
        if (!this.isUpdate) {
            const userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
            this.companyId.setValue(userLogged.companyId);
            this.formGroup.controls['paymentTerm'].setValue(30);
            this.formGroup.controls['creditLimitRate'].setValue(120);
        }
    }



    initForm() {
        this.formGroup = this._fb.group({
            salesmanId: [null, Validators.required],
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
        this.salesmanId = this.formGroup.controls['salesmanId'];
        this.companyId = this.formGroup.controls['companyId'];
        this.officeId = this.formGroup.controls['officeId'];
        this.effectiveDate = this.formGroup.controls['effectiveDate'];
        this.expiredDate = this.formGroup.controls['expiredDate'];
        this.contractType = this.formGroup.controls['contractType'];
        this.saleService = this.formGroup.controls['saleService'];
        this.paymentMethod = this.formGroup.controls['paymentMethod'];
        this.vas = this.formGroup.controls['vas'];
        this.trialEffectDate = this.formGroup.controls['trialEffectDate'];
        this.trialExpiredDate = this.formGroup.controls['trialExpiredDate'];
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

    handleFileInput(event: any) {
        this.fileList = event.target['files'];
        console.log(this.fileList);
        if (this.isUpdate && !this.isCreateNewCommercial) {
            if (!!this.files && !!this.files.id) {
                this.deleteFileContract();
            } else {
                this.uploadFileContract(this.selectedContract.id);
            }
        }
    }

    uploadFileContract(id: string) {
        this._catalogueRepo.uploadFileContract(this.partnerId, id, this.fileList)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.fileList = [];
                        this._toastService.success("Upload file successfully!");
                        if (this.isUpdate) {
                            this.getFileContract();
                        }
                    }
                }
            );
    }

    getFileContract() {
        this.isLoading = true;
        this._catalogueRepo.getContractFilesAttach(this.partnerId, this.selectedContract.id).
            pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: any = []) => {
                    this.files = res;
                    console.log(this.files);
                }
            );
    }

    deleteFileContract() {
        this._progressRef.start();
        this._catalogueRepo.deleteContractFilesAttach(this.files.id)
            .pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: any) => {
                    if (res.result.success) {
                        this.uploadFileContract(this.selectedContract.id);
                    } else {
                        this._toastService.error("some thing wrong");
                    }
                }
            );
    }

    resetFormControl(type: string) {
        switch (type) {
            case 'salesmanId': this.salesmanId.setValue(null);
                break;
            case 'company': this.companyId.setValue(null);
                break;
            case 'office': this.officeId.setValue(null);
                break;
        }
    }

    onSubmit() {
        this.setError(this.vas);
        this.setError(this.paymentMethod);
        this.isSubmitted = true;
        this.selectedContract.index = this.indexDetailContract;
        if (!!this.contractType.value && this.contractType.value.length > 0) {
            if (this.contractType.value[0].id === this.contractTypes[1].id) {
                this.isRequiredContractNo = true;
                return;
            } else {
                this.isRequiredContractNo = false;
            }
        }
        if (this.formGroup.valid) {
            this.asignValueToModel();
            console.log(this.selectedContract);
            if (!this.isUpdate && !this.isCreateNewCommercial) {
                this._catalogueRepo.createContract(this.selectedContract)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);
                                if (!!this.fileList) {
                                    this.uploadFileContract(res.data);
                                }
                                this.onRequest.emit(this.selectedContract);
                                this.hide();
                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
            } else if (this.isUpdate && !this.isCreateNewCommercial) {
                this._catalogueRepo.updateContract(this.selectedContract)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);
                                this.onRequest.emit(this.selectedContract);
                                this.hide();
                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
            } else {
                this.selectedContract.username = this.users.find(x => x.id === this.selectedContract.saleManId).username;
                this.selectedContract.officeNameEn = !!this.selectedContract.officeId ? this.offices.find(x => x.id === this.selectedContract.officeId).branchNameEn : null;
                this.selectedContract.companyNameEn = this.companies.find(x => x.id === this.selectedContract.companyId).bunameEn;
                this.selectedContract.fileList = this.fileList;
                const objCheckContract = !!this.selectedContract.contractNo && this.contracts.length >= 1 ? this.contracts.some(x => x.contractNo === this.selectedContract.contractNo && x.index !== this.selectedContract.index) : null;
                if (!objCheckContract) {
                    this.onRequest.emit(new Contract(this.selectedContract));
                } else {
                    this.isDuplicateContract = true;
                    this._toastService.error('Contract no has been existed!');
                }
                if (!this.isDuplicateContract) {
                    this.hide();
                }
            }

        }
    }

    asignValueToModel() {
        this.selectedContract = new Contract();
        this.selectedContract.saleManId = this.salesmanId.value;
        this.selectedContract.companyId = this.companyId.value;
        this.selectedContract.officeId = this.officeId.value;
        this.selectedContract.index = this.indexDetailContract;
        this.selectedContract.contractNo = this.formGroup.controls['contractNo'].value;
        this.selectedContract.effectiveDate = this.effectiveDate.value ? (this.effectiveDate.value.startDate !== null ? formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null;
        this.selectedContract.expiredDate = !!this.expiredDate.value && !!this.expiredDate.value.startDate ? formatDate(this.expiredDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.selectedContract.contractType = !!this.contractType.value ? this.contractType.value[0].id : null;
        this.selectedContract.saleService = !!this.saleService.value ? this.saleService.value[0].id : null;
        this.selectedContract.paymentMethod = !!this.paymentMethod.value ? this.paymentMethod.value[0].id : null;
        this.selectedContract.vas = !!this.vas.value ? this.vas.value[0].id : null;
        this.selectedContract.trialCreditLimited = this.formGroup.controls['trialCreditLimit'].value;
        this.selectedContract.trialCreditDays = this.formGroup.controls['trialCreditDays'].value;
        this.selectedContract.trialEffectDate = !!this.trialEffectDate.value && !!this.trialEffectDate.value.startDate ? formatDate(this.trialEffectDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.selectedContract.trialExpiredDate = !!this.trialExpiredDate.value && !!this.trialExpiredDate.value.startDate ? formatDate(this.trialExpiredDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.selectedContract.paymentTerm = this.formGroup.controls['paymentTerm'].value;
        this.selectedContract.creditLimit = this.formGroup.controls['creditLimit'].value;
        this.selectedContract.creditLimitRate = this.formGroup.controls['creditLimitRate'].value;
        this.selectedContract.creditAmount = this.formGroup.controls['creditAmount'].value;
        this.selectedContract.billingAmount = this.formGroup.controls['billingAmount'].value;
        this.selectedContract.paidAmount = this.formGroup.controls['paidAmount'].value;
        this.selectedContract.unpaidAmount = this.formGroup.controls['unpaidAmount'].value;
        this.selectedContract.customerAdvanceAmount = this.formGroup.controls['customerAmount'].value;
        this.selectedContract.creditRate = this.formGroup.controls['creditRate'].value;
        this.selectedContract.description = this.formGroup.controls['description'].value;
        this.selectedContract.partnerId = this.partnerId;
    }

    activeInactiveContract(id: string) {
        this._catalogueRepo.activeInactiveContract(id)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.selectedContract.active = !this.selectedContract.active;
                        this._toastService.success(res.message);
                        this.onRequest.emit(this.selectedContract);

                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    close() {
        this.hide();
    }
}
