import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as lodash from 'lodash';

@Component({
  selector: 'app-master-bill',
  templateUrl: './master-bill.component.html',
  styleUrls: ['./master-bill.component.scss']
})
export class MasterBillComponent implements OnInit {
    terms: any[];
    shipmentTypes: any[];
    serviceTypes: any[];
    billOfLadingTypes: any[];
    portOfLoadings: any[];
    portOfDestination: any[];

    constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU) { }

    async ngOnInit() {
       this.getShipmentCommonData();
    }

    async getShipmentCommonData(){
        const data = await shipmentHelper.getShipmentCommonData(this.baseServices,this.api_menu);
        this.billOfLadingTypes = lodash.map(data.billOfLadings,function(x){return {"text":x.displayName,"id":x.value}});
        this.serviceTypes = lodash.map(data.serviceTypes,function(x){return {"text":x.displayName,"id":x.value}});
        this.terms = lodash.map(data.freightTerms,function(x){return {"text":x.displayName,"id":x.value}});
        this.shipmentTypes = lodash.map(data.shipmentTypes,function(x){return {"text":x.displayName,"id":x.value}});
    }



    // async getBillofLadingTypes() {
    //     const response = await this.baseServices.getAsync(this.api_menu.Documentation.Terminology.GetBillofLoadingTypes, false, false);
    //     if(response){
    //         this.billOfLadingTypes = response.map(x=>({"text":x.displayName,"id":x.value}));
    //     }
    //     else{
    //         this.billOfLadingTypes = [];
    //     }
    // }

    // async getServiceTypes() {
    //     const response = await this.baseServices.getAsync(this.api_menu.Documentation.Terminology.GetServiceTypes, false, false);
    //     if(response){
    //         this.serviceTypes = response.map(x=>({"text":x.displayName,"id":x.value}));
    //     }
    //     else{
    //         this.serviceTypes = [];
    //     }
    // }

    // async getFreightTerms(){
    //     const response = await this.baseServices.getAsync(this.api_menu.Documentation.Terminology.GetFreightTerms, false, false);
    //     if(response){
    //         this.terms = response.map(x=>({"text":x.displayName,"id":x.value}));
    //     }
    //     else{
    //         this.terms = [];
    //     }
    // }
    // async getShipmentTypes(){
    //     const response = await this.baseServices.getAsync(this.api_menu.Documentation.Terminology.GetShipmentTypes, false, false);
    //     if(response){
    //         console.log(response);
    //         this.shipmentTypes = response.map(x=>({"text":x.displayName,"id":x.value}));
    //     }
    //     else{
    //         this.shipmentTypes = [];
    //     }
    // }

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
    public items: Array<string> = ['Option 1', 'Option 2', 'Option 3', 'Option 4',
        'Option 5', 'Option 6', 'Option 7', 'Option 8', 'Option 9', 'Option 10',];

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
