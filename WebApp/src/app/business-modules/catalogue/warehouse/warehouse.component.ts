import { Component, OnInit, ViewChild } from '@angular/core';
import { Warehouse } from '../../../shared/models/catalogue/ware-house.model';
import { ColumnSetting } from '../../../shared/models/layout/column-setting.model';
import { SortService } from '../../../shared/services/sort.service';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';
import { PagerSetting } from '../../../shared/models/layout/pager-setting.model';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { NgForm } from '@angular/forms';
import { SystemConstants } from '../../../../constants/system.const';
import { API_MENU } from '../../../../constants/api-menu.const';
import { SelectComponent } from 'ng2-select';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { WAREHOUSECOLUMNSETTING } from 'src/app/business-modules/catalogue/warehouse/warehouse.columns';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
declare var $:any;

@Component({
  selector: 'app-warehouse',
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.sass']
})
export class WarehouseComponent implements OnInit {
  warehouses: Array<Warehouse>;
  countries: any[];
  countryActive: {};
  provinces: any[];
  provinceActive: {};
  districts: any[];
  districtActive: {};
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
  criteria: any = { placeType: 12 };
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
      name: null,
      countryID: null,
      districtID: null,
      provinceID:null,
      countryName: null,
      provinceName: null,
      districtName: null,
      address: null,
      placeType: 12
    };
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
  warehouseSettings: ColumnSetting[] = WAREHOUSECOLUMNSETTING;
  isDesc: boolean = true;
  configSearch: any = {
    selectedFilter: this.selectedFilter,
    settingFields: this.warehouseSettings,
    typeSearch: TypeSearch.outtab
  };
  
  constructor(private sortService: SortService, private baseService: BaseService,
    private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU) { }

  ngOnInit() {
    this.warehouse.placeType = 12;
    this.getWarehouses(this.pager);
    this.getDataCombobox();
  }
  getDataCombobox(){
    this.getCountries();
    this.getProvinces();
    this.getDistricts();
  }
  getCountries(){
    this.baseService.get(this.api_menu.Catalogue.Country.getAllByLanguage).subscribe((response: any) => {
      if(response != null){
        this.countries = response.map(x=>({"text":x.name,"id":x.id}));
      }
      else{
        this.countries = [];
      }
    });
  }
  getProvinces(id?: number){
    let url = this.api_menu.Catalogue.CatPlace.getProvinces;
    if(id != undefined){
      url = url + "?countryId=" + id; 
    }
    this.baseService.get(url).subscribe((response: any) => {
      if(response != null){
        this.provinces = response.map(x=>({"text":x.name_VN,"id":x.id}));
      }
      else{
        this.provinces = [];
      }
      this.countryLookup.dataLookup = this.provinces;
      this.countryLookup.value = "id";
      this.countryLookup.displayName = "nameEn";
      console.log(this.provinces);
    });
  }
  getDistricts(id?: number){
    let url = this.api_menu.Catalogue.CatPlace.getDistricts;
    if(id != undefined){
      url = url + "?provinceId=" + id; 
    }
    this.baseService.get(url).subscribe((response: any) => {
      if(response != null){
        this.districts = response.map(x=>({"text":x.name_VN,"id":x.id}));
      }
      else{
        this.districts = [];
      }
    });
  }
  async getWarehouses(pager: PagerSetting) {
    this.spinnerService.show();
    this.baseService.post(this.api_menu.Catalogue.CatPlace.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.spinnerService.hide();
      this.warehouses = response.data.map(x=>Object.assign({},x));
      this.pager.totalItems = response.totalItems;
    });
  }
  onSortChange(column) {
    if(column.dataType != 'boolean'){
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.warehouses = this.sortService.sort(this.warehouses, property, this.isDesc);
    }
  }
  showDetail(item) {
    this.warehouse = item;
    this.countryActive = this.countries.find(x => x.id == this.warehouse.countryID);
    this.provinceActive = this.provinces.find(x => x.id == this.warehouse.provinceID);
    this.districtActive = this.districts.find(x => x.id == this.warehouse.districtID);
  }
  async onDelete(event) {
    console.log(event);
    if (event) {
      this.baseService.delete(this.api_menu.Catalogue.CatPlace.delete + this.warehouse.id).subscribe((response: any) => {
        if (response.status == true) {
          this.toastr.success(response.message,"",{positionClass:'toast-bottom-right'});
          this.setPageAfterDelete();
        }
        if (response.status == false) {
          this.toastr.error(response.message,"",{positionClass:'toast-bottom-right'});
        }
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
    if(this.form.valid){
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
    this.baseService.put(this.api_menu.Catalogue.CatPlace.update + this.warehouse.id, this.warehouse).subscribe((response: any) => {
      if (response.status == true){
        $('#edit-ware-house-modal').modal('hide');
        this.toastr.success(response.message);
        this.getWarehouses(this.pager);
        
      }
    });
  }
  addNew(){
    this.baseService.post(this.api_menu.Catalogue.CatPlace.add, this.warehouse).subscribe((response: any) => {
      if (response.status == true){
        this.toastr.success(response.message,"",{positionClass:'toast-bottom-right'});
        //this.getWarehouses(this.pager);
        this.pager.totalItems = this.pager.totalItems + 1;
        this.pager.currentPage = 1;
        this.child.setPage(this.pager.currentPage);
        this.resetWarehouse();
        this.form.onReset();
        $('#' + this.addButtonSetting.dataTarget).modal('hide');
      }
      else{
        this.toastr.error(response.message,"",{positionClass:'toast-bottom-right'});
      }
    });
  }
  resetSearch(event){
    this.criteria = {
      placeType: 12
    };
  }
  onSearch(event){
    console.log(event);
    if(event.field == "All"){
      this.criteria.all = event.searchString;
    }
    else{
      this.criteria.all = null;
      let language = localStorage.getItem(SystemConstants.CURRENT_LANGUAGE);
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
  onCountrychange(country){
    this.warehouse.countryID = country.id;
    this.getProvinces(country.id);
    this.refreshProvinceValue(null);
  }
  onProvincechange(province){
    this.warehouse.provinceID = province.id;
    this.getDistricts(province.id);
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
  valueCountry: any = {};
  valueProvince: any = {};
  valueDistrict: any = {};
  public refreshCountryValue(value:any):void {
    this.valueCountry = value;
    this.chooseCountryReset();
  }
  public refreshDistrictValue(value:any):void {
    this.valueDistrict = value;
  }
  public refreshProvinceValue(value: any): void{
    this.valueDistrict = value;
    this.chooseProvinceReset();
  }
  public removed(value:any):void {
    console.log('Removed value is: ', value);
  }
  public typed(value:any):void {
    console.log('New search input: ', value);
  }
  chooseCountryReset(){
    this.ngSelectProvince.active = [];
    this.ngSelectDistrict.active = [];
  }
  chooseProvinceReset(){
    this.ngSelectDistrict.active = [];
  }
  onDistrictchange(district){
    this.warehouse.districtID = district.id;
  }

  /**
   * EXPORT - IMPORT DATA 
   */
  async export(){
    
  }

  async import(){

  }
}
