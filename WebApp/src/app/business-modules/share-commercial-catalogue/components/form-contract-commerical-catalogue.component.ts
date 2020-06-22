import { Component, Input } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { SystemRepo, CatalogueRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { User, Company, Office } from '@models';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';
import { JobConstants } from '@constants';
import { AppForm } from 'src/app/app.form';
import { Router, } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';


@Component({
    selector: 'form-contract-commercial-catalogue',
    templateUrl: 'form-contract-commercial-catalogue.component.html'
})

export class FormContractCommercialCatalogueComponent extends AppForm {

    formGroup: FormGroup;

    @Input() isUpdate: boolean = false;

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

    selectedContract: Contract = new Contract();

    fileToUpload: File = null;
    fileList: any[] = null;

    files: any = {};

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

    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {

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
        if (this.isUpdate) {
            this.uploadFileContract(this.selectedContract.id);
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
                        this.getFileContract();
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
                    // this.files.forEach(f => f.extension = f.name.split("/").pop().split('.').pop());
                    console.log(this.files);
                }
            );
    }




}
interface IContractAttachFile {
    id: string;
    name: string;
    thumb: string;
    url: string;
    folder: string;
    objectId: string;
    childId: string;
    extension: string;
    userCreated: string;
    dateTimeCreated: string;
    fileName: string;
    isChecked: boolean;
}
