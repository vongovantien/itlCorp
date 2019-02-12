import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { API_MENU } from 'src/constants/api-menu.const';
import { BaseService } from 'src/services-base/base.service';

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
    notifyPartries: any[];
    salemans: any[];
    shipments: any[] = [];
    shipmentDetails: any[] = [];

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    async ngOnInit() {
        await this.getShipments();
        this.getCustomers(null);
        this.getNotifyPartries(null);
        this.getUserInCharges(null);
    }
    async getShipmentDetails(jobId: any){
        const responses = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getByJob+"?jobId=" + jobId, false, false);
        this.shipmentDetails = responses;
    }
    async getCustomers(searchText: any){
        let criteriaSearchColoader = { partnerGroup: PartnerGroupEnum.CUSTOMER, modeOfTransport : 'SEA', all: searchText, inactive: false };
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging+"?page=1&size=20", criteriaSearchColoader, false, false);
        if(partners != null){
            this.customers = partners.data;
            console.log(this.customers);
        }
    }
    async getNotifyPartries(searchText: any){
        let criteriaSearchColoader = { modeOfTransport : 'SEA', all: searchText, inactive: false };
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging+"?page=1&size=20", criteriaSearchColoader, false, false);
        if(partners != null){
            this.notifyPartries = partners.data;
            console.log(this.customers);
        }
    }
    
    async getUserInCharges(searchText: any){
        let users = await this.baseServices.getAsync(this.api_menu.System.User_Management.getAll, false, false);
        if(users != null){
            this.salemans = users;
        }
    }
    async getShipments(){
        let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransaction.paging+"?page=1&size=20", this.criteria,true, true);
        this.shipments = responses;
    }
    searchShipment(){
        this.criteria.fromDate = this.selectedRange.startDate;
        this.criteria.toDate = this.selectedRange.endDate;
        if(this.selectFilter === 'Job ID'){
            this.criteria.jobNo = this.criteria.searchString;
        }
        if(this.selectFilter === 'MBL No'){
            this.criteria.mawb = this.criteria.searchString;
        }
        if(this.selectFilter === 'Supplier'){
            this.criteria.supplierName = this.criteria.searchString;
        }
        if(this.selectFilter === 'Agent'){
            this.criteria.agentName = this.criteria.searchString;
        }
        if(this.selectFilter === 'HBL No'){
            this.criteria.hwbNo = this.criteria.searchString;
        }
        this.getShipments();
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
        this.selectFilter = value.id;
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
