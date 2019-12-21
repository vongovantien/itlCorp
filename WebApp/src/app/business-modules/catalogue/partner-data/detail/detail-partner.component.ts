
import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { SortService } from 'src/app/shared/services/sort.service';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
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


@Component({
    selector: 'app-partner-detail',
    templateUrl: './detail-partner.component.html',
    styleUrls: ['./detail-partner.component.scss']
})
export class PartnerDetailComponent extends AppList {
    @ViewChild(FormAddPartnerComponent, { static: false }) formPartnerComponent: FormAddPartnerComponent;
    @ViewChild("popupDeleteSaleman", { static: false }) confirmDeleteSalemanPopup: ConfirmPopupComponent;
    @ViewChild("popupDeletePartner", { static: false }) confirmDeletePartnerPopup: ConfirmPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmTaxcode: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild(SalemanPopupComponent, { static: false }) poupSaleman: SalemanPopupComponent;
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


    list: any[] = [];

    isDup: boolean = false;

    constructor(private route: ActivatedRoute,
        private router: Router,
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService,
        private toastr: ToastrService,
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }


    ngOnInit() {
        this.getComboboxDataSaleman();
        this.initHeaderSalemanTable();
        this.route.params.subscribe(async (prams: any) => {
            if (!!prams.id) {
                this.partner.id = prams.id;
                this.dataSearchSaleman.partnerId = this.partner.id;
                this.getSalemanPagingByPartnerId(this.dataSearchSaleman);
            }
        });
        this.partner.departmentId = "Head Office";
        this.getDataCombobox();
    } RequireSaleman(partnerGroup: string): boolean {
        this.isShowSaleMan = false;
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
                        this.isShowSaleMan = this.checkRequireSaleman(this.partner.partnerGroup);
                        this.formPartnerComponent.setFormData(this.partner);
                    }
                }
            );

    }
    checkRequireSaleman(partnerGroup: string): boolean {
        this.isShowSaleMan = false;
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
        this._catalogueRepo.getListSaleManDetail(this.page, this.pageSize, Object.assign({}, dataSearchSaleman, { partnerId: this.partner.id }))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; })
            ).subscribe(
                (res: any) => {
                    this.saleMandetail = (res.data || []).map((item: Saleman) => new Saleman(item));
                    if (this.saleMandetail.length > 0) {
                        for (const it of this.saleMandetail) {
                            if (it.status === true) {
                                it.statusString = "Active";
                            } else {
                                it.statusString = "InActive";
                            }
                            const index = this.services.findIndex(x => x.id === it.service);
                            if (index > -1) {
                                it.serviceName = this.services[index].text;
                            }
                        }
                    }
                    this.totalItems = res.totalItems || 0;
                },
            );
    }

    initHeaderSalemanTable() {
        this.headerSaleman = [
            { title: '', field: '', sortable: false },
            { title: 'Saleman', field: 'saleman_ID', sortable: true },
            { title: 'Service', field: 'service', sortable: true },
            { title: 'Office', field: 'office', sortable: true },
            { title: 'Company', field: 'company', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'CreatedDate', field: 'createDate', sortable: true }
        ];
    }
    closePopupSaleman(param: SalemanAdd) {
        this.salemanToAdd = param;
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
            this._catalogueRepo.checkExistedSaleman(this.salemanToAdd.service, this.salemanToAdd.office)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (!!res) {
                            if (this.isDup) {
                                console.log("dup");
                                this.toastr.error('Duplicate service, office with sale man!');
                            } else {
                                console.log(this.saleMandetail);
                                console.log(this.salemanToAdd);
                                this.saleMandetail = [...this.saleMandetail, this.salemanToAdd];
                                console.log(this.saleMandetail);
                                console.log(this.saleMandetail[0]);
                                this.getEmployee(this.saleMandetail[0].saleman_ID);
                                this.poupSaleman.hide();
                                for (const it of this.saleMandetail) {

                                    this.services.forEach(item => {
                                        if (it.service === item.id) {
                                            it.serviceName = item.text;
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
        this.deleteSaleman(this.index);
    }

    showPopupSaleman() {
        this.poupSaleman.isSave = false;
        this.poupSaleman.isDetail = false;
        this.poupSaleman.form.reset();
        this.poupSaleman.show();
    }

    onDeleteSaleman() {
        if (this.saleMandetail.length > 0) {
            this.saleMandetail = [...this.saleMandetail.slice(0, this.index), ...this.saleMandetail.slice(this.index + 1)];
            this.confirmDeleteSalemanPopup.hide();
            this.toastr.success('Delete Success !');
        }
        if (this.isExistedTaxcode) {
            this.confirmTaxcode.hide();
        }

    }
    deleteSaleman(index: any) {
        this.index = index;
        this.deleteMessage = `Do you want to delete sale man  ${this.saleMandetail[index].saleman_ID}?`;
        this.confirmDeleteSalemanPopup.show();
    }
    getDataCombobox() {
        forkJoin([
            this._catalogueRepo.getCountryByLanguage(),
            this._catalogueRepo.getProvinces(),
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CUSTOMER),
            this._catalogueRepo.getPartnerGroup(),
            this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Branch })
        ])
            .pipe(catchError(this.catchError))
            .subscribe(
                ([countries, provinces, customers, partnerGroups, workPlaces]) => {
                    this.formPartnerComponent.countries = this.utility.prepareNg2SelectData(countries || [], 'id', 'name');
                    this.formPartnerComponent.billingProvinces = this.utility.prepareNg2SelectData(provinces || [], 'id', 'name_VN');
                    this.formPartnerComponent.shippingProvinces = this.utility.prepareNg2SelectData(provinces || [], 'id', 'name_VN');
                    this.formPartnerComponent.parentCustomers = this.utility.prepareNg2SelectData(customers || [], 'id', 'partnerNameVn');
                    this.formPartnerComponent.partnerGroups = this.utility.prepareNg2SelectData(partnerGroups || [], 'id', 'id');
                    this.getPartnerGroupActive(this.partnerType);
                    this.formPartnerComponent.workPlaces = this.utility.prepareNg2SelectData(workPlaces || [], 'id', 'nameVn');
                    this.getParnerDetails();
                },
                () => { },

            );
    }
    getComboboxDataSaleman(): any {
        this.getService();
        this.getOffice();
        this.getStatus();
    }

    getService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                        this.selectedService = this.services[0];
                    }
                },
            );
    }

    checkTaxcode() {
        this._catalogueRepo.checkTaxCode(this.partner.taxCode)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        console.log(res);
                        this.isExistedTaxcode = true;
                        this.deleteMessage = `This Taxcode already Existed in  ${res.shortName}If you want to Create Internal account, Please fill info to Internal Reference Info.`;
                        this.confirmTaxcode.show();
                    } else {
                        this.isExistedTaxcode = false;
                    }
                },
            );
    }

    getOffice() {
        this._catalogueRepo.getListBranch()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.offices = res;
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
    getEmployee(userId: any): any {
        this._systemRepo.getEmployeeByUserId(userId)
            .pipe(catchError(this.catchError), finalize(() => { }))
            .subscribe(
                (res) => {
                    if (res) {
                        this.formPartnerComponent.partnerForm.controls['employeeWorkPhone'].setValue(res.tel);
                        this.formPartnerComponent.partnerForm.controls['employeeEmail'].setValue(res.email);
                    }
                }
            );
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
        if (partnerGroup === PartnerGroupEnum.ALL) {
            this.partnerGroupActives.push(this.formPartnerComponent.partnerGroups.find(x => x.id === "ALL"));
        }
        if (this.partnerGroupActives.find(x => x.id === "ALL")) {
            this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
            this.isShowSaleMan = true;
        }
        this.formPartnerComponent.partnerForm.controls['partnerGroup'].setValue(this.partnerGroupActives);
    }

    onSubmit() {
        this.formPartnerComponent.isSubmitted = true;
        this.partner.saleMans = this.saleMandetail;
        this.getFormPartnerData();
        if (this.partner.countryId == null || this.partner.provinceId == null
            || this.partner.countryShippingId == null || this.partner.provinceShippingId == null || this.partner.departmentId == null) {
            return;
        }
        if (this.formPartnerComponent.partnerForm.valid) {
            this.partner.accountNo = this.partner.id;
            if (this.saleMandetail.length === 0) {
                if (this.isShowSaleMan) {
                    this.toastr.error('Please add saleman and service for customer!');
                    return;
                }
            }

            if (this.saleMandetail.length > 0) {
                for (const it of this.saleMandetail) {
                    this.services.forEach(item => {
                        if (it.service === item.text) {
                            it.service = item.id;
                        }
                    });
                }
            }

            if (this.isShowSaleMan) {
                if (this.saleMandetail.length === 0) {
                    this.toastr.error('Please add saleman and service for customer!');
                } else {
                    this.updatePartner();
                }
            } else {
                this.updatePartner();
            }
        }
    }
    getFormPartnerData() {
        const formBody = this.formPartnerComponent.partnerForm.getRawValue();
        this.partner.id = formBody.internalReferenceNo + "." + formBody.taxCode;
        if (formBody.partnerGroup != null) {
            if (formBody.partnerGroup.find(x => x.id === "ALL")) {
                this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
            } else {
                let s = '';
                for (const item of formBody.partnerGroup) {
                    s = item['id'] + ';';
                }
                this.partner.partnerGroup = s.substring(0, s.length - 1);
            }
        }
        this.partner.partnerNameVn = formBody.nameLocalFull;
        this.partner.partnerNameEn = formBody.nameENFull;
        this.partner.contactPerson = formBody.partnerContactPerson;
        this.partner.addressVn = formBody.billingAddressLocal;
        this.partner.addressEn = formBody.billingAddressEN;
        this.partner.addressShippingVn = formBody.shippingAddressVN;
        this.partner.addressShippingEn = formBody.shippingAddressEN;
        this.partner.shortName = formBody.shortName;
        if (formBody.billingCountry.length > 0) {
            this.partner.countryId = formBody.billingCountry[0].id;
        }
        if (formBody.shippingCountry.length > 0) {
            this.partner.countryShippingId = formBody.shippingCountry[0].id;
        }
        this.partner.accountNo = formBody.partnerAccountNo;
        this.partner.tel = formBody.partnerContactNumber;
        this.partner.fax = formBody.partnerContactFaxNo;
        this.partner.taxCode = formBody.taxCode;
        this.partner.email = formBody.employeeEmail;
        this.partner.website = formBody.partnerWebsite;
        this.partner.bankAccountNo = formBody.partnerbankAccountNo;
        this.partner.bankAccountName = formBody.partnerBankAccountName;
        this.partner.bankAccountAddress = formBody.partnerBankAccountAddress;
        this.partner.note = formBody.note;
        this.partner.public = formBody.public;
        if (formBody.billingProvince.length > 0) {
            this.partner.provinceId = formBody.billingProvince[0].id;
        }
        if (formBody.shippingProvince.length > 0) {
            this.partner.provinceShippingId = formBody.shippingProvince[0].id;
        }
        if (formBody.partnerAccountRef.length > 0) {
            this.partner.parentId = formBody.partnerAccountRef[0].id;
        }
        this.partner.zipCode = formBody.billingZipcode;
        this.partner.zipCodeShipping = formBody.zipCodeShipping;
        this.partner.swiftCode = formBody.partnerSwiftCode;
        this.partner.active = formBody.active;
        if (formBody.partnerWorkPlace.length > 0) {
            this.partner.workPlaceId = formBody.partnerWorkPlace[0].id;
        }
        this.partner.internalReferenceNo = formBody.internalReferenceNo;
    }

    updatePartner() {
        this._progressRef.start();
        this._catalogueRepo.updatePartner(this.partner.id, this.partner)
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
    showDetailSaleMan(saleman: Saleman, index: any) {
        this.poupSaleman.index = index;
        this.poupSaleman.isDetail = true;
        const saleMane: any = {
            description: saleman.description,
            office: saleman.office,
            effectDate: saleman.effectDate,
            status: saleman.status === true ? 'Active' : 'Inactive',
            partnerId: null,
            saleman_ID: saleman.saleman_ID,
            service: saleman.service,
            createDate: saleman.createDate

        };
        this.poupSaleman.showSaleman(saleMane);
        this.poupSaleman.show();
    }
    showConfirmDelete() {
        this.deleteMessage = `Do you want to delete this partner  ${this.partner.partnerNameEn}?`;
        this.confirmDeletePartnerPopup.show();
    }
    check
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
}
