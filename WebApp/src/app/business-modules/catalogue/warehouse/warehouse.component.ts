import { Component, OnInit, ViewChild } from '@angular/core';
import { Warehouse } from '../../../shared/models/catalogue/ware-house.model';
import { ColumnSetting } from '../../../shared/models/layout/column-setting.model';
import { SortService } from '../../../shared/services/sort.service';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';
import { PagerSetting } from '../../../shared/models/layout/pager-setting.model';
import { BaseService } from 'src/services-base/base.service';
import { NgForm } from '@angular/forms';
import { SystemConstants } from '../../../../constants/system.const';
import { API_MENU } from '../../../../constants/api-menu.const';
import { SelectComponent } from 'ng2-select';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
//import { WAREHOUSECOLUMNSETTING } from 'src/app/business-modules/catalogue/warehouse/warehouse.columns';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { ExcelService } from 'src/app/shared/services/excel.service';
import {PlaceTypeEnum} from 'src/app/shared/enums/placeType-enum';
declare var $:any;
import * as lodash from 'lodash';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import {language} from 'src/languages/language.en';

@Component({
  selector: 'app-warehouse',
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.sass']
})
export class WarehouseComponent implements OnInit {
  warehouses: Array<Warehouse>;
  countries: any[] = [];
  countryActive: any[] = [];
  provinces: any[] = [];
  provinceActive: any[] = [];
  districts: any[] = [];
  districtActive: any[] = [];
  keySortDefault: string = "code";
  warehouse: Warehouse = new Warehouse();
  showModal: boolean = false;
  countryLookup: any = { 
    dataLookup: {},
    value: null,
    displayName: null
  };
  provinceLookup: any;
  districtLookup: any;
  criteria: any = { placeType: PlaceTypeEnum.Warehouse };
  // @ViewChild('formAddEdit') form: NgForm;
  pager: PagerSetting = PAGINGSETTING;
  addButtonSetting: ButtonModalSetting = {
    dataTarget: "add-ware-house-modal",
    typeButton: ButtonType.add
  };
  resetWarehouse(){
    this.warehouse = {
      id: null,
      code: null,
      nameEn: null,
      nameVn: null,
      countryID: null,
      districtID: null,
      provinceID:null,
      countryName: null,
      provinceName: null,
      districtName: null,
      address: null,
      placeType: PlaceTypeEnum.Warehouse
    };
    this.provinces = [];
    this.districts = [];
    this.countryActive = [];
    this.provinceActive = [];
    this.districtActive = [];
    this.ngSelectCountry.active = [];
    this.ngSelectProvince.active = [];
    this.ngSelectDistrict.active = [];
  }
  importButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.import
  };
  exportButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.export
  };
  saveButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.save
  };

  cancelButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.cancel
  };
  
  @ViewChild('chooseCountry') public ngSelectCountry: SelectComponent;
  @ViewChild('chooseProvince') public ngSelectProvince: SelectComponent;
  @ViewChild('chooseDistrict') public ngSelectDistrict: SelectComponent;
  @ViewChild(PaginationComponent) child; 
  @ViewChild('formAddEdit') form: NgForm;
  nameEditModal = "edit-ware-house-modal";
  selectedFilter = "All";
  titleConfirmDelete = "You want to delete this warehouse";
  warehouseSettings: ColumnSetting[] = language.Warehouse;//= WAREHOUSECOLUMNSETTING;
  isDesc: boolean = true;
  configSearch: any = {
    selectedFilter: this.selectedFilter,
    settingFields: this.warehouseSettings,
    typeSearch: TypeSearch.outtab
  };
  
  constructor(private sortService: SortService, 
    private excelService: ExcelService,
    private baseService: BaseService,
    private api_menu: API_MENU) { }

  ngOnInit() {
    this.pager.totalItems = 0;
    this.warehouse.placeType = PlaceTypeEnum.Warehouse;
    this.getWarehouses(this.pager);
    this.getDataCombobox();
  }
  getDataCombobox(){
    this.getCountries();
    this.getProvinces();
    this.getDistricts();
  }
  async getCountries(){
    let responses = await this.baseService.getAsync(this.api_menu.Catalogue.Country.getAllByLanguage, false, true);
    if(responses != null){
      this.countries = responses.map(x=>({"text":x.name,"id":x.id}));
    }
    else{
      this.countries = [];
    }
  }
  async getProvinces(countryId?: number){
    let url = this.api_menu.Catalogue.CatPlace.getProvinces;
    if(countryId != undefined){
      url = url + "?countryId=" + countryId; 
    }
    let responses = await this.baseService.getAsync(url, false, true);
    if(responses != null){
      this.provinces = responses.map(x=>({"text":x.name_VN,"id":x.id}));
    }
    else{
      this.provinces = [];
    }
  }
  async getDistricts(provinceId?: any){
    let url = this.api_menu.Catalogue.CatPlace.getDistricts;
    if(provinceId != undefined){
      url = url + "?provinceId=" + provinceId; 
    }
    let responses = await this.baseService.getAsync(url, false, true);
    if(responses != null){
      this.districts = responses.map(x=>({"text":x.name_VN,"id":x.id}));
    }
    else{
      this.districts = [];
    }
  }
  async getWarehouses(pager: PagerSetting) {
    this.baseService.spinnerShow();
    this.baseService.post(this.api_menu.Catalogue.CatPlace.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.baseService.spinnerHide();
      this.warehouses = response.data.map(x=>Object.assign({},x));
      console.log(this.warehouses);
      this.pager.totalItems = response.totalItems;
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  onSortChange(column) {
    if(column.dataType != 'boolean'){
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.warehouses = this.sortService.sort(this.warehouses, property, this.isDesc);
    }
  }
  async showDetail(item) {
    this.warehouse = item;
    //await this.getCountries();
    if(this.warehouse.countryID != null){
      await this.getProvinces(this.warehouse.countryID);
      let indexOfCountryActive = this.countries.findIndex(x => x.id == this.warehouse.countryID);
      this.countryActive = [this.countries[indexOfCountryActive]];
    }
    if(this.warehouse.provinceID != null){
      //await this.getProvinces(this.warehouse.countryID);
      await this.getDistricts(this.warehouse.provinceID);
      let indexOfProvinceActive = this.provinces.findIndex(x => x.id == this.warehouse.provinceID);
      this.provinceActive = [this.provinces[indexOfProvinceActive]];
    }
    if(this.warehouse.districtID != null){
      //await this.getDistricts(this.warehouse.provinceID);
      let indexOfDistrictActive = this.districts.findIndex(x => x.id == this.warehouse.districtID);
      this.districtActive = [this.districts[indexOfDistrictActive]];
    }
  }
  async onDelete(event) {
    console.log(event);
    if (event) {
      this.baseService.spinnerShow();
      this.baseService.delete(this.api_menu.Catalogue.CatPlace.delete + this.warehouse.id).subscribe((response: any) => {       
          this.baseService.successToast(response.message);
          this.baseService.spinnerHide();
          this.setPageAfterDelete();        
      },err=>{
        this.baseService.spinnerHide();
        this.baseService.handleError(err);
      });
    }
  }
  setPageAfterDelete(){
    this.pager.totalItems = this.pager.totalItems -1;
    let totalPages = Math.ceil(this.pager.totalItems / this.pager.pageSize);
    if (totalPages < this.pager.totalPages) {
      this.pager.currentPage = totalPages;
    }
    this.child.setPage(this.pager.currentPage);
  }
  showConfirmDelete(item) {
    this.warehouse = item;
  }

  setPage(pager) { 
    this.pager.currentPage = pager.currentPage; 
    this.pager.totalPages = pager.totalPages;
    this.pager.pageSize = pager.pageSize
    this.getWarehouses(pager);
  }
  onSubmit(){
    if(this.form.valid && this.warehouse.countryID != null && this.warehouse.provinceID != null && this.warehouse.districtID != null){
      if(this.warehouse.id == null){
        this.addNew();
      }
      else{
        this.update();
      }
    }
    else{
      console.log("submit");
    }
  }
  update(){
    this.baseService.spinnerShow();
    this.baseService.put(this.api_menu.Catalogue.CatPlace.update + this.warehouse.id, this.warehouse).subscribe((response: any) => { 
        $('#edit-ware-house-modal').modal('hide');     
        this.baseService.successToast(response.message);
        this.getWarehouses(this.pager);       
        this.baseService.spinnerHide();
    },err=>{     
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  addNew(){
    this.baseService.spinnerShow();
    this.baseService.post(this.api_menu.Catalogue.CatPlace.add, this.warehouse).subscribe((response: any) => {     
        this.baseService.spinnerHide();
        this.baseService.successToast(response.message);
        this.pager.totalItems = this.pager.totalItems + 1;
        this.pager.currentPage = 1;
        this.child.setPage(this.pager.currentPage);
        this.resetWarehouse();
        this.form.onReset();
        $('#' + this.addButtonSetting.dataTarget).modal('hide');     
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }

  resetSearch(event){
    this.criteria = {
      placeType: PlaceTypeEnum.Warehouse
    };
    this.onSearch(event);
  }
  onSearch(event){
    console.log(event);
    if(event.field == "All"){
      this.criteria.all = event.searchString;
    }
    else{
      this.criteria = {
        placeType: PlaceTypeEnum.Warehouse
      };
      let language = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);
      if(language == SystemConstants.LANGUAGES.ENGLISH){
        if(event.field == "countryName"){
          this.criteria.countryNameEN = event.searchString;
        }
        if(event.field == "provinceName"){
          this.criteria.provinceNameEN = event.searchString;
        }
        if(event.field == "districtName"){
          this.criteria.districtNameEN = event.searchString;
        }
      }
      if(language == SystemConstants.LANGUAGES.VIETNAM){
        if(event.field == "countryName"){
          this.criteria.countryNameVN = event.searchString;
        }
        if(event.field == "provinceName"){
          this.criteria.provinceNameVN = event.searchString;
        }
        if(event.field == "districtName"){
          this.criteria.districtNameVN = event.searchString;
        }
      }
      if(event.field == "code"){
        this.criteria.code = event.searchString;
      }
      if(event.field == "nameEN"){
        this.criteria.nameEN = event.searchString;
      }
      if(event.field == "nameVN"){
        this.criteria.nameVN = event.searchString;
      }
      if(event.field == "address"){
        this.criteria.address = event.searchString;
      }
    }
    this.pager.currentPage = 1;
    this.getWarehouses(this.pager);
  }
  onCancel(){
    this.form.onReset();
    this.resetWarehouse();
    this.setPage(this.pager);
  }
  getColumn(field){
    return this.warehouseSettings.find(x => x.primaryKey == field);
  }
  onChange(value, name: any){
    if(name == 'country')
    {
      this.warehouse.countryID = value.id;
      this.getProvinces(value.id);
      this.chooseCountryReset();
      this.provinces = [];
      this.districts = [];
      //this.refreshValue(null, 'country');
    }
    if(name == 'province'){
      this.warehouse.provinceID = value.id;
      this.getDistricts(value.id);
      this.chooseProvinceReset();
      this.districts = [];
    }
    if(name == 'district'){
      this.warehouse.districtID = value.id;
    }
  }
  showAdd(){
    this.resetWarehouse();
    this.showModal = true;
    this.form.onReset();
    this.warehouse.countryID = null;
    this.warehouse.provinceID = null;
    this.warehouse.districtID = null;
    this.ngSelectCountry.active = [];
    this.ngSelectProvince.active = [];
    this.ngSelectDistrict.active = [];
  }
  // valueCountry: any = {};
  // valueProvince: any = {};
  // valueDistrict: any = {};
  value: any = {};
  public refreshValue(value:any, name: any):void {
    this.value = value;
    // if(name == 'country'){
    //   this.valueCountry = value;
    //   this.chooseCountryReset();
    // }
    // if(name == 'province'){
    //   this.valueProvince = value;
    //   this.chooseProvinceReset();
    // }
    // if(name == 'district'){
    //   this.valueDistrict = value;
    // }
  }
  public removed(value:any, name: any):void {
    if(name == 'country'){
      this.warehouse.countryID = null;
      this.warehouse.provinceID = null;
      this.warehouse.districtID = null;
      this.provinces = [];
      this.districts = [];
      this.chooseCountryReset();
    }
    if(name == 'province'){
      this.warehouse.provinceID = null;
      this.warehouse.districtID = null;
      this.districts = [];
      //this.chooseProvinceReset();
    }
    if(name == 'district'){
      this.warehouse.districtID = null;
    }
    console.log('Removed value is: ', value);
  }
  public typed(value:any):void {
    console.log('New search input: ', value);
  }
  chooseCountryReset(){
    this.ngSelectProvince.active = [];
    this.ngSelectDistrict.active = [];
    this.provinceActive = [];
    this.districtActive = [];
  }
  chooseProvinceReset(){
    this.ngSelectDistrict.active = [];
    this.districtActive = null;
  }

  /**
   * EXPORT - IMPORT DATA 
   */
  async export(){
    var warehouseData = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.query, this.criteria);
    console.log(warehouseData);    
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) == SystemConstants.LANGUAGES.ENGLISH_API) {
      warehouseData = lodash.map(warehouseData, function (item,index) {        
        return [
          index+1,
          item['code'],
          item['name_EN'],
          item['name_VN'],
          item['address'],
          item['districtNameEN'],
          item['provinceNameEN'],
          item['countryNameEN'],
          (item['inactive'] == true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) == SystemConstants.LANGUAGES.VIETNAM_API){
      warehouseData = lodash.map(warehouseData, function (item,index) {       
        return [
          index+1,
          item['code'],
          item['name_EN'],
          item['name_VN'],
          item['address'],
          item['districtNameVN'],
          item['provinceNameVN'],
          item['countryNameVN'],
          (item['inactive'] == true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }
    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Warehouse list";
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.author = currrently_user;
    exportModel.header = [
      {name:"No.",width:10},
      {name:"Code",width:20},
      {name:"Name EN",width:20},
      {name:"Name VN",width:20},
      {name:"Address",width:30},
      {name:"Disctric",width:20},
      {name:"City/Province",width:20},
      {name:"Country",width:20},
      {name:"Status",width:20}
    ]
    exportModel.data = warehouseData;
    exportModel.fileName = "Warehouse";
    
    this.excelService.generateExcel(exportModel);
  }
}
