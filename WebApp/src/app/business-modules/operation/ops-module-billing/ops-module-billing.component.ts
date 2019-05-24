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
    criteria: any = {
        //transactionType: TransactionTypeEnum.CustomClearance
    };
    
    DataStorage:Object = null;

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
        this.baseServices.dataStorage.subscribe(data=>{
            this.DataStorage = data;
        });

    }

    ngOnInit() {
        this.getShipmentCommonData();
        this.getUserInCharges();
        this.getCustomers();
    }
    async getUserInCharges(){
        let responses = await this.baseServices.postAsync(this.api_menu.System.User_Management.paging +"?page=1&size=20", { all: null }, false, false);
        if(responses != null){
            this.userInCharges = responses.data;
            console.log(this.userInCharges);
        }
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
        let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransaction.paging+"?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.criteria,true, true);
        this.shipments = responses.data;
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
