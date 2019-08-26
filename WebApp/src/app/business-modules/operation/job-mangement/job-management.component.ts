import { Component, OnInit, ViewChild } from '@angular/core';
import moment from 'moment/moment';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { SortService } from 'src/app/shared/services/sort.service';
import { JobConstants } from 'src/constants/job.const';
import { DataService } from 'src/app/shared/services';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';


@Component({
    selector: 'app-job-mangement',
    templateUrl: './job-management.component.html',
    styleUrls: ['./job-management.component.scss']
})
export class JobManagementComponent implements OnInit {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;

    jobStatusConts = {
        overdued: JobConstants.Overdued,
        processing: JobConstants.Processing,
        inSchedule: JobConstants.InSchedule,
        pending: JobConstants.Pending,
        finish: JobConstants.Finish,
        canceled: JobConstants.Canceled,
        warning: JobConstants.Warning
    }
    productServices: any[] = [];
    serviceModes: any[] = [];
    shipmentModes: any[] = [];
    shipments: any[] = [];
    userInCharges: any[] = [];
    customers: any[] = [];
    pager: PagerSetting = PAGINGSETTING;
    searchString: string = '';
    criteria: any = {
        //transactionType: TransactionTypeEnum.CustomClearance
    };
    totalInProcess = 0;
    totalComplete = 0;
    totalOverdued = 0;
    totalCanceled = 0;
    selectedFilter = 'Job ID';
    searchFilterActive = ['Job ID'];
    isReset = true;
    isFilterTime = false;
    customClearances: any[] = [];
    itemToDelete: any = null;

    isDesc = true;
    sortKey: string = "clearanceNo";
    isCusDesc = true;

    deleteMessage: string = '';

    searchFilters: Array<string> = ['Job ID', 'HBL'];
    selectedRange: any;
    selectedDate: any;
    keepCalendarOpeningWithRange: true;
    maxDate: moment.Moment = moment().endOf('month');
    ranges: any = {
        Today: [moment(), moment()],
        Yesterday: [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
        'Last 7 Days': [moment().subtract(6, 'days'), moment()],
        'Last 30 Days': [moment().subtract(29, 'days'), moment()],
        'This Month': [moment().startOf('month'), moment().endOf('month')],
        'Last Month': [
            moment()
                .subtract(1, 'month')
                .startOf('month'),
            moment()
                .subtract(1, 'month')
                .endOf('month')
        ]
    };

    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4',
        'option 5', 'option 6', 'option 7'];

    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;


    constructor(private baseServices: BaseService,
        private sortService: SortService,
        private api_menu: API_MENU,
        private _dataService: DataService
    ) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    ngOnInit() {
        this.pager.currentPage = 1;
        this.pager.totalItems = 0;
        this.criteria.serviceDateFrom = moment().startOf('month');
        this.criteria.serviceDateTo = moment().endOf('month');
        this.getShipmentCommonData();
        this.getUserInCharges();
        this.getCustomers();
        this.getShipments();
    }
    async getUserInCharges() {
        const responses = await this.baseServices.postAsync(this.api_menu.System.User_Management.paging + "?page=1&size=20", { all: null }, false, false);
        if (responses != null) {
            this.userInCharges = responses.data;
            console.log(this.userInCharges);
        }
    }
    async setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.pageSize = pager.pageSize;
        this.pager.totalPages = pager.totalPages;
        await this.getShipments();
    }

    searchShipment() {
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;
        this.criteria.jobNo = null;
        this.criteria.hwbno = null;
        this.criteria.all = null;

        if (this.isFilterTime) {
            this.criteria.serviceDateFrom = this.selectedRange.startDate;
            this.criteria.serviceDateTo = this.selectedRange.endDate;
        } else {
            this.criteria.serviceDateFrom = null;
            this.criteria.serviceDateTo = null;
        }
        if (this.selectedFilter === 'Job ID') {
            this.criteria.jobNo = this.searchString;
        } else if (this.selectedFilter === 'HBL') {
            this.criteria.hwbno = this.searchString;
        } else {
            this.criteria.all = this.searchString;
        }
        this.getShipments();
    }

    resetSearch() {
        this.selectedFilter = 'Job ID';
        this.searchFilterActive = ['Job ID'];
        this.criteria = {
        };
        this.searchString = null;
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
        this.criteria.serviceDateFrom = this.selectedRange.startDate;
        this.criteria.serviceDateTo = this.selectedRange.endDate;
        this.isFilterTime = false;
        this.isReset = false;
        setTimeout(() => {
            this.isReset = true;
        }, 100);
        this.getShipments();
    }

    async showCustomClearance(jobNo: string) {
        const responses = await this.baseServices.getAsync(this.api_menu.Operation.CustomClearance.getByJob + "?jobNo=" + jobNo, false, true);
        if (responses) {
            this.customClearances = this.sortService.sort(responses, 'clearanceNo', true);;
        } else {
            this.customClearances = [];
        }
    }

    async confirmDelete(item: any) {
        this.itemToDelete = item;
        const respone = await this.baseServices.getAsync(this.api_menu.Documentation.Operation.checkAllowDelete + item.id, false, true);
        if (respone) {
            this.confirmDeleteJobPopup.show();
            this.deleteMessage = `Do you want to delete job No ${item.jobNo}?`;
        } else {
            this.canNotDeleteJobPopup.show();
        }
    }

    async deleteJob() {
        const respone = await this.baseServices.deleteAsync(this.api_menu.Documentation.Operation.delete + this.itemToDelete.id, true, true);
        if (respone.status) {
            this.confirmDeleteJobPopup.hide();
            this.getShipments();
        }
    }

    sort(property: string) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.shipments = this.sortService.sort(this.shipments, property, this.isDesc);
    }

    sortClearance(property: string) {
        this.isCusDesc = !this.isCusDesc;
        this.sortKey = property;
        this.customClearances = this.sortService.sort(this.customClearances, property, this.isCusDesc);
    }

    async getCustomers() {
        const criteriaSearchColoader = { partnerGroup: PartnerGroupEnum.CUSTOMER, all: null };
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.query, criteriaSearchColoader, false, false);
        if (partners != null) {
            this.customers = partners;
            this._dataService.setData('lstPartners', this.customers);
        }
    }

    async getShipments() {
        const responses = await this.baseServices.postAsync(this.api_menu.Documentation.Operation.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.criteria, true, true);
        if (responses.data != null) {
            this.shipments = responses.data.opsTransactions;
            this.totalInProcess = responses.data.toTalInProcessing;
            this.totalOverdued = responses.data.totalOverdued;
            this.totalComplete = responses.data.toTalFinish;
            this.totalCanceled = responses.data.totalCanceled;
        } else {
            this.shipments = [];
            this.totalInProcess = 0;
            this.totalOverdued = 0;
            this.totalComplete = 0;
            this.totalCanceled = 0;
        }
        this.pager.totalItems = responses.totalItems;
    }
    async getShipmentCommonData() {
        const data = await shipmentHelper.getOPSShipmentCommonData(this.baseServices, this.api_menu);
        this.productServices = dataHelper.prepareNg2SelectData(data.productServices, 'value', 'displayName');
        this.serviceModes = dataHelper.prepareNg2SelectData(data.serviceModes, 'value', 'displayName');
        this.shipmentModes = dataHelper.prepareNg2SelectData(data.shipmentModes, 'value', 'displayName');
    }


    private get disabledV(): string {
        return this._disabledV;
    }

    private set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }

    public selected(value: any): void {
        console.log('Selected value is: ', value);
    }

    public removed(value: any): void {
        console.log('Removed value is: ', value);
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }

}
