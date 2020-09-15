
import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { SortService } from 'src/app/shared/services/sort.service';
import { Saleman } from 'src/app/shared/models/catalogue/saleman.model';
import { SalemanAdd } from 'src/app/shared/models/catalogue/salemanadd.model';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { catchError, finalize } from "rxjs/operators";
import { AppList } from 'src/app/app.list';
import { ToastrService } from 'ngx-toastr';
import { SalemanPopupComponent } from '../components/saleman-popup.component';
import { forkJoin } from 'rxjs';
import { FormAddPartnerComponent } from '../components/form-add-partner/form-add-partner.component';
import { NgProgress } from '@ngx-progressbar/core';
import { SystemConstants } from 'src/constants/system.const';
import { Company } from '@models';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';
import { FormContractCommercialPopupComponent } from 'src/app/business-modules/share-commercial-catalogue/components/form-contract-commercial-catalogue.popup';
import { CommercialContractListComponent } from 'src/app/business-modules/commercial/components/contract/commercial-contract-list.component';
import _merge from 'lodash/merge';


@Component({
    selector: 'app-partner-detail',
    templateUrl: './detail-partner.component.html',
    styleUrls: ['./detail-partner.component.scss']
})
export class PartnerDetailComponent extends AppList {
    @ViewChild(FormAddPartnerComponent, { static: false }) formPartnerComponent: FormAddPartnerComponent;
    @ViewChild("popupDeleteSaleman", { static: false }) confirmDeleteSalemanPopup: ConfirmPopupComponent;
    @ViewChild("popupDeleteContract", { static: false }) confirmDeleteContract: ConfirmPopupComponent;

    @ViewChild("popupDeletePartner", { static: false }) confirmDeletePartnerPopup: ConfirmPopupComponent;
    @ViewChild('internalReferenceConfirmPopup', { static: false }) confirmTaxcode: ConfirmPopupComponent;
    @ViewChild('duplicatePartnerPopup', { static: false }) confirmDuplicatePartner: InfoPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild(SalemanPopupComponent, { static: false }) poupSaleman: SalemanPopupComponent;
    @ViewChild(FormContractCommercialPopupComponent, { static: false }) formContractPopup: FormContractCommercialPopupComponent;
    @ViewChild(CommercialContractListComponent, { static: false }) listContract: CommercialContractListComponent;

    contracts: Contract[] = [];
    selectedContract: Contract = new Contract();
    contract: Contract = new Contract();

    indexToRemove: number = 0;
    indexlstContract: number = null;

    saleMans = [];
    activeNg: boolean = true;
    partner: Partner = new Partner();
    partnerGroupActives: any = [];
    partnerType: any;
    saleMandetail: any[] = [];
    headerSaleman: CommonInterface.IHeaderTable[];
    services: any[] = [];
    status: CommonInterface.ICommonTitleValue[] = [];
    offices: any[] = [];
    salemanToAdd: SalemanAdd = new SalemanAdd();
    strOfficeCurrent: any = '';
    strSalemanCurrent: any = '';
    selectedStatus: any = {};
    selectedService: any = {};
    deleteMessage: string = '';
    selectedSaleman: Saleman = null;
    saleMantoView: Saleman = new Saleman();
    dataSearchSaleman: any = {};
    isShowSaleMan: boolean = false;
    index: number = 0;
    isExistedTaxcode: boolean = false;
    currenctUser: any = '';
    company: Company[] = [];
    salemansId: string = null;

    list: any[] = [];

    isDup: boolean = false;

    constructor(private route: ActivatedRoute,
        private router: Router,
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService,
        private toastr: ToastrService,
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService,
        private _cd: ChangeDetectorRef,
        private _activedRoute: ActivatedRoute,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortLocal;
    }

    sortLocal(sort: string): void {
        this.contracts = this._sortService.sort(this.contracts, sort, this.order);
    }

    ngOnInit() {

        this.getComboboxDataSaleman();
        this.initHeaderSalemanTable();
        this.route.params.subscribe((prams: any) => {
            if (!!prams.id) {
                this.partner.id = prams.id;
                this.getListContract(this.partner.id);
            }
        });
        this.getDataCombobox();
        const claim = localStorage.getItem(SystemConstants.USER_CLAIMS);
        this.currenctUser = JSON.parse(claim)["id"];
    }
    ngAfterViewInit() {
        this.formPartnerComponent.isUpdate = true;

        this._cd.detectChanges();
    }


    // getDetailCustomer(partnerId: string) {
    //     this._catalogueRepo.getDetailPartner(partnerId)
    //         .subscribe(
    //             (res: Partner) => {
    //                 this.partner = res;
    //                 console.log("detail partner:", this.partner);
    //                 //this.formPartnerComponent.formGroup.patchValue(res);
    //                 this.formPartnerComponent.getShippingProvinces(res.countryShippingId);
    //                 this.formPartnerComponent.getBillingProvinces(res.countryId);
    //                 console.log("flag: ", this.formPartnerComponent.activePartner);

    //             }
    //         );
    // }

    RequireSaleman(partnerGroup: string): boolean {
        if (partnerGroup != null) {
            if (partnerGroup.includes('CUSTOMER') || partnerGroup.includes('ALL')) {
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }
    getParnerDetails() {
        this._progressRef.start();
        this._catalogueRepo.getDetailPartner(this.partner.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.partner = res;
                        this.formPartnerComponent.groups = this.partner.partnerGroup;
                        // this.isShowSaleMan = this.checkRequireSaleman(this.partner.partnerGroup);
                        console.log("res: ", res);
                        this.formPartnerComponent.setFormData(this.partner);
                        console.log(this.partner.partnerMode);
                        if (this.partner.partnerMode === 'External') {
                            this.formPartnerComponent.isDisabledInternalCode = true;
                        }

                        this.formPartnerComponent.activePartner = this.partner.active;
                    }
                }
            );

    }
    checkRequireSaleman(partnerGroup: string): boolean {
        if (partnerGroup != null) {
            if (partnerGroup.includes('CUSTOMER') || partnerGroup.includes('ALL')) {
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }


    getSalemanPagingByPartnerId(dataSearchSaleman?: any) {
        this.isLoading = true;
        this._catalogueRepo.getListSaleManDetail(Object.assign({}, dataSearchSaleman, { partnerId: this.partner.id }))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; })
            ).subscribe(
                (res: any) => {
                    if (!!res) {
                        console.log(res);
                        this.saleMandetail = (res || []).map((item: Saleman) => new Saleman(item));
                        if (this.saleMandetail.length > 0) {
                            for (const it of this.saleMandetail) {
                                this.services.forEach(item => {
                                    if (it.service === item.id) {
                                        it.serviceName = item.text;
                                    }
                                });
                                this.offices.forEach(item => {
                                    if (it.office === item.id) {
                                        it.officeName = item.branchNameEn;
                                    }
                                    if (it.company === item.buid) {
                                        const objCompany = this.company.find(x => x.id === item.buid);
                                        it.companyName = objCompany.bunameAbbr;
                                    }
                                });
                            }
                        }
                    }

                },
            );
    }

    initHeaderSalemanTable() {
        this.headerSaleman = [
            { title: 'Salesman', field: 'username', sortable: true },
            { title: 'Contract No', field: 'username', sortable: true },
            { title: 'Contract Type', field: 'username', sortable: true },
            { title: 'Service', field: 'username', sortable: true },
            { title: 'Effective Date', field: 'username', sortable: true },
            { title: 'Expired Date', field: 'username', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'Office', field: 'officeName', sortable: true },
            { title: 'Company', field: 'companyName', sortable: true },
        ];
    }
    closePopupSaleman(param: SalemanAdd) {
        this.salemanToAdd = param;
        this.salemanToAdd.partnerId = this.partner.id;
        this.poupSaleman.isDetail = false;
        this.isDup = this.saleMandetail.some((saleMane: Saleman) => (saleMane.service === this.salemanToAdd.service && saleMane.office === this.salemanToAdd.office));

        if (this.isDup) {
            for (const it of this.saleMandetail) {
                const index = this.services.findIndex(x => x.id === it.service);
                if (index > -1) {
                    it.serviceName = this.services[index].text;
                }
            }
        }


        if (this.salemanToAdd.service !== null && this.salemanToAdd.office !== null) {
            this._catalogueRepo.checkExistedSaleman(this.salemanToAdd)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (!!res) {
                            if (this.isDup) {
                                console.log("dup");
                                this.toastr.error('Duplicate service, office with sale man!');
                            } else {
                                // this.saleMandetail.push(this.salemanToAdd);
                                this.saleMandetail = [...this.saleMandetail, this.salemanToAdd];
                                this.poupSaleman.hide();
                                for (const it of this.saleMandetail) {

                                    this.services.forEach(item => {
                                        if (it.service === item.id) {
                                            it.serviceName = item.text;
                                        }
                                    });
                                    this.offices.forEach(item => {
                                        if (it.office === item.id) {
                                            it.officeName = item.branchNameEn;
                                        }
                                        if (it.company === item.buid) {
                                            const objCompany = this.company.find(x => x.id === item.buid);
                                            it.companyName = objCompany.bunameAbbr;
                                        }
                                    });
                                }


                            }
                        }

                    },
                );
        }


    }
    closeppAndDeleteSaleman(index: any) {
        this.index = index;
        const id = this.saleMandetail[index].id;
        this.deleteSaleman(this.index, id);
    }

    showPopupSaleman() {
        this.poupSaleman.isSave = false;
        this.poupSaleman.isDetail = false;
        this.poupSaleman.resetForm();
        this.poupSaleman.show();
    }

    onDeleteSaleman() {
        if (this.saleMandetail.length === 1) {
            this._toastService.error('Salesman must have one row!');
            this.confirmDeleteSalemanPopup.hide();
            return;
        }
        this.confirmDeleteSalemanPopup.hide();
        if (!!this.salemansId) {
            this._catalogueRepo.deleteContract(this.salemansId, this.partner.id)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.saleMandetail = [...this.saleMandetail.slice(0, this.index), ...this.saleMandetail.slice(this.index + 1)];
                            this.confirmDeleteSalemanPopup.hide();
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        } else {
            if (this.saleMandetail.length > 0) {
                this.saleMandetail = [...this.saleMandetail.slice(0, this.index), ...this.saleMandetail.slice(this.index + 1)];
                if (!this.salemansId) {
                    this._toastService.success('Data delete success!');
                }
            }
        }
    }
    deleteSaleman(index: any, id: string) {
        this.index = index;
        this.salemansId = id;
        this.deleteMessage = `Do you want to delete sale man  ${this.saleMandetail[index].username}?`;
        this.confirmDeleteSalemanPopup.show();
    }
    getDataCombobox() {
        forkJoin([
            this._catalogueRepo.getCountryByLanguage(),
            this._catalogueRepo.getProvinces(),
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.ALL),
            this._catalogueRepo.getPartnerGroup(),
            this._systemRepo.getAllOffice(),
            this._catalogueRepo.getPartnerCharge(this.partner.id)
        ])
            .pipe(catchError(this.catchError))
            .subscribe(
                ([countries, provinces, customers, partnerGroups, workPlaces, partnerCharge]) => {
                    this.formPartnerComponent.countries = this.utility.prepareNg2SelectData(countries || [], 'id', 'name');
                    this.formPartnerComponent.billingProvinces = this.utility.prepareNg2SelectData(provinces || [], 'id', 'name_VN');
                    this.formPartnerComponent.shippingProvinces = this.utility.prepareNg2SelectData(provinces || [], 'id', 'name_VN');
                    // this.formPartnerComponent.parentCustomers = this.utility.prepareNg2SelectData(customers || [], 'id', 'partnerNameVn');
                    this.formPartnerComponent.parentCustomers = customers;
                    this.formPartnerComponent.partnerGroups = this.utility.prepareNg2SelectData(partnerGroups || [], 'id', 'id');
                    this.getPartnerGroupActive(this.partnerType);
                    this.formPartnerComponent.workPlaces = workPlaces.map(x => ({ "text": x.code + ' - ' + x.branchNameEn, "id": x.id }));
                    this.getParnerDetails();


                    // * Update other charge.
                    this.formPartnerComponent.otherChargePopup.initCharges = partnerCharge || [];
                    this.formPartnerComponent.otherChargePopup.charges = partnerCharge || [];

                },
                () => { },

            );
    }
    getComboboxDataSaleman(): any {
        this.getService();
        this.getOffice();
        this.getCompany();
        this.getStatus();
    }

    getService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                        this.getSalemanPagingByPartnerId(this.dataSearchSaleman);
                    }
                },
            );
    }

    getOffice() {
        this._systemRepo.getListOffices()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.offices = res;
                    }
                },
            );
    }

    getCompany() {
        this._systemRepo.getListCompany()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.company = res;
                    }
                },
            );
    }

    getStatus(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Active', value: true },
            { title: 'Inactive', value: false },
        ];
    }
    getPartnerGroupActive(partnerGroup: any): any {
        if (partnerGroup === PartnerGroupEnum.AGENT) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "AGENT"));
        }
        if (partnerGroup === PartnerGroupEnum.AIRSHIPSUP) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "AIRSHIPSUP"));
        }
        if (partnerGroup === PartnerGroupEnum.CARRIER) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "CARRIER"));
        }
        if (partnerGroup === PartnerGroupEnum.CONSIGNEE) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "CONSIGNEE"));
        }
        if (partnerGroup === PartnerGroupEnum.CUSTOMER) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "CUSTOMER"));
            this.isShowSaleMan = true;
        }
        if (partnerGroup === PartnerGroupEnum.SHIPPER) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "SHIPPER"));
        }
        if (partnerGroup === PartnerGroupEnum.SUPPLIER) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "SUPPLIER"));
        }
        if (partnerGroup === PartnerGroupEnum.STAFF) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "STAFF"));
        }
        if (partnerGroup === PartnerGroupEnum.PERSONAL) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "PERSONAL"));
        }
        if (partnerGroup === PartnerGroupEnum.ALL) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "ALL"));
        }
        if (this.partnerGroupActives.find(x => x.id === "ALL")) {
            this.partner.partnerGroup = 'AGENT;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER;STAFF;PERSONAL';
            this.isShowSaleMan = true;
        }
        this.formPartnerComponent.partnerForm.controls['partnerGroup'].setValue(this.partnerGroupActives);
    }

    onSubmit() {
        // this.partner.saleMans = this.saleMandetail;
        this.formPartnerComponent.isSubmitted = true;

        //const formBody = this.formPartnerComponent.partnerForm.getRawValue();
        //console.log(formBody);
        if (!this.formPartnerComponent.partnerForm.valid) {
            return;
        }
        this.getFormPartnerData();
        console.log("this.partner: ", this.partner);

    }
    getFormPartnerData() {
        const formBody = this.formPartnerComponent.partnerForm.getRawValue();
        this.trimInputForm(formBody);
        this.partner.partnerGroup = !!formBody.partnerGroup ? formBody.partnerGroup[0].id : null;
        if (formBody.partnerGroup != null) {
            if (formBody.partnerGroup.find(x => x.id === "ALL")) {
                this.partner.partnerGroup = 'AGENT;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER;STAFF;PERSONAL';
            } else {
                let s = '';
                for (const item of formBody.partnerGroup) {
                    s = s + item['id'] + ';';
                }
                this.partner.partnerGroup = s.substring(0, s.length - 1);
            }
        }
        const cloneObject = {
            countryId: formBody.countryId,
            provinceId: formBody.provinceId,
            countryShippingId: formBody.countryShippingId,
            provinceShippingId: formBody.provinceShippingId,
            parentId: formBody.partnerAccountRef,

            roundUpMethod: formBody.roundUpMethod.length > 0 ? formBody.roundUpMethod[0].id : null,
            applyDim: formBody.applyDim.length > 0 ? formBody.applyDim[0].id : null,
            partnerGroup: this.partner.partnerGroup,
            partnerMode: formBody.partnerMode != null && formBody.partnerMode.length > 0 ? formBody.partnerMode[0].id : null,
            partnerLocation: formBody.partnerLocation != null && formBody.partnerLocation.length > 0 ? formBody.partnerLocation[0].id : null,
            id: this.partner.id,
        };
        console.log("formBody: ", formBody);
        console.log("clone: ", cloneObject);
        const mergeObj = Object.assign(_merge(formBody, cloneObject));
        //merge clone & this.partner.
        const mergeObjPartner = Object.assign(_merge(this.partner, mergeObj));

        console.log("merge2: ", mergeObjPartner);
        //
        this.updatePartner(mergeObjPartner);
    }
    trimInputForm(formBody: any) {
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.partnerNameEn, formBody.partnerNameEn);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.partnerNameVn, formBody.partnerNameVn);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.shortName, formBody.shortName);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.taxCode, formBody.taxCode);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.addressEn, formBody.addressEn);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.addressVn, formBody.addressVn);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.zipCode, formBody.zipCode);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.addressShippingEn, formBody.addressShippingEn);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.addressShippingVn, formBody.addressShippingVn);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.zipCodeShipping, formBody.zipCodeShipping);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.internalReferenceNo, formBody.internalReferenceNo);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.coLoaderCode, formBody.coLoaderCode);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.contactPerson, formBody.contactPerson);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.tel, formBody.tel);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.fax, formBody.fax);

        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.bankAccountNo, formBody.bankAccountNo);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.bankAccountName, formBody.bankAccountName);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.bankAccountAddress, formBody.bankAccountAddress);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.swiftCode, formBody.swiftCode);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.note, formBody.note);
        //
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.billingEmail, formBody.billingEmail);
        this.formPartnerComponent.trimInputValue(this.formPartnerComponent.billingPhone, formBody.billingPhone);
    }

    onFocusInternalReference() {
        this.confirmTaxcode.hide();
        //
        this.formPartnerComponent.handleFocusInternalReference();
    }

    updatePartner(body: any) {
        this._catalogueRepo.checkTaxCode(body)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    console.log("res check: ", res);

                    if (!!res) {

                        this.formPartnerComponent.isExistedTaxcode = true;

                        if (!!res.internalReferenceNo) {
                            this.deleteMessage = `This Partner is existed, please you check again!`;
                            this.confirmDuplicatePartner.show();
                        } else {
                            this.deleteMessage = `This <b>Taxcode</b> already <b>Existed</b> in  <b>${res.shortName}</b>, If you want to Create Internal account, Please fill info to <b>Internal Reference Info</b>.`;
                            this.confirmTaxcode.show();
                        }


                    } else {
                        this.onSave(body);
                    }
                },
            );
    }
    onSave(body: any) {

        this._progressRef.start();
        this._catalogueRepo.updatePartner(body.id, body)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.router.navigate(["/home/catalogue/partner-data"]);
                    } else {
                        this._toastService.warning(res.message);
                    }
                }
            );
    }
    sortBySaleMan(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.saleMandetail = this._sortService.sort(this.saleMandetail, sortData.sortField, sortData.order);
        }
    }

    showDetailSaleMan(saleman: Saleman, id: any) {
        this.poupSaleman.isDetail = true;

        //const obj = this.saleMandetail.find(x => x.id === id);
        const saleMane: any = {
            description: saleman.description,
            office: saleman.office,
            effectDate: saleman.effectDate,
            status: saleman.status,
            partnerId: null,
            saleManId: saleman.saleManId,
            service: saleman.service,
            freightPayment: saleman.freightPayment,
            serviceName: saleman.serviceName
        };
        this.poupSaleman.allowDelete = this.partner.permission.allowDelete;
        this.poupSaleman.showSaleman(saleMane);
        this.poupSaleman.show();
    }

    showConfirmDelete() {
        this.deleteMessage = `Do you want to delete this partner  ${this.partner.partnerNameEn}?`;
        this.confirmDeletePartnerPopup.show();
    }

    onDelete() {
        this._catalogueRepo.deletePartner(this.partner.id)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.confirmDeletePartnerPopup.hide();
                        this.router.navigate(["/home/catalogue/partner-data"]);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    getListContract(partneId: string) {
        this.isLoading = true;
        this._catalogueRepo.getListContract(partneId)
            .pipe(
                finalize(() => this.isLoading = false)
            )
            .subscribe(
                (res: any[]) => {
                    this.listContract.contracts = res || [];
                    this.listContract.isActiveNewContract = false;
                    // this.contracts = res || [];

                }
            );
    }

    onRequestContract($event: any) {
        this.contract = $event;
        this.selectedContract = new Contract(this.contract);
        if (!!this.selectedContract && !this.formContractPopup.isCreateNewCommercial) {
            this.getListContract(this.partner.id);
        } else {
            console.log(this.selectedContract);
            const objCheckContract = !!this.selectedContract.contractNo && this.contracts.length >= 1 ? this.contracts.some(x => x.contractNo === this.selectedContract.contractNo) : null;
            if (this.indexlstContract !== null) {
                this.contracts[this.indexlstContract] = this.selectedContract;
                this.formContractPopup.hide();
            } else {
                if (objCheckContract && objCheckContract != null) {
                    this.formContractPopup.isDuplicateContract = true;
                    this._toastService.error('Contract no has been existed!');
                } else {
                    this.formContractPopup.isDuplicateContract = false;
                    this.contracts.push(this.selectedContract);
                }
            }
        }
        this.formContractPopup.contracts = this.contracts;
    }

    getDetailContract(id: string, index: number) {
        this.formContractPopup.isUpdate = true;
        this.formContractPopup.partnerId = this.partner.id;
        this.formContractPopup.selectedContract.id = id;
        this.indexlstContract = index;
        if (this.formContractPopup.selectedContract.id !== SystemConstants.EMPTY_GUID && this.formContractPopup.selectedContract.id !== "") {
            this.formContractPopup.getFileContract();
            this._catalogueRepo.getDetailContract(this.formContractPopup.selectedContract.id)
                .subscribe(
                    (res: Contract) => {
                        this.selectedContract = res;
                        this.formContractPopup.idContract = this.selectedContract.id;
                        this.formContractPopup.selectedContract = res;
                        this.formContractPopup.pachValueToFormContract();
                        this.formContractPopup.show();
                    }
                );
        } else {
            if (this.contracts.length > 0) {
                this.formContractPopup.selectedContract = this.contracts[this.indexlstContract];
                this.formContractPopup.indexDetailContract = this.indexlstContract;
                this.formContractPopup.fileList = this.formContractPopup.selectedContract.fileList;
            }
            this.formContractPopup.pachValueToFormContract();
            this.formContractPopup.show();
        }

    }

    showConfirmDeleteContract(contract: Contract, index: number) {
        this.selectedContract = contract;
        this.indexToRemove = index;
        if (this.selectedContract.id === SystemConstants.EMPTY_GUID) {
            this.contracts = [...this.contracts.slice(0, index), ...this.contracts.slice(index + 1)];
        } else {
            this.confirmDeleteContract.show();
        }
    }

    onDeleteContract() {
        if (this.contracts.length === 1) {
            this._toastService.error('Contract must have one row!');
            this.confirmDeleteContract.hide();
            return;
        }
        this.confirmDeleteContract.hide();
        this._catalogueRepo.deleteContract(this.selectedContract.id, this.partner.id)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.contracts.splice(this.indexToRemove, 1);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }


}
