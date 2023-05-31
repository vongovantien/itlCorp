import { formatDate } from '@angular/common';
import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmPopupComponent, InfoPopupComponent } from '@common';
import { JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Company, Customer, Office, Partner, User } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, SystemFileManageRepo, SystemRepo } from '@repositories';
import { GetCatalogueCurrencyAction, IAppState, getCatalogueCurrencyState, getCurrentUserState, getMenuUserSpecialPermissionState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { catchError, concatMap, distinctUntilChanged, finalize, map, takeUntil} from 'rxjs/operators';
import { SalesmanCreditLimitPopupComponent } from 'src/app/business-modules/commercial/components/popup/salesman-credit-limit.popup';
import { PopupBase } from 'src/app/popup.base';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';
import { PartnerRejectPopupComponent } from './partner-reject/partner-reject.popup';
import { getDetailPartnerDataState } from 'src/app/business-modules/commercial/store';

@Component({
    selector: 'popup-form-contract-commercial-catalogue',
    templateUrl: 'form-contract-commercial-catalogue.popup.html'
})

export class FormContractCommercialPopupComponent extends PopupBase {

    formGroup: FormGroup;
    partners: Observable<Customer[]>;
    isUpdate: boolean = false;
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild(SalesmanCreditLimitPopupComponent) salesmanCreditLimitPopup: SalesmanCreditLimitPopupComponent;
    @ViewChild(PartnerRejectPopupComponent) popupRejectPartner: PartnerRejectPopupComponent;
    @ViewChild(ConfirmPopupComponent) confirmChangeAgreementTypePopup: ConfirmPopupComponent;
    detailPartner: any;
    openOnPartner: boolean = false;

    isRequiredContractNo: boolean = false;
    isCreateNewCommercial: boolean = false;
    isDuplicateContract: boolean = false;
    statusContract: boolean = false;
    isChangeSaleMan: boolean = false;

    // salesmanId: AbstractControl;
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
    creditCurrency: AbstractControl;
    partnerIds: AbstractControl;
    creditLimit: AbstractControl;
    trialCreditLimit: AbstractControl;
    autoExtendDays: AbstractControl;
    noDue: AbstractControl;
    emailAddress: AbstractControl;
    paymentTermObh: AbstractControl;
    paymentTerm: AbstractControl;
    firstShipmentDate: AbstractControl;
    creditLimitRate: AbstractControl;

    minDateEffective: any = null;
    minDateExpired: any = null;
    minDateExpiredTrial: any = null;


    partnerId: string = null;
    users: any[] = [];
    companies: Company[] = [];
    contracts: Contract[] = [];

    offices: CommonInterface.INg2Select[] = [];
    listOffice: Office[] = [];
    activeServices: any = [];
    activeVas: any = [];
    activeOffice: any = [];

    selectedContract: Contract = new Contract();
    contract: Contract = new Contract();

    idContract: string = SystemConstants.EMPTY_GUID;
    type: string = '';
    contractTypeDetail: string = '';
    confirmChangeAgreementTypeText: string = '';
    partnerLocation: string = '';
    isChangeAgrmentType: boolean = false;
    status: boolean = false;
    isAllowActiveContract: boolean = false;
    isDisabledExpiredDateField: boolean = false;

    indexDetailContract: number = null;

    fileToUpload: File = null;
    fileList: any = null;
    files: any = {};
    selectedFile: any = {};

    menuSpecialPermission: Observable<any[]>;
    listCurrency: Observable<CommonInterface.INg2Select[]>;

    contractTypes: Array<string> = ["Trial", "Official", "Parent Contract", "Cash", "Guarantee", "Prepaid"];
    serviceTypes: CommonInterface.INg2Select[] = [
        { id: "All", text: "All" },
        { id: "AI", text: "Air Import" },
        { id: "AE", text: "Air Export" },
        { id: "SCE", text: "Sea Consol Export" },
        { id: "SCI", text: "Sea Consol Import" },
        { id: "SFE", text: "Sea FCL Export" },
        { id: "SLE", text: "Sea LCL Export" },
        { id: "SFI", text: "Sea FCL Import" },
        { id: "SLI", text: "Sea LCL Import" },
        { id: "CL", text: "Custom Logistic" },
        { id: "TK", text: "Trucking Inland" }
    ];


    paymentMethods: Array<string> = ["All", "Collect", "Prepaid"];

    basesOn: Array<string> = ["Invoice Date", "Confirmed Billing"];

    selectedSalesman: any = {};
    selectedSalesmanData: any = null;
    displayFieldSalesman: CommonInterface.IComboGridDisplayField[] = [
        { field: 'userName', label: 'User Name' },
        { field: 'employeeNameVn', label: 'Full Name' },
        { field: 'userGroupName', label: 'Group' },
        { field: 'userDeparmentName', label: 'Department' }
    ];
    selectedDisplaySalesman = ['userName'];
    salesmanName: string = '';
    vaslst: CommonInterface.INg2Select[] = this.serviceTypes;
    isCollapsed: boolean = false;
    isCustomerRequest: boolean = false;

    constructor(
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _systemFileManageRepo: SystemFileManageRepo,
        private _catalogueRepo: CatalogueRepo,
        protected _router: Router,
        protected _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        protected _activeRoute: ActivatedRoute,
        private _store: Store<IAppState>,

    ) {
        super();
        this._progressRef = this._ngProgressService.ref();

    }

    ngOnInit() {
        this._store.select(getDetailPartnerDataState).subscribe(res => this.detailPartner = res)
        this._store.select(getCurrentUserState).pipe(takeUntil(this.ngUnsubscribe)).subscribe((res) => {
            if (!!res) {
                this.currentUser = res;
            }
        })
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this._store.dispatch(new GetCatalogueCurrencyAction());
        this.listCurrency = this._store.select(getCatalogueCurrencyState).pipe(map(data => this.utility.prepareNg2SelectData(data, 'id', 'id')));
        this.initForm();
        this.initDataForm();
        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL, null);
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
                    this.isAllowActiveContract = res[0]?.isAllow;
                }

            });

        this._activeRoute.data.subscribe((result: { name: string, type: string }) => {
            this.type = result.type;
        });

        // this.formGroup.get("effectiveDate").valueChanges
        //     .pipe(
        //         distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
        //         map((data: any) => data.startDate)
        //     )
        //     .subscribe((value: any) => {
        //         // this.minDateExpired = this.createMoment(value); // * Update MinDate -> ExpiredDate.
        //         console.log(value);
        //     });

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
            companyId: [null, Validators.required],
            partnerId: [null],
            officeId: [null, Validators.required],
            contractNo: [null, Validators.maxLength(50)],
            effectiveDate: [null, Validators.required],
            expiredDate: [null, Validators.compose([
                Validators.required,
                this.checkExpiredDate
            ])],
            contractType: [null, Validators.required],
            saleService: [null, Validators.required],
            paymentMethod: ['All'],
            vas: [],
            trialEffectDate: [],
            trialExpiredDate: [],
            trialCreditLimit: [],
            trialCreditDays: [null, Validators.compose([
                Validators.min(0),
                Validators.max(365),
                Validators.maxLength(3)
            ])],
            paymentTerm: [null, Validators.compose([
                Validators.min(0),
                Validators.max(365)
            ])],
            baseOn: [null],
            creditLimit: [],
            creditLimitRate: [null, Validators.compose([
                Validators.min(0),
                Validators.max(SystemConstants.MAX_NUMBER_INT)
            ])],
            debitAmount: [],
            billingAmount: [],
            paidAmount: [],
            unpaidAmount: [],
            customerAdvanceAmountVnd: [],
            customerAdvanceAmountUsd: [],
            creditRate: [],
            description: [],
            currencyId: [null, Validators.required],
            creditCurrency: [null, Validators.required],
            creditUnlimited: [],
            autoExtendDays: [],
            noDue: [],
            shipmentType: [],
            emailAddress: [null],
            firstShipmentDate: [null],
            paymentTermObh: [null, Validators.compose([
                Validators.min(0),
                Validators.max(365)
            ])],
        });
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
        this.partnerIds = this.formGroup.controls['partnerId'];
        this.creditCurrency = this.formGroup.controls['creditCurrency'];
        this.creditLimit = this.formGroup.controls['creditLimit'];
        this.trialCreditLimit = this.formGroup.controls['trialCreditLimit'];
        this.autoExtendDays = this.formGroup.controls['autoExtendDays'];
        this.noDue = this.formGroup.controls['noDue'];
        this.emailAddress = this.formGroup.controls['emailAddress'];
        this.firstShipmentDate = this.formGroup.controls['firstShipmentDate'];
        this.paymentTermObh = this.formGroup.controls['paymentTermObh'];
        this.paymentTerm = this.formGroup.controls['paymentTerm'];
        this.creditLimitRate = this.formGroup.controls['creditLimitRate'];
    }

    initDataForm() {
        this.getUsers();
        this.getCompanies();
        this.getOffices();
    }

    getUsers() {
        this._systemRepo.getUserActiveInfo()
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
        this._systemRepo.queryOffices({ officeType: ['Head', 'Branch', 'Repo'] })
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: Office[]) => {
                    if (!!res) {
                        this.listOffice = res;
                        this.offices = this.utility.prepareNg2SelectData(res || [], 'id', 'shortName');
                        this.offices = [{ id: 'All', text: 'All' }, ...this.offices];
                    }
                },
            );
    }

    onSelectedDataFormInfo($event, type: string) {
        switch (type) {
            case 'salesman':
                const data = $event;
                if (!data.userGroupId) {
                    this.selectedSalesman = { field: 'userId', value: data.userId };
                } else {
                    this.selectedSalesman = { field: 'id', value: data.userId + '-' + data.userGroupId + '-' + data.userDeparmentId };
                    if (!!data.userId && data.userId !== this.selectedContract.saleManId) {
                        this.isChangeSaleMan = true;
                    }
                }
                this.selectedSalesmanData = data;
                break;
            case 'company':
                this.companyId.setValue($event.id);
                break;
            case 'office':
                this.officeId.setValue($event.id);
                break;
            case 'partner':
                this.partnerIds.setValue($event.id);
                break;
            case 'noDue':
                break;
        }
    }

    handleFileInput(event: any) {
        this.fileList = event.target['files'];
        if (this.isUpdate && !this.isCreateNewCommercial) {
            if (!!this.files && !!this.files.id && this.fileList.length > 0) {
                this.deleteFileContract();
            } else {
                this.uploadFileContract(this.selectedContract.id);
            }
        }
    }

    uploadFileContract(id: string) {
        this._systemFileManageRepo.uploadFile('Catalogue', 'CatContract', id, this.fileList)
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
        // this._catalogueRepo.getContractFilesAttach(this.partnerId, this.selectedContract.id).
        //     pipe(catchError(this.catchError), finalize(() => {
        //         this._progressRef.complete();
        //         this.isLoading = false;
        //     }))
        //     .subscribe(
        //         (res: any = []) => {
        //             this.files = res;
        //         }
        //     );
        this._systemFileManageRepo.getFile('Catalogue', 'CatContract', this.selectedContract.id).
            pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: any = []) => {
                    this.files = res;
                }
            );

    }

    deleteFileContract() {
        this._progressRef.start();
        // this._catalogueRepo.deleteContractFilesAttach(this.files.id)
        //     .pipe(catchError(this.catchError), finalize(() => {
        //         this._progressRef.complete();
        //         this.isLoading = false;
        //     }))
        //     .subscribe(
        //         (res: any) => {
        //             if (res.result.success) {
        //                 this.uploadFileContract(this.selectedContract.id);
        //             } else {
        //                 this._toastService.error("some thing wrong");
        //             }
        //         }
        //     );
        this._systemFileManageRepo.deleteFile('Catalogue', 'CatContract', this.selectedContract.id, this.files.id)
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
            case 'salesmanId':
                this.selectedSalesman = null;
                this.selectedSalesmanData = null;
                break;
            case 'company': this.companyId.setValue(null);
                break;
            case 'office': this.officeId.setValue(null);
                break;
            case 'partner': this.partnerIds.setValue(null);
                break;
            case 'effectiveDate': this.effectiveDate.setValue(null);
                break;
            case 'expiredDate': this.expiredDate.setValue(null);
                break;
        }
    }

    checkSubmitData() {
        if ((this.effectiveDate.value == null || (!this.effectiveDate.value.startDate || this.effectiveDate.value.startDate == null)) ||
            (this.contractType.value !== 'Cash' && this.contractType.value !== 'Guarantee' && this.contractType.value !== 'Prepaid' && (this.expiredDate.value == null || (!this.expiredDate.value.startDate || this.expiredDate.value.startDate == null)))) {
            return false;
        }
        if (this.contractTypeDetail == 'Prepaid' && this.contractType.value != this.contractTypeDetail || this.selectedContract.debitAmount > 0 && this.contractTypeDetail != 'Prepaid' && this.contractType.value == 'Prepaid') {
            this._toastService.error('Cannot change agreement type!');
            this.contractType.setValue(this.contractTypeDetail);
            return false;
        }
        if (!!this.contractType.value && this.contractType.value.length > 0) {
            if (this.contractType.value === this.contractTypes[1] && !this.contractNo.value) {
                this.isRequiredContractNo = true;
                return false;
            } else {
                this.isRequiredContractNo = false;
            }
        }
        if (!this.saleService.value) {
            return false;
        }

        if (!this.selectedSalesmanData) {
            return false;
        }

        return true;
    }


    onSubmit(isRequestApproval: boolean = false) {
        this.setError(this.vas);
        this.setError(this.paymentMethod);
        this.setError(this.currencyId);
        this.isSubmitted = true;
        this.selectedContract.index = this.indexDetailContract;
        if (!this.checkSubmitData()) {
            return;
        }

        if (this.formGroup.valid) {
            const objCheckContract = !!this.contractNo.value && this.contracts.length >= 1 ? this.contracts.filter(x => x.contractNo === this.contractNo.value && x.contractType === "Official").length > 1 : null;
            if (objCheckContract) {
                this.isDuplicateContract = true;
                this._toastService.error('Contract no has been existed!');
                return;
            }
            this.assignValueToModel();
            if (this.isCustomerRequest === true) {
                if (!this.partnerIds.value) return;
                this.selectedContract.partnerId = this.partnerIds.value;
                this.selectedContract.isRequestApproval = isRequestApproval;
                this._catalogueRepo.customerRequest(this.selectedContract)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);
                                if (!!this.fileList) {
                                    this.partnerId = this.selectedContract.partnerId;
                                    this.uploadFileContract(res.data);
                                }
                                this.onRequest.emit(true);
                                this.formGroup.reset();
                                this.hide();
                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
                return;
            }
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
                                this.hide();
                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
            } else if (this.isUpdate && !this.isCreateNewCommercial) {
                const body = new Contract(this.selectedContract);
                if (this.contractTypeDetail !== this.contractType.value && this.selectedContract.active === true && this.isAllowActiveContract === false) { //&& this.isChangeAgrmentType === false && this.isAllowActiveContract === false) {
                    this.status = this.statusContract;
                    this.confirmChangeAgreementTypeText = "You have changed Agreement type "
                        + this.contractTypeDetail + " to "
                        + this.selectedContract.contractType
                        + ", So your agreement will be inactive and request approval again. do you want to change it?";
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
                this.selectedContract.username = this.users.find(x => x.userId === this.selectedContract.saleManId).userName;
                if (this.selectedContract.officeId.includes(';')) {
                    const arrayOffice = this.selectedContract.officeId.split(';');
                    this.selectedContract.officeNameAbbr = '';
                    arrayOffice.forEach(itemOffice => {
                        this.selectedContract.officeNameAbbr += this.offices.find(x => x.id === itemOffice).text + "; ";
                    });
                    if (this.selectedContract.officeNameAbbr.charAt(this.selectedContract.officeNameAbbr.length - 2) === ';') {
                        this.selectedContract.officeNameAbbr = this.selectedContract.officeNameAbbr.substr(0, this.selectedContract.officeNameAbbr.length - 2);
                    }
                } else {
                    this.selectedContract.officeId = this.selectedContract.officeId.toLowerCase();
                    const obj = this.offices.find(x => x.id === this.selectedContract.officeId);
                    this.selectedContract.officeNameAbbr = !!obj ? obj.text : null;
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
                this.onRequest.emit(this.selectedContract);
            }
        }
    }

    updateContract(body: Contract) {
        this._catalogueRepo.updateContract(body)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.onRequest.emit(this.selectedContract);
                        this.formGroup.reset();
                        this.hide();
                    } else {
                        if (!!res.data.errorCode) {
                            this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                                body: res.message,
                                label: 'OK'
                            }, () => {
                                return;
                            })
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                }
            );
    }

    onSubmitChangeAgreementType() {
        if (this.formGroup.valid) {
            this.assignValueToModel();
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
        // Set salemsman data info
        this.onSelectedDataFormInfo({
            userId: this.selectedContract.saleManId,
            userGroupId: this.selectedContract.salesGroup,
            userDeparmentId: this.selectedContract.salesDepartment, userOfficeId: this.selectedContract.salesOfficeId,
            userCompanyId: this.selectedContract.salesCompanyId
        }, 'salesman');
        this.formGroup.patchValue({
            // salesmanId: !!this.selectedContract.salesGroup ? this.selectedContract.saleManId + '-' + this.selectedContract.salesGroup + '-' + this.selectedContract.salesDepartment : this.selectedContract.saleManId,
            companyId: !!this.selectedContract.companyId ? this.selectedContract.companyId : null,
            officeId: this.activeOffice,
            contractNo: this.selectedContract.contractNo,
            effectiveDate: !!this.selectedContract.effectiveDate ? { startDate: new Date(this.selectedContract.effectiveDate), endDate: new Date(this.selectedContract.effectiveDate) } : null,
            expiredDate: !!this.selectedContract.expiredDate ? { startDate: new Date(this.selectedContract.expiredDate), endDate: new Date(this.selectedContract.expiredDate) } : null,
            contractType: !!this.selectedContract.contractType ? this.selectedContract.contractType : null,
            paymentTerm: this.selectedContract.paymentTerm,
            creditLimit: this.selectedContract.creditLimit,
            creditLimitRate: this.selectedContract.creditLimitRate,
            trialCreditLimit: this.selectedContract.trialCreditLimited,
            trialCreditDays: this.selectedContract.trialCreditDays,
            trialEffectDate: !!this.selectedContract.trialEffectDate ? { startDate: new Date(this.selectedContract.trialEffectDate), endDate: new Date(this.selectedContract.trialEffectDate) } : null,
            trialExpiredDate: !!this.selectedContract.trialExpiredDate ? { startDate: new Date(this.selectedContract.trialExpiredDate), endDate: new Date(this.selectedContract.trialExpiredDate) } : null,
            debitAmount: this.selectedContract.debitAmount,
            billingAmount: this.selectedContract.billingAmount,
            paidAmount: this.selectedContract.paidAmount,
            unpaidAmount: this.selectedContract.unpaidAmount,
            customerAdvanceAmountVnd: this.selectedContract.customerAdvanceAmountVnd,
            customerAdvanceAmountUsd: this.selectedContract.customerAdvanceAmountUsd,
            creditRate: this.selectedContract.creditRate,
            description: this.selectedContract.description,
            saleService: this.activeServices,
            vas: this.activeVas,
            paymentMethod: !!this.selectedContract.paymentMethod ? this.paymentMethods.find(type => type === this.selectedContract.paymentMethod) : null,
            baseOn: !!this.selectedContract.baseOn ? this.basesOn.find(type => type === this.selectedContract.baseOn) : null,
            currencyId: !!this.selectedContract.currencyId ? { id: this.selectedContract.currencyId, text: this.selectedContract.currencyId } : null,
            creditUnlimited: this.selectedContract.creditUnlimited,
            creditCurrency: this.selectedContract.creditCurrency,
            autoExtendDays: this.selectedContract.autoExtendDays,
            noDue: this.selectedContract.noDue,
            shipmentType: this.selectedContract.shipmentType,
            emailAddress: this.selectedContract.emailAddress,
            firstShipmentDate: !!this.selectedContract.firstShipmentDate ? { startDate: new Date(this.selectedContract.firstShipmentDate), endDate: new Date(this.selectedContract.firstShipmentDate) } : null,
            paymentTermObh: this.selectedContract.paymentTermObh,
        });
        this.contractTypeDetail = this.selectedContract.contractType;

        this.formatAutoExtendDays();
        this.minDateExpired = this.createMoment(this.selectedContract?.effectiveDate);
    }

    assignValueToModel() {
        if (this.isUpdate) {
            this.selectedContract.id = this.idContract;
        }
        this.selectedContract.currencyId = !!this.currencyId.value ? !!this.currencyId.value.id ? this.currencyId.value.id : this.currencyId.value : null;
        this.selectedContract.active = this.selectedContract.id !== SystemConstants.EMPTY_GUID ? this.statusContract : false;
        this.selectedContract.saleManId = this.selectedSalesmanData?.userId;
        this.selectedContract.companyId = this.companyId.value;
        this.selectedContract.index = this.indexDetailContract;
        this.selectedContract.contractNo = this.formGroup.controls['contractNo'].value;
        this.selectedContract.effectiveDate = this.effectiveDate.value ? (this.effectiveDate.value.startDate !== null ? formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null;
        this.selectedContract.expiredDate = this.expiredDate.value ? (this.expiredDate.value.startDate !== null ? formatDate(this.expiredDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null;
        this.selectedContract.contractType = !!this.contractType.value ? this.contractType.value : null;
        const services = this.saleService.value ? (this.saleService.value.length > 0 ? this.saleService.value.map((item: any) => item.id).toString().replace(/(?:,)/g, ';') : '') : '';
        this.selectedContract.saleService = services;
        const vass = this.vas.value ? (this.vas.value.length > 0 ? this.vas.value.map((item: any) => item.id).toString().replace(/(?:,)/g, ';') : '') : '';
        this.selectedContract.vas = vass;
        const offices = this.officeId.value ? (this.officeId.value.length > 0 ? this.officeId.value.map((item: any) => item.id).toString().replace(/(?:,)/g, ';') : '') : '';
        this.selectedContract.officeId = offices;
        this.selectedContract.paymentMethod = !!this.paymentMethod.value ? this.paymentMethod.value : null;
        this.selectedContract.baseOn = !!this.baseOn.value ? this.baseOn.value : "Invoice Date";
        this.selectedContract.trialCreditLimited = !!this.formGroup.controls['trialCreditLimit'].value ? this.formGroup.controls['trialCreditLimit'].value :
            this.formGroup.controls['creditLimit'].value;
        this.selectedContract.trialCreditDays = this.formGroup.controls['trialCreditDays'].value;
        if (this.officeId.value[0].id === 'All') {
            this.selectedContract.officeId = this.mapOfficeId();
        }
        if (this.saleService.value[0].id === 'All') {
            this.selectedContract.saleService = this.mapServiceId();
        }
        if (this.vas.value != null && this.vas.value.length > 0 && this.vas.value[0].id === 'All') {
            this.selectedContract.vas = this.mapVas();
        }
        if (this.contractType.value === 'Trial' && this.selectedContract.active === false) {
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
                const date2: any = new Date(this.selectedContract.expiredDate);
                const date1: any = new Date(this.selectedContract.effectiveDate);
                const diffTime = Math.abs(date1 - date2);
                const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                this.trialCreditDays.setValue(diffDays);
            }
        }
        if (this.contractType.value === 'Official') {
            this.trialEffectDate.setValue(null);
            this.trialCreditDays.setValue(null);
            this.trialExpiredDate.setValue(null);
        }
        this.selectedContract.trialEffectDate = !!this.trialEffectDate.value && !!this.trialEffectDate.value.startDate ? formatDate(this.trialEffectDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.selectedContract.trialExpiredDate = !!this.trialExpiredDate.value && !!this.trialExpiredDate.value.startDate ? formatDate(this.trialExpiredDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.selectedContract.paymentTerm = this.formGroup.controls['paymentTerm'].value;
        this.selectedContract.creditLimit = !!this.formGroup.controls['creditLimit'].value ? this.formGroup.controls['creditLimit'].value :
            this.formGroup.controls['trialCreditLimit'].value;
        this.selectedContract.creditLimitRate = this.formGroup.controls['creditLimitRate'].value;
        this.selectedContract.debitAmount = this.formGroup.controls['debitAmount'].value;
        this.selectedContract.billingAmount = this.formGroup.controls['billingAmount'].value;
        this.selectedContract.paidAmount = this.formGroup.controls['paidAmount'].value;
        this.selectedContract.unpaidAmount = this.formGroup.controls['unpaidAmount'].value;
        this.selectedContract.customerAdvanceAmountVnd = this.formGroup.controls['customerAdvanceAmountVnd'].value;
        this.selectedContract.customerAdvanceAmountUsd = this.formGroup.controls['customerAdvanceAmountUsd'].value;
        this.selectedContract.creditRate = this.formGroup.controls['creditRate'].value;
        this.selectedContract.description = this.formGroup.controls['description'].value;
        this.selectedContract.creditUnlimited = this.formGroup.controls['creditUnlimited'].value;
        this.selectedContract.trialCreditDays = this.trialCreditDays.value;
        this.selectedContract.partnerId = this.partnerId;
        this.selectedContract.creditCurrency = !!this.creditCurrency.value ? (!!this.creditCurrency.value.id ? this.creditCurrency.value.id : this.creditCurrency.value) : this.selectedContract.currencyId;
        this.selectedContract.autoExtendDays = this.autoExtendDays.value;
        this.selectedContract.noDue = this.noDue.value;
        this.selectedContract.salesGroup = this.selectedSalesmanData?.userGroupId;
        this.selectedContract.salesDepartment = this.selectedSalesmanData?.userDeparmentId;
        this.selectedContract.salesOfficeId = this.selectedSalesmanData?.userOfficeId;
        this.selectedContract.salesCompanyId = this.selectedSalesmanData?.userCompanyId;
        this.selectedContract.shipmentType = this.formGroup.controls['shipmentType'].value;
        this.selectedContract.emailAddress = this.formGroup.controls['emailAddress'].value;
        this.selectedContract.firstShipmentDate = !!this.firstShipmentDate.value && this.firstShipmentDate.value.startDate ? formatDate(this.firstShipmentDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.selectedContract.paymentTermObh = this.formGroup.controls['paymentTermObh'].value;
    }

    activeInactiveContract(id: string) {
        if (this.contractType.value === 'Guaranteed'
            && ((this.formGroup.controls['creditLimit'].value <= 0
                || !this.formGroup.controls['creditLimit'].value)
                && this.selectedContract.active === false)) {
            this.salesmanCreditLimitPopup.show();
            return;
        }
        if (this.selectedContract.active === false) {
            this._catalogueRepo.checkExistedContractActive(id, this.partnerId).pipe(
                catchError(this.catchError)
            ).subscribe(
                (res: boolean) => {
                    if (res === true) {
                        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                            body: 'There are Other Agreement that same service, If Agreement is activated, Those Agreement will be Inactive. Are You Sure Active this agreement ?',
                            label: 'OK'
                        }, () => {
                            this.processActiveInActiveContract(id);
                        });
                    }
                    else {
                        this.processActiveInActiveContract(id);
                    }
                }
            );
        }
        else {
            this.processActiveInActiveContract(id);
        }
    }

    onSalesmanCreditRequest($event: any) {
        const data = $event;
        if (!!data.creditRate || data.creditLimit) {
            this.processActiveInActiveContract(this.selectedContract.id, data);
            this.selectedContract.creditLimit = data.creditLimit;
            this.selectedContract.creditLimitRate = data.creditRate;
            this.formGroup.controls['creditLimit'].setValue(this.selectedContract.creditLimit);
            this.formGroup.controls['creditLimitRate'].setValue(this.selectedContract.creditLimitRate);
        }
    }

    processActiveInActiveContract(id: string, bodyCredit?: any) {
        this._progressRef.start();
        this._catalogueRepo.activeInactiveContract(id, this.partnerId, bodyCredit)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.selectedContract.active = !this.selectedContract.active;
                        this.statusContract = this.selectedContract.active;
                        const message = this.selectedContract.active ? 'Active success !!' : 'Inactive success !!';
                        if (this.selectedContract.active) {
                            const action = !!this.detailPartner.sysMappingId ? 'UPDATE' : 'ADD';
                            this.syncPartnerToAccountantSystem([{ Id: this.partnerId, action }]);
                        }
                        this._toastService.success(message);
                        this.onRequest.emit(this.selectedContract);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }
    syncPartnerToAccountantSystem(partnerRequest: any[]) {
        this._catalogueRepo.syncPartnerToAccountantSystem(partnerRequest).pipe(
            catchError(this.catchError),
            concatMap((res: CommonInterface.IResult) => {
                const { status, message } = res;
                if (status) {
                    this._toastService.success(message);
                } else {
                    this._toastService.error(message);
                }
                return this._catalogueRepo.getDetailPartner(this.detailPartner.id);
            })
        ).subscribe(
            (res: Partner) => {
                this.detailPartner.sysMappingId = res.sysMappingId;
            },
            (error) => {
                console.log(error);
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
        if (!!this.trialEffectDate.value && !!this.trialEffectDate.value.startDate) {
            this.effectiveDate.setValue({
                startDate: new Date(new Date(value.startDate).setDate(new Date(value.startDate).getDate() + trialDays)),
                endDate: new Date(new Date(value.endDate).setDate(new Date(value.endDate).getDate() + trialDays)),
            });
        }
    }
    onUpdateEffectiveDate(value: { startDate: any; endDate: any }) {
        if (!!value.startDate && this.contractType.value === 'Trial') {
            this.expiredDate.setValue({
                startDate: new Date(new Date(value.startDate).setDate(new Date(value.startDate).getDate() + 30)),
                endDate: new Date(new Date(value.endDate).setDate(new Date(value.endDate).getDate() + 30)),
            });
        }
    }
    checkExpiredDate: ValidatorFn = (): { [key: string]: any; } | null => {
        if (this.contractType?.value === 'Trial') {
            let effDate = this.formGroup && this.formGroup.get('effectiveDate').value;
            let expDate = this.formGroup && this.formGroup.get('expiredDate').value;
            let expDateValid = null;
            let expDate1 = null;
            let checkError = false;
            if (!!effDate) {
                expDateValid = new Date(new Date(effDate.startDate).setDate(new Date(effDate.startDate)?.getDate() + 30));
                expDate1 = new Date(new Date(expDate.startDate));
            }
            const date2: any = new Date(expDateValid).valueOf();
            const date1: any = new Date(expDate1).valueOf();
            if (!!expDateValid) {
                if (date1 > date2)
                    checkError = true;
            }
            return checkError ? { invalidRange: { effDate, expDate } } : null;
        }
        return null;
    }

    selectedService($event: any) {
        if ($event.length > 0) {
            if ($event[$event.length - 1].id === 'All') {
                this.saleService.setValue([{ id: 'All', text: 'All' }]);
            } else {
                const arrNotIncludeAll = $event.filter(x => x.id !== 'All'); //
                this.saleService.setValue(arrNotIncludeAll);
            }
        }
    }

    selectedVas($event: any) {
        if ($event.length > 0) {
            if ($event[$event.length - 1].id === 'All') {
                this.vas.setValue([{ id: 'All', text: 'All' }]);
            } else {
                const arrNotIncludeAll = $event.filter(x => x.id !== 'All'); //
                this.vas.setValue(arrNotIncludeAll);
            }
        }

    }

    selectedOffice($event: any) {
        if ($event.length > 0) {
            if ($event[$event.length - 1].id === 'All') {
                this.officeId.setValue([{ id: 'All', text: 'All' }]);
            } else {
                const arrNotIncludeAll = $event.filter(x => x.id !== 'All'); //
                this.officeId.setValue(arrNotIncludeAll);
            }
        }
    }

    selectedAgreementType($event: any) {
        switch ($event) {
            case 'Trial':
                if (!!this.effectiveDate.value.startDate) {
                    this.expiredDate.setValue({
                        startDate: new Date(new Date(this.effectiveDate.value.startDate).setDate(new Date(this.effectiveDate.value.startDate).getDate() + 30)),
                        endDate: new Date(new Date(this.effectiveDate.value.endDate).setDate(new Date(this.effectiveDate.value.endDate).getDate() + 30)),
                    });
                }
                this.formGroup.controls['shipmentType'].setValue('Freehand & Nominated');
                this.formGroup.controls['paymentTerm'].setValue(30);
                this.formGroup.get('expiredDate').setValidators([
                    this.checkExpiredDate
                ])
                break;
            case 'Guarantee':
                if (this.isCreateNewCommercial) {
                    this.formGroup.controls['creditLimit'].setValue(20000000);
                    this.formGroup.controls['creditLimitRate'].setValue(120);
                } else {
                    this.isDisabledExpiredDateField = false;
                }
                this.formGroup.controls['paymentTerm'].setValue(1);
                this.formGroup.controls['shipmentType'].setValue('Freehand & Nominated');
                this.formGroup.controls['creditCurrency'].setValue("VND");
                this.formGroup.controls['currencyId'].setValue("VND");
                this.formGroup.controls['expiredDate'].setErrors(null);
                break;
            case 'Cash':
                this.formGroup.controls['paymentTerm'].setValue(1);
                this.formGroup.controls['shipmentType'].setValue(JobConstants.COMMON_DATA.SHIPMENTTYPES[1]);
                this.formGroup.controls['expiredDate'].setErrors(null);
                break;
            case 'Official':
                this.formGroup.controls['paymentTerm'].setValue(30);
                this.formGroup.controls['expiredDate'].setErrors(null);
                break;
            case 'Prepaid':
            case 'Parent Contract':
                this.formGroup.controls['paymentTerm'].setValue(1);
                this.formGroup.controls['expiredDate'].setErrors(null);
                break;
            default:
                this.formGroup.controls['shipmentType'].setValue('Freehand & Nominated');
                break;
        }

    }

    mapOfficeId() {
        let officeId = '';
        const off = this.listOffice.filter((office: Office) => ['Head', 'Branch'].includes(office?.officeType));
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
                        this.onRequest.emit(this.selectedContract);
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
        this.selectedSalesmanData = null;
        this.isChangeSaleMan = null;
        this.formGroup.reset()
        this.hide();
    }

    formatAutoExtendDays() {
        let num = this.autoExtendDays.value;
        if (num >= 0) {
            this.autoExtendDays.setValue(((Math.round(num * 100) / 100).toFixed(2)));
        } else {
            this.autoExtendDays.setValue(((0).toFixed(2)));
        }
    }

    deleteFile(file: any) {
        if (!!file) {
            this.selectedFile = file;
        }
        this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Are you sure to delete this file ?',
            label: 'OK'
        }, () => {
            this.onDeleteFile();
        })
    }

    onDeleteFile() {
        this._systemFileManageRepo.deleteFile('Catalogue', 'CatContract', this.selectedContract.id, this.selectedFile.name)
            .pipe(catchError(this.catchError), finalize(() => {
                this.isLoading = false;
            }))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success("File deleted successfully!");
                        this.getFileContract();
                    } else {
                        this._toastService.error("Something wrong");
                    }
                }
            );
    }
}
