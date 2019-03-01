
import { Component, OnInit, Input, AfterViewInit, AfterViewChecked, OnChanges, SimpleChange, SimpleChanges } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as lodash from 'lodash';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { FirstLoadData } from '../sea-fcl-export-create.component';

@Component({
  selector: 'app-housebill-list',
  templateUrl: './housebill-list.component.html',
  styleUrls: ['./housebill-list.component.scss']
})
export class HousebillListComponent implements OnInit{

  lstBuyingRateCharges: any[] = [];
  lstSellingRateCharges: any[] = [];
  lstOBHCharges: any[] = [];
  lstUnit: any[] = [];
  lstPartner: any = null;
  BuyingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
  HouseBillListData: any[] = [];
  ConstHouseBillListData: any[] = [];

  comboBoxData : FirstLoadData = new FirstLoadData();

  @Input() set firstLoadData(data:FirstLoadData){
    this.comboBoxData = data;
  };

  @Input() set houseBillList(lstHB: any[]) {
    this.HouseBillListData = lstHB;
    this.ConstHouseBillListData = lstHB;
  }

  partnerTypes = [
    { text: "Agent", id: "AGENT" },
    { text: "Customer", id: "CUSTOMER" },
    { text: "Other", id: "OTHER" }

  ]


  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU) { }


  async ngOnInit() {
    this.getListBuyingRateCharges();
    this.getListSellingRateCharges();
    this.getListOBHCharges();
    // const d = this.firstLoadData;
    // this.lstPartner =
   
    // setTimeout(() => {
    //   this.lstPartner = d.listPartner;
    //   console.log({"DATA_PN":this.lstPartner})
    // }, 10000);
    
  }

  getListBuyingRateCharges(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3) {
      return;
    } else {
      key = search_key;
    }

    this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'CREDIT', serviceTypeId: 'SEF', all: key }).subscribe(res => {
      this.lstBuyingRateCharges = res['data'];
    });
  }

  getListSellingRateCharges(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3) {
      return;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'DEBIT', serviceTypeId: 'SEF', all: key }).subscribe(res => {
      this.lstSellingRateCharges = res['data'];
      console.log({ "lstSellingRateCharges": this.lstSellingRateCharges });
    });
  }

  getListOBHCharges(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3) {
      return;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'OBH', serviceTypeId: 'SEF', all: key }).subscribe(res => {
      this.lstOBHCharges = res['data'];
    });
  }

  getListPartner(search_key: string = null) {
    var key = "";
    if (search_key !== null && search_key.length < 3) {
      return;
    } else {
      key = search_key;
    }
    this.baseServices.post(this.api_menu.Catalogue.PartnerData.paging + "?page=" + 1 + "&size=" + 20, { partnerGroup: PartnerGroupEnum.ALL, inactive: false, all: key }).subscribe(res => {
      var data = res['data']
      this.lstPartner = data;
      console.log({ "List_parner ": this.lstPartner });
    });
  }

  save() {
    console.log(this.BuyingRateChargeToAdd);
  }

  viewdata() {
    console.log({ "HB L": this.HouseBillListData });
    console.log({ "Const hbl ": this.ConstHouseBillListData });
  }

  searchHouseBill(key: any) {
    const search_key = key.toLowerCase();
    this.HouseBillListData = lodash.filter(this.ConstHouseBillListData, function (x: { data: CsTransactionDetail, extend_data: any }) {
      return (
        x.data.hwbno.toLowerCase().includes(search_key) ||
        x.extend_data.customer_nameEn.toLowerCase().includes(search_key) ||
        x.extend_data.customer_nameVn.toLowerCase().includes(search_key) ||
        x.extend_data.saleman_nameEn.toLowerCase().includes(search_key) ||
        x.extend_data.notify_party_nameEn.toLowerCase().includes(search_key) ||
        x.extend_data.notify_party_nameVn.toLowerCase().includes(search_key) ||
        x.data.finalDestinationPlace.toLowerCase().includes(search_key)
      )
    });
    console.log({ "HBL AFTER SEARCH": this.HouseBillListData });
  }

  getUnits() {
    this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, { all: "", inactive: false }).subscribe((data: any) => {
      this.lstUnit = data;
    });
  }

  getCurrencies(chargeId: string) {
    console.log("")
  }
  async getListCharge(type: 'CREDIT' | 'DEBIT' | 'OBH', serviceType: 'SEF' | 'SIF' | 'SEL' | 'SIL' | 'SEC' | 'SIC', key_search: String = null) {
    const res = await this.baseServices.postAsync(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: type, serviceTypeId: serviceType });
    return res['data'];
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


