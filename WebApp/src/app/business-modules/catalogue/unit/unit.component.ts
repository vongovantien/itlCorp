import { Component, OnInit, ViewChild,ElementRef } from '@angular/core';
import * as lodash from 'lodash';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import * as dataHelper from 'src/helper/data.helper';
import { from } from 'rxjs';
import { SystemConstants } from 'src/constants/system.const';
import { CatUnitModel } from 'src/app/shared/models/catalogue/catUnit.model';
import { reserveSlots } from '@angular/core/src/render3/instructions';
import { SortService } from 'src/app/shared/services/sort.service';
// import {DataHelper} from 'src/helper/data.helper';
declare var $: any;

@Component({
  selector: 'app-unit',
  templateUrl: './unit.component.html',
  styleUrls: ['./unit.component.sass']
})
export class UnitComponent implements OnInit {
  listUnitFilter = [
    { filter: "All", field: "all" }, { filter: "Code", field: "code" },
    { filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" }];
  selectedUnitFilter = this.listUnitFilter[0].filter;
  pager: PagerSetting = {
    currentPage: 1,
    pageSize: 30,
    numberToShow: [3, 5, 10, 15, 30, 50],
    totalPageBtn: 7
  }
  searchKey:string = "";
  ListUnits:any=[];
  UnitToAdd:CatUnitModel = new CatUnitModel();
  UnitToUpdate:CatUnitModel = new CatUnitModel();
  idUnitToUpdate:any= "";
  idUnitToDelete:any= "";
  searchObject:any ={};

  @ViewChild(PaginationComponent) child;

  constructor(
    private baseServices: BaseService,
    private toastr: ToastrService,
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private el:ElementRef,
    private sortService: SortService) { }

  async ngOnInit() {
    await this.getUnits();
  }

  async searchUnit(){
    
    this.searchObject = {};
    if (this.selectedUnitFilter == "All") {
      this.searchObject.All = this.searchKey;
    } else {
      this.searchObject = {};
      for (var i = 1; i < this.listUnitFilter.length; i++) {
        console.log(this.listUnitFilter[i].field);
        if (this.selectedUnitFilter == this.listUnitFilter[i].filter) {
          eval("this.searchObject[this.listUnitFilter[i].field]=this.searchKey");
        }

      }
    }
    console.log(JSON.stringify(this.searchObject));
    await this.getUnits();
  }

  async resetSearch(){
    this.searchKey = "";
    this.searchObject = {};
    this.selectedUnitFilter = this.listUnitFilter[0].filter;
    await this.getUnits();
  }

  async setPage(pager:PagerSetting){
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    await this.getUnits();
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

  async getUnits(){
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.searchObject, false, true);
    this.ListUnits = response.data;
    this.pager.totalItems = response.totalItems;
  }

  async showUpdateUnit(id){
    this.UnitToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.Unit.getById + id, true, true);
  }

  async updateUnit(form:NgForm,action){
    if (action == "yes") {
      if (form.form.status != "INVALID") {
        await this.baseServices.putAsync(this.api_menu.Catalogue.Unit.update, this.UnitToUpdate);
        await this.getUnits();
        form.onReset();        
        $('#update-unit-modal').modal('hide');
      }
    } else {
      form.onReset();
      $('#update-unit-modal').modal('hide');
    }
  }


  async addNewUnit(form:NgForm,action){
    if (action == "yes") {
      delete this.UnitToAdd.id;
      if (form.form.status != "INVALID") {
        var respone = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.addNew, this.UnitToAdd, true, true);  
        await this.getUnits();     
        if(respone!=undefined && respone.status){
          this.setPageAfterAdd();
          form.onReset();
          $('#add-unit-modal').modal('hide');
         
        }
      }
    } else {
      this.UnitToAdd = new CatUnitModel();
      form.onReset();
      $('#add-unit-modal').modal('hide');
    }
  }

  prepareDeleteUnit(id){
    this.idUnitToDelete = id;
  }

  async delete(){
    await this.baseServices.deleteAsync(this.api_menu.Catalogue.Unit.delete+this.idUnitToDelete,true,true);    
    await this.getUnits();
    this.setPageAfterDelete();
    
  }
  isDesc = true;
  sortKey: string = "code";
  sort(property){
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.ListUnits = this.sortService.sort(this.ListUnits, property, this.isDesc);
  }


}
