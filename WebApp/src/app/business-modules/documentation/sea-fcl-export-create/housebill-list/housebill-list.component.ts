
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
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
import * as moment from 'moment';

declare var $: any;

@Component({
  selector: 'app-housebill-list',
  templateUrl: './housebill-list.component.html',
  styleUrls: ['./housebill-list.component.scss']
})
export class HousebillListComponent implements OnInit {


  MasterBillData: any = null;
  @Input() set masterBillData(_masterBilData: CsTransaction) {
    this.MasterBillData = _masterBilData;
    if(this.MasterBillData!=null && this.MasterBillData.id!=null){
      this.getHouseBillsOfMaster();
    }    
    console.log({"MASTER_DETAILS":this.MasterBillData});
  }
   @Output() currentHouseBill = new EventEmitter<any>();


  BuyingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
  SellingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
  OBHChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();

  
  BuyingRateChargeToEdit: any = null;
  SellingRateChargeToEdit: any = null
  OBHChargeToEdit: any = null;

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

  calculateTotalEachBuying(isEdit:boolean=false) {   
    if(isEdit){
      if (this.BuyingRateChargeToEdit.vatrate >= 0) {
        this.BuyingRateChargeToEdit.total = this.BuyingRateChargeToEdit.quantity * this.BuyingRateChargeToEdit.unitPrice * (1 + (this.BuyingRateChargeToEdit.vatrate / 100));
      } else {
        this.BuyingRateChargeToEdit.total = this.BuyingRateChargeToEdit.quantity * this.BuyingRateChargeToEdit.unitPrice + this.BuyingRateChargeToEdit.vatrate;
      }
    }
    else{
      if (this.BuyingRateChargeToAdd.vatrate >= 0) {
        this.BuyingRateChargeToAdd.total = this.BuyingRateChargeToAdd.quantity * this.BuyingRateChargeToAdd.unitPrice * (1 + (this.BuyingRateChargeToAdd.vatrate / 100));
      } else {
        this.BuyingRateChargeToAdd.total = this.BuyingRateChargeToAdd.quantity * this.BuyingRateChargeToAdd.unitPrice + this.BuyingRateChargeToAdd.vatrate;
      }
    }
  }

  calculateTotalEachSelling(isEdit:boolean=false) {
    if(isEdit){
      if (this.SellingRateChargeToEdit.vatrate >= 0) {
        this.SellingRateChargeToEdit.total = this.SellingRateChargeToEdit.quantity * this.SellingRateChargeToEdit.unitPrice * (1 + (this.SellingRateChargeToEdit.vatrate / 100));
      } else {
        this.SellingRateChargeToEdit.total = this.SellingRateChargeToEdit.quantity * this.SellingRateChargeToEdit.unitPrice + this.SellingRateChargeToEdit.vatrate;
      }
    }else{
      if (this.SellingRateChargeToAdd.vatrate >= 0) {
        this.SellingRateChargeToAdd.total = this.SellingRateChargeToAdd.quantity * this.SellingRateChargeToAdd.unitPrice * (1 + (this.SellingRateChargeToAdd.vatrate / 100));
      } else {
        this.SellingRateChargeToAdd.total = this.SellingRateChargeToAdd.quantity * this.SellingRateChargeToAdd.unitPrice + this.SellingRateChargeToAdd.vatrate;
      }
    }    
  }


  calculateTotalEachOBH(isEdit:boolean=false){
    if(isEdit){
      if (this.OBHChargeToEdit.vatrate >= 0) {
        this.OBHChargeToEdit.total = this.OBHChargeToEdit.quantity * this.OBHChargeToEdit.unitPrice * (1 + (this.OBHChargeToEdit.vatrate / 100));
      } else {
        this.OBHChargeToEdit.total = this.OBHChargeToEdit.quantity * this.OBHChargeToEdit.unitPrice + this.OBHChargeToEdit.vatrate;
      }
    }else{
      if (this.OBHChargeToAdd.vatrate >= 0) {
        this.OBHChargeToAdd.total = this.OBHChargeToAdd.quantity * this.OBHChargeToAdd.unitPrice * (1 + (this.OBHChargeToAdd.vatrate / 100));
      } else {
        this.OBHChargeToAdd.total = this.OBHChargeToAdd.quantity * this.OBHChargeToAdd.unitPrice + this.OBHChargeToAdd.vatrate;
      }
    }   
  }


  totalBuyingUSD :number = 0;
  totalBuyingLocal : number = 0;
  totalBuyingCharge(){
    
    this.totalBuyingUSD = 0;
    this.totalBuyingLocal = 0;
    if(this.ListBuyingRateCharges.length>0){

      this.ListBuyingRateCharges.forEach(element => {

        this.totalBuyingUSD +=element.total;
        this.totalBuyingLocal += element.total*23000;
        this.totalProfit();

      });
    }
  }

  totalSellingUSD :number = 0;
  totalSellingLocal : number = 0;
  totalSellingCharge(){
    this.totalSellingUSD = 0;
    this.totalSellingLocal = 0;
    if(this.ListSellingRateCharges.length>0){

      this.ListSellingRateCharges.forEach(element => {

        this.totalSellingUSD +=element.total;
        this.totalSellingLocal += element.total*23000;
        this.totalProfit();

      });

    }
  }


  totalOBHUSD :number = 0;
  totalOBHLocal : number = 0;
  totalOBHCharge(){
    this.totalOBHUSD = 0;
    this.totalOBHLocal = 0;
    if(this.ListOBHCharges.length>0){

      this.ListOBHCharges.forEach(element => {

        this.totalOBHUSD +=element.total;
        this.totalOBHLocal += element.total*23000;
        this.totalProfit();
      });

    }
  }


  totalLogisticChargeUSD :number = 0;
  totalLogisticChargeLocal : number = 0;
  totalLogisticCharge(){
    this.totalLogisticChargeUSD = 0;
    this.totalLogisticChargeUSD = 0;

    /**
     * Implement Later 
     */
    
  }

  


  totalProfitUSD:number= 0;
  totalProfitLocal:number= 0;
  totalProfit(){
    this.totalProfitUSD = this.totalSellingUSD - this.totalBuyingUSD - this.totalLogisticChargeUSD;
    this.totalProfitLocal = this.totalSellingLocal - this.totalBuyingLocal - this.totalLogisticChargeLocal;
  }




  partnerTypes = [
    { text: "AGENT", id: "AGENT" },
    { text: "CUSTOMER", id: "CUSTOMER" },
    { text: "OTHER", id: "OTHER" }

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
  }

  getHouseBillsOfMaster() {
    this.baseServices.get(this.api_menu.Documentation.CsTransactionDetail.getByJob + "?jobId=" + this.MasterBillData.id).subscribe((res: any) => {
      this.HouseBillListData = res;
      this.ConstHouseBillListData = res;
      this.getBuyingChargesOfHouseBill(this.HouseBillListData[0]);
      this.getSellingChargesOfHouseBill(this.HouseBillListData[0]);
      this.getOBHChargesOfHouseBill(this.HouseBillListData[0]);
      console.log({"LIST_HB":this.HouseBillListData});
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

  async saveNewOBHCharge(form: NgForm, IsContinue: boolean = false) {


    if (form.valid) {
      this.OBHChargeToAdd.type = SurchargeTypeEnum.OBH;
      this.OBHChargeToAdd.hblid = this.houseBillSelected.id;
      var res = await this.baseServices.postAsync(this.api_menu.Documentation.CsShipmentSurcharge.addNew, this.OBHChargeToAdd);
      this.getOBHChargesOfHouseBill(this.houseBillSelected);
      if (IsContinue && res.status) {
        this.OBHChargeToAdd = new CsShipmentSurcharge();
      } else if (res.status) {
        this.OBHChargeToAdd = new CsShipmentSurcharge();
        $('#add-obh-charge-modal').modal('hide');
      } else {

      }

    }
  }

  async editBuyingRateCharge(form:NgForm){
      if(form.valid){
        var res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update,this.BuyingRateChargeToEdit);
        if(res.status){
          $('#edit-buying-rate-modal').modal('hide');
          this.getBuyingChargesOfHouseBill(this.houseBillSelected);
        }
      }
  }

  async editSellingRateCharge(form:NgForm){
    console.log(this.SellingRateChargeToEdit);
    if(form.valid){
      var res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update,this.SellingRateChargeToEdit);
      if(res.status){
        $('#edit-selling-rate-modal').modal('hide');
        this.getSellingChargesOfHouseBill(this.houseBillSelected);
      }
    }
}

async editOBHCharge(form:NgForm){
  console.log(this.OBHChargeToEdit);
  if(form.valid){
    var res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update,this.OBHChargeToEdit);
    if(res.status){
      $('#edit-obh-rate-modal').modal('hide');
      this.getOBHChargesOfHouseBill(this.houseBillSelected);
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

  searchBuyingRate(key:string){
    const search_key = key.toString().toLowerCase();
    this.ListBuyingRateCharges = lodash.filter(this.ConstListBuyingRateCharges, function(x:any){
      return (
        ((x.partnerName==null?"":x.partnerName.toLowerCase().includes(search_key)) ||
        (x.nameEn==null?"":x.nameEn.toLowerCase().includes(search_key)) ||      
        (x.unit==null?"":x.unit.toLowerCase().includes(search_key)) ||
        (x.currency==null?"":x.currency.toLowerCase().includes(search_key)) ||
        (x.notes==null?"":x.notes.toLowerCase().includes(search_key)) ||
        (x.docNo==null?"":x.docNo.toLowerCase().includes(search_key)) ||
        (x.quantity==null?"":x.quantity.toString().toLowerCase().includes(search_key)) ||
        (x.unitPrice==null?"":x.unitPrice.toString().toLowerCase().includes(search_key)) ||
        (x.vatrate==null?"":x.vatrate.toString().toLowerCase().includes(search_key)) ||
        (x.total==null?"":x.total.toString().toLowerCase().includes(search_key))) 
      )
    });
  }

  
  searchSellingRate(key:string){
    const search_key = key.toString().toLowerCase();
    this.ListSellingRateCharges = lodash.filter(this.ConstListSellingRateCharges, function(x:any){
      return (
        ((x.partnerName==null?"":x.partnerName.toLowerCase().includes(search_key)) ||
        (x.nameEn==null?"":x.nameEn.toLowerCase().includes(search_key)) ||      
        (x.unit==null?"":x.unit.toLowerCase().includes(search_key)) ||
        (x.currency==null?"":x.currency.toLowerCase().includes(search_key)) ||
        (x.notes==null?"":x.notes.toLowerCase().includes(search_key)) ||
        (x.docNo==null?"":x.docNo.toLowerCase().includes(search_key)) ||
        (x.quantity==null?"":x.quantity.toString().toLowerCase().includes(search_key)) ||
        (x.unitPrice==null?"":x.unitPrice.toString().toLowerCase().includes(search_key)) ||
        (x.vatrate==null?"":x.vatrate.toString().toLowerCase().includes(search_key)) ||
        (x.total==null?"":x.total.toString().toLowerCase().includes(search_key))) 
      )
    });
  }

  searchOBH(key:string){
    const search_key = key.toString().toLowerCase();
    this.ListOBHCharges = lodash.filter(this.ConstListOBHCharges, function(x:any){
      return (
        ((x.partnerName==null?"":x.partnerName.toLowerCase().includes(search_key)) ||
        (x.nameEn==null?"":x.nameEn.toLowerCase().includes(search_key)) ||   
        (x.receiverName==null?"":x.receiverName.toLowerCase().includes(search_key)) ||     
        (x.payerName==null?"":x.payerName.toLowerCase().includes(search_key)) ||     
        (x.unit==null?"":x.unit.toLowerCase().includes(search_key)) ||
        (x.currency==null?"":x.currency.toLowerCase().includes(search_key)) ||
        (x.notes==null?"":x.notes.toLowerCase().includes(search_key)) ||
        (x.docNo==null?"":x.docNo.toLowerCase().includes(search_key)) ||
        (x.quantity==null?"":x.quantity.toString().toLowerCase().includes(search_key)) ||
        (x.unitPrice==null?"":x.unitPrice.toString().toLowerCase().includes(search_key)) ||
        (x.vatrate==null?"":x.vatrate.toString().toLowerCase().includes(search_key)) ||
        (x.total==null?"":x.total.toString().toLowerCase().includes(search_key))) 
      )
    });
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
      this.totalBuyingCharge()
    })
  }

  getSellingChargesOfHouseBill(currentlyHB: any) {
    this.houseBillSelected = currentlyHB;
    this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getByHBId + "?hbId=" + this.houseBillSelected.id + "&type=SELL").subscribe((res: any) => {
      this.ListSellingRateCharges = res;
      this.ConstListSellingRateCharges = res;
      this.totalSellingCharge();
    })
  }

  getOBHChargesOfHouseBill(currentlyHB: any) {
    this.houseBillSelected = currentlyHB;
    this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getByHBId + "?hbId=" + this.houseBillSelected.id + "&type=OBH").subscribe((res: any) => {
      this.ListOBHCharges = res;
      this.ConstListOBHCharges = res;
      this.totalOBHCharge();
    })
  }


  emitSelectedHB(hb:any){
    this.currentHouseBill.emit(hb);
  }
  prepareEdit(type:string){

    /**
     * format exchangeDate to binding to ngx-daterangepicker-material  
     */
    if(type==="buy"){
      this.BuyingRateChargeToEdit.exchangeDate = { startDate: moment(this.BuyingRateChargeToEdit.exchangeDate), endDate: moment(this.BuyingRateChargeToEdit.exchangeDate) };
    }
  }





  currencies: any[] = [];
 
  getListCurrency(key: string) {
    // this.baseServices.post(this.api_menu.Catalogue.Currency.paging + "?page=" + 1 + "&size=" + 20, { inactive: false, all: key }).subscribe(res => {
    //   this.currencies = prepareNg2SelectData(res['data'], "id", "currencyName");
    // });
  }

  searchPartner(key:string){
    
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


