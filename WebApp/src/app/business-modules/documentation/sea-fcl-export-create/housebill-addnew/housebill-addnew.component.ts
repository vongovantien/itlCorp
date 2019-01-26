import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
// import { PaginationComponent } from 'ngx-bootstrap';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { SystemConstants } from 'src/constants/system.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import * as lodash from 'lodash';
import * as moment from 'moment';
import {CsTransactionDetail} from 'src/app/shared/models/document/csTransactionDetail';

@Component({
  selector: 'app-housebill-addnew',
  templateUrl: './housebill-addnew.component.html',
  styleUrls: ['./housebill-addnew.component.scss']
})
export class HousebillAddnewComponent implements OnInit {
    pager: PagerSetting = PAGINGSETTING;


    listCustomers:any=[];
    listSaleMan:any=[];
    listShipper:any=[];
    listConsignee:any=[];
    listNotifyParty:any=[];
    listHouseBillLadingType:any=[];
    listCountryOrigin:any=[];
    listPortOfLoading:any=[];
    listPortOfDischarge:any=[];
    listFreightPayment:any=[];
    listFreightPayableAt:any=[];
    listFowardingAgent:any=[];
    listDeliveryOfGoods:any=[];
    listNumberOfOriginBL:any=[{id:1,text:'1'},{id:2,text:'2'},{id:3,text:'3'}];
    listTypeOfMove:any=[];
    listTypeOfService:any=[];

    /**
     * House Bill Variables 
     */

    HouseBillToAdd :CsTransactionDetail = new CsTransactionDetail();


  constructor(
    private baseServices: BaseService, 
    private api_menu: API_MENU,
    private sortService: SortService
  ) { }

  ngOnInit() {
       this.getListCustomers();
      this.getShipmentCommonData();
     
     console.log(this.HouseBillToAdd);
  }

  select(form){
      console.log(form)
  }

  async getShipmentCommonData(){
    const data = await shipmentHelper.getShipmentCommonData(this.baseServices,this.api_menu);
    this.listTypeOfService = dataHelper.prepareNg2SelectData(data.serviceTypes,'value','displayName'); 
    this.listTypeOfMove = dataHelper.prepareNg2SelectData(data.typeOfMoves,'value','displayName');  
    this.listHouseBillLadingType = dataHelper.prepareNg2SelectData(data.billOfLadings,'value','displayName'); 
    this.listFreightPayment = dataHelper.prepareNg2SelectData(data.freightTerms,'value','displayName'); 
  }

  public getListCustomers(search_key:string=null){
      var key = "";
      if(search_key!==null && search_key.length<3 && search_key.length>0){
        return 0;
      }else{
          key = search_key;
      }      
      this.baseServices.post(this.api_menu.Catalogue.PartnerData.paging+"?page=" + 1 + "&size=" + 20, { partnerGroup: PartnerGroupEnum.CUSTOMER ,inactive:false,all:key}).subscribe(res=>{
        var data = res['data']
        this.listCustomers = lodash.map(data, function(d){           
            return {partnerID:d['id'],nameABBR:d['shortName'],nameEN:d['partnerNameEn'],taxCode:d['taxCode']}
        });       
      });
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

    public typed(value: any): void {
    console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
    this.value = value;
    }

    save(){
        console.log(this.HouseBillToAdd);
        console.log(this.listCustomers);
    }
}
