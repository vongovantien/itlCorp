import { Component, OnInit, ViewChild } from '@angular/core';
import * as lodash from 'lodash';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import * as dataHelper from 'src/helper/data.helper';
import { SystemConstants } from 'src/constants/system.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { ExcelService } from 'src/app/shared/services/excel.service';
declare var $: any;

@Component({
  selector: 'app-location',
  templateUrl: './location.component.html',
  styleUrls: ['./location.component.sass']
})
export class LocationComponent implements OnInit {

  /**
   *START VARIABLES DEFINITIONS
   */
  ListCountries: any = [];
  ConstListCountries: any = [];
  CountryToAdd = new CountryModel();
  CountryToUpdate = new CountryModel();
  indexCountryDelete: Number = -1;
  indexCountryUpdate: Number = -1;
  ObjectToDelete: String = "";
  deleteWhat: any = null;
  idCountryToDelete: any = null;
  currentActiveCountry: any = [];

  ListProvinceCities: any = [];
  ConstListProvinceCities: any = [];
  ProvinceCityToAdd = new CatPlaceModel();
  ProvinceCityToUpdate = new CatPlaceModel();
  currentActiveProvince: any = [];

  ListDistricts: any = [];
  ConstListDistrict: any = [];
  DistrictToAdd = new CatPlaceModel();
  DistrictToUpdate = new CatPlaceModel();
  currentActiveDistrict: any = [];

  ListWards: any = [];
  ConstListWards: any = [];
  WardToAdd = new CatPlaceModel();
  WardToUpdate = new CatPlaceModel();


  pager: PagerSetting = PAGINGSETTING;

  searchKey: string = "";

  searchKeyCountryTab:string = "";
  searchKeyProvinceTab:string = "";
  searchKeyDistrictTab:string = "";
  searchKeyWardTab:string = "";
  CURRENT_LANGUAGE = localStorage.getItem(SystemConstants.CURRENT_LANGUAGE);

  fieldNameByLanguage(field){
    if(this.CURRENT_LANGUAGE.toLowerCase()===SystemConstants.DEFAULT_LANGUAGE.toLowerCase()){
      return field+"EN";
    }else{
      return field+"VN";
    }
  }

  listFilterCountryTab = [
    { filter: "All", field: "all" }, { filter: "Code", field: "code" },
    { filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" }];
  selectedFilterCountryTab = this.listFilterCountryTab[0].filter;

  listFilterProvinceCityTab = [
    { filter: "All", field: "all" }, { filter: "Code", field: "code" },
    { filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" },
    { filter: "Country", field: this.fieldNameByLanguage("countryName") }];
  selectedFilterProvinceCityTab = this.listFilterProvinceCityTab[0].filter;

  listFilterDistrictTab = [
    { filter: "All", field: "all" }, { filter: "Code", field: "code" },
    { filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" },
    { filter: "Country", field: this.fieldNameByLanguage("countryName") }, { filter: "Province-City", field: this.fieldNameByLanguage("provinceName") }];
  selectedFilterDistrictTab = this.listFilterDistrictTab[0].filter;

  listFilterWardTab = [
    { filter: "All", field: "all" }, { filter: "Code", field: "code" },
    { filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" },
    { filter: "Country", field: this.fieldNameByLanguage("countryName") }, { filter: "Province-City", field: this.fieldNameByLanguage("provinceName") }, { filter: "District", field: this.fieldNameByLanguage("districtName") }];
  selectedFilterWardTab = this.listFilterWardTab[0].filter;

  countrySearchObject = {
    code: "",
    nameVn: "",
    nameEn: "",
    condition: "OR",
  }
  searchObject: any = { condition: "OR" };
  @ViewChild(PaginationComponent) child;

  /**
   * END OF VARIABLES DEFINITIONS
   */

  constructor(
    private excelService: ExcelService,
    private baseServices: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  resetNg2SelectCountry = true;
  resetNg2SelectProvince = true;
  resetNg2SelectDistrict = true;

  resetNgSelect(type) {
    if (type == "country") {
      this.resetNg2SelectCountry = false;
      setTimeout(() => {
        this.resetNg2SelectCountry = true;
      }, 300);
    }
    if (type == "province") {
      this.resetNg2SelectProvince = false;
      setTimeout(() => {
        this.resetNg2SelectProvince = true;
      }, 300);
    }
    if (type == "district") {
      this.resetNg2SelectDistrict = false;
      setTimeout(() => {
        this.resetNg2SelectDistrict = true;
      }, 300);
    }

    if (type == "all") {
      this.resetNg2SelectCountry = false;
      this.resetNg2SelectProvince = false;
      this.resetNg2SelectDistrict = false;
      setTimeout(() => {
        this.resetNg2SelectCountry = true;
        this.resetNg2SelectProvince = true;
        this.resetNg2SelectDistrict = true;
      }, 300);
    }

  }

  showAdd(){
    this.ngSelectDataProvinces = [];
    this.ngSelectDataDistricts = [];    
  }
  ngSelectData(sourceData:any){
    var dataReturn:any=null;
    var isCountry :Boolean = false;

    if(sourceData[0].placeTypeID==undefined){
      isCountry = true;
    }

    if (this.CURRENT_LANGUAGE.toLowerCase() == SystemConstants.DEFAULT_LANGUAGE.toLowerCase()) {
      if(isCountry){
        dataReturn = sourceData.map(x => ({ "text": x.code + " - " + x.nameEn, "id": x.id }));    
      }else{
        dataReturn = sourceData.map(x => ({ "text": x.code + " - " + x.name_EN, "id": x.id }));    
      }  
    } else {
      if(isCountry){
        dataReturn = sourceData.map(x => ({ "text": x.code + " - " + x.nameVn, "id": x.id }));    
      }else{
        dataReturn = sourceData.map(x => ({ "text": x.code + " - " + x.name_VN, "id": x.id }));     
      }      
    }
    return dataReturn;
  }

  async ngOnInit() {
    await this.getCountries();
    this.getProvinceCities();
    this.getDistrict();
    this.getWards();
    this.getAllCountries();
  }
  activeTab:string = "country";
  changeTab(activeTab){
    this.activeTab = activeTab;
    if(activeTab==="country"){
        this.pager.currentPage = 1;
        this.pager.pageSize = 10;
        this.pager.totalItems = this.totalItemCountries;
        this.child.setPage(this.pager.currentPage);
    }
    if(activeTab==="province"){
      this.pager.currentPage = 1;
      this.pager.pageSize = 10;
      this.pager.totalItems = this.totalItemProvinces;
      this.child.setPage(this.pager.currentPage);
    }
    if(activeTab==="district"){
      this.pager.currentPage = 1;
      this.pager.pageSize = 10;
      this.pager.totalItems = this.totalItemDistricts;
      this.child.setPage(this.pager.currentPage);
    }
    if(activeTab==="ward"){
       this.pager.currentPage = 1;
        this.pager.pageSize = 10;
       this.pager.totalItems = this.totalItemWards;
       this.child.setPage(this.pager.currentPage);
    }
}


  async setPage(pager:PagerSetting) {
    this.pager.currentPage = pager.currentPage; 
    this.pager.totalPages = pager.totalPages;
    this.pager.pageSize = pager.pageSize
    if(this.activeTab==="country"){
      this.ListCountries = await this.getCountries();
    }
    if(this.activeTab==="province"){
      this.ListProvinceCities = await this.getProvinceCities();
    }
    if(this.activeTab==="district"){
      this.ListDistricts = await this.getDistrict();
    }
    if(this.activeTab==="ward"){
      this.ListWards = await this.getWards();
    }
    
  }

  setPageAfterAdd() {
    this.child.setPage(this.pager.currentPage);
    if (this.pager.currentPage < this.pager.totalPages) {
      this.pager.currentPage = this.pager.totalPages;
      this.child.setPage(this.pager.currentPage);
    }
  }

  setPageAfterDelete() {
    this.child.setPage(this.pager.currentPage);
    if (this.pager.currentPage > this.pager.totalPages) {
      this.pager.currentPage = this.pager.totalPages;
      this.child.setPage(this.pager.currentPage);
    }
  }





  /**
   * BEGIN COUNTRY METHODS
   */

  async searchInCountryTab() {
    this.searchObject = {};
    if (this.selectedFilterCountryTab == "All") {
      this.searchObject.condition = "OR";
      for (var i = 1; i < this.listFilterCountryTab.length; i++) {
        eval("this.searchObject[this.listFilterCountryTab[i].field]=this.searchKeyCountryTab");
      }

    } else {

      this.searchObject.condition = "AND";
      for (var i = 1; i < this.listFilterCountryTab.length; i++) {
        console.log(this.listFilterCountryTab[i].field);
        if (this.selectedFilterCountryTab == this.listFilterCountryTab[i].filter) {
          eval("this.searchObject[this.listFilterCountryTab[i].field]=this.searchKeyCountryTab");
        }

      }
    }
    await this.getCountries();
  }

  async resetCountryTab() {
    this.searchKeyCountryTab = "";
    this.searchObject = {};
    this.selectedFilterCountryTab = this.listFilterCountryTab[0].filter;
    await this.getCountries();
  }

  totalItemCountries:number = null;
  totalItemProvinces:number = null;
  totalItemDistricts:number = null;
  totalItemWards:number = null;

  async getCountries() {
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.Country.paging + "/" + this.pager.currentPage + "/" + this.pager.pageSize, this.searchObject, false, true);
    this.ListCountries = response.data;
    this.totalItemCountries = response.totalItems;
    this.pager.totalItems = this.totalItemCountries;
    this.ConstListCountries = response.data;
    return response.data;
  }

  async addCountry(form: NgForm, action) {
    if (action == "yes") {
      delete this.CountryToAdd.id;
      if (form.form.status != "INVALID") {
        var respone = await this.baseServices.postAsync(this.api_menu.Catalogue.Country.addNew, this.CountryToAdd, true, true);
        await this.getCountries();
        if(respone!=undefined && respone.status){
          this.setPageAfterAdd();
          form.onReset();
          $('#add-country-modal').modal('hide');
        }
      }
    } else {
      this.CountryToAdd = new CountryModel();
      form.onReset();
      $('#add-country-modal').modal('hide');
    }
  }

  async showUpdateCountry(id) {
    this.CountryToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.Country.getById + id, true, true);
  }

  async updateCountry(form: NgForm, action) {
    if (action == "yes") {
      if (form.form.status != "INVALID") {
        await this.baseServices.putAsync(this.api_menu.Catalogue.Country.update, this.CountryToUpdate);
        await this.getCountries();
        await this.getProvinceCities();
        form.onReset();
        $('#update-country-modal').modal('hide');
      }
    } else {
      form.onReset();
      $('#update-country-modal').modal('hide');
    }
  }

  prepareDeleteCountry(id) {
    this.idCountryToDelete = id;
    this.ObjectToDelete = "country";
  }

  async delete(action) {
    if (action == "yes") {
      if (this.ObjectToDelete == "country") {
        await this.baseServices.deleteAsync(this.api_menu.Catalogue.Country.delete + this.idCountryToDelete);
        await this.getCountries();        
        this.setPageAfterDelete();
      }

      if (this.ObjectToDelete == "province-city") {
        await this.baseServices.deleteAsync(this.api_menu.Catalogue.CatPlace.delete + this.idProvinceCityToDelete);
        await this.getProvinceCities();
        this.setPageAfterDelete();
      }

      if (this.ObjectToDelete == "district") {
        await this.baseServices.deleteAsync(this.api_menu.Catalogue.CatPlace.delete + this.idDistrictToDelete);
        await this.getDistrict();
        this.setPageAfterDelete();
      }

      if(this.ObjectToDelete == "ward"){
        await this.baseServices.deleteAsync(this.api_menu.Catalogue.CatPlace.delete + this.idWardToDelete);
        await this.getWards();
        this.setPageAfterDelete();
      }
     
    }
  }




  /**
   * END COUNTRY METHODS 
   */


  /**
   * START PROVINCE-CITY METHODS
   */

  async searchInProvinceCityTab() {

    this.searchObject = {};
    this.searchObject.placeType = PlaceTypeEnum.Province; //9;
    if (this.selectedFilterProvinceCityTab == "All") {
      this.searchObject.All = this.searchKeyProvinceTab;
    } else {
      this.searchObject = {};
      for (var i = 1; i < this.listFilterProvinceCityTab.length; i++) {
        console.log(this.listFilterProvinceCityTab[i].field);
        if (this.selectedFilterProvinceCityTab == this.listFilterProvinceCityTab[i].filter) {
          eval("this.searchObject[this.listFilterProvinceCityTab[i].field]=this.searchKeyProvinceTab");
        }

      }
    }
    await this.getProvinceCities();
  }

  async resetProvinceCityTab() {
    this.searchKeyProvinceTab = "";
    this.searchObject = {};
    this.selectedFilterProvinceCityTab = this.listFilterProvinceCityTab[0].filter;
    await this.getProvinceCities();
  }

  async getProvinceCities() {
    this.searchObject.placeType = PlaceTypeEnum.Province; //9;
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.searchObject);
    this.ListProvinceCities = response.data;
    this.ConstListProvinceCities = response.data;
    this.totalItemProvinces = response.totalItems;
    this.pager.totalItems = this.totalItemProvinces;
    return response.data;
  }

  async addProvinceCity(form: NgForm, action) {
    if (action == "yes") {
      if (form.form.status != "INVALID" && this.ProvinceCityToAdd.countryId != null) {
        this.ProvinceCityToAdd.placeType = PlaceTypeEnum.Province;
        var response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.add, this.ProvinceCityToAdd);
        await this.getProvinceCities();
        console.log(response);
        if(response!=undefined && response.status){
          this.setPageAfterAdd();
          form.onReset();
          this.resetNgSelect("all");
          $('#add-city-province-modal').modal('hide');
        }

      }
    } else {
      this.ProvinceCityToAdd = new CatPlaceModel();
      form.onReset();
      this.resetNgSelect("all");
      $('#add-city-province-modal').modal('hide');
    }

  }

  idProvinceCityToUpdate: string = "";
  async showUpdateProvince(id) {    
    this.idProvinceCityToUpdate = id;
    this.ProvinceCityToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.CatPlace.getById + id);

    var countryId = this.ProvinceCityToUpdate.countryId;

    var indexCurrentCountry = lodash.findIndex(this.ngSelectDataCountries, function (o) {
      return o['id'] == countryId
    });
    this.currentActiveCountry = [this.ngSelectDataCountries[indexCurrentCountry]];
  }

  async updateProvinceCity(form: NgForm, action) {
    if (action == "yes") {
      if (form.form.status != "INVALID" && this.ProvinceCityToUpdate.countryId != null) {
        console.log(JSON.stringify(this.ProvinceCityToUpdate));
        await this.baseServices.putAsync(this.api_menu.Catalogue.CatPlace.update + this.idProvinceCityToUpdate, this.ProvinceCityToUpdate);
        await this.getProvinceCities();
        form.onReset();
        $('#update-city-province-modal').modal('hide');
      }
    } else {
      form.onReset();
      $('#update-city-province-modal').modal('hide');
    }
  }

  idProvinceCityToDelete: string = "";
  prepareDeleteProvince(id) {
    this.idProvinceCityToDelete = id;
    this.ObjectToDelete = "province-city";
  }


  /**
   * END PROVINCE-CITY METHODS
   */


  /**
   * START DISTRICT METHODS
   */

 
  async searchInDistrictTab() {

    this.searchObject = {};
    this.searchObject.placeType = PlaceTypeEnum.District; //9;
    if (this.selectedFilterDistrictTab == "All") {
      this.searchObject.All = this.searchKeyDistrictTab;
    } else {
      this.searchObject = {};
      for (var i = 1; i < this.listFilterDistrictTab.length; i++) {
        console.log(this.listFilterDistrictTab[i].field);
        if (this.selectedFilterDistrictTab == this.listFilterDistrictTab[i].filter) {
          eval("this.searchObject[this.listFilterDistrictTab[i].field]=this.searchKeyDistrictTab");
        }

      }
    }
    await this.getDistrict();
  }

  async resetDistrictTab() {
    this.searchKeyDistrictTab = "";
    this.searchObject = {};
    this.selectedFilterDistrictTab = this.listFilterDistrictTab[0].filter;
    await this.getDistrict();
  }

  public async getDistrict() {
    this.searchObject.placeType = PlaceTypeEnum.District;
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.searchObject);
    this.ListDistricts = response.data;
    this.totalItemDistricts = response.totalItems;
    this.pager.totalItems = this.totalItemDistricts;
    return response.data;
  }

  async addDistrict(form: NgForm, action) {
    console.log(this.DistrictToAdd);
    if (action == "yes") {
      if (form.form.status != "INVALID" && this.DistrictToAdd.countryId != null && this.DistrictToAdd.provinceId != null) {
        this.DistrictToAdd.placeType = PlaceTypeEnum.District;
        var response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.add, this.DistrictToAdd);
        await this.getDistrict();
        if(response!=undefined && response.status){
          this.setPageAfterAdd();
          form.onReset();
          this.resetNgSelect("all");
          $('#add-district-modal').modal('hide');
        }

      }
    } else {
      this.DistrictToAdd = new CatPlaceModel();
      form.onReset();
      this.resetNgSelect("all");
      $('#add-district-modal').modal('hide');
    }
  }

  

  idDistrictToUpdate: string = "";
  async showUpdateDistrict(id) {
    this.idDistrictToUpdate = id;
    this.DistrictToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.CatPlace.getById + id);
    console.log(this.DistrictToUpdate);
    var countryId = this.DistrictToUpdate.countryId;
    var provinceId = this.DistrictToUpdate.provinceId;

    var indexCurrentCountry = lodash.findIndex(this.ngSelectDataCountries, function (o) {
      return o['id'] == countryId
    });

    var provinces = await dataHelper.getProvinces(countryId, this.baseServices, this.api_menu);
    this.ngSelectDataProvinces = this.ngSelectData(provinces);
    this.resetNgSelect("province");

    var indexCurrentProvince = lodash.findIndex(this.ngSelectDataProvinces, function (o) {
      return o['id'] == provinceId;
    });

    this.currentActiveCountry = [this.ngSelectDataCountries[indexCurrentCountry]];
    this.currentActiveProvince = [this.ngSelectDataProvinces[indexCurrentProvince]];

  }

  async updateDistrict(form: NgForm, action) {
    if (action == "yes") {
      if (form.form.status != "INVALID" && this.DistrictToUpdate.countryId != null && this.DistrictToUpdate.provinceId != null) {
        await this.baseServices.putAsync(this.api_menu.Catalogue.CatPlace.update + this.idDistrictToUpdate, this.DistrictToUpdate);
        await this.getDistrict();
        form.onReset();
        $('#update-district-modal').modal('hide');
      }
    } else {
      form.onReset();
      $('#update-district-modal').modal('hide');
    }
  }

  idDistrictToDelete: string = "";
  prepareDeleteDistrict(id) {
    this.idDistrictToDelete = id;
    this.ObjectToDelete = "district";
  }


  ngSelectDataCountries: any = [];
  ngSelectDataProvinces: any = [];
  ngSelectDataDistricts: any = [];

  async getAllCountries(){
    var countries = await this.baseServices.getAsync(this.api_menu.Catalogue.Country.getAll,false,false);
    this.ngSelectDataCountries = this.ngSelectData(countries);
  }


  /**
   * END DISTRICT METHODS
   */

  /**
   * BEGIN WARD METHODS
   */
  async searchInWardTab() {

    this.searchObject = {};
    this.searchObject.placeType = PlaceTypeEnum.Ward; //9;
    if (this.selectedFilterWardTab == "All") {
      this.searchObject.All = this.searchKeyWardTab;
    } else {
      this.searchObject = {};
      for (var i = 1; i < this.listFilterWardTab.length; i++) {
        if (this.selectedFilterWardTab == this.listFilterWardTab[i].filter) {
          eval("this.searchObject[this.listFilterWardTab[i].field]=this.searchKeyWardTab");
        }

      }
    }
    console.log(this.searchObject);
    await this.getWards();
  }

  async resetWardTab() {
    this.searchKeyWardTab = "";
    this.searchObject = {};
    this.selectedFilterWardTab = this.listFilterWardTab[0].filter;
    await this.getWards();
  }
  

  public async getWards() {
    this.searchObject.placeType = PlaceTypeEnum.Ward;
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.searchObject);
    this.ListWards = response.data;
    this.ConstListWards = response.data;
    this.totalItemWards = response.totalItems;
    this.pager.totalItems = this.totalItemWards;
    return response.data;
  }



  async addWard(form: NgForm, action) {
    console.log(this.WardToAdd);
    if (action == "yes") {
      if (form.form.status != "INVALID" && this.WardToAdd.countryId != null && this.WardToAdd.provinceId != null && this.WardToAdd.districtId != null) {
        this.WardToAdd.placeType = PlaceTypeEnum.Ward;
        var response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.add, this.WardToAdd);
        await this.getWards();        
        if(response!=undefined && response.status){
          this.setPageAfterAdd();
          form.onReset();
          this.resetNgSelect("all");
          $('#add-ward-modal').modal('hide');
        }

      }
    } else {
      this.WardToAdd = new CatPlaceModel();
      form.onReset();
      this.resetNgSelect("all");
      $('#add-ward-modal').modal('hide');
    }
  }


  idWardoUpdate: string = ""
  async showUpdateWard(id) {
    this.idWardoUpdate = id;
    this.WardToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.CatPlace.getById + id);

    var countryId = this.WardToUpdate.countryId;
    var provinceId = this.WardToUpdate.provinceId;
    var districtId = this.WardToUpdate.districtId;

    var provinces = await dataHelper.getProvinces(countryId, this.baseServices, this.api_menu);
    var districts = await dataHelper.getDistricts(countryId, provinceId, this.baseServices, this.api_menu)

    this.ngSelectDataProvinces = this.ngSelectData(provinces);
    this.ngSelectDataDistricts = this.ngSelectData(districts);

    console.log({ provinces: this.ngSelectDataProvinces }, { districts: this.ngSelectDataDistricts });

    var indexCurrentCountry = lodash.findIndex(this.ngSelectDataCountries, function (o) {
      return o['id'] == countryId
    });

    var indexCurrentProvince = lodash.findIndex(this.ngSelectDataProvinces, function (o) {
      return o['id'] == provinceId;
    });

    var indexCurrentDistrict = lodash.findIndex(this.ngSelectDataDistricts, function (o) {
      return o['id'] == districtId;
    });
    this.currentActiveCountry = [this.ngSelectDataCountries[indexCurrentCountry]];
    this.currentActiveProvince = [this.ngSelectDataProvinces[indexCurrentProvince]];
    this.currentActiveDistrict = [this.ngSelectDataDistricts[indexCurrentDistrict]];
  }

  async updateWard(form: NgForm, action) {
    console.log(this.WardToUpdate);
    if (action == "yes") {
      if (form.form.status != "INVALID" && this.WardToUpdate.countryId != null && this.WardToUpdate.provinceId != null && this.WardToUpdate.districtId) {
        var respone = await this.baseServices.putAsync(this.api_menu.Catalogue.CatPlace.update + this.idWardoUpdate, this.WardToUpdate);
        await this.getWards();
        if (respone!= undefined && respone.status) {
          form.onReset();
          $('#update-ward-modal').modal('hide');
        }

      }
    } else {
      form.onReset();
      $('#update-ward-modal').modal('hide');
    }
  }

  idWardToDelete:any = null;
  prepareDeleteWard(id) {
    this.idWardToDelete = id;
    this.ObjectToDelete = "ward";
  }





  /**
   * END WARD METHODS
   */


  private value: any = {};
  private _disabledV: string = '0';
  private disabled: boolean = false;

  private get disabledV(): string {
    return this._disabledV;
  }

  private set disabledV(value: string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }

  public async selectedCountry(value: any, action, type_location) {

    if (action == "add") {
      if (type_location == "province-city") {
        this.ProvinceCityToAdd.countryId = value.id;
      }
      if (type_location == "district") {
        this.DistrictToAdd.countryId = value.id;
        this.DistrictToAdd.provinceId = null;
        let provinces = await dataHelper.getProvinces(value.id, this.baseServices, this.api_menu);
        this.ngSelectDataProvinces = provinces.length==0 ?[]: this.ngSelectData(provinces);
        this.resetNgSelect("province");
      }
      if (type_location == "ward") {
        this.WardToAdd.countryId = value.id;
        this.WardToAdd.provinceId = null;
        this.WardToAdd.districtId = null;
        this.ngSelectDataDistricts=[];
        this.ngSelectDataProvinces = [];
        let provinces = await dataHelper.getProvinces(value.id, this.baseServices, this.api_menu);
        this.ngSelectDataProvinces = provinces.length==0 ?[]: this.ngSelectData(provinces);
        this.resetNgSelect("province");
        this.resetNgSelect("district");
      }
    } else {
      if (type_location == "province-city") {
        this.ProvinceCityToUpdate.countryId = value.id;
      }
      if (type_location == "district") {
        this.DistrictToUpdate.countryId = value.id;
        this.DistrictToUpdate.provinceId = null;
        let provinces = await dataHelper.getProvinces(value.id, this.baseServices, this.api_menu);
        this.ngSelectDataProvinces = provinces.length==0 ?[]: this.ngSelectData(provinces);
        this.resetNgSelect("province");
      }
      if (type_location == "ward") {
        this.WardToUpdate.countryId = value.id;
        this.WardToUpdate.provinceId = null;
        this.WardToUpdate.districtId = null;
        this.currentActiveDistrict = [];
        this.currentActiveProvince = [];
        let provinces = await dataHelper.getProvinces(value.id, this.baseServices, this.api_menu);
        this.ngSelectDataProvinces = provinces.length==0 ?[]: this.ngSelectData(provinces);
        this.resetNgSelect("province");
        this.resetNgSelect("district");   
      }
    }

  }


  public async selectedProvince(value: any, action, type_location) {
    if (action == "add") {
      if (type_location == "district") {
        this.DistrictToAdd.provinceId = value.id
      }
      if (type_location == "ward") {
        this.WardToAdd.provinceId = value.id;
        this.WardToAdd.districtId = null;
        let districts = await dataHelper.getDistricts(this.WardToAdd.countryId, this.WardToAdd.provinceId, this.baseServices, this.api_menu);
        this.ngSelectDataDistricts = districts.length==0 ? []: this.ngSelectData(districts);
        this.resetNgSelect("ward");
      }

    }
    else {
      if (type_location == "district") {
        this.DistrictToUpdate.provinceId = value.id;
      }
      if (type_location == "ward") {
        this.WardToUpdate.provinceId = value.id;        
        this.WardToAdd.districtId = null;
        this.currentActiveDistrict=[];
        // this.currentActiveProvince = [value]
        let districts = await dataHelper.getDistricts(this.WardToUpdate.countryId, this.WardToUpdate.provinceId, this.baseServices, this.api_menu);
        this.ngSelectDataDistricts = districts.length==0 ? []: this.ngSelectData(districts);
        this.resetNgSelect("district");
      }
    }
  }



  public async selectedDistrict(value: any, action, type_location) {
    if (action == "add") {
      if (type_location == "ward") {
        this.WardToAdd.districtId = value.id
      }

    }
    else {
      if (type_location == "ward") {
        this.WardToUpdate.districtId = value.id;
      }
    }
  }


  public removed(value: any, action): void {
    if (action == "addCountryProvince") {
      this.ProvinceCityToAdd.countryId = null;
    }
    if (action == "updateCountryProvince") {
      this.ProvinceCityToUpdate.countryId = null;
    }

    if (action == "addCountryDistrict") {
      this.DistrictToAdd.countryId = null;
      this.DistrictToAdd.provinceId = null;
      this.ngSelectDataProvinces = [];
      this.resetNgSelect("province");
    }
    if (action == "updateCountryDistrict") {
      this.DistrictToUpdate.countryId = null;
      this.DistrictToUpdate.provinceId = null;
      this.ngSelectDataProvinces = [];
      this.currentActiveProvince =[] ;     
      this.resetNgSelect("province");
    }

    if (action == "updateProvinceDistrict") {
      this.DistrictToUpdate.provinceId = null;
    }

    if (action == "addProvinceDistrict") {
      this.DistrictToAdd.provinceId = null;
    }

    if (action == "addCountryWard") {
      this.WardToAdd.countryId = null;
      this.WardToAdd.districtId = null
      this.WardToAdd.provinceId = null;
      this.ngSelectDataProvinces = [];
      this.ngSelectDataDistricts = [];
      this.currentActiveProvince = [];
      this.currentActiveDistrict = [];
      this.resetNgSelect("all");

    }

    if (action == "addProvinceWard") {
      this.WardToAdd.provinceId = null;
      this.WardToAdd.districtId = null;
      this.resetNgSelect("district");
    }

    if (action == "updateCountryWard") {
      this.WardToUpdate.countryId = null;
      this.WardToUpdate.provinceId = null;
      this.WardToUpdate.districtId = null;
      this.currentActiveCountry = [];
      this.currentActiveDistrict = [];
      this.currentActiveProvince = [];
      this.ngSelectDataProvinces = [];
      this.ngSelectDataDistricts = []
      this.resetNgSelect("province");
      this.resetNgSelect("district");
    }

    if (action == "updateProvinceWard") {
      this.WardToUpdate.provinceId = null;
      this.WardToUpdate.districtId = null;
      this.ngSelectDataDistricts = [];
      this.currentActiveDistrict =[];
      this.resetNgSelect("district");
    }

    if (action == "updateDistrictWard") {
      this.WardToUpdate.districtId = null;
    }
    console.log('Removed value is: ', value);
  }

  public typed(value: any): void {

  }

  public refreshValue(value: any): void {
    this.value = value;
  }

  isDesc = true;
  sortKey: string = "code";
  sort(property){
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    if(this.activeTab==="country"){
      this.ListCountries = this.sortService.sort(this.ListCountries, property, this.isDesc);
    }
    if(this.activeTab==="province"){
      this.ListProvinceCities = this.sortService.sort(this.ListProvinceCities, property, this.isDesc);
    }
    if(this.activeTab==="district"){
      this.ListDistricts = this.sortService.sort(this.ListDistricts, property, this.isDesc);
    }
    if(this.activeTab==="ward"){
      this.ListWards = this.sortService.sort(this.ListWards, property, this.isDesc);
    }
  }



   async import(){
   
  }

  async export(){
    if(this.activeTab==='country'){
      await this.exportCountry();
    }
    if(this.activeTab==='province'){
      await this.exportProvince();
    }
    if(this.activeTab==='district'){
      await this.exportDistrict();
    }
    if(this.activeTab==='ward'){
      await this.exportTownWard()
    }
  }

  async exportCountry(){
    var countries = await this.baseServices.postAsync(this.api_menu.Catalogue.Country.query,this.searchObject);
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.ENGLISH_API){
        countries = lodash.map(countries,function(country,index){
          return [
            index+1,
            country['code'],
            country['nameEn'],
            country['nameVn'],
            (country['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
          ]
        });
    }

    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.VIETNAM_API){
      countries = lodash.map(countries,function(country,index){
        return [
          index+1,
          country['code'],
          country['nameEn'],
          country['nameVn'],
          (country['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }

    /**Set up stylesheet */
    var exportModel:ExportExcel = new ExportExcel();
    exportModel.fileName = "Countries List";    
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.title = "Countries List";
    exportModel.author = currrently_user;
    exportModel.sheetName = "Sheet 1";
    exportModel.header = [
      {name:"No.",width:10},
      {name:"Country Code",width:10},
      {name:"English Name",width:20},
      {name:"Local Name",width:20},
      {name:"Inactive",width:20}
    ]
    exportModel.data = countries;
    this.excelService.generateExcel(exportModel);
  }


  async exportProvince(){
    this.searchObject.placeType = PlaceTypeEnum.Province;
    var provinces = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query,this.searchObject);
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.ENGLISH_API){
      provinces = lodash.map(provinces,function(province,index){
          return [
            index+1,
            province['code'],
            province['name_EN'],
            province['name_VN'],
            province['countryNameEN'],
            (province['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
          ]
        });
    }

    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.VIETNAM_API){
      provinces = lodash.map(provinces,function(province,index){
        return [
          index+1,
          province['code'],
          province['name_EN'],
          province['name_VN'],
          province['countryNameVN'],
          (province['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }

    /**Set up stylesheet */
    var exportModel:ExportExcel = new ExportExcel();
    exportModel.fileName = "Provinces List";    
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.title = "Provinces List";
    exportModel.author = currrently_user;
    exportModel.sheetName = "Sheet 1";
    exportModel.header = [
      {name:"No.",width:10},
      {name:"Province Code",width:20},
      {name:"English Name",width:20},
      {name:"Local Name",width:20},
      {name:"Country",width:20},
      {name:"Inactive",width:20}
    ]
    exportModel.data = provinces;
    this.excelService.generateExcel(exportModel);

  }

  async exportDistrict(){
    this.searchObject.placeType = PlaceTypeEnum.District;
    var districts = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query,this.searchObject);
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.ENGLISH_API){
      districts = lodash.map(districts,function(dist,index){
          return [
            index+1,
            dist['code'],
            dist['name_EN'],
            dist['name_VN'],
            dist['provinceNameEN'],
            dist['countryNameEN'],
            (dist['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
          ]
        });
    }

    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.VIETNAM_API){
      districts = lodash.map(districts,function(dist,index){
        return [
          index+1,
          dist['code'],
          dist['name_EN'],
          dist['name_VN'],
          dist['provinceNameVN'],
          dist['countryNameVN'],
          (dist['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }

    /**Set up stylesheet */
    var exportModel:ExportExcel = new ExportExcel();
    exportModel.fileName = "Districts List";    
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.title = "Districts List";
    exportModel.author = currrently_user;
    exportModel.sheetName = "Sheet 1";
    exportModel.header = [
      {name:"No.",width:10},
      {name:"District Code",width:20},
      {name:"English Name",width:20},
      {name:"Local Name",width:20},
      {name:"Province",width:20},
      {name:"Country",width:20},
      {name:"Inactive",width:20}
    ]
    exportModel.data = districts;
    this.excelService.generateExcel(exportModel);

  }


  async exportTownWard(){
    this.searchObject.placeType = PlaceTypeEnum.Ward;
    var wards = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query,this.searchObject);
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.ENGLISH_API){
      wards = lodash.map(wards,function(ward,index){
          return [
            index+1,
            ward['code'],
            ward['name_EN'],
            ward['name_VN'],
            ward['districtNameEN'],
            ward['provinceNameEN'],
            ward['countryNameEN'],
            (ward['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
          ]
        });
    }

    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.VIETNAM_API){
      wards = lodash.map(wards,function(ward,index){
        return [
          index+1,
          ward['code'],
          ward['name_EN'],
          ward['name_VN'],
          ward['districtNameVN'],
          ward['provinceNameVN'],
          ward['countryNameVN'],
          (ward['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }

    /**Set up stylesheet */
    var exportModel:ExportExcel = new ExportExcel();
    exportModel.fileName = "Town-Ward List";    
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.title = "Town-Ward List";
    exportModel.author = currrently_user;
    exportModel.sheetName = "Sheet 1";
    exportModel.header = [
      {name:"No.",width:10},
      {name:"Town-Ward Code",width:20},
      {name:"English Name",width:20},
      {name:"Local Name",width:20},
      {name:"District",width:20},
      {name:"Province",width:20},
      {name:"Country",width:20},
      {name:"Inactive",width:20}
    ]
    exportModel.data = wards;
    this.excelService.generateExcel(exportModel);

  }

}
