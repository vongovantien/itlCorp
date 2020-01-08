import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { BaseService } from 'src/app/shared/services/base.service';
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
import { formatDate } from '@angular/common';

@Component({
    selector: 'app-partner-data-add',
    templateUrl: './add-partner.component.html',
    styleUrls: ['./add-partner.component.scss']
})
export class AddPartnerDataComponent extends AppList {
    @ViewChild(FormAddPartnerComponent, { static: false }) formPartnerComponent: FormAddPartnerComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) confirmTaxcode: InfoPopupComponent;
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


    list: any[] = [];

    isDup: boolean = false;

    constructor(private route: ActivatedRoute,
        private baseService: BaseService,
        private router: Router,
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService,
        private toastr: ToastrService,
        private _systemRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit() {

        this.getComboboxData();
        this.initHeaderSalemanTable();
        this.route.params.subscribe(prams => {
            if (prams.partnerType !== undefined) {
                this.partnerType = Number(prams.partnerType);
                if (this.partnerType === '3') {
                    this.isShowSaleMan = true;
                }
            }
        });
        this.partner.departmentId = "Head Office";
        this.getDataCombobox();
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
    closepp(param: SalemanAdd) {
        this.salemanToAdd = param;
        this.poupSaleman.isDetail = false;
        if (this.saleMandetail.length > 0) {
            for (const it of this.saleMandetail) {
                this.services.forEach(item => {
                    if (it.service === item.id) {
                        it.service = item.id;
                    }
                });

            }
        }
        this.isDup = this.saleMandetail.some((saleMane: Saleman) => (saleMane.service === this.salemanToAdd.service && saleMane.office === this.salemanToAdd.office));
        if (this.isDup) {
            for (const it of this.saleMandetail) {
                this.services.forEach(item => {
                    if (it.service === item.id) {
                        it.service = item.id;
                    }
                });
            }
        }


        if (this.salemanToAdd.service !== null && this.salemanToAdd.office !== null) {
            this._catalogueRepo.checkExistedSaleman(this.salemanToAdd.service[0].id, this.salemanToAdd.office)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (!!res) {
                            if (this.isDup) {
                                console.log("dup");
                                this.toastr.error('Duplicate service, office with sale man!');
                            } else {
                                this.saleMandetail.push(this.salemanToAdd);
                                /// get detail employee --- to be continue
                                this.getEmployee(this.saleMandetail[0].saleManId);
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
        this.poupSaleman.resetForm();
        this.poupSaleman.show();
    }

    onDeleteSaleman() {
        if (this.saleMandetail.length > 0) {
            this.saleMandetail.splice(this.index, 1);
            this.confirmDeleteJobPopup.hide();
            this.toastr.success('Delete Success !');
        }

    }
    deleteSaleman(index: any) {
        this.index = index;
        this.deleteMessage = `Do you want to delete sale man  ${this.saleMandetail[index].username}?`;
        this.confirmDeleteJobPopup.show();
    }
    getDataCombobox() {
        forkJoin([
            this._catalogueRepo.getCountryByLanguage(),
            this._catalogueRepo.getProvinces()
        ])
            .pipe(catchError(this.catchError))
            .subscribe(
                ([countries, provinces]) => {
                    this.formPartnerComponent.countries = this.utility.prepareNg2SelectData(countries || [], 'id', 'name');
                    this.formPartnerComponent.billingProvinces = this.utility.prepareNg2SelectData(provinces || [], 'id', 'name_VN');
                    this.formPartnerComponent.shippingProvinces = this.utility.prepareNg2SelectData(provinces || [], 'id', 'name_VN');
                },
                () => { },

            );
    }
    getComboboxData(): any {
        this.getPartnerGroups();
        this.getWorkPlaces();
        this.getparentCustomers();
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
    getparentCustomers() {
        this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CUSTOMER)
            .pipe(catchError(this.catchError), finalize(() => { }))
            .subscribe(
                (res) => {
                    if (res) {
                        this.formPartnerComponent.parentCustomers = res.map(x => ({ "text": x.partnerNameVn, "id": x.id }));
                    } else { this.formPartnerComponent.parentCustomers = []; }
                }
            );
    }
    getWorkPlaces() {
        this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Branch })
            .pipe(catchError(this.catchError), finalize(() => { }))
            .subscribe(
                (res) => {
                    if (res) {
                        this.formPartnerComponent.workPlaces = res.map(x => ({ "text": x.code + ' - ' + x.nameVn, "id": x.id }));
                    } else { this.formPartnerComponent.workPlaces = []; }
                }
            );
    }
    getPartnerGroups(): any {
        this._catalogueRepo.getPartnerGroup().subscribe((response: any) => {
            if (response != null) {
                console.log(response);
                this.formPartnerComponent.partnerGroups = response.map(x => ({ "text": x.id, "id": x.id }));
                this.getPartnerGroupActive(this.partnerType);
            }
        }, err => {
            this.baseService.handleError(err);
        });
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
        this.partner.saleMans.forEach(element => {
            element.effectDate = element.effectDate !== null ? formatDate(element.effectDate.startDate !== undefined ? element.effectDate.startDate : element.effectDate, 'yyyy-MM-dd', 'en') : null;
            element.createDate = element.createDate !== null ? formatDate(element.createDate.startDate !== undefined ? element.createDate.startDate : element.createDate, 'yyyy-MM-dd', 'en') : null;
        });
        this.getFormPartnerData();
        if (this.partner.countryId == null || this.partner.provinceId == null
            || this.partner.countryShippingId == null || this.partner.provinceShippingId == null || this.partner.departmentId == null) {
            return;
        }

        this.formPartnerComponent.partnerWorkPlace.setErrors(null);
        this.formPartnerComponent.partnerAccountRef.setErrors(null);
        if (this.formPartnerComponent.partnerForm.valid) {
            if (this.saleMandetail.length === 0) {
                if (this.isShowSaleMan) {
                    this.toastr.error('Please add saleman and service for customer!');
                    return;
                }
            }

            if (this.saleMandetail.length > 0) {
                for (const it of this.saleMandetail) {
                    this.services.forEach(item => {
                        if (it.service[0].id === item.id) {
                            it.service = item.id;
                        }
                    });
                }
            }

            if (this.isShowSaleMan) {
                if (this.saleMandetail.length === 0) {
                    this.toastr.error('Please add saleman and service for customer!');
                } else {
                    this.onCreatePartner();
                }
            } else {
                this.onCreatePartner();
            }
        }
    }
    getFormPartnerData() {
        const formBody = this.formPartnerComponent.partnerForm.getRawValue();
        // if (formBody.internalReferenceNo != null) {
        //     this.partner.accountNo = formBody.internalReferenceNo + "." + formBody.taxCode;
        // } else {
        //     this.partner.accountNo = formBody.taxCode;
        // }
        this.partner.partnerGroup = formBody.partnerGroup[0].id;
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
        if (formBody.billingCountry !== null) {
            if (formBody.billingCountry.length > 0) {
                this.partner.countryId = formBody.billingCountry[0].id;
            }
        }
        if (formBody.shippingCountry !== null) {
            if (formBody.shippingCountry.length > 0) {
                this.partner.countryShippingId = formBody.shippingCountry[0].id;
            }
        }

        this.partner.tel = formBody.partnerContactNumber;
        this.partner.fax = formBody.partnerContactFaxNo;
        this.partner.taxCode = formBody.taxCode;
        this.partner.email = formBody.employeeEmail;
        this.partner.website = formBody.partnerWebsite;
        this.partner.bankAccountNo = formBody.partnerbankAccountNo;
        this.partner.bankAccountName = formBody.partnerBankAccountName;
        this.partner.bankAccountAddress = formBody.partnerBankAccountAddress;
        this.partner.note = formBody.note;
        this.partner.public = this.formPartnerComponent.isPublic;
        // this.partner.public = formBody.public;
        if (formBody.billingProvince.length > 0) {
            this.partner.provinceId = formBody.billingProvince[0].id;
        }
        if (formBody.shippingProvince.length > 0) {
            this.partner.provinceShippingId = formBody.shippingProvince[0].id;
        }
        if (formBody.partnerAccountRef != null) {
            this.partner.parentId = formBody.partnerAccountRef.length > 0 ? formBody.partnerAccountRef[0].id : null;
        }
        this.partner.zipCode = formBody.billingZipcode;
        this.partner.zipCodeShipping = formBody.zipCodeShipping;
        this.partner.swiftCode = formBody.partnerSwiftCode;
        this.partner.active = formBody.active;
        if (formBody.partnerWorkPlace != null) {
            this.partner.workPlaceId = formBody.partnerWorkPlace.length > 0 ? formBody.partnerWorkPlace[0].id : null;
        }
        this.partner.internalReferenceNo = formBody.internalReferenceNo;
        this.partner.coLoaderCode = formBody.coLoaderCode;
    }

    onCreatePartner() {
        this._catalogueRepo.checkTaxCode(this.partner)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        console.log(res);
                        this.formPartnerComponent.isExistedTaxcode = true;
                        this.deleteMessage = `This Taxcode already Existed in  ${res.shortName}If you want to Create Internal account, Please fill info to Internal Reference Info.`;
                        this.confirmTaxcode.show();
                    } else {
                        this.onSave();
                    }
                },
            );
    }
    onSave() {
        this.baseService.spinnerShow();
        this._catalogueRepo.createPartner(this.partner)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this.baseService.spinnerHide();
                        this.baseService.successToast(res.message);
                        this.router.navigate(["/home/catalogue/partner-data"]);

                    }

                }, err => {
                    this.baseService.spinnerHide();
                    this.baseService.handleError(err);

                });
    }
    sortBySaleMan(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.saleMandetail = this._sortService.sort(this.saleMandetail, sortData.sortField, sortData.order);
        }
    }

    showDetailSaleMan(saleman: Saleman, id: any) {
        this.poupSaleman.isDetail = true;
        const obj = this.saleMandetail.find(x => x.id === id);
        const saleMane: any = {
            description: obj.description,
            office: obj.office,
            effectDate: obj.effectDate,
            status: obj.status,
            partnerId: null,
            saleManId: obj.saleManId,
            service: obj.service,
            createDate: obj.createDate,
            freightPayment: obj.freightPayment,
            serviceName: obj.serviceName

        };
        this.poupSaleman.showSaleman(saleMane);
        this.poupSaleman.show();
    }
}
