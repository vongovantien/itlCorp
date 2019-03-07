
import { Component, OnInit, Input } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as lodash from 'lodash';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { FirstLoadData } from '../sea-fcl-export-create.component';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { NgForm } from '@angular/forms';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';
import { SurchargeTypeEnum } from 'src/app/shared/enums/csShipmentSurchargeType-enum';
declare var $: any;

@Component({
  selector: 'app-housebill-list',
  templateUrl: './housebill-list.component.html',
  styleUrls: ['./housebill-list.component.scss']
})
export class HousebillListComponent implements OnInit {


  MasterBillData: CsTransaction = null;
  @Input() set masterBillData(_masterBilData: CsTransaction) {
    this.MasterBillData = _masterBilData;
  }
  BuyingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
  SellingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
  OBHChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();

  HouseBillListData: any[] = [];
  ConstHouseBillListData: any[] = [];

  ListBuyingRateCharges: any[] = [];
  ConstListBuyingRateCharges: any = [];

  ListSellingRateCharges: any[] = [];
  ConstListSellingRateCharges: any[] = [];

  ListOBHCharges: any[] = [];
  ConstListOBHCharges: any[] = [];

  lstBuyingRateChargesComboBox: any[] = [];
  lstSellingRateChargesComboBox: any[] = [];
  lstOBHChargesComboBox: any[] = [];

  comboBoxData: FirstLoadData = new FirstLoadData();
  houseBillSelected: any = null; //{ data: CsTransactionDetail, extend_data: any } = { data: null, extend_data: null };

  @Input() set firstLoadData(data: FirstLoadData) {
    this.comboBoxData = data;
  };


  @Input() set houseBillList(lstHB: any[]) {
    this.HouseBillListData = lstHB;
    this.ConstHouseBillListData = lstHB;
    this.ListBuyingRateCharges = [];
    this.ConstListBuyingRateCharges = [];

    this.ListSellingRateCharges = [];
    this.ConstListSellingRateCharges = [];

    this.ListOBHCharges = [];
    this.ConstListOBHCharges = [];
  }

  /**
   * Calculate 'total' base on 'quantity' , 'unit price', 'VAT'
   */
  calculateTotalBuying() {
    if (this.BuyingRateChargeToAdd.vatrate >= 0) {
      this.BuyingRateChargeToAdd.total = this.BuyingRateChargeToAdd.quantity * this.BuyingRateChargeToAdd.unitPrice * (1 + (this.BuyingRateChargeToAdd.vatrate / 100));
    } else {
      this.BuyingRateChargeToAdd.total = this.BuyingRateChargeToAdd.quantity * this.BuyingRateChargeToAdd.unitPrice + this.BuyingRateChargeToAdd.vatrate;
    }
  }

  calculateTotalSelling() {
    if (this.SellingRateChargeToAdd.vatrate >= 0) {
      this.SellingRateChargeToAdd.total = this.SellingRateChargeToAdd.quantity * this.SellingRateChargeToAdd.unitPrice * (1 + (this.SellingRateChargeToAdd.vatrate / 100));
    } else {
      this.SellingRateChargeToAdd.total = this.SellingRateChargeToAdd.quantity * this.SellingRateChargeToAdd.unitPrice + this.SellingRateChargeToAdd.vatrate;
    }
  }


  partnerTypes = [
    { text: "Agent", id: "AGENT" },
    { text: "Customer", id: "CUSTOMER" },
    { text: "Other", id: "OTHER" }

  ]

  selectPartnerType() {
    console.log(this.BuyingRateChargeToAdd);
    console.log(this.houseBillSelected);
  }


  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU) { }


  async ngOnInit() {
    this.getListBuyingRateCharges();
    this.getListSellingRateCharges();
    this.getListOBHCharges();
    this.getHouseBillsOfMaster();
  }

  getHouseBillsOfMaster() {
    this.baseServices.get(this.api_menu.Documentation.CsTransactionDetail.getByJob + "?jobId=" + this.MasterBillData.id).subscribe((res: any) => {
      this.HouseBillListData = res;
      this.ConstHouseBillListData = res;
    });
  }

  getListBuyingRateCharges(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return
    } else {
      key = search_key == null ? "" : search_key.trim();
    }

    this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'CREDIT', serviceTypeId: 'SEF', all: key }).subscribe(res => {
      this.lstBuyingRateChargesComboBox = res['data'];
    });
  }

  getListSellingRateCharges(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return;
    } else {
      key = search_key == null ? "" : search_key.trim();
    }
    this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'DEBIT', serviceTypeId: 'SEF', all: key }).subscribe(res => {
      this.lstSellingRateChargesComboBox = res['data'];
    });
  }

  getListOBHCharges(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return;
    } else {
      key = search_key == null ? "" : search_key.trim();
    }
    this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'OBH', serviceTypeId: 'SEF', all: key }).subscribe(res => {
      this.lstOBHChargesComboBox = res['data'];
    });
  }

  getListPartner(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3 && search_key.length > 0) {
      return;
    } else {
      key = search_key == null ? "" : search_key.trim();
    }
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.paging + "?page=" + 1 + "&size=" + 20, { partnerGroup: PartnerGroupEnum.ALL, inactive: false, all: key }).subscribe(res => {
      var data = res['data']
      this.comboBoxData.lstPartner = data;
    });
  }

  async saveNewBuyingRateCharge(form: NgForm, IsContinue: boolean = false) {


    if (form.valid) {
      this.BuyingRateChargeToAdd.type = SurchargeTypeEnum.BUYING_RATE;
      this.BuyingRateChargeToAdd.hblid = this.houseBillSelected.id;
      var res = await this.baseServices.postAsync(this.api_menu.Documentation.CsShipmentSurcharge.addNew, this.BuyingRateChargeToAdd);
      this.getBuyingChargesOfHouseBill(this.houseBillSelected);
      if (IsContinue && res.status) {
        this.BuyingRateChargeToAdd = new CsShipmentSurcharge();
      } else if (res.status) {
        this.BuyingRateChargeToAdd = new CsShipmentSurcharge();
        $('#add-buying-rate-modal').modal('hide');
      } else {

      }

    }
  }

  async saveNewSellingRateCharge(form: NgForm, IsContinue: boolean = false) {


    if (form.valid) {
      this.SellingRateChargeToAdd.type = SurchargeTypeEnum.SELLING_RATE;
      this.SellingRateChargeToAdd.hblid = this.houseBillSelected.id;
      var res = await this.baseServices.postAsync(this.api_menu.Documentation.CsShipmentSurcharge.addNew, this.SellingRateChargeToAdd);
      this.getSellingChargesOfHouseBill(this.houseBillSelected);
      if (IsContinue && res.status) {
        this.SellingRateChargeToAdd = new CsShipmentSurcharge();
      } else if (res.status) {
        this.SellingRateChargeToAdd = new CsShipmentSurcharge();
        $('#add-selling-rate-modal').modal('hide');
      } else {

      }

    }
  }




  viewdata() {
    console.log({ "HB L": this.HouseBillListData });
    console.log({ "Const hbl ": this.ConstHouseBillListData });
  }

  searchHouseBill(key: any) {
    const search_key = key.toLowerCase();
    this.HouseBillListData = lodash.filter(this.ConstHouseBillListData, function (x: any) {

      return (
        x.hwbno.toLowerCase().includes(search_key) ||
        x.customerName.toLowerCase().includes(search_key) ||
        x.customerNameVn.toLowerCase().includes(search_key) ||
        x.saleManName.toLowerCase().includes(search_key) ||
        x.notifyParty.toLowerCase().includes(search_key) ||
        x.finalDestinationPlace.toLowerCase().includes(search_key)
      )
    });
    const idSelectedHB = this.houseBillSelected.id;
    if (lodash.findIndex(this.HouseBillListData, function (o) { return o.id === idSelectedHB }) < 0) {
      this.ListBuyingRateCharges = [];
    }
  }





  getUnits() {
    this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, { all: "", inactive: false }).subscribe((data: any) => {
      this.comboBoxData.lstUnit = data;
    });
  }

  // async getChargesOfHouseBill(hb: any,type:'BUY' | 'SELL' | 'OBH') {
  //   this.houseBillSelected = hb;
  //   this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getByHBId + "?hbId=" + this.houseBillSelected.id).subscribe((res: any) => {
  //     this.ListBuyingRateCharges = res;
  //     this.ConstListBuyingRateCharges = res;
  //   })
  //   return await this.baseServices.getAsync(this.api_menu.Documentation.CsShipmentSurcharge.getByHBId+"?hbId="+this.houseBillSelected.id+"&type="+type);

  // }

  getBuyingChargesOfHouseBill(currentlyHB: any) {
    this.houseBillSelected = currentlyHB;
    this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getByHBId + "?hbId=" + this.houseBillSelected.id + "&type=BUY").subscribe((res: any) => {
      this.ListBuyingRateCharges = res;
      this.ConstListBuyingRateCharges = res;
    })
  }

  getSellingChargesOfHouseBill(currentlyHB: any) {
    this.houseBillSelected = currentlyHB;
    this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getByHBId + "?hbId=" + this.houseBillSelected.id + "&type=SELL").subscribe((res: any) => {
      this.ListSellingRateCharges = res;
      this.ConstListSellingRateCharges = res;
    })
  }





  currencies: any[] = [];
  addChargeClick() {
    // this.currencies = prepareNg2SelectData(this.comboBoxData.lstCurrency, "id", "currencyName");
  }
  getListCurrency(key: string) {
    // this.baseServices.post(this.api_menu.Catalogue.Currency.paging + "?page=" + 1 + "&size=" + 20, { inactive: false, all: key }).subscribe(res => {
    //   this.currencies = prepareNg2SelectData(res['data'], "id", "currencyName");
    // });
  }






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


