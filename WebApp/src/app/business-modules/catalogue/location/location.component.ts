import { Component, OnInit, Output, ViewChild, AfterViewChecked, AfterContentInit, EventEmitter } from '@angular/core';
import * as lodash from 'lodash';
import * as moment from 'moment';
import { ActivatedRoute, Router } from '@angular/router';
import * as SearchHelper from 'src/helper/SearchHelper';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { SystemConstants } from '../../../../constants/system.const';
import { StageModel } from 'src/app/shared/models/catalogue/stage.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
declare var jquery: any;
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

  ListProvinceCities:any = [];
  ConstListProvinceCities:any = [];
  ProvinceCityToAdd = new CatPlaceModel();
  ProvinceCityToUpdate = new CatPlaceModel();


  pager: PagerSetting = {
    currentPage: 1,
    pageSize: 30,
    numberToShow: [3, 5, 10, 15, 30, 50],
    totalPageBtn: 7
  }
  countrySearchObject = {
    code: "",
    nameVn: "",
    nameEn: "",
    condition: "OR",
  }
  @ViewChild(PaginationComponent) child;

  /**
   * END OF VARIABLES DEFINITIONS
   */

  constructor(private baseServices: BaseService, private toastr: ToastrService, private spinnerService: Ng4LoadingSpinnerService, private api_menu: API_MENU) { }

  resetNg2Select = true;
  resetNgSelect() {
    this.resetNg2Select = false;
    setTimeout(() => {
      this.resetNg2Select = true;
    }, 200);
  }

  async ngOnInit() {
    await this.getCountries();
    await this.getProvinceCities();
  }

  async setPage(pager) {
    this.pager.currentPage = pager.currentPage;
    this.pager.totalPages = pager.totalPages;
    this.ListCountries = await this.getCountries();
  }





  /**
   * BEGIN COUNTRY METHODS
   */

  async getCountries() {
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.Location.getAll + "/" + this.pager.currentPage + "/" + this.pager.pageSize, this.countrySearchObject, false, true);
    this.ListCountries = response.data;
    this.ConstListCountries = response.data;
    this.prepareNgSelectData("country");
    return response.data;
  }

  async addCountry(form: NgForm, action) {
    if (action == "yes") {
      delete this.CountryToAdd.id;
      if (form.form.status != "INVALID") {
        await this.baseServices.postAsync(this.api_menu.Catalogue.Location.addNew, this.CountryToAdd, true, true);
        await this.getCountries();
        form.onReset();
        $('#add-country-modal').modal('hide');
      }
    } else {
      form.onReset();
      $('#add-country-modal').modal('hide');
    }
  }

  async showUpdateCountry(id) {
    this.CountryToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.Location.getById + id, true, true);
  }

  async updateCountry(form: NgForm, action) {
    if (action == "yes") {
      if (form.form.status != "INVALID") {
        await this.baseServices.putAsync(this.api_menu.Catalogue.Location.update, this.CountryToUpdate);
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
      if(this.ObjectToDelete=="country"){
        await this.baseServices.deleteAsync(this.api_menu.Catalogue.Location.delete + this.idCountryToDelete);
        await this.getCountries();
      }

      if(this.ObjectToDelete == "province-city"){
        await this.baseServices.deleteAsync(this.api_menu.Catalogue.CatPlace.delete+this.idProvinceCityToDelete);
        await this.getProvinceCities();
      }
    
    }
  }


  /**
   * END COUNTRY METHODS 
   */


  /**
   * START PROVINCE-CITY METHODS
   */
  async getProvinceCities(){
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging+ "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize,{"placeType": 9,});
    this.ListProvinceCities = response.data;
    this.ConstListProvinceCities = response.data;
    console.log(this.ConstListProvinceCities);
    return response.data;
  }

  async addProvinceCity(form:NgForm,action){
    if(action=="yes"){
      if(form.form.status!="INVALID"){
        this.ProvinceCityToAdd.placeType = 9;        
        await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.add,this.ProvinceCityToAdd);
        await this.getProvinceCities();
        form.onReset();
        this.resetNgSelect();
        $('#add-city-province-modal').modal('hide');
      }
    }else{
      form.onReset();
      this.resetNgSelect();
      $('#add-city-province-modal').modal('hide');
    }
    
  }

  indexCurrentCountry:number = -1;
  idProvinceCityToUpdate:string="";
  async showUpdateProvince(id){
    this.idProvinceCityToUpdate = id;
    this.ProvinceCityToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.CatPlace.getById +id);
    var countryId = this.ProvinceCityToUpdate.countryId;

    console.log(this.ngSelectDataCountries);
    var indexCurrentCountry = lodash.findIndex(this.ngSelectDataCountries,function(o){
      return o.id == countryId
    });
    console.log(indexCurrentCountry);
    this.indexCurrentCountry = indexCurrentCountry;

  }

  async updateProvinceCity(form:NgForm,action){
    if (action == "yes") {
      if (form.form.status != "INVALID") {
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

  idProvinceCityToDelete :string="";
  prepareDeleteProvince(id){
    this.idProvinceCityToDelete = id;
    this.ObjectToDelete = "province-city";
  }




  /**
   * END PROVINCE-CITY METHODS
   */














  ngSelectDataCountries:any=[];

   prepareNgSelectData(type_location) { 
    if (type_location == "country") {
      this.ngSelectDataCountries = this.ConstListCountries.map(x => ({ "text": x.code.toUpperCase() + " - " + x.nameEn, "id": x.id }));
      console.log(this.ngSelectDataCountries)
    }
  }


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

  public selectedCountry(value: any, action,type_location): void {

    if(action=="add"){
      if(type_location=="province-city"){
        this.ProvinceCityToAdd.countryId = value.id;
      }
    }else{
      if(type_location=="province-city"){
        this.ProvinceCityToUpdate.countryId = value.id;
      }
    }
    

  }

  public removed(value: any, action): void {

    console.log('Removed value is: ', value);
  }

  public typed(value: any): void {
    console.log(this.ListCountries)
    console.log('New search input: ', value);
  }

  public refreshValue(value: any): void {
    this.value = value;
  }

}
