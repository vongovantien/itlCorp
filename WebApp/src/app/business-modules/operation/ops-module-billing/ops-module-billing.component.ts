import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { TransactionTypeEnum } from 'src/app/shared/enums/transaction-type.enum';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { SortService } from 'src/app/shared/services/sort.service';

@Component({
    selector: 'app-ops-module-billing',
    templateUrl: './ops-module-billing.component.html',
    styleUrls: ['./ops-module-billing.component.scss']
})
export class OpsModuleBillingComponent implements OnInit {
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
    
    constructor(private baseServices: BaseService,
        private sortService: SortService,
        private api_menu: API_MENU) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    ngOnInit() {
        this.getShipmentCommonData();
        this.getUserInCharges();
        this.getCustomers();
        this.getShipments();
    }
    async getUserInCharges(){
        let responses = await this.baseServices.postAsync(this.api_menu.System.User_Management.paging +"?page=1&size=20", { all: null }, false, false);
        if(responses != null){
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
    searchShipment(){
        this.pager.totalItems = 0;
        this.criteria.mblno =null;
        this.criteria.hblno = null;
        this.criteria.customerId = null;
        this.criteria.fieldOps = null;
        this.criteria.all = null;
        if(this.isFilterTime){
            this.criteria.serviceDateFrom = this.selectedRange.startDate;
            this.criteria.serviceDateTo = this.selectedRange.endDate;
        }
        else{
            this.criteria.serviceDateFrom = null;
            this.criteria.serviceDateTo = null;
        }
        if(this.selectedFilter === 'Job ID'){
            this.criteria.jobNo = this.searchString;
        }
        else if(this.selectedFilter === 'HBL'){
            this.criteria.mawb = this.searchString;
        }
        else{
            this.criteria.all = this.searchString;
        }
        this.getShipments();
    }
    resetSearch(){
        this.selectedFilter = 'Job ID';
        this.searchFilterActive = ['Job ID'];
        this.criteria = {
        };
        this.searchString = null;
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
        this.criteria.serviceDateFrom = this.selectedRange.startDate;
        this.criteria.serviceDateTo = this.selectedRange.endDate;
        this.isReset = false;
        setTimeout(() => {
            this.isReset = true;
          }, 100);
        this.getShipments();
    }
    async showCustomClearance(jobId){
        let responses = await this.baseServices.getAsync(this.api_menu.Documentation.CustomClearance.getByJob + "?jobId=" + jobId, false, true);
        if(responses){
            this.customClearances = responses;
        }
        else{
            this.customClearances = [];
        }
    }
    isDesc = true;
    sortKey: string = "";
    sort(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.shipments = this.sortService.sort(this.shipments, property, this.isDesc);
    }

    async getCustomers(){
        let criteriaSearchColoader = { partnerGroup: PartnerGroupEnum.CUSTOMER, all: null };
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.query, criteriaSearchColoader, false, false);
        if(partners != null){
            this.customers = partners;
            console.log(this.customers);
        }
    }
    async getShipments(){
        let responses = await this.baseServices.postAsync(this.api_menu.Documentation.Operation.paging+"?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.criteria,true, true);
        if(responses.data != null){
            this.shipments = responses.data.opsTransactions;
        }
        else{
            this.shipments = [];
        }
        console.log(this.shipments);
        this.pager.totalItems = responses.totalItems;
    }
    async getShipmentCommonData() {
        const data = await shipmentHelper.getOPSShipmentCommonData(this.baseServices, this.api_menu);
        this.productServices = dataHelper.prepareNg2SelectData(data.productServices, 'value', 'displayName');
        this.serviceModes = dataHelper.prepareNg2SelectData(data.serviceModes, 'value', 'displayName');
        this.shipmentModes = dataHelper.prepareNg2SelectData(data.shipmentModes, 'value', 'displayName');
    }
    /**
     * Daterange picker
     */
    searchFilters: Array<string> = ['Job ID', 'HBL'];
    selectedRange: any;
    selectedDate: any;
    keepCalendarOpeningWithRange: true;
    maxDate: moment.Moment = moment();
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

    /**
   * ng2-select
   */
    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4',
        'option 5', 'option 6', 'option 7'];

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
