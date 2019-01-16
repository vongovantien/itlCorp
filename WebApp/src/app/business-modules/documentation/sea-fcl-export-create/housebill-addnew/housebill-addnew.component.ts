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

@Component({
  selector: 'app-housebill-addnew',
  templateUrl: './housebill-addnew.component.html',
  styleUrls: ['./housebill-addnew.component.scss']
})
export class HousebillAddnewComponent implements OnInit {
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
    MasterBillOfLading:String = null;
    Customer:String = null;
    SaleMan:String = null;
    Shipper:String = null;
    Consignee:String = null;
    NotifyParty:String = null;
    HouseBillOfLadingNo:String = null;
    HouseBullOfLadingType:String = null;
    BookingNo:String = null;
    LocalVesselAndVoyNo:String = null;
    OceanVesselAndVoyNo:String = null;
    CountryOrigin:String = null;
    PlaceOfReceipt:String = null;
    PortOfLoading:String = null;
    PortOfDischarge:String = null;
    PlaceOfDelivery:String = null;
    FinalDestination:String = null;
    FreightPayment:String = null;
    ClosingDate:Date = null;
    SellingDate:Date = null;
    FreightPayableAt:String = null;
    ForwardingAgent:String = null;
    NumberOfOriginBL:Number = null;
    PlaceDateIssueHBL:String = null;
    ReferenceNo:String = null;
    ExportReferenceNo:String = null;
    DeliveryOfGoods:String = null;
    TypeOfMove :String = null;
    PurchaseOrderNo : String = null;
    TypeOfService : String = null;
    DescriptionOfGoods: String = null;
    ShippingMark : String = null;
    InWord:String = null;
    OnBoardStatus:String = null;

  constructor(
    private baseServices: BaseService, 
    private api_menu: API_MENU,
    private sortService: SortService
  ) { }

  ngOnInit() {
      this.getShipmentCommonData();
  }

  async getShipmentCommonData(){
    const data = await shipmentHelper.getShipmentCommonData(this.baseServices,this.api_menu);
    this.listTypeOfService = dataHelper.prepareNg2SelectData(data.serviceTypes,'value','displayName');  //lodash.map(data.serviceTypes,function(x){return {"text":x.displayName,"id":x.value}});
    this.listTypeOfMove = dataHelper.prepareNg2SelectData(data.typeOfMoves,'value','displayName');  //lodash.map(data.typeOfMoves,function(x){return {"text":x.displayName,"id":x.value}});
    this.listHouseBillLadingType = dataHelper.prepareNg2SelectData(data.billOfLadings,'value','displayName'); //lodash.map(data.billOfLadings,function(x){return {"text":x.displayName,"id":x.value}});
    this.listFreightPayment = dataHelper.prepareNg2SelectData(data.freightTerms,'value','displayName'); //lodash.map(data.freightTerms,function(x){return {"text":x.displayName,"id":x.value}});
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
