import { Component, OnInit, ViewChild } from '@angular/core';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { BaseService } from 'src/services-base/base.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { EXCHANGERATECOLUMNSETTING } from './exchange-rate.columns';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { CatCurrencyExchange } from 'src/app/shared/models/tool-setting/exchange-rate';
import { ToastrService } from 'ngx-toastr';
import { SelectComponent } from 'ng2-select';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
//import { moment } from 'ngx-bootstrap/chronos/test/chain';
declare var $:any;
import * as moment from 'moment';

@Component({
  selector: 'app-exchange-rate',
  templateUrl: './exchange-rate.component.html',
  styleUrls: ['./exchange-rate.component.scss']
})
export class ExchangeRateComponent implements OnInit {
  exchangeRates: any[];
  exchangeRatesOfDay: any[];
  exchangeRateNewest: any = {};
  currencyRateToDelete: any;
  pager: PagerSetting = PAGINGSETTING;
  localCurrency = "VND";
  rate: any;
  criteria: any = { localCurrencyId : this.localCurrency };
  exchangeRateToAdd: any ={
    currencyToId: this.localCurrency,
    CatCurrencyExchangeRates: new Array<CatCurrencyExchange>(),
    userModified: ''
  };
  cancelButtonSetting: ButtonModalSetting = {
    buttonAttribute: {titleButton: "no",
    classStyle: "btn m-btn--square m-btn--icon m-btn--uppercase",
    icon: "la la-ban"},
    typeButton: ButtonType.cancel,
  };
  addButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.add
  };
  saveButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.save
  };
  ExchangeRateSettings: ColumnSetting[] = EXCHANGERATECOLUMNSETTING;
  fromCurrencies: any[];
  toCurrencies: any[];
  catCurrencies: any[];
  rateNewest: any[];
  isDesc: boolean = false;
  nameDetailModal = "detail-history-modal";
  nameSettingExchangeRateModal = "setting-exchange-rate-modal";
  nameUpdateRateModal = "update-exchange-rate-modal";
  selectedrange: any;
  selectTypeName = {
    rateSetting: 'rateSetting',
    fromCurrency: 'fromCurrency',
    toCurrency: 'toCurrency'
  };
  titleConfirmDelete = "You want to delete this currency?";
  convertDate: any;
  convert: any = {
    selectedRangeDate: null,
    fromCurrency: null,
    toCurrency: null
  }
  isAllowUpdateRate: boolean = false;
   /**
  /**
   * Daterange picker
   */
  
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

  @ViewChild('currencyRateSelect') public ngSelectCurrencyRate: SelectComponent;

  constructor(
    private api_menu: API_MENU,
    private sortService: SortService, 
    private baseService: BaseService,
    private toastr: ToastrService) { }

  async ngOnInit() {
    this.getExchangeRates(this.pager);
    await this.getExchangeNewest();
    this.getcurrencies();
  }
  setPage(pager) { 
    this.pager.currentPage = pager.currentPage; 
    this.pager.totalPages = pager.totalPages;
    this.pager.pageSize = pager.pageSize
    this.getExchangeRates(pager);
  }
  
  onSortChange(column) {
    if(column.dataType != 'boolean'){
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.exchangeRates = this.sortService.sort(this.exchangeRates, property, this.isDesc);
    }
  }
  searchHistory(){
    console.log(this.selectedrange);
    if(this.selectedrange != null){
      this.criteria.fromDate = this.selectedrange.startDate;
      this.criteria.toDate = this.selectedrange.endDate;
      this.pager.currentPage = 1;
      this.getExchangeRates(this.pager);
    }
  }
  showDetail(item){
    this.getChargeRateBy(item.datetimeCreated, item.localCurrency, '');
  }
  showSetting(){
    this.exchangeRateToAdd = {
      currencyToId: this.localCurrency,
      CatCurrencyExchangeRates: new Array<CatCurrencyExchange>(),
      userModified: ''
    };
    this.getCatCurrencies();
  }
  addNewRate(){
    if(this.catCurrencies.length == 0){
      this.toastr.warning("All currencies have added.");
    }
    else{
      if(this.exchangeRateToAdd.CatCurrencyExchangeRates.length > 0){
        if(this.exchangeRateToAdd.CatCurrencyExchangeRates[this.exchangeRateToAdd.CatCurrencyExchangeRates.length-1].currencyFromId == null){
  
          this.toastr.warning("Please select currency to add new Rate");
        }
        else{
          this.exchangeRateToAdd.CatCurrencyExchangeRates.push({ currencyFromId: null, rate: 0 });
        }
      }
      else{
        this.exchangeRateToAdd.CatCurrencyExchangeRates.push({ currencyFromId: null, rate: 0 });
      }
    }
  }
  async saveNewRate(){
    let index = this.exchangeRateToAdd.CatCurrencyExchangeRates.findIndex(x => x.currencyFromId == null);
    if(index < 0){
      if(this.exchangeRateToAdd.CatCurrencyExchangeRates.length > 0){
        this.exchangeRateNewest.exchangeRates.forEach(element => {
          this.exchangeRateToAdd.CatCurrencyExchangeRates.push({currencyFromId: element.currencyFromId, rate: element.rate, isUpdate : false });
        });
       var res = await this.baseService.putAsync(this.api_menu.ToolSetting.ExchangeRate.updateRate, this.exchangeRateToAdd, true, false);
       if(res){
        $('#setting-exchange-rate-modal').modal('hide');
        this.ngSelectCurrencyRate.active = [];
        this.getExchangeNewest();
        this.getExchangeRates(this.pager);
        this.exchangeRateToAdd = {
          currencyToId: this.localCurrency,
          CatCurrencyExchangeRates: new Array<CatCurrencyExchange>(),
          userModified: ''
        };
       }    
      }
    }
    else{
      this.toastr.warning("Please select currency to add new Rate");
    }
  }
  async updateRate(){
    this.exchangeRateToAdd = {
      currencyToId: this.localCurrency,
      CatCurrencyExchangeRates: new Array<CatCurrencyExchange>(),
      userModified: ''
    };
    this.exchangeRateNewest.exchangeRates.forEach(element => {
      if(element.newRate != undefined){
        this.exchangeRateToAdd.CatCurrencyExchangeRates.push({currencyFromId: element.currencyFromId, rate: element.newRate, isUpdate : true });
      }
      else{
        
        this.exchangeRateToAdd.CatCurrencyExchangeRates.push({currencyFromId: element.currencyFromId, rate: element.rate });
      }
    });
    var res = await this.baseService.putAsync(this.api_menu.ToolSetting.ExchangeRate.updateRate, this.exchangeRateToAdd, true, false);
    if(res){
      this.getExchangeNewest();
      this.pager.currentPage = 1;
      this.getExchangeRates(this.pager);
      $('#update-exchange-rate-modal').modal('hide');
      console.log(this.exchangeRateToAdd);
    } 

  }
  valueChange(value){
    if(value != null){
      this.isAllowUpdateRate = true;
    }
    else{
      this.isAllowUpdateRate = false;
      for(let element of this.exchangeRateNewest.exchangeRates){
        if(element.newRate != null){
          this.isAllowUpdateRate = true;
          break;
        }
      }
    }
  }
  
  resetForm(form){
    this.exchangeRateToAdd = {
      currencyToId: this.localCurrency,
      CatCurrencyExchangeRates: new Array<CatCurrencyExchange>(),
      userModified: ''
    };
    form.onReset();
  }
  confirmDeleteRate(item){
    this.currencyRateToDelete = item;
  }
  removeNewRate(index){
    const currency = this.exchangeRateToAdd.CatCurrencyExchangeRates[index];
    this.exchangeRateToAdd.CatCurrencyExchangeRates.splice(index, 1);
    this.catCurrencies.push({"text": currency.currencyFromId,"id": currency.currencyFromId});
  }
  convertRate(form){
    if(form.valid && this.convert.fromCurrency != null && this.convert.toCurrency != null){

      console.log(this.convert);

      this.baseService.get(this.api_menu.ToolSetting.ExchangeRate.convertRate + '?date=' + new Date(this.convert.selectedRangeDate.startDate).toISOString()
       + '&localCurrency=' + this.convert.toCurrency + "&fromCurrency=" + this.convert.fromCurrency)
      .subscribe((response: any) => {
        this.rate = response;
        console.log(this.exchangeRatesOfDay);
      })
    }
  }
  cancelAddRate(){
    this.getCatCurrencies();
  }
  getCatCurrencies(){
    
    this.baseService.get(this.api_menu.Catalogue.Currency.getAll).subscribe((response: any) =>{
      if(response != null){
        this.catCurrencies = response.map(x=>({"text":x.id,"id":x.id}));
        if(this.catCurrencies.length > 0){
          this.catCurrencies.splice(this.catCurrencies.indexOf({"text":this.localCurrency,"id": this.localCurrency}), 1 );
        }
        this.exchangeRateNewest.exchangeRates.forEach(element => {
          let index = this.catCurrencies.findIndex(x => x.id == element.currencyFromId);
          this.catCurrencies.splice(index, 1 );
        });
      }else{
        this.catCurrencies = [];
      }
    });
  }
  async getExchangeRates(pager: PagerSetting) {
    this.baseService.spinnerShow();
    this.baseService.post(this.api_menu.ToolSetting.ExchangeRate.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.baseService.spinnerHide();
      this.exchangeRates = response.data;
      console.log(this.exchangeRates);
      this.pager.totalItems = response.totalItems;
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  async getExchangeNewest(){
    var responses = await this.baseService.getAsync(this.api_menu.ToolSetting.ExchangeRate.getNewest);
    this.exchangeRateNewest = responses;
    console.log(this.exchangeRateNewest);
  }
  getcurrencies(){
    this.baseService.get(this.api_menu.ToolSetting.ExchangeRate.getCurrencies).subscribe((response: any) => {
      if(response != null){
        this.fromCurrencies = response.fromCurrencies;
        this.toCurrencies = response.toCurrencies;
      }
      else{
        this.fromCurrencies = [];
        this.toCurrencies = [];
      }
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  getChargeRateBy(datetimeCreated, localCurrency, fromCurrency?){
    this.baseService.get(this.api_menu.ToolSetting.ExchangeRate.getBy + '?date=' + datetimeCreated + '&localCurrency=' + localCurrency + "&fromCurrency=" + fromCurrency)
    .subscribe((response: any) => {
      this.exchangeRatesOfDay = response;
      console.log(this.exchangeRatesOfDay);
    },err=>{
      this.baseService.handleError(err);
    })
  }
   /**
   * ng2-select
   */
  public items: Array<string> = ['USD', 'JPY', 'SGD', 'EUR', 'GBP', 'HKD',];
  private value: any = {};
  private _disabledV: string = '0';
  public disabled: boolean = false;
  
  private get disabledV():string {
    return this._disabledV;
  }
 
  private set disabledV(value:string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }
 
  public selected(value:any, selectTypeName):void {
    if(selectTypeName == this.selectTypeName.rateSetting){
      const checkCurrencyFrom = obj => obj.currencyFromId === value.id;
      const isExist = this.exchangeRateNewest.exchangeRates.some(checkCurrencyFrom);
      if(!isExist){
        this.exchangeRateToAdd.CatCurrencyExchangeRates[this.exchangeRateToAdd.CatCurrencyExchangeRates.length -1].currencyFromId = value.id;
        this.catCurrencies.splice(this.catCurrencies.findIndex(x => x.id == value.id),1);
      }
      console.log(this.exchangeRateToAdd);
    }
    if(selectTypeName == this.selectTypeName.fromCurrency){
      this.convert.fromCurrency = value.id;
    }
    if(selectTypeName == this.selectTypeName.toCurrency){
      this.convert.toCurrency = value.id;
    }
    console.log('Selected value is: ', value);
  }
 
  public removed(value: any, selectTypeName):void {
    if(selectTypeName == this.selectTypeName.rateSetting){
      let index = this.exchangeRateToAdd.CatCurrencyExchangeRates.findIndex(x => x.currencyFromId == value.id);
      this.exchangeRateToAdd.CatCurrencyExchangeRates.splice(index, 1);
      this.getCatCurrencies();
    }
    if(selectTypeName == this.selectTypeName.fromCurrency){
      this.convert.fromCurrency = null;
      this.rate = null;
    }
    if(selectTypeName == this.selectTypeName.toCurrency){
      this.convert.toCurrency = null;
      this.rate = null;
    }
  }
 
  public typed(value:any):void {
    console.log('New search input: ', value);
  }
 
  public refreshValue(value:any):void {
    this.value = value;
  }

  async onDelete(event){
    if(event == true){
      await this.baseService.deleteAsync(this.api_menu.ToolSetting.ExchangeRate.delete + this.currencyRateToDelete.id, true, true);
      this.getExchangeNewest();
    }
  }

}
