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
  ObjectToDelete:String = "";
  deleteWhat:any = null;
  idCountryToDelete:any = null;
  pager: PagerSetting = {
    currentPage: 1,
    pageSize: 15,
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

  async ngOnInit() {
    await this.getCountries();
  }

  async setPage(pager) {
    this.pager.currentPage = pager.currentPage;
    this.pager.totalPages = pager.totalPages;
    this.ListCountries = await this.getCountries();
  }

  async getCountries() {
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.Location.getAll + "/" + this.pager.currentPage + "/" + this.pager.pageSize, this.countrySearchObject, false, true);
    this.ListCountries = response.data;
    return response.data;
  }



  /**
   * BEGIN COUNTRY METHODS
   */
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

  async showUpdateCountry(id){
    this.CountryToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.Location.getById+id,true,true);
  }

  async updateCountry(form:NgForm,action){
    if(action=="yes"){
      if(form.form.status != "INVALID"){
        await this.baseServices.putAsync(this.api_menu.Catalogue.Location.update,this.CountryToUpdate);
        await this.getCountries();
        form.onReset();
        $('#update-country-modal').modal('hide');
      }
    }else{
      form.onReset();
        $('#update-country-modal').modal('hide');
    }
  }


  prepareDelete(id,deleteWhat){
    this.idCountryToDelete = id;
    this.deleteWhat = deleteWhat;
  }

 async delete(action){
    if(action=="yes"){
      if(this.deleteWhat=="country"){
        console.log(this.idCountryToDelete);
        await this.baseServices.deleteAsync(this.api_menu.Catalogue.Location.delete+this.idCountryToDelete);
        await this.getCountries();
      }
    } 
  }


  /**
   * END COUNTRY METHODS 
   */

}
