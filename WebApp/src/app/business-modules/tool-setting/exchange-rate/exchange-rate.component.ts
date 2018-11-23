import { Component, OnInit } from '@angular/core';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { BaseService } from 'src/services-base/base.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { EXCHANGERATECOLUMNSETTING } from './exchange-rate.columns';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { flatten } from '@angular/core/src/render3/util';
import { CatCurrencyExchange } from 'src/app/shared/models/tool-setting/exchange-rate';

@Component({
  selector: 'app-exchange-rate',
  templateUrl: './exchange-rate.component.html',
  styleUrls: ['./exchange-rate.component.scss']
})
export class ExchangeRateComponent implements OnInit {
  exchangeRates: any[];
  exchangeRate: CatCurrencyExchange;
  exchangeRateToAdd: Array<CatCurrencyExchange>;
  exchangeRatesOfDay: any[];
  exchangeRateNewest: any[];
  pager: PagerSetting = PAGINGSETTING;
  localCurrency = "VND";
  criteria: any = { localCurrencyId : this.localCurrency };
  ExchangeRateSettings: ColumnSetting[] = EXCHANGERATECOLUMNSETTING;
  currencies: any[];
  rateNewest: any[];
  isDesc: boolean = false;
  nameDetailModal = "detail-history-modal";
  selectedrange: any;


  constructor(private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private sortService: SortService, 
    private baseService: BaseService) { }

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
      this.criteria.fromDate = this.selectedrange.startDate.toDate;
      this.criteria.toDate = this.selectedrange.endDate.toDate;
      this.pager.currentPage = 1;
      this.getExchangeRates(this.pager);
    }
  }
  showDetail(item){
    this.getChargeRateBy(item.datetimeCreated, item.localCurrency);
  }
  addNewRate(){
    this.exchangeRateToAdd = new Array<CatCurrencyExchange>();
  }
  async getExchangeRates(pager: PagerSetting) {
    this.spinnerService.show();
    this.baseService.post(this.api_menu.ToolSetting.ExchangeRate.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.spinnerService.hide();
      this.exchangeRates = response.data;
      this.pager.totalItems = response.totalItems;
    });
  }
  async getExchangeNewest(){
    var responses = await this.baseService.getAsync(this.api_menu.ToolSetting.ExchangeRate.getNewest);
    this.exchangeRateNewest = responses;
    console.log(this.exchangeRateNewest);
  }
  getcurrencies(){
    this.baseService.get(this.api_menu.Catalogue.Currency.getAll).subscribe((response: any) => {
      if(response != null){
        this.currencies = response.map(x=>({"text":x.id,"id":x.id}));
      }
      else{
        this.currencies = [];
      }
    });
  }
  getChargeRateBy(datetimeCreated, localCurrency){
    this.baseService.get(this.api_menu.ToolSetting.ExchangeRate.getBy + '?date=' + datetimeCreated + '&localCurrency=' + localCurrency).subscribe((response: any) => {
      this.exchangeRatesOfDay = response;
      console.log(this.exchangeRatesOfDay);
    })
  }
   /**
   * ng2-select
   */
  public items: Array<string> = ['USD', 'JPY', 'SGD', 'EUR', 'GBP', 'HKD',];
  private value: any = {};
  private _disabledV: string = '0';
  private disabled: boolean = false;
  
  private get disabledV():string {
    return this._disabledV;
  }
 
  private set disabledV(value:string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }
 
  public selected(value:any):void {
    console.log('Selected value is: ', value);
  }
 
  public removed(value:any):void {
    console.log('Removed value is: ', value);
  }
 
  public typed(value:any):void {
    console.log('New search input: ', value);
  }
 
  public refreshValue(value:any):void {
    this.value = value;
  }
}
