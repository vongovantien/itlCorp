import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
// import * as lodash from 'lodash';
import filter from 'lodash/filter';
import moment from 'moment/moment';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { Container } from 'src/app/shared/models/document/container.model';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { API_MENU } from 'src/constants/api-menu.const';
import { PAGINGSETTING } from 'src/constants/paging.const';
import * as dataHelper from 'src/helper/data.helper';
import * as shipmentHelper from 'src/helper/shipment.helper';
import { BaseService } from 'src/app/shared/services/base.service';
import cloneDeep from 'lodash/cloneDeep';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';

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

  totalGrossWeight: number = 0;
  totalNetWeight: number = 0;
  totalCharWeight: number = 0;
  totalCBM: number = 0;
  numberOfTimeSaveContainer: number = 0;

  @Input() set masterBillData(_masterBilData: CsTransaction) {
    if (_masterBilData != null) {
      this.MasterBillData = _masterBilData;
      this.HouseBillWorking.jobId = this.MasterBillData.id;
      this.HouseBillWorking.mawb = this.HouseBillWorking.mawb == null ? this.MasterBillData.mawb : this.HouseBillWorking.mawb;
      this.HouseBillWorking.jobNo = this.MasterBillData.jobNo;
      this.HouseBillWorking.oceanVoyNo = this.MasterBillData.voyNo + "" + this.MasterBillData.flightVesselName;
      this.HouseBillWorking.customsBookingNo = this.MasterBillData.bookingNo;
      this.activePortOfLoading = this.MasterBillData.polName + "";
      this.HouseBillWorking.pol = this.MasterBillData.pol;
      this.activePortOfDischarge = this.MasterBillData.podName + "";
      this.HouseBillWorking.pod = this.MasterBillData.pod;
      this.getListContsOfAllHB();
      // this.numberOfTimeSaveContainer = 0;
    }

  }

  @Input() set currentHouseBill(_currentHouseBill: any) {
    if (_currentHouseBill != null && _currentHouseBill != 'addnew') {
      this.isEditing = true;
      this.getHouseBillDetails(_currentHouseBill);
    }
    if (_currentHouseBill === 'addnew') {
      this.isEditing = false;
      this.lstHouseBillContainers = this.MasterBillData.csMawbcontainers;
      this.HouseBillWorking = new CsTransactionDetail();
      this.HouseBillWorking.jobId = this.MasterBillData.id;
      this.HouseBillWorking.mawb = this.MasterBillData.mawb;
      this.HouseBillWorking.jobNo = this.MasterBillData.jobNo;
      this.HouseBillWorking.oceanVoyNo = this.MasterBillData.voyNo + "" + this.MasterBillData.flightVesselName;
      this.HouseBillWorking.customsBookingNo = this.MasterBillData.bookingNo;
      this.HouseBillWorking.serviceType = this.MasterBillData.typeOfService;
      this.HouseBillWorking.purchaseOrderNo = this.MasterBillData.pono;
      this.activePortOfLoading = this.MasterBillData.polName + "";
      this.HouseBillWorking.pol = this.MasterBillData.pol;
      this.activePortOfDischarge = this.MasterBillData.podName + "";
      this.HouseBillWorking.pod = this.MasterBillData.pod;
      this.customerSaleman = null;
    }

    $('#hb-mblno').focus();
    this.numberOfTimeSaveContainer = 0;
  }


  async getHouseBillDetails(hblid: string) {
    this.HouseBillWorking = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getHBDetails + "?JobId=" + this.MasterBillData.id + "&HbId=" + hblid);
    this.EditingHouseBill = cloneDeep(this.HouseBillWorking);
    this.lstHouseBillContainers = this.HouseBillWorking.csMawbcontainers;
    this.customerSaleman = [{ id: this.HouseBillWorking.saleManId, text: this.HouseBillWorking.saleManName.split(".")[0] }];
    this.getActiveOriginCountry();
    this.getActivePortOfLoading();
    this.getActivePortOfDischarge();
    this.getHouseBillContainers(this.HouseBillWorking.id);
    this.HouseBillWorking.sailingDate = this.HouseBillWorking.sailingDate == null ? this.HouseBillWorking.sailingDate : { startDate: moment(this.HouseBillWorking.sailingDate), endDate: moment(this.HouseBillWorking.sailingDate) };
    this.HouseBillWorking.closingDate = this.HouseBillWorking.closingDate == null ? this.HouseBillWorking.closingDate : { startDate: moment(this.HouseBillWorking.closingDate), endDate: moment(this.HouseBillWorking.closingDate) };
    this.HouseBillWorking.mawb = this.HouseBillWorking.mawb == null ? this.MasterBillData.mawb : this.HouseBillWorking.mawb;
    this.HouseBillWorking.jobNo = this.MasterBillData.jobNo;
  }



  getHouseBillContainers(hblid: string) {
    this.baseServices.post(this.api_menu.Documentation.CsMawbcontainer.query, { "hblid": hblid }).subscribe((res: any) => {
      this.lstHouseBillContainers = res;
      this.calculateHbWeight();
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
  listContsOfHB: any[] = [];

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

  getListContsOfAllHB() {
    this.baseServices.get(this.api_menu.Documentation.CsMawbcontainer.getHBLConts + "?JobId=" + this.MasterBillData.id).subscribe((res: any) => {
      console.log({ "List_conts_hbl": res });
      this.listContsOfHB = res;
    });
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
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER, inactive: false, all: key }).subscribe(res => {
      this.listCustomers = res;
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
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.SHIPPER, inactive: false, all: key }).subscribe(res => {
      this.listShipper = res;
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
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CONSIGNEE, inactive: false, all: key }).subscribe(res => {
      this.listConsignee = res;
    });
  }

  public getListNotifyParty(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CONSIGNEE, inactive: false, all: key }).subscribe(res => {
      this.listConsignee = res;
    });
  }


  public getlistCountryOrigin(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 2 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.Country.query, { inactive: false, code: key, nameEn: key, nameVn: key, condition: 1 }).subscribe(res => {
      this.listCountryOrigin = res;
    });
  }

  getListPorts(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 2 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Port, modeOfTransport: "sea", inactive: false, all: key }).subscribe(res => {
      this.listPort = res;
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
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.AGENT, inactive: false, all: key }).subscribe(res => {
      this.listFowardingAgent = res;
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
    this.HouseBillWorking.sailingDate = this.HouseBillWorking.sailingDate == null ? this.HouseBillWorking.sailingDate : { startDate: moment(this.HouseBillWorking.sailingDate), endDate: moment(this.HouseBillWorking.sailingDate) };
    this.HouseBillWorking.closingDate = this.HouseBillWorking.closingDate == null ? this.HouseBillWorking.closingDate : { startDate: moment(this.HouseBillWorking.closingDate), endDate: moment(this.HouseBillWorking.closingDate) };
    this.isImporting = true;
    this.HouseBillWorking.jobId = this.MasterBillData.id;
    this.HouseBillWorking.mawb = this.MasterBillData.mawb;
    this.HouseBillWorking.jobNo = this.MasterBillData.jobNo;
    this.HouseBillWorking.hwbno = null;
    this.HouseBillWorking.closingDate = null;
    this.HouseBillWorking.sailingDate = null;
    this.HouseBillWorking.referenceNo = null;
    this.HouseBillWorking.issueHblplaceAndDate = null;
    this.HouseBillWorking.exportReferenceNo = null;
    this.customerSaleman = [{ id: this.HouseBillWorking.saleManId, text: this.HouseBillWorking.saleManName.split(".")[0] }];
    // this.lstHouseBillContainers = _currentHouseBill.csMawbcontainers;
    this.getActiveOriginCountry();
    this.getActivePortOfLoading();
    this.getActivePortOfDischarge();
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
      this.HouseBillWorking.sailingDate = this.HouseBillWorking.sailingDate.startDate == null ? null : dataHelper.dateTimeToUTC(this.HouseBillWorking.sailingDate.startDate);
      this.HouseBillWorking.closingDate = this.HouseBillWorking.closingDate.startDate == null ? null : dataHelper.dateTimeToUTC(this.HouseBillWorking.closingDate.startDate);


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
            this.isImporting = false;
            this.houseBillComing.emit(latestListHouseBill);
            $('#add-house-bill-modal').modal('hide');
          }
        }
      }

      this.getListContsOfAllHB();
    } else {
      $("#alert-cannot-create-hbl").modal('show');
    }
  }

  closeAlert() {
    $("#alert-cannot-create-hbl").modal('hide');
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
  @ViewChild('containerListForm', { static: false }) containerListForm: NgForm;
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

  async getComodities(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return 0;
    } else {
      key = search_key;
    }
    let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Commodity.query, { inactive: false, all: key }, false, false);
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

    if (this.lstHouseBillContainers.length == 0) {
      return true;
    }
    this.lstHouseBillContainers[index].verifying = true;

    if (this.lstHouseBillContainers[index].containerNo.length > 0 || this.lstHouseBillContainers[index].sealNo.length > 0 || this.lstHouseBillContainers[index].markNo.length > 0) {
      this.lstHouseBillContainers[index].quantity = 1;
    }

    if (this.containerListForm.invalid) return;

    if (this.compareContainerList(this.lstHouseBillContainers[index], this.MasterBillData.csMawbcontainers) != true) {

      this.baseServices.errorToast(
        "The Cont qty value you entered will make the Total Container number of all HBL exceeded the Container number of Shipment detail. Please recheck and try again !",
        "Cannot save container detail");
      return false;
    }
    //Cont Type, Cont Q'ty, Container No, Package Type
    if (this.lstHouseBillContainers[index].containerTypeId != null && this.lstHouseBillContainers[index].quantity != 0 && this.lstHouseBillContainers[index].containerNo != null && this.lstHouseBillContainers[index].packageTypeId != null) {

    }
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


  getPackageContainerSummary() {
    var returnSummary: string = "";

    for (var i = 0; i < this.lstHouseBillContainers.length; i++) {
      returnSummary += this.lstHouseBillContainers[i].quantity + "x" + this.lstHouseBillContainers[i].containerTypeName;
      if (this.lstHouseBillContainers[i].packageTypeName != null && this.lstHouseBillContainers[i].packageTypeName.trim() != "") {
        returnSummary += " : " + this.lstHouseBillContainers[i].packageTypeName + "x" + this.lstHouseBillContainers[i].packageQuantity + ",";
      } else {
        returnSummary += this.lstHouseBillContainers[i].quantity + "x" + this.lstHouseBillContainers[i].containerTypeName + ","
      }
    }
    return returnSummary;
  }

  getCommoditySummary() {
    var returnSummary: string = "";
    for (var i = 0; i < this.lstHouseBillContainers.length; i++) {
      returnSummary += this.lstHouseBillContainers[i].commodityName == "" ? "" : this.lstHouseBillContainers[i].commodityName + ","
    }
    return returnSummary;
  }

  getDescriptionOfGoodSummary() {
    var returnSummary: string = "";
    for (var i = 0; i < this.lstHouseBillContainers.length; i++) {
      returnSummary += this.lstHouseBillContainers[i].description == "" ? "" : this.lstHouseBillContainers[i].description + ",\n"
    }
    return returnSummary;
  }

  refreshHBLSummary() {
    this.totalGrossWeight = 0;
    this.totalNetWeight = 0;
    this.totalCharWeight = 0;
    this.totalCBM = 0;
    this.HouseBillWorking.packageContainer = '';
    for (var i = 0; i < this.lstHouseBillContainers.length; i++) {
      this.HouseBillWorking.commodity = this.getCommoditySummary();
      this.HouseBillWorking.desOfGoods = this.getDescriptionOfGoodSummary();
      this.totalGrossWeight = this.totalGrossWeight + this.lstHouseBillContainers[i].gw;
      this.totalNetWeight = this.totalNetWeight + this.lstHouseBillContainers[i].nw;
      this.totalCharWeight = this.totalCharWeight + this.lstHouseBillContainers[i].chargeAbleWeight;
      this.totalCBM = this.totalCBM + this.lstHouseBillContainers[i].cbm;
      this.HouseBillWorking.packageContainer = this.getPackageContainerSummary();
    }
  }

  onSubmitContainer(form: NgForm) {


    if (!this.saveNewContainer(this.lstHouseBillContainers.length - 1, form)) {
      return;
    }

    if (this.containerListForm.valid) {
      this.totalGrossWeight = 0;
      this.totalNetWeight = 0;
      this.totalCharWeight = 0;
      this.totalCBM = 0;
      this.HouseBillWorking.packageContainer = '';
      for (var i = 0; i < this.lstHouseBillContainers.length; i++) {
        if (this.numberOfTimeSaveContainer == 0) {
          this.HouseBillWorking.commodity = this.getCommoditySummary();
          this.HouseBillWorking.desOfGoods = this.getDescriptionOfGoodSummary();
        }
        this.lstHouseBillContainers[i].isSave = true;
        this.totalGrossWeight = this.totalGrossWeight + this.lstHouseBillContainers[i].gw;
        this.totalNetWeight = this.totalNetWeight + this.lstHouseBillContainers[i].nw;
        this.totalCharWeight = this.totalCharWeight + this.lstHouseBillContainers[i].chargeAbleWeight;
        this.totalCBM = this.totalCBM + this.lstHouseBillContainers[i].cbm;
        this.HouseBillWorking.packageContainer = this.getPackageContainerSummary();
      }
      $('#container-list-of-job-modal-house').modal('hide');
      this.numberOfTimeSaveContainer = this.numberOfTimeSaveContainer + 1;
    }
  }

  calculateHbWeight() {
    this.totalGrossWeight = 0;
    this.totalNetWeight = 0;
    this.totalCharWeight = 0;
    this.totalCBM = 0;
    for (var i = 0; i < this.lstHouseBillContainers.length; i++) {
      this.lstHouseBillContainers[i].isSave = true;
      this.totalGrossWeight = this.totalGrossWeight + this.lstHouseBillContainers[i].gw;
      this.totalNetWeight = this.totalNetWeight + this.lstHouseBillContainers[i].nw;
      this.totalCharWeight = this.totalCharWeight + this.lstHouseBillContainers[i].chargeAbleWeight;
      this.totalCBM = this.totalCBM + this.lstHouseBillContainers[i].cbm;
    }
  }


  compareContainerList(currentContainer: Container, masterBillContainerList: Container[]): Boolean {


    masterBillContainerList = filter(masterBillContainerList, function (o: Container) {
      return o.containerTypeId == currentContainer.containerTypeId;
    });

    const listHBWithCurrentContainerType = filter(this.lstHouseBillContainers, function (o: Container) {
      return o.containerTypeId == currentContainer.containerTypeId;
    });

    const currentHBId = this.HouseBillWorking.id;
    const listHBConts = filter(this.listContsOfHB, function (o) {
      return (o.containerTypeId == currentContainer.containerTypeId && o.hblid != currentHBId);
    });

    const totalAllHBContainer = listHBConts.length == 0 ? 0 : listHBConts.map(x => x.quantity).reduce((a, c) => a + c);
    const totalHBContainer = listHBWithCurrentContainerType.length == 0 ? 0 : listHBWithCurrentContainerType.map(x => x.quantity).reduce((a, c) => a + c);
    const totalMasterContainer = masterBillContainerList.length == 0 ? 0 : masterBillContainerList.map(x => x.quantity).reduce((a, c) => a + c);
    if ((totalHBContainer + totalAllHBContainer) > totalMasterContainer) {
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
    var saveBtns = $('#hb-containers-list').find('i.la-save')
    if (saveBtns.length > 0) {
      this.baseServices.errorToast("You must save current container before continue !");
      return;
    }

    if (this.lstHouseBillContainers[index].allowEdit == false) {
      this.lstHouseBillContainers[index].allowEdit = true;
      this.lstHouseBillContainers[index].containerTypeActive = this.lstHouseBillContainers[index].containerTypeId != null ? [{ id: this.lstHouseBillContainers[index].containerTypeId, text: this.lstHouseBillContainers[index].containerTypeName }] : [];
      this.lstHouseBillContainers[index].packageTypeActive = this.lstHouseBillContainers[index].packageTypeId != null ? [{ id: this.lstHouseBillContainers[index].packageTypeId, text: this.lstHouseBillContainers[index].packageTypeName }] : [];
      this.lstHouseBillContainers[index].unitOfMeasureActive = this.lstHouseBillContainers[index].unitOfMeasureId != null ? [{ id: this.lstHouseBillContainers[index].unitOfMeasureId, text: this.lstHouseBillContainers[index].unitOfMeasureName }] : [];
      this.lstHouseBillContainers[index].commodityName = this.lstHouseBillContainers[index].commodityName != null ? this.lstHouseBillContainers[index].commodityName : null;
    }
    else {
      this.lstHouseBillContainers[index].allowEdit = false;
    }
  }




  closeAddContainerForm() {
    $('#container-list-of-job-modal-house').modal('hide');
  }

  closeAddNewHBForm(form: NgForm) {
    // form.reset();
    this.HouseBillWorking = new CsTransactionDetail();
    this.HouseBillWorking.jobId = this.MasterBillData.id;
    this.HouseBillWorking.mawb = this.MasterBillData.mawb;
    this.HouseBillWorking.jobNo = this.MasterBillData.jobNo;
    this.HouseBillWorking.oceanVoyNo = this.MasterBillData.voyNo + "" + this.MasterBillData.flightVesselName;
    this.HouseBillWorking.customsBookingNo = this.MasterBillData.bookingNo;
    this.activePortOfLoading = this.MasterBillData.polName + "";
    this.HouseBillWorking.pol = this.MasterBillData.pol;
    this.activePortOfDischarge = this.MasterBillData.podName + "";
    this.HouseBillWorking.pod = this.MasterBillData.pod;
    this.lstHouseBillContainers = this.MasterBillData.csMawbcontainers;
    this.isDisplay = false;
    setTimeout(() => {
      this.isDisplay = true;
    }, 500);
    $('#add-house-bill-modal').modal('hide');
  }

  showAddContainerForm() {
    if (!this.isEditing) {
      this.lstHouseBillContainers = this.MasterBillData.csMawbcontainers;
    }
  }

  SearchCommodity(key: string) {

  }



}
