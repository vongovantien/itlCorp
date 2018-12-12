import { Component, OnInit, ViewChild } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { CatUnitModel } from 'src/app/shared/models/catalogue/catUnit.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import * as DataHelper from 'src/helper/xlsx.helper';
import * as lodash from 'lodash';
import { ExcelService } from 'src/app/shared/services/excel.service';
import {ExportExcel} from 'src/app/shared/models/layout/exportExcel.models';
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
  searchKey:string = "";
  ListUnits:any=[]; 
  UnitToAdd:CatUnitModel = new CatUnitModel();
  UnitToUpdate:CatUnitModel = new CatUnitModel();
  idUnitToUpdate:any= "";
  idUnitToDelete:any= "";
  searchObject:any ={};

  @ViewChild(PaginationComponent) child;

  constructor(
    private excelService: ExcelService,
    private baseServices: BaseService,
    private api_menu: API_MENU,
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

  
  async export(){
    // var units = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, this.searchObject);
    // console.log(units);
    // units = lodash.map(units, function (unit) {
    //   return {
    //     "Code": unit.code,
    //     "Name_Vn": unit.unitNameVn,
    //     "Name_En": unit.unitNameEn,
    //     "Description_En": unit.descriptionEn,
    //     "Description_Vn": unit.descriptionVn,
    //     "Inactive": unit.inactive
    //   }
    // });


    /**Prepare data */
    var units = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, this.searchObject);
    console.log(units);
    units = lodash.map(units, function (unit) {
      return [
        unit.code,
        unit.unitNameVn,
        unit.unitNameEn,
        unit.descriptionEn,
        unit.descriptionVn,
        unit.inactive
      ]
    });

    /**Set up stylesheet */
    var exportModel:ExportExcel = new ExportExcel();
    exportModel.fileName = "Unit Report";    
    const currrently_user = sessionStorage.getItem('currently_userName');
    exportModel.title = "Unit Report ";
    exportModel.author = currrently_user;
    exportModel.header = ["Code","Name_Vn","Name_En","Description_En","Description_Vn","Inactive"];
    exportModel.data = units;

    exportModel.titleStyle.fontFamily = 'Century Gothic';
    exportModel.titleStyle.isBold = true;
    exportModel.titleStyle.fontSize = 20;

    exportModel.cellStyle.fontFamily = 'Kodchasan SemiBold';
    exportModel.cellStyle.fontSize = 11;
    exportModel.cellStyle.isBold = false;
 
    this.excelService.generateExcel(exportModel);
  }


}
