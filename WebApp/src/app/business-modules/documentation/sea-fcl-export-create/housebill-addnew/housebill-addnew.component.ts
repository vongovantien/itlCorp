import { Component, OnInit, ViewChild, Output, EventEmitter, AfterViewInit, ChangeDetectorRef, AfterViewChecked } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { SystemConstants } from 'src/constants/system.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import * as lodash from 'lodash';
import * as moment from 'moment';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { NgForm } from '@angular/forms';
import { Container } from 'src/app/shared/models/document/container.model';
declare var $: any;

@Component({
  selector: 'app-housebill-addnew',
  templateUrl: './housebill-addnew.component.html',
  styleUrls: ['./housebill-addnew.component.scss']
})
export class HousebillAddnewComponent implements OnInit {

  pager: PagerSetting = PAGINGSETTING;

  listContainerTypes: any[] = [];
  listCustomers: any = [];
  listSaleMan: any = [];
  listShipper: any = [];
  listConsignee: any = [];
  listNotifyParty: any = [];
  listHouseBillLadingType: any = [];
  listCountryOrigin: any = [];
  listPort: any = [];
  listFreightPayment: any = [];
  listFreightPayableAt: any = [];
  listFowardingAgent: any = [];
  listDeliveryOfGoods: any = [];
  listNumberOfOriginBL: any = [{ id: 1, text: '1' }, { id: 2, text: '2' }, { id: 3, text: '3' }];
  listTypeOfMove: any = [];
  listTypeOfService: any = [];
  customerSaleman: any = null;
  extend_data:any= {};

  /**
   * House Bill Variables 
   */

  HouseBillToAdd: CsTransactionDetail = new CsTransactionDetail();
  ListHouseBill: any = ["gggg"];
  ListContainers: Array<Container> = [new Container()];
  @Output() houseBillComing = new EventEmitter<{data:CsTransactionDetail,extend_data:any}>();


  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    console.log(this.ListContainers);
    this.getListCustomers();
    this.getShipmentCommonData();
    this.getListShippers();
    this.getListConsignees();
    this.getlistCountryOrigin()
    this.getListPorts();
    this.getListForwardingAgent();
    this.getListSaleman();
    this.getContainerTypes();
    this.getComodities();
    this.getWeightTypes();
    this.getPackageTypes();
  }

  select(form) {
    console.log(form)
  }

  async getShipmentCommonData() {
    const data = await shipmentHelper.getShipmentCommonData(this.baseServices, this.api_menu);
    this.listTypeOfService = dataHelper.prepareNg2SelectData(data.serviceTypes, 'value', 'displayName');
    this.listTypeOfMove = dataHelper.prepareNg2SelectData(data.typeOfMoves, 'value', 'displayName');
    this.listHouseBillLadingType = dataHelper.prepareNg2SelectData(data.billOfLadings, 'value', 'displayName');
    this.listFreightPayment = dataHelper.prepareNg2SelectData(data.freightTerms, 'value', 'displayName');
  }

  public getListCustomers(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.paging + "?page=" + 1 + "&size=" + 20, { partnerGroup: PartnerGroupEnum.CUSTOMER, inactive: false, all: key }).subscribe(res => {
      var data = res['data']
      this.listCustomers = data;

    });

  }

  public getShipperDescription(shipper: any) {
    this.HouseBillToAdd.shipperDescription =
      "Name: " + shipper.partnerNameEn + "\n" +
      "Billing Address: " + shipper.addressEn + "\n" +
      "Tel: " + shipper.tel + "\n" +
      "Fax: " + shipper.fax + "\n";

  }

  public getListShippers(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.paging + "?page=" + 1 + "&size=" + 20, { partnerGroup: PartnerGroupEnum.SHIPPER, inactive: false, all: key }).subscribe(res => {
      var data = res['data']
      this.listShipper = data;

    });
  }

  public getNotifyPartyDescription(notifyParty: any) {
    this.HouseBillToAdd.notifyPartyDescription =
      "Name: " + notifyParty.partnerNameEn + "\n" +
      "Billing Address: " + notifyParty.addressEn + "\n" +
      "Tel: " + notifyParty.tel + "\n" +
      "Fax: " + notifyParty.fax + "\n";
  }

  public getConsigneeDescription(consignee: any) {
    this.HouseBillToAdd.consigneeDescription =
      "Name: " + consignee.partnerNameEn + "\n" +
      "Billing Address: " + consignee.addressEn + "\n" +
      "Tel: " + consignee.tel + "\n" +
      "Fax: " + consignee.fax + "\n";
  }

  public getListConsignees(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.paging + "?page=" + 1 + "&size=" + 20, { partnerGroup: PartnerGroupEnum.CONSIGNEE, inactive: false, all: key }).subscribe(res => {
      var data = res['data']
      this.listConsignee = data;
    });
  }


  public getlistCountryOrigin(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 2 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.Country.paging + "?page=" + 1 + "&size=" + 20, { inactive: false, code: key, nameEn: key, nameVn: key, condition: 1 }).subscribe(res => {
      var data = res['data'];
      this.listCountryOrigin = data;
    });

    console.log(this.listCountryOrigin);
  }

  getListPorts(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 2 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.CatPlace.paging + "?page=" + 1 + "&size=" + 20, { modeOfTransport: "sea", inactive: false, all: key }).subscribe(res => {
      var data = res['data'];
      this.listPort = data;
      console.log({ list_port: this.listPort });
    });
  }

  public getForwardingAgentDescription(forwardingAgent: any) {
    this.HouseBillToAdd.forwardingAgentDescription =
      "Name: " + forwardingAgent.partnerNameEn + "\n" +
      "Billing Address: " + forwardingAgent.addressEn + "\n" +
      "Tel: " + forwardingAgent.tel + "\n" +
      "Fax: " + forwardingAgent.fax + "\n";
  }

  public getGoodDeliveryDescription(goodDelivery: any) {
    this.HouseBillToAdd.goodsDeliveryDescription =
      "Name: " + goodDelivery.partnerNameEn + "\n" +
      "Billing Address: " + goodDelivery.addressEn + "\n" +
      "Tel: " + goodDelivery.tel + "\n" +
      "Fax: " + goodDelivery.fax + "\n";
  }

  getListForwardingAgent(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.paging + "?page=" + 1 + "&size=" + 20, { partnerGroup: PartnerGroupEnum.AGENT, inactive: false, all: key }).subscribe(res => {
      var data = res['data'];
      this.listFowardingAgent = data;
    });
  }

  public async getCustomerSaleman(idSaleMan: string) {
    var saleMan = await this.baseServices.getAsync(this.api_menu.System.User_Management.getUserByID + idSaleMan);
    this.customerSaleman = [{ id: saleMan['id'], text: saleMan["employeeNameEn"] }];
    console.log({ SALE_MAN: this.customerSaleman });
    this.HouseBillToAdd.saleManId = saleMan['id'];
    this.extend_data.saleman_nameEn = saleMan['employeeNameEn'];
    // var users = await this.baseServices.getAsync(this.api_menu.System.User_Management.getAll);
    // this.listSaleMan = dataHelper.prepareNg2SelectData(users, "id", "employeeNameEn");
  }

  public async getListSaleman(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.System.User_Management.paging + "?page=" + 1 + "&size=" + 20, { all: key }).subscribe(res => {
      var data = res['data'];
      this.listSaleMan = dataHelper.prepareNg2SelectData(data, "id", "employeeNameEn");
    });
  }



  public onSubmitContainer(form: NgForm) {
    console.log(this.ListContainers)
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

  isDisplay = true;
  save(form: NgForm) {
    if (form.valid) {
      this.houseBillComing.emit({data:this.HouseBillToAdd,extend_data:this.extend_data});
      this.ListHouseBill.push(Object.assign({}, this.HouseBillToAdd));
      this.resetForm();
      $('#add-house-bill-modal').modal('hide');
    }
  }

  resetForm(){    
    this.HouseBillToAdd = new CsTransactionDetail();      
    this.customerSaleman = null;
    this.isDisplay = false;
    setTimeout(() => {
      this.isDisplay = true;
    }, 300);
    $('#add-house-bill-modal').modal('hide');
  }



  /**
   * ADD CONTAINER LIST
   */
  lstHouseBillContainers: any[] = [];
  containerTypes: any[] = [];
  weightMesurements: any[] = [];
  packageTypes: any[] = [];
  commodities: any[] = [];
  @ViewChild('containerListForm') containerListForm: NgForm;

  async getContainerTypes() {
    let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Container", inactive: false }, false, false);
    if (responses != null) {
        this.containerTypes = dataHelper.prepareNg2SelectData(responses, 'id', 'unitNameEn');
    }
}
async getWeightTypes() {
    let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Weight Measurement", inactive: false }, false, false);
    if (responses != null) {
        this.weightMesurements = dataHelper.prepareNg2SelectData(responses, 'id', 'unitNameEn');
        console.log(this.weightMesurements);
    }
}
async getPackageTypes() {
    let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Package", inactive: false }, false, false);
    if (responses != null) {
        this.packageTypes = dataHelper.prepareNg2SelectData(responses, 'id', 'unitNameEn');
        console.log(this.packageTypes);
    }
}
async getComodities() {
    let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Commodity.query, { inactive: false }, false, false);
    this.commodities = responses;
    console.log(this.commodities);
}

addNewContainer() {
  let hasItemEdited = false;
  for(let i=0; i< this.lstHouseBillContainers.length; i++){
      if(this.lstHouseBillContainers[i].allowEdit == true){
          hasItemEdited = true;
          break;
      }
  }
  if(hasItemEdited == false){
      console.log(this.containerListForm);
      //this.containerMasterForm.onReset();
      this.lstHouseBillContainers.push(this.initNewContainer());
  }
  else{
      this.baseServices.errorToast("Current container must be save!!!");
  }
}

initNewContainer(){
  var container = {
      containerTypeId: null,
      containerTypeName: '',
      containerTypeActive: [],
      quantity: null,
      containerNo: '',
      sealNo: '',
      markNo: '',
      unitOfMeasureId: null,
      unitOfMeasureName: '',
      unitOfMeasureActive: [],
      commodityId: null,
      commodityName: '',
      packageTypeId: null,
      packageTypeName: '',
      packageTypeActive: [],
      packageQuantity: null,
      description: '',
      gw: null,
      nw: null,
      chargeAbleWeight :null,
      cbm: null,
      packageContainer: '',
      //desOfGoods: '',
      allowEdit: true,
      isNew: true
  };
  return container;
}

 

}
