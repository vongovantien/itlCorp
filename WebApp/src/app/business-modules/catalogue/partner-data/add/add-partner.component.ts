import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { API_MENU } from 'src/constants/api-menu.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { NgForm } from '@angular/forms';
import { SelectComponent } from 'ng2-select';
import { SortService } from 'src/app/shared/services/sort.service';
import * as dataHelper from 'src/helper/data.helper';
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

@Component({
    selector: 'app-partner-data-add',
    templateUrl: './add-partner.component.html',
    styleUrls: ['./add-partner.component.scss']
})
export class AddPartnerDataComponent extends AppList {
    @ViewChild(FormAddPartnerComponent, { static: false }) formPartnerComponent: FormAddPartnerComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmTaxcode: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild(SalemanPopupComponent, { static: false }) poupSaleman: SalemanPopupComponent;
    saleMans = [];
    activeNg: boolean = true;
    partner: Partner = new Partner();
    partnerGroups: any;
    partnerGroupActives: any = [];
    countries: any[];
    billingProvinces: any[];
    shippingProvinces: any[];
    workPlaces: any[];
    parentCustomers: any[];
    departments: any[];
    partnerType: any;
    isRequiredSaleman = false;
    employee: any = {};
    users: any[] = [];
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
        private baseService: BaseService,
        private api_menu: API_MENU,
        private router: Router,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private sortService: SortService,
        private toastr: ToastrService,
    ) {
        super();
    }

    closepp(param: SalemanAdd) {
        this.salemanToAdd = param;
        this.poupSaleman.isDetail = false;
        if (this.saleMandetail.length > 0) {
            for (const it of this.saleMandetail) {
                this.services.forEach(item => {
                    if (it.service === item.text) {
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
                        it.service = item.text;
                    }
                });
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
                                this.saleMandetail.push(this.salemanToAdd);
                                this.poupSaleman.hide();
                                for (const it of this.saleMandetail) {

                                    this.services.forEach(item => {
                                        if (it.service === item.id) {
                                            it.service = item.text;
                                        }
                                    });
                                }
                                this.saleMandetail.forEach(element => {
                                    element.status = element.status.value;
                                });

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
        this.poupSaleman.show();
    }

    onDeleteSaleman() {
        if (this.saleMandetail.length > 0) {
            this.saleMandetail.splice(this.index, 1);
            this.confirmDeleteJobPopup.hide();
            this.toastr.success('Delete Success !');
        }
        if (this.isExistedTaxcode) {
            this.confirmTaxcode.hide();
        }

    }
    deleteSaleman(index: any) {
        this.index = index;
        this.deleteMessage = `Do you want to delete sale man  ${this.saleMandetail[index].saleman_ID}?`;
        this.confirmDeleteJobPopup.show();
    }

    ngOnInit() {

        this.getComboboxData();
        this.headerSaleman = [
            { title: '', field: '', sortable: false },
            { title: 'Saleman', field: 'saleman_ID', sortable: true },
            { title: 'Service', field: 'service', sortable: true },
            { title: 'Office', field: 'office', sortable: true },
            { title: 'Company', field: 'company', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'CreatedDate', field: 'createDate', sortable: true }
        ];
        this.route.params.subscribe(prams => {
            console.log({ param: prams });
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
        // this.getSalemans();
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
    // getSalemans() {
    //     this._systemRepo.getSystemUsers()
    //         .pipe(catchError(this.catchError), finalize(() => { }))
    //         .subscribe(
    //             (res) => {
    //                 if (res) {
    //                     this.saleMans = res.map(x => ({ "text": x.code + ' - ' + x.nameVn, "id": x.id }));
    //                 } else { this.saleMans = []; }
    //             }
    //         );
    // }
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
            this.isRequiredSaleman = true;
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
            this.isRequiredSaleman = true;
        }
        this.formPartnerComponent.partnerForm.controls['partnerGroup'].setValue(this.partnerGroupActives);
    }

    onSubmit() {
    }

    onCreatePartner() {
        this.baseService.spinnerShow();
        this.saleMandetail.forEach(element => {
            element.status = element.status.value;
        });
        this.partner.saleMans = this.saleMandetail;
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

                }
