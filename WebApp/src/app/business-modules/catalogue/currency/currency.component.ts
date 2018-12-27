import { Component, OnInit, ViewChild } from '@angular/core';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { CURRENCYCOLUMNSETTING } from '../currency/currency.columns';
import { catCurrency } from 'src/app/shared/models/catalogue/catCurrency.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { NgForm } from '@angular/forms';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import * as lodash from 'lodash';
import { ExcelService } from 'src/app/shared/services/excel.service';
import {ExportExcel} from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';

declare var $: any;

@Component({
  selector: 'app-currency',
  templateUrl: './currency.component.html',
  styleUrls: ['./currency.component.scss']
})
export class CurrencyComponent implements OnInit {
  currencies: Array<catCurrency>;
  currency: catCurrency = new catCurrency();
  currenciesSettings: ColumnSetting[] = CURRENCYCOLUMNSETTING;
  pager: PagerSetting = PAGINGSETTING;
  criteria: any = {};
  selectedFilter = "All";
  configSearch: any = {
    selectedFilter: this.selectedFilter,
    settingFields: this.currenciesSettings,
    typeSearch: TypeSearch.outtab
  };
  keySortDefault = "id";
  isDesc: boolean = true;
  nameModal: string = 'edit-currency-modal';
  titleAddModal: string = 'Add currency';
  titleEditModal: string = 'Edit currency';
  addButtonSetting: ButtonModalSetting = {
    dataTarget: this.nameModal,
    typeButton: ButtonType.add
  };
  importButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.import
  };
  exportButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.export
  };
  saveButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.save
  };
  titleConfirmDelete = "You want to delete this currency";
  cancelButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.cancel
  };
  isAddnew: boolean;
  @ViewChild(PaginationComponent) child;
  @ViewChild('formAddEdit') form: NgForm;
  totalPages: number;
  constructor(private sortService: SortService, private baseService: BaseService,
    private excelService: ExcelService,
    private api_menu: API_MENU) { }

  ngOnInit() {
    this.pager.totalItems = 0;
    this.getCurrencies(this.pager);
  }

  async getCurrencies(pager: PagerSetting) {
    this.baseService.spinnerShow();
    console.log(this.criteria);
    this.baseService.post(this.api_menu.Catalogue.Currency.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.baseService.spinnerHide();
      this.currencies = response.data;
      this.pager.totalItems = response.totalItems;
      console.log(response.totalPages);
      this.totalPages = response.totalPages;
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  setPage(pager) {
    console.log({ PAGER: pager })
    this.pager.currentPage = pager.currentPage;
    this.pager.totalPages = pager.totalPages;
    this.pager.pageSize = pager.pageSize;
    this.getCurrencies(pager);
  }
  onSearch(event) {
    console.log(event);
    if (event.fieldDisplayName == "All") {
      this.criteria.all = event.searchString;
    }
    else {
      this.criteria.all = null;
      if (event.field == "id") {
        this.criteria.id = event.searchString;
      }
      if (event.field == "currencyName") {
        this.criteria.currencyName = event.searchString;
      }
    }
    this.pager.currentPage = 1;
    this.getCurrencies(this.pager);
  }

  resetSearch(event) {
    this.criteria = {};
  }
  onSortChange(column) {
    if (column.dataType != 'boolean') {
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.currencies = this.sortService.sort(this.currencies, property, this.isDesc);
    }
  }
  showAdd() {
    this.isAddnew = true;
    this.currency = new catCurrency();
  }
  onCancel() {
    this.form.onReset();
    this.currency = new catCurrency();
    this.setPage(this.pager);
  }
  onSubmit() {
    if (this.form.valid) {
      if (this.isAddnew) {
        this.addNew();
      }
      else {
        this.update();
      }
    }
  }
  update(): any {
    this.baseService.spinnerShow();
    this.baseService.put(this.api_menu.Catalogue.Currency.update, this.currency).subscribe((response: any) => {

      $('#' + this.nameModal).modal('hide');
      this.baseService.successToast(response.message);
      this.getCurrencies(this.pager);
      this.baseService.spinnerShow();

    }, err => {
      this.baseService.errorToast(err.error.message);
      this.baseService.spinnerHide();
    });
  }
  addNew(): any {
    this.baseService.spinnerShow();
    this.baseService.post(this.api_menu.Catalogue.Currency.addNew, this.currency).subscribe((response: any) => {

      this.baseService.successToast(response.message);
      this.form.onReset();
      $('#' + this.nameModal).modal('hide');
      this.pager.totalItems = this.pager.totalItems + 1;
      this.pager.currentPage = 1;
      this.child.setPage(this.pager.currentPage);
      this.baseService.spinnerHide();

    }, err => {
      this.baseService.errorToast(err.error.message);
      this.baseService.spinnerHide();
    });
  }
  showDetail(item) {
    this.isAddnew = false;
    this.currency = item;
  }
  showConfirmDelete(item) {
    this.currency = item;
  }
  async onDelete(event) {
    console.log(event);
    if (event) {
      this.baseService.spinnerShow();
      this.baseService.delete(this.api_menu.Catalogue.Currency.delete + this.currency.id).subscribe((response: any) => {

        this.baseService.successToast(response.message);
        this.setPageAfterDelete();
        this.baseService.spinnerHide();

      }, err => {
        this.baseService.errorToast(err.error.message);
        this.baseService.spinnerHide();
      });
    }
  }
  setPageAfterDelete() {
    this.pager.totalItems = this.pager.totalItems - 1;
    let totalPages = Math.ceil(this.pager.totalItems / this.pager.pageSize);
    if (totalPages < this.pager.totalPages) {
      this.pager.currentPage = totalPages;
    }
    this.child.setPage(this.pager.currentPage);
  }


  async export() {    
    var currenciesList = await this.baseService.postAsync(this.api_menu.Catalogue.Currency.getAllByQuery, this.criteria);      
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.ENGLISH_API){
      currenciesList = lodash.map(currenciesList, function (currency,index) {
        return [
          index+1,
          currency['id'],
          currency['currencyName'],
          currency['isDefault'],
          (currency['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }

    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.VIETNAM_API){
      currenciesList = lodash.map(currenciesList, function (currency,index) {
        return [
          index+1,
          currency['id'],
          currency['currencyName'],
          currency['isDefault'],
          (currency['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }
    

     /**Set up stylesheet */
     var exportModel:ExportExcel = new ExportExcel();
     exportModel.fileName = "Currency Report";    
     const currrently_user = localStorage.getItem('currently_userName');
     exportModel.title = "Currency Report ";
     exportModel.author = currrently_user;
     exportModel.sheetName = "Sheet 1";
     exportModel.header = [
       {name:"No.",width:10},
       {name:"Code",width:10},
       {name:"Currency Name",width:20},
       {name:"Is Default",width:20},
       {name:"Inactive",width:20}
     ]
     exportModel.data = currenciesList;
     this.excelService.generateExcel(exportModel);
    
  }

  async import() {

  }
}
