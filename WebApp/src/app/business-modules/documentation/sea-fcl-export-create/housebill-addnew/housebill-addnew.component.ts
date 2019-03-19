import { Component, OnInit, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import * as lodash from 'lodash';
import * as moment from 'moment';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { NgForm } from '@angular/forms';
import { Container } from 'src/app/shared/models/document/container.model';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';
declare var $: any;

@Component({
  selector: 'app-housebill-addnew',
  templateUrl: './housebill-addnew.component.html',
  styleUrls: ['./housebill-addnew.component.scss']
})
export class HousebillAddnewComponent implements OnInit {

  MasterBillData: CsTransaction = null;
  EditingHouseBill: any = null;
  isEditing: boolean = false;
  activeOriginCountry: string = null;
  activePortOfLoading: string = null;
  activePortOfDischarge: string = null;

  @Input() set masterBillData(_masterBilData: CsTransaction) {
    this.MasterBillData = _masterBilData;
    this.HouseBillWorking.jobId = this.MasterBillData.id;
  }

  @Input() set currentHouseBill(_currentHouseBill: any) {
    if (_currentHouseBill != null) {
      this.isEditing = true;
      this.EditingHouseBill = _currentHouseBill;
      this.HouseBillWorking = this.EditingHouseBill;
      this.customerSaleman = [{ id: this.HouseBillWorking.saleManId, text: this.HouseBillWorking.saleManName.split(".")[0] }];
      this.lstHouseBillContainers = _currentHouseBill.csMawbcontainers;
      this.getActiveOriginCountry();
      this.getActivePortOfLoading();
      this.getActivePortOfDischarge();
      this.getHouseBillContainers(this.HouseBillWorking.id);
      this.HouseBillWorking.sailingDate = this.HouseBillWorking.sailingDate == null ? this.HouseBillWorking.sailingDate : { startDate: moment(this.HouseBillWorking.sailingDate), endDate: moment(this.HouseBillWorking.sailingDate) };
      this.HouseBillWorking.closingDate = this.HouseBillWorking.closingDate == null ? this.HouseBillWorking.closingDate : { startDate: moment(this.HouseBillWorking.closingDate), endDate: moment(this.HouseBillWorking.closingDate) };
    } else {
      this.isEditing = false;
      this.HouseBillWorking = new CsTransactionDetail();
      this.HouseBillWorking.jobId = this.MasterBillData.id;
      this.customerSaleman = null;
    }
  }



  getHouseBillContainers(hblid: String) {
    this.baseServices.post(this.api_menu.Documentation.CsMawbcontainer.query, { "hblid": hblid }).subscribe((res: any) => {
      this.lstHouseBillContainers = res;
    })
  }

  getActiveOriginCountry() {
    const index = this.listCountryOrigin.map(function (e: any) { return e.id; }).indexOf(this.HouseBillWorking.originCountryId);
    this.activeOriginCountry = index === -1 ? null : this.listCountryOrigin[index].nameEn;
  }

  getActivePortOfLoading() {
    const index = this.listPort.map(function (e: any) { return e.id }).indexOf(this.HouseBillWorking.pol);
    this.activePortOfLoading = index === -1 ? null : this.listPort[index].nameEN;
  }

  getActivePortOfDischarge() {
    const index = this.listPort.map(function (e: any) { return e.id }).indexOf(this.HouseBillWorking.pod);
    this.activePortOfDischarge = index === -1 ? null : this.listPort[index].nameEN;
  }


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
  extend_data: any = {};

  /**
   * House Bill Variables 
   */

  HouseBillWorking: CsTransactionDetail = new CsTransactionDetail();
  ListContainers: Array<Container> = [new Container()];
  @Output() houseBillComing = new EventEmitter<any>();


  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU
  ) { }

  ngOnInit() {
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



  async getShipmentCommonData() {
    const data = await shipmentHelper.getShipmentCommonData(this.baseServices, this.api_menu);
    this.listTypeOfService = dataHelper.prepareNg2SelectData(data.serviceTypes, 'value', 'displayName');
    this.listTypeOfMove = dataHelper.prepareNg2SelectData(data.typeOfMoves, 'value', 'displayName');
    this.listHouseBillLadingType = dataHelper.prepareNg2SelectData(data.billOfLadings, 'value', 'displayName');
    this.listFreightPayment = dataHelper.prepareNg2SelectData(data.freightTerms, 'value', 'displayName');
  }

  public getListCustomers(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3) {
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
    this.HouseBillWorking.shipperDescription =
      shipper.partnerNameEn + "\n" +
      shipper.addressShippingEn + "\n" +
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
    this.HouseBillWorking.notifyPartyDescription =
      notifyParty.partnerNameEn + "\n" +
      notifyParty.addressShippingEn + "\n" +
      "Tel: " + notifyParty.tel + "\n" +
      "Fax: " + notifyParty.fax + "\n";
  }

  public getConsigneeDescription(consignee: any) {
    this.HouseBillWorking.consigneeDescription =
      consignee.partnerNameEn + "\n" +
      consignee.addressShippingEn + "\n" +
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
    this.HouseBillWorking.forwardingAgentDescription =
      "Name: " + forwardingAgent.partnerNameEn + "\n" +
      "Billing Address: " + forwardingAgent.addressEn + "\n" +
      "Tel: " + forwardingAgent.tel + "\n" +
      "Fax: " + forwardingAgent.fax + "\n";
  }

  public getGoodDeliveryDescription(goodDelivery: any) {
    this.HouseBillWorking.goodsDeliveryDescription =
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
    this.HouseBillWorking.saleManId = saleMan['id'];
    // this.extend_data.saleman_nameEn = saleMan['employeeNameEn'];



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

  listShipmentDetails: any[] = [];
  async showImportHouseBill() {
    let criteria = { fromDate: moment().startOf('month'), toDate: moment().endOf('month') };
    let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransactionDetail.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, criteria, true, true);
    this.listShipmentDetails = responses.data;
    this.pager.totalItems = responses.totalItems;
    if (this.pager.totalItems > 0) {
      $('#import-housebill-detail-modal').modal('show');
    }
  }
  isImporting = false;
  async showShipmentDetail(event) {
    this.HouseBillWorking = event;
    this.isImporting = true;
    this.HouseBillWorking.jobId = this.MasterBillData.id;
    this.HouseBillWorking.hwbno = null;
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
  ListHouseBill: any[] = [];
  async save(form: NgForm) {
    console.log(this.HouseBillWorking)
    if (form.valid) {
      this.HouseBillWorking.csMawbcontainers = this.lstHouseBillContainers;
      this.HouseBillWorking.sailingDate = this.HouseBillWorking.sailingDate == null ? null : this.HouseBillWorking.sailingDate.startDate;
      this.HouseBillWorking.closingDate = this.HouseBillWorking.closingDate == null ? null : this.HouseBillWorking.closingDate.startDate;
      
      if (this.isImporting == false) {
        if (this.isEditing) {
          const res = await this.baseServices.putAsync(this.api_menu.Documentation.CsTransactionDetail.update, this.HouseBillWorking);
          if (res.status) {
            var latestListHouseBill = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getByJob + "?jobId=" + this.MasterBillData.id);
            this.houseBillComing.emit(latestListHouseBill);
            this.resetForm();
            // $('#add-house-bill-modal').modal('hide');
          }
        }
        else {
          const res = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransactionDetail.addNew, this.HouseBillWorking);
          if (res.status) {
            var latestListHouseBill = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getByJob + "?jobId=" + this.MasterBillData.id);
            this.houseBillComing.emit(latestListHouseBill);
            this.resetForm();
          }
        }
      }
      else {
        let response = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransactionDetail.import, this.HouseBillWorking);
        if (response != null) {
          if (response.result.success) {
            var latestListHouseBill = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getByJob + "?jobId=" + this.MasterBillData.id);
            this.houseBillComing.emit(latestListHouseBill);
            $('#add-house-bill-modal').modal('hide');
          }
        }
      }


    }
  }

  resetForm() {
    this.HouseBillWorking = new CsTransactionDetail();
    this.HouseBillWorking.jobId = this.MasterBillData.id;
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
    for (let i = 0; i < this.lstHouseBillContainers.length; i++) {
      if (this.lstHouseBillContainers[i].allowEdit == true) {
        hasItemEdited = true;
        break;
      }
    }
    if (hasItemEdited == false) {
      console.log(this.containerListForm);
      //this.containerMasterForm.onReset();      
      this.lstHouseBillContainers.push(this.initNewContainer());
    }
    else {
      this.baseServices.errorToast("Current container must be save!!!");
    }

    console.log(this.lstHouseBillContainers);
  }

  saveNewContainer(index: number, form: NgForm) {
    this.lstHouseBillContainers[index].verifying = true;
    if (this.containerListForm.invalid) return;

    if (this.compareContainerList(this.lstHouseBillContainers[index], this.MasterBillData.csMawbcontainers) != true) {

      this.baseServices.errorToast(
        "The Cont qty value you entered will make the Total Container number of all HBL exceeded the Container number of Shipment detail. Please recheck and try again !",
        "Cannot save container detail");
      return false;
    }
    //Cont Type, Cont Q'ty, Container No, Package Type
    let existedItem = this.lstHouseBillContainers.filter(x => x.containerTypeId == this.lstHouseBillContainers[index].containerTypeId
      && x.quantity == this.lstHouseBillContainers[index].quantity
      && x.containerNo == this.lstHouseBillContainers[index].containerNo
      && x.packageTypeId == this.lstHouseBillContainers[index].packageTypeId);
    if (existedItem.length > 1) {
      this.baseServices.errorToast("This container has been existed");
      return false;
    }
    else {
      if (this.lstHouseBillContainers[index].isNew == true) this.lstHouseBillContainers[index].isNew = false;
      this.lstHouseBillContainers[index].verifying = false;
      this.lstHouseBillContainers[index].allowEdit = false;
      this.lstHouseBillContainers[index].containerTypeActive = this.lstHouseBillContainers[index].containerTypeId != null ? [{ id: this.lstHouseBillContainers[index].containerTypeId, text: this.lstHouseBillContainers[index].containerTypeName }] : [];
      this.lstHouseBillContainers[index].packageTypeActive = this.lstHouseBillContainers[index].packageTypeId != null ? [{ id: this.lstHouseBillContainers[index].packageTypeId, text: this.lstHouseBillContainers[index].packageTypeName }] : [];
      this.lstHouseBillContainers[index].unitOfMeasureActive = this.lstHouseBillContainers[index].unitOfMeasureId != null ? [{ id: this.lstHouseBillContainers[index].unitOfMeasureId, text: this.lstHouseBillContainers[index].unitOfMeasureName }] : [];
      return true;
    }
    // this.lstContainerTemp = Object.assign([], this.lstMasterContainers);
  }

  totalGrossWeight: number;
  totalNetWeight: number;
  totalCharWeight: number;
  totalCBM: number;
  numberOfTimeSaveContainer: number = 0;
  onSubmitContainer(form: NgForm) {


    if (!this.saveNewContainer(this.lstHouseBillContainers.length - 1, form)) {
      return;
    }




    this.numberOfTimeSaveContainer = this.numberOfTimeSaveContainer + 1;
    if (this.containerListForm.valid) {
      this.totalGrossWeight = 0;
      this.totalNetWeight = 0;
      this.totalCharWeight = 0;
      this.totalCBM = 0;
      this.HouseBillWorking.commodity = '';
      this.HouseBillWorking.desOfGoods = '';
      this.HouseBillWorking.packageContainer = '';
      for (var i = 0; i < this.lstHouseBillContainers.length; i++) {
        this.lstHouseBillContainers[i].isSave = true;
        this.totalGrossWeight = this.totalGrossWeight + this.lstHouseBillContainers[i].gw;
        this.totalNetWeight = this.totalNetWeight + this.lstHouseBillContainers[i].nw;
        this.totalCharWeight = this.totalCharWeight + this.lstHouseBillContainers[i].chargeAbleWeight;
        this.totalCBM = this.totalCBM + this.lstHouseBillContainers[i].cbm;
        this.HouseBillWorking.packageContainer = this.HouseBillWorking.packageContainer + (this.lstHouseBillContainers[i].quantity == "" ? "" : this.lstHouseBillContainers[i].quantity + "x" + this.lstHouseBillContainers[i].containerTypeName + ", ");
        if (this.numberOfTimeSaveContainer == 1) {
          this.HouseBillWorking.commodity = this.HouseBillWorking.commodity + (this.lstHouseBillContainers[i].commodityName == "" ? "" : this.lstHouseBillContainers[i].commodityName + ", ");
          this.HouseBillWorking.desOfGoods = this.HouseBillWorking.desOfGoods + (this.lstHouseBillContainers[i].description == "" ? "" : this.lstHouseBillContainers[i].description + ", ");
        }
      }
      $('#container-list-of-job-modal-house').modal('hide');
    }
  }


  compareContainerList(currentContainer: Container, masterBillContainerList: Container[]): Boolean {
    masterBillContainerList = lodash.filter(masterBillContainerList, function (o: Container) {
      return o.containerTypeId == currentContainer.containerTypeId;
    });

    const listHBWithCurrentContainerType = lodash.filter(this.lstHouseBillContainers, function (o: Container) {
      return o.containerTypeId == currentContainer.containerTypeId;
    });

    const totalHBContainer = listHBWithCurrentContainerType.length == 0 ? 0 : listHBWithCurrentContainerType.map(x => x.quantity).reduce((a, c) => a + c);
    const totalMasterContainer = masterBillContainerList.length == 0 ? 0 : masterBillContainerList.map(x => x.quantity).reduce((a, c) => a + c);
    if (totalHBContainer > totalMasterContainer) {
      return false;
    } else {
      return true;
    }

  }



  removeAContainer(index: number) {
    this.lstHouseBillContainers.splice(index, 1);
  }
  cancelNewContainer(index: number) {
    if (this.lstHouseBillContainers[index].isNew == true) {
      this.lstHouseBillContainers.splice(index, 1);
    }
    else {
      this.lstHouseBillContainers[index].allowEdit = false;
    }
  }


  initNewContainer() {
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
      chargeAbleWeight: null,
      cbm: null,
      packageContainer: '',
      //desOfGoods: '',
      allowEdit: true,
      isNew: true,
      verifying: false
    };
    return container;
  }

  changeEditStatus(index: any) {
    if (this.lstHouseBillContainers[index].allowEdit == false) {
      this.lstHouseBillContainers[index].allowEdit = true;
    }
    else {
      this.lstHouseBillContainers[index].allowEdit = false;
    }
  }



}
