import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseService } from 'src/app/shared/services/base.service';
import { ToastrService } from 'ngx-toastr';
import { API_MENU } from 'src/constants/api-menu.const';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { NgForm } from '@angular/forms';
import { SelectComponent } from 'ng2-select';
import { SortService } from 'src/app/shared/services/sort.service';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from "rxjs/operators";
import { AppList } from "src/app/app.list";
import { Saleman } from 'src/app/shared/models/catalogue/saleman.model';
import { formatDate } from '@angular/common';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { SalemanPopupComponent } from '../components/saleman-popup.component';
import { SystemRepo } from 'src/app/shared/repositories';
declare var $: any;

@Component({
    selector: 'app-partner-data-detail',
    templateUrl: './partner-data-detail.component.html',
    styleUrls: ['./partner-data-detail.component.scss']
})
export class PartnerDataDetailComponent extends AppList {
    departments: any[];
    users: any[] = [];
    departmentActive: any[] = [];
    partner: Partner = new Partner();
    parentCustomers: any[];
    parentCustomerActive: any[] = [];
    workPlaces: any[];
    workPlaceActive: any[] = [];
    saleMans: any[] = [];
    countries: any[];
    billingCountryActive: any[] = [];
    billingProvinces: any[];
    billingProvinceActive: any[];
    shippingCountryActive: any[] = [];
    shippingProvinces: any[];
    shippingProvinceActive: any[] = [];
    partnerGroups: any[];
    partnerGroupActives: any[] = [];
    salemanActive: any[] = [];
    activeNg = true;
    isRequiredSaleman = false;
    employee: any = {};
    dataSearchSaleman: any = {};
    titleConfirmDelete = "You want to delete this Partner?";
    saleMandetail: any[] = [];
    headerSaleman: CommonInterface.IHeaderTable[];
    services: any[] = [];
    status: CommonInterface.ICommonTitleValue[] = [];
    offices: any[] = [];
    saleManToAdd: Saleman = new Saleman();
    strOfficeCurrent: any = '';
    strSalemanCurrent: any = '';
    selectedStatus: any = {};
    selectedService: any = {};
    deleteMessage: string = '';
    selectedSaleman: Saleman = null;
    saleMantoView: Saleman = new Saleman();
    isShowSaleMan: boolean = false;


    @Output() isCloseModal = new EventEmitter();
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild(SalemanPopupComponent, { static: false }) poupSaleman: SalemanPopupComponent;
    @ViewChild('formAddEdit', { static: false }) form: NgForm;
    @ViewChild('chooseBillingCountry', { static: false }) public chooseBillingCountry: SelectComponent;
    @ViewChild('chooseBillingProvince', { static: false }) public chooseBillingProvince: SelectComponent;
    @ViewChild('chooseShippingCountry', { static: false }) public chooseShippingCountry: SelectComponent;
    @ViewChild('chooseShippingProvince', { static: false }) public chooseShippingProvince: SelectComponent;



    constructor(private route: ActivatedRoute,
        private router: Router,
        private baseService: BaseService,
        private toastr: ToastrService,
        private api_menu: API_MENU,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private sortService: SortService) {

        super();
        this.requestList = this.getSalemanPagingByPartnerId;
    }
    getDataCombobox() {
        this.status = this.getStatus();
        this.selectedStatus = this.status[1];
        this.getService();
        this.getOffice();
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



    async ngOnInit() {
        this.headerSaleman = [
            { title: '', field: '', sortable: false },
            { title: 'Saleman', field: 'saleman_ID', sortable: true },
            { title: 'Service', field: 'service', sortable: true },
            { title: 'Office', field: 'office', sortable: true },
            { title: 'Company', field: 'company', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'ModifiedDate', field: 'modifiedDate', sortable: true }
        ];

        this.route.params.subscribe(async (prams: any) => {
            if (!!prams.id) {
                this.partner.id = prams.id;
                this.dataSearchSaleman.partnerId = this.partner.id;
                await this.getComboboxData();
                await this.getParnerDetails();
                this.getSalemanPagingByPartnerId(this.dataSearchSaleman);
            }

        });
        this.getDataCombobox();

    }
    async getParnerDetails() {
        this.partner = await this.baseService.getAsync(this.api_menu.Catalogue.PartnerData.getById + this.partner.id, false, true);
        this.getReferenceData();
    }

    getReferenceData(): any {
        if (this.partner.partnerGroup.includes('CUSTOMER') || this.partner.partnerGroup.includes('ALL')) {
            this.isRequiredSaleman = true;
        }
        let index = this.saleMans.findIndex(x => x.id === this.partner.salePersonId)
        if (index > -1) { this.salemanActive = [this.saleMans.find(x => x.id === this.partner.salePersonId)]; }
        if (this.partner.partnerGroup.includes('CUSTOMER')) {
            this.isRequiredSaleman = true;
            this.isShowSaleMan = true;
        }
        console.log(this.isRequiredSaleman);
        console.log(this.partner.salePersonId);
        this.getPartnerGroupActives(this.partner.partnerGroup.split(';'));
        index = this.departments.findIndex(x => x.id === this.partner.departmentId);
        if (index > -1) { this.departmentActive = [this.departments[index].id]; }
        index = this.parentCustomers.findIndex(x => x.id === this.partner.parentId);
        if (index > -1) { this.parentCustomerActive = [this.parentCustomers[index]]; }
        index = this.workPlaces.findIndex(x => x.id === this.partner.workPlaceId);
        if (index > -1) { this.workPlaceActive = [this.workPlaces[index]]; }
        index = this.countries.findIndex(x => x.id === this.partner.countryId);
        if (index > - 1) {
            this.billingCountryActive = [this.countries[index]];
            this.getProvincesByCountry(this.countries[index].id, true);
        }
        index = this.countries.findIndex(x => x.id === this.partner.countryShippingId);
        if (index > -1) {
            this.shippingCountryActive = [this.countries[index]];
            this.getProvincesByCountry(this.countries[index].id, false);
        }
        if (this.partner.salePersonId) {
            const user = this.users.find(x => x.id === this.partner.salePersonId);
            if (user) {
                this.getEmployee(user.employeeId);
            }
        }
    }
    getPartnerGroupActives(arg0: string[]): any {
        if (arg0.length > 0) {
            for (let i = 0; i < arg0.length; i++) {
                let group = this.partnerGroups.find(x => x.id == arg0[i]);
                if (group) {
                    this.partnerGroupActives.push(group);
                }
            }
        }
        this.activeNg = false;
        setTimeout(() => {
            this.activeNg = true;
        }, 200);
    }
    async getComboboxData() {
        await this.getPartnerGroups();
        await this.getCountries();
        await this.getSalemans();
        await this.getWorkPlaces();
        await this.getParentCustomers();
        await this.getDepartments();
    }
    async getDepartments() {
        let respones = await this.baseService.getAsync(this.api_menu.Catalogue.PartnerData.getDepartments);
        if (respones != null) {
            this.departments = respones.map(x => ({ "text": x.name, "id": x.id }));
        }
    }

    async getParentCustomers() {
        let respones = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: 3 });
        if (respones != null) {
            this.parentCustomers = respones.map(x => ({ "text": x.partnerNameVn, "id": x.id }));
        }
    }
    async getWorkPlaces() {
        let responses = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.query, { placeType: 2 });
        if (responses != null) {
            this.workPlaces = responses.map(x => ({ "text": x.code + ' - ' + x.nameVn, "id": x.id }));
        }
    }
    async getSalemans() {
        let responses = await this.baseService.getAsync(this.api_menu.System.User_Management.getAll);
        if (responses != null) {
            this.users = responses;
            this.saleMans = responses.map(x => ({ "text": x.username, "id": x.id }));

            if (this.saleMans.length != null) {
                this.saleMans = this.sortService.sort(this.saleMans, 'text', true);
            }
        }
    }
    async getCountries() {
        let responses = await this.baseService.getAsync(this.api_menu.Catalogue.Country.getAllByLanguage);
        if (responses != null) {
            this.countries = responses.map(x => ({ "text": x.name, "id": x.id }));
        }
        else {
            this.countries = [];
        }
    }
    async getPartnerGroups() {
        let responses = await this.baseService.getAsync(this.api_menu.Catalogue.partnerGroup.getAll);
        if (responses != null) {
            this.partnerGroups = responses.map(x => ({ "text": x.id, "id": x.id }));
        }
    }
    async getProvincesByCountry(countryId: number, isBilling: boolean) {
        let url = this.api_menu.Catalogue.CatPlace.getProvinces;
        if (countryId != undefined) {
            url = url + "?countryId=" + countryId;
        }
        let responses = await this.baseService.getAsync(url, false, false);
        if (responses != null) {
            let index = -1;
            if (isBilling) {
                this.billingProvinces = responses.map(x => ({ "text": x.name_VN, "id": x.id }));
                index = this.billingProvinces.findIndex(x => x.id == this.partner.provinceId);
                if (index > -1) this.billingProvinceActive = [this.billingProvinces[index]];
            }
            else {
                this.shippingProvinces = responses.map(x => ({ "text": x.name_VN, "id": x.id }));
                index = this.shippingProvinces.findIndex(x => x.id == this.partner.provinceShippingId);
                if (index > -1) this.shippingProvinceActive = [this.shippingProvinces[index]];
            }
        }
        else {
            this.billingProvinces = [];
            this.shippingProvinces = [];
        }
    }
    onSubmit() {
        if (this.partner.countryId == null || this.partner.provinceId == null || this.partner.countryShippingId == null || this.partner.provinceShippingId == null || this.partner.departmentId == null) {
            return;
        }
        if (this.form.valid) {
            // this.partner.accountNo = this.partner.id = this.partner.taxCode;
            if (this.saleMandetail.length === 0) {
                if (this.isShowSaleMan) {
                    this.toastr.error('Please add saleman and service for customer!');
                    return;
                }
            }
            if (this.isRequiredSaleman) {
                this.partner.salePersonId = this.saleMans[0].id;
                this.update();
            }
            else {
                if (this.isRequiredSaleman === false) {
                    this.partner.accountNo = this.partner.id = this.partner.taxCode;
                    this.update();
                }
            }
        }
    }

    update(): any {
        this.baseService.spinnerShow();
        this.baseService.put(this.api_menu.Catalogue.PartnerData.update + this.partner.id, this.partner).subscribe((response: any) => {
            this.baseService.spinnerHide();
            this.baseService.successToast(response.message);
            this.router.navigate(["/home/catalogue/partner-data"]);
        }, err => {
            this.baseService.handleError(err);
            this.baseService.spinnerHide();
        });
    }

    onDelete(event) {
        if (event) {
            this.baseService.spinnerShow();
            this.baseService.delete(this.api_menu.Catalogue.PartnerData.delete + this.partner.id).subscribe((response: any) => {
                this.baseService.spinnerHide();
                this.baseService.successToast(response.message);
                this.router.navigate(["/home/catalogue/partner-data", { id: this.partner.id }]);
            }, err => {
                this.baseService.spinnerHide();
                this.baseService.handleError(err);
            });
        }
    }
    /**
     * ng2-select
     */
    private value: any = {};
    private _disabledV: string = '0';
    private disabled: boolean = false;

    private get disabledV(): string {
        return this._disabledV;
    }

    private set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }

    public selected(value: any, selectName): void {
        console.log('Selected value is: ', value);
        if (selectName === 'billingCountry') {
            this.partner.countryId = value.id;
            this.getProvincesByCountry(this.partner.countryId, true);
        }
        if (selectName === 'shippingCountry') {
            this.partner.countryShippingId = value.id;
            this.getProvincesByCountry(this.partner.countryShippingId, false);
        }
        if (selectName === 'billingProvince') {
            this.partner.provinceId = value.id;
        }
        if (selectName === 'shippingProvince') {
            this.partner.provinceShippingId = value.id;
        }
        if (selectName === 'saleman') {
            // this.partner.salePersonId = value.id;
            const user = this.users.find(x => x.id === value.id);
            if (user) {
                this.getEmployee(user.employeeId);
            }
            if (this.partner.partnerGroup.includes('CUSTOMER')) {
                this.isRequiredSaleman = true;
            }
        }
        if (selectName === 'category') {
            this.partner.partnerGroup = '';
            if (value.id === "ALL") {
                this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
            } else {
                this.partnerGroupActives.push({ id: value.id, text: value.text });
                this.partnerGroupActives.forEach(element => {
                    this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
                });
                if (this.partnerGroupActives.length > 0) {
                    this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length - 1));
                }
            }
            this.isRequiredSaleman = this.checkRequireSaleman(this.partner.partnerGroup);
        }
    }
    checkRequireSaleman(partnerGroup: string): boolean {
        this.isShowSaleMan = false;
        if (partnerGroup != null) {
            if (partnerGroup.includes('CUSTOMER')) {
                this.isShowSaleMan = true;
            }
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
    getEmployee(employeeId: any) {
        this._systemRepo.getEmployeeByemployeeid(employeeId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.employee = res;
                },
            );
    }

    // getEmployee(employeeId: any): any {
    //     this.baseService.post(this.api_menu.System.Employee.query, { id: employeeId }).subscribe((responses: any) => {
    //         if (responses.length > 0) {
    //             this.employee = responses[0];
    //         }
    //         else {
    //             this.employee = {};
    //         }
    //         console.log(this.employee);
    //     }, err => {
    //         this.baseService.handleError(err);
    //     });
    // }

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
            var index = this.partnerGroupActives.indexOf(this.partnerGroupActives.find(x => x.id == value.id));
            if (index > -1) {
                this.partnerGroupActives.splice(index, 1);
            }
            this.partner.partnerGroup = '';
            if (value.id != "ALL") {
                this.partnerGroupActives.forEach(element => {
                    this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
                });
                if (this.partnerGroupActives.length > 0) {
                    this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length - 1));
                }
            }
        }
        if (selectName == 'saleman') {
            this.employee = {};
        }
        this.isRequiredSaleman = this.checkRequireSaleman(this.partner.partnerGroup);
        console.log('Removed value is: ', value);
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
                .pipe(catchError(this.catchError), finalize(() => {
                    this.baseService.spinnerHide();
                }))
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

    showPopupSaleman() {
        this.poupSaleman.show();
    }



}
