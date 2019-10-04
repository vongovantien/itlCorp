import { Component, OnInit, ViewChild } from '@angular/core';
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
import { CatalogueRepo } from 'src/app/shared/repositories';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { catchError, finalize } from "rxjs/operators";
import { AppList } from 'src/app/app.list';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';
import { SalemanPopupComponent } from '../components/saleman-popup.component';

declare var $: any;
@Component({
    selector: 'app-partner-data-addnew',
    templateUrl: './partner-data-addnew.component.html',
    styleUrls: ['./partner-data-addnew.component.scss']
})
export class PartnerDataAddnewComponent extends AppList {
    @ViewChild('formAddEdit', { static: false }) form: NgForm;
    @ViewChild('chooseBillingCountry', { static: false }) public chooseBillingCountry: SelectComponent;
    @ViewChild('chooseBillingProvince', { static: false }) public chooseBillingProvince: SelectComponent;
    @ViewChild('chooseShippingCountry', { static: false }) public chooseShippingCountry: SelectComponent;
    @ViewChild('chooseShippingProvince', { static: false }) public chooseShippingProvince: SelectComponent;
    @ViewChild('chooseSaleman', { static: false }) public chooseSaleman: SelectComponent;
    @ViewChild('chooseDepartment', { static: false }) public chooseDepartment: SelectComponent;
    @ViewChild('chooseAccountRef', { static: false }) public chooseAccountRef: SelectComponent;
    @ViewChild('chooseWorkplace', { static: false }) public chooseWorkplace: SelectComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild(SalemanPopupComponent, { static: false }) poupSaleman: SalemanPopupComponent;

    activeNg: boolean = true;
    partner: Partner = new Partner();
    partnerGroups: any;
    partnerGroupActives: any = [];
    countries: any[];
    billingProvinces: any[];
    shippingProvinces: any[];
    saleMans: any[];
    workPlaces: any[];
    parentCustomers: any[];
    departments: any[];
    partnerType: any;
    isRequiredSaleman = false;
    employee: any = {};
    users: any[] = [];
    saleMandetail: any = [];
    headerSaleman: CommonInterface.IHeaderTable[];
    services: any[] = [];
    status: CommonInterface.ICommonTitleValue[] = [];
    offices: any[] = [];
    saleManToAdd: SalemanAdd = new SalemanAdd();
    strOfficeCurrent: any = '';
    strSalemanCurrent: any = '';
    selectedStatus: any = {};
    selectedService: any = {};
    deleteMessage: string = '';
    selectedSaleman: Saleman = null;
    saleMantoView: Saleman = new Saleman();
    dataSearchSaleman: any = {};
    isShowSaleMan: boolean = false;
    salemanTemp: Array<Object> = [];

    list: any[] = [];
    constructor(private route: ActivatedRoute,
        private baseService: BaseService,
        private api_menu: API_MENU,
        private router: Router,
        private _catalogueRepo: CatalogueRepo,
        private sortService: SortService,
        private toastr: ToastrService,
    ) {
        super();
    }
    closepp(param: SalemanAdd) {
        this.saleManToAdd = param;
        this.saleMandetail.push(this.saleManToAdd);
    }

    showPopupSaleman() {
        this.poupSaleman.show();
    }

    ngOnInit() {
        this.headerSaleman = [
            { title: '', field: '', sortable: false },
            { title: 'Saleman', field: 'saleman_ID', sortable: true },
            { title: 'Service', field: 'service', sortable: true },
            { title: 'Office', field: 'office', sortable: true },
            { title: 'Company', field: 'company', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'ModifiedDate', field: 'modifiedDate', sortable: true }
        ];
        this.route.params.subscribe(prams => {
            console.log({ param: prams });
            if (prams.partnerType != undefined) {
                this.partnerType = prams.partnerType;
                if (this.partnerType == '3') {
                    this.isShowSaleMan = true;
                }
            }
        });

        this.getComboboxData();
        this.partner.departmentId = "Head Office";
    }
    getComboboxData(): any {
        this.getPartnerGroups();
        this.getCountries();
        this.getSalemans();
        this.getWorkPlaces();
        this.getparentCustomers();
        this.getDepartments();
        console.log(this.partner.salePersonId);
        console.log(this.isRequiredSaleman);
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

    departmentActive: any[] = [];
    getDepartments(): any {
        this.baseService.get(this.api_menu.Catalogue.PartnerData.getDepartments).subscribe((response: any) => {
            if (response != null) {
                this.departments = response.map(x => ({ "text": x.name, "id": x.id }));
            }
            this.departmentActive = ["Head Office"];
        }, err => {
            this.baseService.handleError(err);
        });
    }
    async getparentCustomers() {
        let responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER }, false, true);
        if (responses != null) {
            this.parentCustomers = responses.map(x => ({ "text": x.partnerNameVn, "id": x.id }));
        }
    }
    async getWorkPlaces() {
        let responses = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Branch }, false, true);
        if (responses != null) {
            this.workPlaces = responses.map(x => ({ "text": x.code + ' - ' + x.nameVn, "id": x.id }));
        }
    }
    async getSalemans() {
        let responses = await this.baseService.getAsync(this.api_menu.System.User_Management.getAll, false, true);
        if (responses != null) {
            this.users = responses;
            this.saleMans = responses.map(x => ({ "text": x.username, "id": x.id }));
            this.saleMans = this.sortService.sort(this.saleMans, 'text', true);
        }
    }
    async getProvinces(id: number, isBilling: boolean) {
        let url = this.api_menu.Catalogue.CatPlace.getProvinces;
        if (id != undefined) {
            url = url + "?countryId=" + id;
        }
        let responses = await this.baseService.getAsync(url, false, false);
        if (responses != null) {
            if (isBilling) {
                this.billingProvinces = responses.map(x => ({ "text": x.name_VN, "id": x.id }));
            }
            else {
                this.shippingProvinces = responses.map(x => ({ "text": x.name_VN, "id": x.id }));
            }
        }
        else {
            this.billingProvinces = [];
            this.shippingProvinces = [];
        }
    }
    async getCountries() {
        let responses = await this.baseService.getAsync(this.api_menu.Catalogue.Country.getAllByLanguage, false, true);
        if (responses != null) {
            this.countries = dataHelper.prepareNg2SelectData(responses, 'id', 'name');
        }
        else {
            this.countries = [];
        }
    }
    getPartnerGroups(): any {
        this.baseService.get(this.api_menu.Catalogue.partnerGroup.getAll).subscribe((response: any) => {
            if (response != null) {
                this.partnerGroups = response.map(x => ({ "text": x.id, "id": x.id }));
                this.getPartnerGroupActive(this.partnerType);
            }
        }, err => {
            this.baseService.handleError(err);
        });
    }
    getPartnerGroupActive(partnerGroup: any): any {
        if (partnerGroup == PartnerGroupEnum.AGENT) {
            this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "AGENT"));
        }
        if (partnerGroup == PartnerGroupEnum.AIRSHIPSUP) {
            this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "AIRSHIPSUP"));
        }
        if (partnerGroup == PartnerGroupEnum.CARRIER) {
            this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "CARRIER"));
        }
        if (partnerGroup == PartnerGroupEnum.CONSIGNEE) {
            this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "CONSIGNEE"));
        }
        if (partnerGroup == PartnerGroupEnum.CUSTOMER) {
            this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "CUSTOMER"));
            this.isRequiredSaleman = true;
        }
        if (partnerGroup == PartnerGroupEnum.SHIPPER) {
            this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "SHIPPER"));
        }
        if (partnerGroup == PartnerGroupEnum.SUPPLIER) {
            this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "SUPPLIER"));
        }
        if (partnerGroup == PartnerGroupEnum.ALL) {
            this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "ALL"));
        }
        this.partner.partnerGroup = '';
        if (this.partnerGroupActives.find(x => x.id == "ALL")) {
            this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
            this.isRequiredSaleman = true;
        }
        else {
            this.partnerGroupActives.forEach(element => {
                this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
            });
            if (this.partnerGroupActives.length > 0) {
                this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length - 1));
            }
        }
        // this.isRequiredSaleman = this.checkRequireSaleman(this.partner.partnerGroup);
        console.log(this.partner.partnerGroup);
        this.activeNg = false;
        setTimeout(() => {
            this.activeNg = true;
        }, 200);
    }

    onSubmit() {
        if (this.partner.countryId == null || this.partner.provinceId == null
            || this.partner.countryShippingId == null || this.partner.provinceShippingId == null || this.partner.departmentId == null) {
            return;
        }
        if (this.form.valid) {
            this.partner.accountNo = this.partner.id = this.partner.taxCode;
            if (this.isRequiredSaleman && this.partner.salePersonId != null) {
                this.addNew();
            }
            else {
                if (this.isRequiredSaleman == false) {
                    this.addNew();
                }
            }
        }
    }
    addNew(): any {
        this.baseService.spinnerShow();
        this.baseService.post(this.api_menu.Catalogue.PartnerData.add, this.partner).subscribe((response: any) => {
            this.baseService.spinnerHide();
            this.baseService.successToast(response.message);
            this.router.navigate(["/home/catalogue/partner-data"]);
            //this.resetForm();     
        }, err => {
            this.baseService.spinnerHide();
            this.baseService.handleError(err);
        });
    }

    resetForm(): any {
        this.form.onReset();
        this.partner.parentId = null;
        this.partner.countryId = null;
        this.partner.provinceId = null;
        this.partner.countryShippingId = null;
        this.partner.provinceShippingId = null;
        this.partner.departmentId = "1";
        //this.partner.partnerGroup = '';
        this.partner.salePersonId = null;
        this.partner.workPlaceId = null;
        this.partner.public = false;
        //this.partnerGroupActives = [];
        this.chooseBillingCountry.active = [];
        this.chooseBillingProvince.active = [];
        this.chooseShippingCountry.active = [];
        this.chooseShippingProvince.active = [];
        this.chooseSaleman.active = [];
        this.chooseDepartment.active = [];
        this.chooseAccountRef.active = [];
        this.chooseWorkplace.active = [];
    }
    getEmployee(employeeId: any): any {
        this.baseService.post(this.api_menu.System.Employee.query, { id: employeeId }).subscribe((responses: any) => {
            if (responses.length > 0) {
                this.employee = responses[0];
            }
            else {
                this.employee = {};
            }
            console.log(this.employee);
        });
    }
    /**
     * ng2-select
     */
    public items: Array<string> = ['Amsterdam', 'Antwerp', 'Athens', 'Barcelona',
        'Berlin', 'Birmingham', 'Bradford', 'Bremen', 'Brussels', 'Bucharest',];

    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;

    private get disabledV(): string {
        return this._disabledV;
    }

    private set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }


    public selected(value: any, selectName?: string): void {
        if (selectName == 'billingCountry') {
            this.partner.countryId = value.id;
            this.partner.provinceId = null;
            this.getProvinces(value.id, true);
        }
        if (selectName == 'billingProvince') {
            this.partner.provinceId = value.id;
        }
        if (selectName == 'shippingCountry') {
            this.partner.countryShippingId = value.id;
            this.partner.provinceShippingId = null;
            this.getProvinces(value.id, false);
        }
        if (selectName == 'shippingProvince') {
            this.partner.provinceShippingId = value.id;
        }
        if (selectName == 'saleman') {
            //this.partner.salePersonId = value.id;
            let user = this.users.find(x => x.id == value.id);
            if (user) {
                this.getEmployee(user.employeeId);
            }
        }
        if (selectName == 'category') {
            this.changePartnerGroup(value);
            this.isRequiredSaleman = this.checkRequireSaleman(this.partner.partnerGroup);
            console.log(this.partner.partnerGroup);
        }
    }
    checkRequireSaleman(partnerGroup: string): boolean {
        this.isShowSaleMan = false;
        if (partnerGroup.includes('CUSTOMER')) {
            this.isShowSaleMan = true;
        }
        else {
            this.isShowSaleMan = false;
        }
        if (partnerGroup == null) {
            return false;
        } else if (partnerGroup.includes('CUSTOMER') || partnerGroup.includes('ALL')) {

            return true;
        } else {
            return false;
        }

    }
    changePartnerGroup(value: { id: string; text: any; }) {
        this.partner.partnerGroup = '';
        if (value.id == "ALL") {
            this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
        }
        else {
            this.partnerGroupActives.push({ id: value.id, text: value.text });
            this.partnerGroupActives.forEach(element => {
                this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
            });
            if (this.partnerGroupActives.length > 0) {
                this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length - 1));
            }
        }
    }

    public removed(value: any, selectName?: string): void {
        if (selectName == 'billingCountry') {
            this.partner.countryId = null;
            this.partner.provinceId = null;
            this.billingProvinces = [];
            this.chooseBillingProvince.active = [];
        }
        if (selectName == 'billingProvince') {
            this.partner.provinceId = null;
        }
        if (selectName == 'shippingCountry') {
            this.partner.countryShippingId = null;
            this.partner.provinceShippingId = null;
            this.shippingProvinces = [];
            this.chooseShippingProvince.active = [];
        }
        if (selectName == 'shippingProvince') {
            this.partner.provinceShippingId = null;
        }
        if (selectName == 'category') {
            this.removePartnerGroup(value);
            this.isRequiredSaleman = this.checkRequireSaleman(this.partner.partnerGroup);
        }
        console.log('Removed value is: ', value);
    }
    removePartnerGroup(value: any) {
        var index = this.partnerGroupActives.indexOf(this.partnerGroupActives.find(x => x.id == value.id));
        if (index > -1) {
            this.partnerGroupActives.splice(index, 1);
        }
        this.partner.partnerGroup = null;
        if (value.id != "ALL") {
            this.partnerGroupActives.forEach(element => {
                this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
            });
            if (this.partnerGroupActives.length > 0) {
                this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length - 1));
            }
        }
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }

    deleteSaleman(saleman: Saleman) {

        this.selectedSaleman = new Saleman(saleman);
        this.deleteMessage = `Do you want to delete sale man ${saleman.saleman_ID}?`;
        this.confirmDeleteJobPopup.show();
    }

    onDeleteSaleman() {
        this.baseService.spinnerShow();
        this._catalogueRepo.deleteSaleman(this.selectedSaleman.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.confirmDeleteJobPopup.hide();
                })
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {
                        this.baseService.spinnerHide();
                        this.toastr.success(respone.message, 'Delete Success !');
                        $('#saleman-detail-modal').modal('hide');
                        this.getSalemanPagingByPartnerId(this.dataSearchSaleman);

                    }
                },
            );

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
                    console.log(this.saleMandetail);
                    if (this.saleMandetail.length > 0) {
                        for (let it of this.saleMandetail) {
                            if (it.status === true) {
                                it.status = "Active";
                            }
                            else {
                                it.status = "InActive";
                            }
                        }
                    }
                    this.totalItems = res.totalItems || 0;
                },
            );
    }


    onCreateSaleman(ngform: NgForm) {
        if (this.strSalemanCurrent.length > 0) {
            this.baseService.spinnerShow();
            const body = {
                saleman_Id: this.strSalemanCurrent,
                office: this.strOfficeCurrent,
                company: this.strOfficeCurrent,
                partnerId: this.partner.id,
                effectdate: this.saleManToAdd.effectDate == null ? null : formatDate(this.saleManToAdd.effectDate.startDate, 'yyyy-MM-dd', 'en'),
                description: this.saleManToAdd.description,
                status: this.selectedStatus.value,
                service: this.selectedService.id

            };
            this._catalogueRepo.createSaleman(body)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.status) {
                            this.baseService.spinnerHide();
                            $('#saleman-modal').modal('hide');
                            this.getSalemanPagingByPartnerId(this.dataSearchSaleman);

                        }

                    },
                );
        }
    }

    showDetailSaleMan(saleman: Saleman) {
        $('#saleman-detail-modal').modal('show');
        this.saleMantoView.description = saleman.description;
        this.saleMantoView.effectDate = saleman.effectDate == null ? null : formatDate(saleman.effectDate, 'yyyy-MM-dd', 'en');
        this.saleMantoView.statusString = saleman.status === true ? 'Active' : 'Inactive';
        this.saleMantoView.office = saleman.office;
        this.saleMantoView.service = saleman.service;
        this.saleMantoView.saleman_ID = saleman.saleman_ID;
        this.saleMantoView.id = saleman.id;
        this.saleMantoView.createDate = saleman.createDate;
        this.saleMantoView.userCreated = saleman.userCreated;
        this.saleMantoView.userCreated = saleman.userCreated;
    }

    sortBySaleMan(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.saleMandetail = this.sortService.sort(this.saleMandetail, sortData.sortField, sortData.order);
        }
    }


}
