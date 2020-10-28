import { Component, Output, EventEmitter, ViewChild } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { finalize, catchError, distinctUntilChanged, map, takeUntil } from 'rxjs/operators';
import { Office, Company, User } from '@models';
import { Validators, FormBuilder, FormGroup, AbstractControl } from '@angular/forms';
import { JobConstants, SystemConstants } from '@constants';
import { SystemRepo, CatalogueRepo } from '@repositories';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { Store } from '@ngrx/store';
import { IAppState, getMenuUserSpecialPermissionState, GetCatalogueCurrencyAction, getCatalogueCurrencyState } from '@store';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';
import { Observable } from 'rxjs';
import { formatDate } from '@angular/common';
import { SalesmanCreditLimitPopupComponent } from '../../commercial/components/popup/salesman-credit-limit.popup';
import { PartnerRejectPopupComponent } from './partner-reject/partner-reject.popup';
import { ConfirmPopupComponent } from '@common';

@Component({
    selector: 'popup-form-contract-commercial-catalogue',
    templateUrl: 'form-contract-commercial-catalogue.popup.html'
})

export class FormContractCommercialPopupComponent extends PopupBase {

    formGroup: FormGroup;

    isUpdate: boolean = false;
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild(SalesmanCreditLimitPopupComponent, { static: false }) salesmanCreditLimitPopup: SalesmanCreditLimitPopupComponent;
    @ViewChild(PartnerRejectPopupComponent, { static: false }) popupRejectPartner: PartnerRejectPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmChangeAgreementTypePopup: ConfirmPopupComponent;

    openOnPartner: boolean = false;

    isRequiredContractNo: boolean = false;
    isCreateNewCommercial: boolean = false;
    isDuplicateContract: boolean = false;
    statusContract: boolean = false;

    salesmanId: AbstractControl;
    companyId: AbstractControl;
    officeId: AbstractControl;
    effectiveDate: AbstractControl;
    expiredDate: AbstractControl;
    contractType: AbstractControl;
    saleService: AbstractControl;
    paymentMethod: AbstractControl;
    baseOn: AbstractControl;
    vas: AbstractControl;
    trialEffectDate: AbstractControl;
    trialExpiredDate: AbstractControl;
    trialCreditDays: AbstractControl;
    contractNo: AbstractControl;
    currencyId: AbstractControl;


    minDateEffective: any = null;
    minDateExpired: any = null;
    minDateExpiredTrial: any = null;


    partnerId: string = null;

    users: User[] = [];
    companies: Company[] = [];
    // offices: Office[] = [];
    offices: CommonInterface.INg2Select[] = [];
    contracts: Contract[] = [];
    activeServices: any = [];
    activeVas: any = [];
    activeOffice: any = [];

    selectedContract: Contract = new Contract();

    idContract: string = SystemConstants.EMPTY_GUID;
    type: string = '';
    contractTypeDetail: string = '';
    confirmChangeAgreementTypeText: string = '';
    isChangeAgrmentType: boolean = false;
    status: boolean = false;
    isAllowActiveContract: boolean = false;
    isDisabledExpiredDateField: boolean = false;

    indexDetailContract: number = null;

    fileToUpload: File = null;
    fileList: any = null;

    files: any = {};


    menuSpecialPermission: Observable<any[]>;

    listCurrency: Observable<CommonInterface.INg2Select[]>;

    contractTypes: CommonInterface.INg2Select[] = [
        { id: "Trial", text: "Trial" },
        { id: "Official", text: "Official" },
        { id: "Guaranteed", text: "Guaranteed" },
        { id: "Cash", text: "Cash" }
    ];
    serviceTypes: CommonInterface.INg2Select[] = [
        { id: "All", text: "All" },
        { id: "AI", text: "Air Import" },
        { id: "AE", text: "Air Export" },
        { id: "SFE", text: "Sea FCL Export" },
        { id: "SLE", text: "Sea LCL Export" },
        { id: "SFI", text: "Sea FCL Import" },
        { id: "SLI", text: "Sea LCL Import" },
        { id: "CL", text: "Custom Logistic" },
        { id: "IT", text: "Trucking" }
    ];


    paymentMethods: CommonInterface.INg2Select[] = [
        { id: "All", text: "All" },
        ...JobConstants.COMMON_DATA.FREIGHTTERMS
    ];

    basesOn: CommonInterface.INg2Select[] = [
        { id: "Invoice Date", text: "Invoice Date" },
        { id: "Confirmed Billing", text: "Confirmed Billing" }
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
        protected _activeRoute: ActivatedRoute,
        private _store: Store<IAppState>

    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {

        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this._store.dispatch(new GetCatalogueCurrencyAction());
        this.listCurrency = this._store.select(getCatalogueCurrencyState).pipe(map(data => this.utility.prepareNg2SelectData(data, 'id', 'id')));
        this.initForm();
        this.initDataForm();

        if (!this.isUpdate) {
            const userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
            this.companyId.setValue(userLogged.companyId);
            this.formGroup.controls['paymentTerm'].setValue(30);
            this.formGroup.controls['creditLimitRate'].setValue(120);

        }
        this._store.select(getMenuUserSpecialPermissionState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((res: SystemInterface.ISpecialAction) => {
                if (!!res) {
                    this.isAllowActiveContract = res[0].isAllow;
                    console.log(this.isAllowActiveContract);
                }

            });

        this._activeRoute.data.subscribe((result: { name: string, type: string }) => {
            this.type = result.type;
        });

        this.formGroup.get("effectiveDate").valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                map((data: any) => data.startDate)
            )
            .subscribe((value: any) => {
                this.minDateExpired = this.createMoment(value); // * Update MinDate -> ExpiredDate.
            });
        if (!!this.trialEffectDate.value) {
            this.formGroup.get("trialEffectDate").valueChanges
                .pipe(
                    distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                    map((data: any) => data.startDate)
                )
                .subscribe((value: any) => {
                    this.minDateExpiredTrial = this.createMoment(value); // * Update MinDate -> ExpiredDate.
                });

        }

    }

    initForm() {
        this.formGroup = this._fb.group({
            salesmanId: [null, Validators.required],
            companyId: [null, Validators.required],
            officeId: [null, Validators.required],
            contractNo: [null, Validators.maxLength(50)],
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
            baseOn: [[{ id: "Invoice Date", text: "Invoice Date" }]],
            creditLimit: [],
            creditLimitRate: [],
            creditAmount: [],
            billingAmount: [],
            paidAmount: [],
            unpaidAmount: [],
            customerAmount: [],
            creditRate: [],
            description: [],
            currencyId: []
        });
        this.salesmanId = this.formGroup.controls['salesmanId'];
        this.companyId = this.formGroup.controls['companyId'];
        this.officeId = this.formGroup.controls['officeId'];
        this.effectiveDate = this.formGroup.controls['effectiveDate'];
        this.expiredDate = this.formGroup.controls['expiredDate'];
        this.contractType = this.formGroup.controls['contractType'];
        this.saleService = this.formGroup.controls['saleService'];
        this.paymentMethod = this.formGroup.controls['paymentMethod'];
        this.baseOn = this.formGroup.controls['baseOn'];
        this.vas = this.formGroup.controls['vas'];
        this.trialEffectDate = this.formGroup.controls['trialEffectDate'];
        this.trialExpiredDate = this.formGroup.controls['trialExpiredDate'];
        this.trialCreditDays = this.formGroup.controls['trialCreditDays'];
        this.contractNo = this.formGroup.controls['contractNo'];
        this.currencyId = this.formGroup.controls['currencyId'];
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
                    if (!!res) {
                        this.offices = this.utility.prepareNg2SelectData(res || [], 'id', 'shortName');
                        this.offices = [{ id: 'All', text: 'All' }, ...this.offices];
                    }
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
            if (!!this.files && !!this.files.id && this.fileList.length > 0) {
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
                        this.fileList = null;
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

    onSubmit(isRequestApproval: boolean = false) {
        this.setError(this.vas);
        this.setError(this.paymentMethod);
        this.setError(this.currencyId);
        this.isSubmitted = true;

        this.selectedContract.index = this.indexDetailContract;
        if (!this.effectiveDate.value.startDate) {
            return;
        }
        if (!!this.contractType.value && this.contractType.value.length > 0) {
            if (this.contractType.value[0].id === this.contractTypes[1].id && !this.contractNo.value) {
                this.isRequiredContractNo = true;
                return;
            } else {
                this.isRequiredContractNo = false;
            }
        }
        if (!this.saleService.value) {
            return;
        }
        if (this.formGroup.valid) {
            this.asignValueToModel();
            if (isRequestApproval === true) {
                this.selectedContract.isRequestApproval = true;
            } else {
                this.selectedContract.isRequestApproval = false;
            }
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
                                this.removeKeyworkNg2Select();
                                this.hide();
                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
            } else if (this.isUpdate && !this.isCreateNewCommercial) {
                const body = new Contract(this.selectedContract);
                if (this.contractTypeDetail !== this.contractType.value[0].id && this.selectedContract.active === true && this.isAllowActiveContract === false) { //&& this.isChangeAgrmentType === false && this.isAllowActiveContract === false) {
                    this.status = this.statusContract;
                    this.confirmChangeAgreementTypeText = "You have changed Agreement type " + this.contractTypeDetail + " to " + this.selectedContract.contractType + ", So your agreement will be inactive and request approval again. do you want to change it?";
                    this.confirmChangeAgreementTypePopup.show();
                    this.isChangeAgrmentType = true;
                    return;
                } else if (this.isChangeAgrmentType === true) {
                    this.isChangeAgrmentType = true;
                } else {
                    this.isChangeAgrmentType = false;
                }
                if (this.selectedContract.active === false) {
                    this.isChangeAgrmentType = null;
                }
                this.updateContract(body);

            } else {
                this.selectedContract.username = this.users.find(x => x.id === this.selectedContract.saleManId).username;
                if (this.selectedContract.officeId.includes(';')) {
                    const arrayOffice = this.selectedContract.officeId.split(';');
                    this.selectedContract.officeNameEn = '';
                    arrayOffice.forEach(itemOffice => {
                        this.selectedContract.officeNameEn += this.offices.find(x => x.id === itemOffice).text + "; ";
                    });
                    if (this.selectedContract.officeNameEn.charAt(this.selectedContract.officeNameEn.length - 2) === ';') {
                        this.selectedContract.officeNameEn = this.selectedContract.officeNameEn.substr(0, this.selectedContract.officeNameEn.length - 2);
                    }
                } else {
                    this.selectedContract.officeId = this.selectedContract.officeId.toLowerCase();
                    const obj = this.offices.find(x => x.id === this.selectedContract.officeId);

                    this.selectedContract.officeNameEn = !!obj ? obj.text : null;
                }

                if (this.selectedContract.saleService.includes(';')) {
                    const arrayOffice = this.selectedContract.saleService.split(';');
                    this.selectedContract.saleServiceName = '';
                    arrayOffice.forEach(itemOffice => {
                        this.selectedContract.saleServiceName += this.serviceTypes.find(x => x.id === itemOffice).text + "; ";
                    });
                    if (this.selectedContract.saleServiceName.charAt(this.selectedContract.saleServiceName.length - 2) === ';') {
                        this.selectedContract.saleServiceName = this.selectedContract.saleServiceName.substr(0, this.selectedContract.saleServiceName.length - 2);
                    }
                } else {
                    this.selectedContract.saleServiceName = this.selectedContract.saleService.toLowerCase();
                    const obj = this.serviceTypes.find(x => x.id === this.selectedContract.saleService);

                    this.selectedContract.saleServiceName = !!obj ? obj.text : null;
                }

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

    updateContract(body: Contract) {
        this._catalogueRepo.updateContract(body)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.onRequest.emit(this.selectedContract);

                        this.removeKeyworkNg2Select();
                        this.hide();
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onSubmitChangeAgreementType() {
        if (this.formGroup.valid) {
            this.asignValueToModel();
            const body = new Contract(this.selectedContract);
            body.isChangeAgrmentType = this.isChangeAgrmentType;
            this.confirmChangeAgreementTypePopup.hide();
            this.updateContract(body);
        }

    }

    getCurrentActiveService(Service: any) {
        const listService = Service.split(";");
        const activeServiceList: any = [];
        listService.forEach(item => {
            const element = this.serviceTypes.find(x => x.id === item.trim());
            if (element !== undefined) {
                const activeService = element;
                activeServiceList.push(activeService);
            }
        });
        return activeServiceList;
    }

    getCurrentActiveVas(Vas: any) {
        const listVas = Vas.split(";");
        const activeVasList: any = [];
        listVas.forEach(item => {
            const element = this.vaslst.find(x => x.id === item.trim());
            if (element !== undefined) {
                const activeVas = element;
                activeVasList.push(activeVas);
            }
        });
        return activeVasList;
    }

    getCurrentActiveOffice(Office: any) {
        const listOffice = Office.split(";");
        const activeOfficeList: any = [];
        listOffice.forEach(item => {
            const element = this.offices.find(x => x.id === item.toLowerCase());
            if (element !== undefined) {
                const activeOffice = element;
                activeOfficeList.push(activeOffice);
            }
        });
        return activeOfficeList;
    }

    pachValueToFormContract() {
        this.activeServices = this.getCurrentActiveService(this.selectedContract.saleService);
        this.activeVas = this.getCurrentActiveVas(this.selectedContract.vas);
        if (!!this.selectedContract.officeId) {
            this.activeOffice = this.getCurrentActiveOffice(this.selectedContract.officeId);
        }
        this.setError(this.saleService);
        this.formGroup.patchValue({
            salesmanId: !!this.selectedContract.saleManId ? this.selectedContract.saleManId : null,
            companyId: !!this.selectedContract.companyId ? this.selectedContract.companyId : null,
            officeId: !!this.selectedContract.officeId ? [<CommonInterface.INg2Select>{ id: this.selectedContract.officeId, text: '' }] : null,
            contractNo: this.selectedContract.contractNo,
            effectiveDate: !!this.selectedContract.effectiveDate ? { startDate: new Date(this.selectedContract.effectiveDate), endDate: new Date(this.selectedContract.effectiveDate) } : null,
            expiredDate: !!this.selectedContract.expiredDate ? { startDate: new Date(this.selectedContract.expiredDate), endDate: new Date(this.selectedContract.expiredDate) } : null,
            contractType: !!this.selectedContract.contractType ? [this.contractTypes.find(type => type.id === this.selectedContract.contractType)] : null,
            paymentTerm: this.selectedContract.paymentTerm,
            creditLimit: this.selectedContract.creditLimit,
            creditLimitRate: this.selectedContract.creditLimitRate,
            trialCreditLimit: this.selectedContract.trialCreditLimited,
            trialCreditDays: this.selectedContract.trialCreditDays,
            trialEffectDate: !!this.selectedContract.trialEffectDate ? { startDate: new Date(this.selectedContract.trialEffectDate), endDate: new Date(this.selectedContract.trialEffectDate) } : null,
            trialExpiredDate: !!this.selectedContract.trialExpiredDate ? { startDate: new Date(this.selectedContract.trialExpiredDate), endDate: new Date(this.selectedContract.trialExpiredDate) } : null,
            creditAmount: this.selectedContract.creditAmount,
            billingAmount: this.selectedContract.billingAmount,
            paidAmount: this.selectedContract.paidAmount,
            unpaidAmount: this.selectedContract.unpaidAmount,
            customerAmount: this.selectedContract.customerAdvanceAmount,
            creditRate: this.selectedContract.creditRate,
            description: this.selectedContract.description,
            saleService: !!this.selectedContract.saleService ? [<CommonInterface.INg2Select>{ id: this.selectedContract.saleService, text: '' }] : null,
            vas: !!this.selectedContract.vas ? [<CommonInterface.INg2Select>{ id: this.selectedContract.vas, text: '' }] : null,
            paymentMethod: !!this.selectedContract.paymentMethod ? [this.paymentMethods.find(type => type.id === this.selectedContract.paymentMethod)] : null,
            baseOn: !!this.selectedContract.baseOn ? [this.basesOn.find(type => type.id === this.selectedContract.baseOn)] : null,
            currencyId: !!this.selectedContract.currencyId ? [{ id: this.selectedContract.currencyId, text: this.selectedContract.currencyId }] : null
        });
        this.contractTypeDetail = this.selectedContract.contractType;
        if (this.selectedContract.contractType === 'Trial') {
            this.isDisabledExpiredDateField = true;
        } else {
            this.isDisabledExpiredDateField = false;
        }
    }


    asignValueToModel() {
        // this.selectedContract = new Contract();
        if (this.isUpdate) {
            this.selectedContract.id = this.idContract;
        }
        this.selectedContract.currencyId = !!this.currencyId.value ? this.currencyId.value[0].id : null;
        this.selectedContract.active = this.statusContract;
        this.selectedContract.saleManId = this.salesmanId.value;
        this.selectedContract.companyId = this.companyId.value;
        this.selectedContract.index = this.indexDetailContract;
        this.selectedContract.contractNo = this.formGroup.controls['contractNo'].value;
        this.selectedContract.effectiveDate = this.effectiveDate.value ? (this.effectiveDate.value.startDate !== null ? formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null;
        this.selectedContract.expiredDate = !!this.expiredDate.value && !!this.expiredDate.value.startDate ? formatDate(this.expiredDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.selectedContract.contractType = !!this.contractType.value ? this.contractType.value[0].id : null;
        const services = this.saleService.value ? (this.saleService.value.length > 0 ? this.saleService.value.map((item: any) => item.id).toString().replace(/(?:,)/g, ';') : '') : '';
        this.selectedContract.saleService = services;
        const vass = this.vas.value ? (this.vas.value.length > 0 ? this.vas.value.map((item: any) => item.id).toString().replace(/(?:,)/g, ';') : '') : '';
        this.selectedContract.vas = vass;
        const offices = this.officeId.value ? (this.officeId.value.length > 0 ? this.officeId.value.map((item: any) => item.id).toString().replace(/(?:,)/g, ';') : '') : '';
        this.selectedContract.officeId = offices;
        this.selectedContract.paymentMethod = !!this.paymentMethod.value ? this.paymentMethod.value[0].id : null;
        this.selectedContract.baseOn = !!this.baseOn.value ? this.baseOn.value[0].id : null;
        this.selectedContract.trialCreditLimited = this.formGroup.controls['trialCreditLimit'].value;
        this.selectedContract.trialCreditDays = this.formGroup.controls['trialCreditDays'].value;
        if (this.officeId.value[0].id === 'All') {
            this.selectedContract.officeId = this.mapOfficeId();
        }
        if (this.saleService.value[0].id === 'All') {
            this.selectedContract.saleService = this.mapServiceId();
        }
        if (this.vas.value != null && this.vas.value[0].id === 'All') {
            this.selectedContract.vas = this.mapVas();
        }
        if (this.contractType.value[0].id === 'Trial' && !this.isUpdate) {
            if (!!this.effectiveDate.value.startDate) {
                this.trialEffectDate.setValue({
                    startDate: new Date(new Date(this.effectiveDate.value.startDate).setDate(new Date(this.effectiveDate.value.startDate).getDate())),
                    endDate: new Date(new Date(this.effectiveDate.value.endDate).setDate(new Date(this.effectiveDate.value.endDate).getDate())),
                });
            }

            if (!!this.expiredDate.value.startDate) {
                this.trialExpiredDate.setValue({
                    startDate: new Date(new Date(this.expiredDate.value.startDate).setDate(new Date(this.expiredDate.value.startDate).getDate())),
                    endDate: new Date(new Date(this.expiredDate.value.endDate).setDate(new Date(this.expiredDate.value.endDate).getDate())),
                });
            }

            if (!!this.effectiveDate.value.startDate && !!this.expiredDate.value.startDate) {
                this.trialCreditDays.setValue(this.expiredDate.value.startDate.diff(this.effectiveDate.value.startDate, 'days'));
            }
        }

        if (this.contractType.value[0].id === 'Official') {
            this.trialEffectDate.setValue(null);
            this.trialCreditDays.setValue(null);
            this.trialExpiredDate.setValue(null);
        }

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
        this.selectedContract.trialCreditDays = this.trialCreditDays.value;
        this.selectedContract.partnerId = this.partnerId;

    }

    activeInactiveContract(id: string) {
        if (this.contractType.value[0].id === 'Guaranteed'
            && ((this.formGroup.controls['creditLimit'].value <= 0
                || !this.formGroup.controls['creditLimit'].value)
                && this.selectedContract.active === false)) {
            this.salesmanCreditLimitPopup.show();
            return;
        }
        this.processActiveInActiveContract(id);
    }

    onSalesmanCreditRequest($event: any) {
        const data = $event;
        console.log(data);
        if (!!data.creditRate || data.creditLimit) {
            this.processActiveInActiveContract(this.selectedContract.id, true, data);
            this.selectedContract.creditLimit = data.creditLimit;
            this.selectedContract.creditLimitRate = data.creditRate;
            this.formGroup.controls['creditLimit'].setValue(this.selectedContract.creditLimit);
            this.formGroup.controls['creditLimitRate'].setValue(this.selectedContract.creditLimitRate);
        }
    }

    processActiveInActiveContract(id: string, salesmanCreditRequest?: boolean, bodyCredit?: any) {
        this._progressRef.start();
        this._catalogueRepo.activeInactiveContract(id, this.partnerId, bodyCredit)
            .pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
            }))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.selectedContract.active = !this.selectedContract.active;
                        this.statusContract = this.selectedContract.active;
                        let message = '';
                        if (!this.selectedContract.active) {
                            message = 'Inactive success !!';
                        } else {
                            message = 'Active success !!';
                            this.selectedContract.partnerStatus = true;
                        }
                        this._toastService.success(message);
                        this.onRequest.emit(this.selectedContract);


                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onChageTrialCreditDays() {
        const trialDays = !!this.formGroup.controls['trialCreditDays'].value ? this.formGroup.controls['trialCreditDays'].value : 0;
        if (!!this.trialExpiredDate.value.startDate) {
            this.trialExpiredDate.setValue({
                startDate: new Date(new Date(this.trialExpiredDate.value.startDate).setDate(new Date(this.trialExpiredDate.value.startDate).getDate() + trialDays)),
                endDate: new Date(new Date(this.trialExpiredDate.value.endDate).setDate(new Date(this.trialExpiredDate.value.endDate).getDate() + trialDays)),
            });
        }
    }

    onUpdateTrialEffectiveDate(value: { startDate: any; endDate: any }) {
        const trialDays = !!this.formGroup.controls['trialCreditDays'].value ? this.formGroup.controls['trialCreditDays'].value : 0;
        this.trialExpiredDate.setValue({
            startDate: new Date(new Date(value.startDate).setDate(new Date(value.startDate).getDate() + trialDays)),
            endDate: new Date(new Date(value.endDate).setDate(new Date(value.endDate).getDate() + trialDays)),
        });
    }

    selectedService($event: any) {
        if ($event.id === 'All' || (!!this.saleService.value && this.saleService.value.some(x => x.id === 'All'))) {
            this.saleService.setValue([]);
            this.saleService.setValue([<CommonInterface.INg2Select>{ id: $event.id, text: $event.text }]);
        }
    }

    selectedVas($event: any) {
        if ($event.id === 'All' || (!!this.vas.value && this.vas.value.some(x => x.id === 'All'))) {
            this.vas.setValue([]);
            this.vas.setValue([<CommonInterface.INg2Select>{ id: $event.id, text: $event.text }]);
        }
    }

    selectedOffice($event: any) {
        if ($event.id === 'All' || (!!this.officeId.value && this.officeId.value.some(x => x.id === 'All'))) {
            this.officeId.setValue([]);
            this.officeId.setValue([<CommonInterface.INg2Select>{ id: $event.id, text: $event.text }]);
        }
    }

    selectedAgreementType($event: any) {
        console.log($event);
        if ($event.id === 'Trial') {
            this.isDisabledExpiredDateField = true;
            if (!!this.effectiveDate.value.startDate) {
                this.expiredDate.setValue({
                    startDate: new Date(new Date(this.effectiveDate.value.startDate).setDate(new Date(this.effectiveDate.value.startDate).getDate() + 30)),
                    endDate: new Date(new Date(this.effectiveDate.value.endDate).setDate(new Date(this.effectiveDate.value.endDate).getDate() + 30)),
                });
            }
        } else {
            this.isDisabledExpiredDateField = false;
        }
    }

    mapOfficeId() {
        let officeId = '';
        const off = this.offices.filter(office => office.id !== 'All');
        officeId = off.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
        return officeId;
    }

    mapServiceId() {
        let serviceId = '';
        const serv = this.serviceTypes.filter(service => service.id !== 'All');
        serviceId = serv.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
        return serviceId;
    }

    mapVas() {
        let vasId = '';
        const serv = this.vaslst.filter(vas => vas.id !== 'All');
        vasId = serv.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
        return vasId;
    }

    onSaveReject($event: string) {
        const comment = $event;
        this._progressRef.start();
        this._catalogueRepo.rejectCommentCommercial(this.partnerId, this.selectedContract.id, comment, this.type)
            .pipe(
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: boolean) => {
                    if (res === true) {
                        this._toastService.success('Sent Successfully!');
                    } else {
                        this._toastService.error('something went wrong!');
                    }
                }
            );
    }

    onARConfirmed() {
        this._progressRef.start();
        this._catalogueRepo.arConfirmed(this.partnerId, this.selectedContract.id, this.type)
            .pipe(
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: boolean) => {
                    if (res === true) {
                        this.selectedContract.arconfirmed = true;
                        this.onRequest.emit(this.selectedContract);
                        this._toastService.success('AR Confirm Successfully!');
                    } else {
                        this._toastService.error('something went wrong!');
                    }
                }
            );
    }

    showRejectCommentPopup() {
        this.popupRejectPartner.comment = '';
        this.popupRejectPartner.show();
    }

    close() {
        this.hide();
    }
}
