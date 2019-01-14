import { Component, OnInit, ViewChild } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { CatUnitModel } from 'src/app/shared/models/catalogue/catUnit.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import * as lodash from 'lodash';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
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
  pager: PagerSetting = PAGINGSETTING;
  searchKey: string = "";
  ListUnits: any = [];
  UnitToAdd: CatUnitModel = new CatUnitModel();
  UnitToUpdate: CatUnitModel = new CatUnitModel();
  idUnitToUpdate: any = "";
  idUnitToDelete: any = "";
  searchObject: any = {};
  unitTypes: any[];
  currentUnitType: any = [];

  @ViewChild(PaginationComponent) child;

  constructor(
    private excelService: ExcelService,
    private baseServices: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  async ngOnInit() {
    this.pager.totalItems = 0;
    this.getUnitTypes();
    await this.getUnits();
  }

  async searchUnit() {

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

  async resetSearch() {
    this.searchKey = "";
    this.searchObject = {};
    this.selectedUnitFilter = this.listUnitFilter[0].filter;
    await this.getUnits();
  }

  async setPage(pager: PagerSetting) {
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

  async getUnitTypes(){
    const response = await this.baseServices.getAsync(this.api_menu.Catalogue.Unit.getUnitTypes, false, false);
    if(response){
      this.unitTypes = response.map(x=>({"text":x.displayName,"id":x.value}));
    }
    else{
      this.unitTypes = [];
    }
  }
  async getUnits() {
    const response = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.searchObject, false, true);
    this.ListUnits = response.data;
    this.pager.totalItems = response.totalItems;
  }

  async showUpdateUnit(id) {
    this.currentUnitType = [];
    this.UnitToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.Unit.getById + id, false, true);
    if(this.UnitToUpdate != null){
      const index = this.unitTypes.findIndex(x => x.id == this.UnitToUpdate.unitType);
      if(index > -1){
        this.currentUnitType = [this.unitTypes[index]];
      }
    }
  }

  async updateUnit(form: NgForm, action) {
    if (action == "yes") {
      if (form.form.status != "INVALID") {
        const res = await this.baseServices.putAsync(this.api_menu.Catalogue.Unit.update, this.UnitToUpdate);
        if(res){
          await this.getUnits();
          form.onReset();
          $('#update-unit-modal').modal('hide');
        }
      }
    } else {
      form.onReset();
      $('#update-unit-modal').modal('hide');
    }
  }


  async addNewUnit(form: NgForm, action) {
    console.log(this.UnitToAdd);
    if (action == "yes") {
      delete this.UnitToAdd.id;
      if (form.form.status != "INVALID" && this.UnitToAdd.unitType != null) {
        const respone = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.addNew, this.UnitToAdd, true, true);
        await this.getUnits();
        if (respone) {
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

  prepareDeleteUnit(id) {
    this.idUnitToDelete = id;
  }

  async delete() {
    await this.baseServices.deleteAsync(this.api_menu.Catalogue.Unit.delete + this.idUnitToDelete, true, true);
    await this.getUnits();
    this.setPageAfterDelete();

  }
  isDesc = true;
  sortKey: string = "code";
  sort(property) {
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.ListUnits = this.sortService.sort(this.ListUnits, property, this.isDesc);
  }


  async export() {
    /**Prepare data */
    var units = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, this.searchObject);
    
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.ENGLISH_API){

      units = lodash.map(units, function (unit,index) {
        return [
          index+1,
          unit.code,
          unit.unitNameVn,
          unit.unitNameEn,
          unit.descriptionEn,
          unit.descriptionVn,
          (unit.inactive===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.VIETNAM_API){
      units = lodash.map(units, function (unit,index) {
        return [
          index+1,
          unit.code,
          unit.unitNameVn,
          unit.unitNameEn,
          unit.descriptionEn,
          unit.descriptionVn,
          (unit.inactive===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }


    /**Set up stylesheet */
    var exportModel: ExportExcel = new ExportExcel();
    exportModel.fileName = "Unit Report";
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.title = "Unit Report ";
    exportModel.author = currrently_user;
    exportModel.header = [
      { name: "No.", width: 10 },
      { name: "Code", width: 10 },
      { name: "Name Vn", width: 25 },
      { name: "Name En", width: 25 },
      { name: "Description En", width: 25 },
      { name: "Description Vn", width: 25 },
      { name: "Inactive", width: 25 }];   

    exportModel.data = units;
    this.excelService.generateExcel(exportModel);
  }

  import() {

  }

  /*ng-select 2
  */
 value: any;
 selected(value: any): void{
  this.UnitToAdd.unitType = value.id;
  this.UnitToUpdate.unitType = value.id;
 }
 refreshValue(value: any): void {
    this.value = value;
  }
  public removed(value: any): void {
    console.log('Removed value is: ', value);
  }
  public typed(value: any): void {
    console.log('New search input: ', value);
  }
}
