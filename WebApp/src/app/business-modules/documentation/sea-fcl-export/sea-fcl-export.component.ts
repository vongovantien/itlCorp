import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { API_MENU } from 'src/constants/api-menu.const';
import { BaseService } from 'src/services-base/base.service';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { Router } from '@angular/router';
import { timeout } from 'q';
import { SortService } from 'src/app/shared/services/sort.service';

@Component({
    selector: 'app-sea-fcl-export',
    templateUrl: './sea-fcl-export.component.html',
    styleUrls: ['./sea-fcl-export.component.scss']
})
export class SeaFCLExportComponent implements OnInit {
    customers: any[];
    selectFilter: string = 'Job ID';
    criteria: any = {
    };
    searchString: string = '';
    notifyPartries: any[];
    salemans: any[];
    shipments: any[] = [];
    shipmentDetails: any[] = [];
    isReset= true;
    pager: PagerSetting = PAGINGSETTING;

    constructor(private baseServices: BaseService,
        private router:Router,
        private api_menu: API_MENU,
        private sortService: SortService) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    async ngOnInit() {
        this.pager.totalItems = 0;
        this.criteria.fromDate = this.selectedRange.startDate;
        this.criteria.toDate = this.selectedRange.endDate;
        await this.getShipments();
        this.getCustomers(null);
        this.getNotifyPartries(null);
        this.getUserInCharges(null);
    }
    async getShipmentDetails(jobId: any){
        const responses = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getByJob+"?jobId=" + jobId, false, false);
        this.shipmentDetails = responses;
        console.log(this.shipmentDetails);
        if(this.shipmentDetails != null){
            this.shipmentDetails = this.sortService.sort(this.shipmentDetails, 'hwbno', true);
        }
    }
    async getCustomers(searchText: any){
        let criteriaSearchColoader = { partnerGroup: PartnerGroupEnum.CUSTOMER, modeOfTransport : 'SEA', all: searchText };
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging+"?page=1&size=20", criteriaSearchColoader, false, false);
        if(partners != null){
            this.customers = partners.data;
            console.log(this.customers);
        }
    }
    async getNotifyPartries(searchText: any){
        let criteriaSearchColoader = { modeOfTransport : 'SEA', all: searchText };
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging+"?page=1&size=20", criteriaSearchColoader, false, false);
        if(partners != null){
            this.notifyPartries = partners.data;
            console.log(this.customers);
        }
    }
    
    async getUserInCharges(searchText: any){
        let responses = await this.baseServices.postAsync(this.api_menu.System.User_Management.paging +"?page=1&size=20", { all: searchText }, false, false);
        if(responses != null){
            this.salemans = responses.data;
        }
    }
    async getShipments(){
        let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransaction.paging+"?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.criteria,true, true);
        this.shipments = responses.data;
        this.pager.totalItems = responses.totalItems;
    }
    searchShipment(){
        this.criteria.jobNo ='';
        this.criteria.mawb = '';
        this.criteria.supplierName = '';
        this.criteria.agentName = '';
        this.criteria.hwbNo = '';
        this.criteria.fromDate = this.selectedRange.startDate;
        this.criteria.toDate = this.selectedRange.endDate;
        if(this.selectFilter === 'Job ID'){
            this.criteria.jobNo = this.searchString;
        }
        if(this.selectFilter === 'MBL No'){
            this.criteria.mawb = this.searchString;
        }
        if(this.selectFilter === 'Supplier'){
            this.criteria.supplierName = this.searchString;
        }
        if(this.selectFilter === 'Agent'){
            this.criteria.agentName = this.searchString;
        }
        if(this.selectFilter === 'HBL No'){
            this.criteria.hwbNo = this.searchString;
        }
        this.getShipments();
    }
    changeCustomer(keySearch: any){
        if(keySearch!==null && keySearch.length<3 && keySearch.length>0){
        return 0;
        }
        this.getCustomers(keySearch);
    }
    changeNotifyparty(keySearch: any){
        if(keySearch!==null && keySearch.length<3 && keySearch.length>0){
            return 0;
        }
        this.getNotifyPartries(keySearch);
    }
    changeSaleman(keySearch: any){
        if(keySearch!==null && keySearch.length<3 && keySearch.length>0){
            return 0;
        }
        this.getUserInCharges(keySearch);
    }
    resetSearch(){
        this.searchFilterActive = ['Job ID'];
        this.criteria = {};
        this.searchString = null;
        this.pager.currentPage = 1;
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
        this.criteria.fromDate = this.selectedRange.startDate;
        this.criteria.toDate = this.selectedRange.endDate;
        this.isReset = false;
        setTimeout(() => {
            this.isReset = true;
          }, 100);
        this.getShipments();
    }
    async setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.pageSize = pager.pageSize;
        this.pager.totalPages = pager.totalPages;
        await this.getShipments();
    }
    showDetail(item: { id: any; }){
        this.router.navigate(["/home/documentation/sea-fcl-export-create/",{ id: item.id }]);
    }
    isDesc = false;
    sortKey: string = "jobNo";
    sortShipment(property: string) {
      this.isDesc = !this.isDesc;
      this.sortKey = property;
      this.shipments = this.sortService.sort(this.shipments, property, this.isDesc);
    }
    sortShipmentDetail(property: string){
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.shipmentDetails = this.sortService.sort(this.shipmentDetails, property, this.isDesc);
    }
  
    /**
       * Daterange picker
       */
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
    public items: Array<string> = ['item 1', 'item 2', 'item 3', 'item 4', 'item 5'];
    searchFilters: Array<string> = ['Job ID', 'MBL No', 'Supplier', 'Agent', 'HBL No'];
    searchFilterActive = ['Job ID'];
    private value: any = ['Athens'];
    private _disabledV: string = '0';
    public disabled: boolean = false;

    public get disabledV(): string {
        return this._disabledV;
    }

    public set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }

    public selected(value: any): void {
        console.log('Selected value is: ', value);
        //this.selectFilter = value.id;
    }

    public removed(value: any): void {
        console.log('Removed value is: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }

    public typed(e){

    }

    public itemsToString(value:Array<any> = []):string {
    return value
        .map((item:any) => {
        return item.text;
        }).join(',');
    }

}
