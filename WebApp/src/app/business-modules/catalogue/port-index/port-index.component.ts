import { Component, OnInit, ViewChild } from '@angular/core';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PORTINDEXCOLUMNSETTING } from './port-index.columns';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { NgForm } from '@angular/forms';
import { SelectComponent } from 'ng2-select';
import { SortService } from 'src/app/shared/services/sort.service';
import { SystemConstants } from 'src/constants/system.const';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import * as lodash from 'lodash';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { ExcelService } from 'src/app/shared/services/excel.service';
import {PlaceTypeEnum} from 'src/app/shared/enums/placeType-enum';
declare var $: any;
import * as dataHelper from 'src/helper/data.helper';

@Component({
  selector: 'app-port-index',
  templateUrl: './port-index.component.html',
  styleUrls: ['./port-index.component.sass']
})
export class PortIndexComponent implements OnInit {

  portIndexSettings: ColumnSetting[] = PORTINDEXCOLUMNSETTING;
  portIndexs: Array<PortIndex>;
  portIndex: PortIndex = new PortIndex();
  pager: PagerSetting = PAGINGSETTING;
  criteria: any = { placeType: PlaceTypeEnum.Port };
  //nameModal = "edit-port-index-modal";
  addButtonSetting: ButtonModalSetting = {
    dataTarget: 'edit-port-index-modal',
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

  cancelButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.cancel
  };
  configSearch: any = {
    settingFields: this.portIndexSettings.filter(x => x.allowSearch == true).map(x=>({"fieldName": x.primaryKey,"displayName": x.header})),
    typeSearch: TypeSearch.outtab
  };
  @ViewChild('formAddEdit',{static:false}) form: NgForm;
  @ViewChild('chooseCountry',{static:false}) public ngSelectCountry: SelectComponent;
  @ViewChild('chooseArea',{static:false}) public ngSelectArea: SelectComponent;
  @ViewChild('chooseMode',{static:false}) public ngSelectMode: SelectComponent;
  @ViewChild(PaginationComponent,{static:false}) child;
  countries: any[];
  areas: any[];
  modes: any[];
  countryActive: any[] = [];
  areaActive: any[] = [];
  isDesc: boolean = false;
  modeActive: any[] = [];

  constructor(private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService,
    private excelService: ExcelService,) {
        this.baseService.get(this.api_menu.System.User_Management.getAll).subscribe(data=>{
          this.baseService.changeData("data",data);
        });
     }

  ngOnInit() {
    this.initPager();
    this.getPortIndexs(this.pager);
    this.getDataCombobox();
  }
  setPage(pager: PagerSetting): any {
    this.pager.currentPage = pager.currentPage;
    this.pager.totalPages = pager.totalPages;
    this.pager.pageSize = pager.pageSize;
    this.getPortIndexs(pager);
  }
  initPager() {
    this.pager.currentPage = 1;
    this.pager.totalItems = 0;
  }
  async getPortIndexs(pager: PagerSetting) {
    let responses = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, true, true);
    if(responses != null){
      this.portIndexs = responses.data;
      this.pager.totalItems = responses.totalItems;
    }
    else{
      this.portIndexs = [];
      this.pager.totalItems = 0;
    }
  }
  onSearch(event) {
    this.criteria = {
      placeType: PlaceTypeEnum.Port
    };
    if (event.field == "All") {
      this.criteria.all = event.searchString;
    }
    else {
      let language = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);
      if (language == SystemConstants.LANGUAGES.ENGLISH) {
        if (event.field == "countryName") {
          this.criteria.countryNameEN = event.searchString;
        }
        if (event.field == "areaName") {
          this.criteria.areaNameEN = event.searchString;
        }
      }
      else {
        if (event.field == "countryName") {
          this.criteria.countryNameVN = event.searchString;
        }
        if (event.field == "areaName") {
          this.criteria.areaNameVN = event.searchString;
        }
      }
      if (language == SystemConstants.LANGUAGES.VIETNAM) {
      }
      if (event.field == "code") {
        this.criteria.code = event.searchString;
      }
      if (event.field == "nameEn") {
        this.criteria.nameEN = event.searchString;
      }
      if (event.field == "nameVn") {
        this.criteria.nameVN = event.searchString;
      }
      if (event.field == "modeOfTransport") {
        this.criteria.modeOfTransport = event.searchString;
      }
    }
    this.initPager();
    this.setPage(this.pager);
  }
  resetSearch(event) {
    this.criteria = {
      placeType: PlaceTypeEnum.Port
    };
    this.onSearch(event);
  }
  showAdd() {
    this.initPortIndex();
  }
  initPortIndex() {
    this.form.onReset();
    this.portIndex = new PortIndex();
    this.portIndex.placeType = PlaceTypeEnum.Port;
    this.modeActive = [];
    this.countryActive = [];
    this.areaActive = [];
    this.portIndex.countryID = null;
    this.portIndex.areaID = null;
    this.portIndex.modeOfTransport = null;
  }
  onSubmit() {
    if (this.form.valid && this.portIndex.countryID != null && this.portIndex.modeOfTransport != null) {
      if (this.portIndex.id == null) {
        this.addNew();
      }
      else {
        this.update();
      }
    }
  }

  async update() {
    var response = await this.baseService.putAsync(this.api_menu.Catalogue.CatPlace.update + this.portIndex.id, this.portIndex, true, true);
    if(response != null){
      if(response.status){
        this.initPortIndex();
        $('#edit-port-index-modal').modal('hide');
        this.getPortIndexs(this.pager);
      }
    }
  }

  async addNew() {
    let response = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.add, this.portIndex, true, true);
    if(response != null){
      if(response.status){
        this.initPager();
        this.getPortIndexs(this.pager);
        $('#edit-port-index-modal').modal('hide');
      }
    }
  }

  onCancel() {
    this.initPortIndex();
    this.getPortIndexs(this.pager);
  }

  getDataCombobox() {
    this.getCountries();
    this.getAreas();
    this.getModeOfTransport();
  }
  async getModeOfTransport() {
    let responses = await this.baseService.getAsync(this.api_menu.Catalogue.CatPlace.getModeOfTransport, false, false);
    if(responses != null){
      this.modes = dataHelper.prepareNg2SelectData(responses, 'id', 'name');
    }
    else{
      this.modes = [];
    }
  }
  async getAreas() {
    let responses = await this.baseService.getAsync(this.api_menu.Catalogue.Area.getAllByLanguage, false, false);
    if(responses != null){
      this.areas = dataHelper.prepareNg2SelectData(responses, 'id', 'name');
    }
    else{
      this.areas = [];
    }
  }
  async getCountries() {
    let responses = await this.baseService.getAsync(this.api_menu.Catalogue.Country.getAllByLanguage, false, true);
    if(responses != null){
      this.countries = dataHelper.prepareNg2SelectData(responses, 'id', 'name');
    }
    else{
      this.countries = [];
    }
  }
  value: any = {};
  refreshValue(value: any): void {
    this.value = value;
  }
  public removed(value: any): void {
    console.log('Removed value is: ', value);
  }
  public typed(value: any): void {
    console.log('New search input: ', value);
  }
  showConfirmDelete(item) {
    this.portIndex = item;
  }
  showDetail(item: PortIndex) {
    this.portIndex = item;
    this.countryActive = this.getCountryActive(this.portIndex.countryID);
    this.areaActive = this.getDistrictAactive(this.portIndex.areaID);
    this.modeActive = this.getModeActive(this.portIndex.modeOfTransport);
  }
  getModeActive(modeOfTransport: string){
    let index = this.modes.findIndex(x => x.id == modeOfTransport);
    if(index > -1){
      return [this.modes[index]];
    }
    else{
      return [];
    }
  }
  getDistrictAactive(areaID: string) {
    let index = this.areas.findIndex(x => x.id == areaID);
    if(index > -1){
      return [this.areas[index]];
    }
    else{
      return [];
    }
  }
  getCountryActive(countryID: number) {
    let index = this.countries.findIndex(x => x.id == countryID);
    if(index > -1)
    {
      return [this.countries[index]];
    }
    else{
      return [];
    }
  }


  async onDelete(event) {
    console.log(event);
    if (event) {
      this.baseService.spinnerShow();
      this.baseService.delete(this.api_menu.Catalogue.CatPlace.delete + this.portIndex.id).subscribe((response: any) => {
        this.baseService.spinnerHide();
        this.baseService.successToast(response.message);
        this.setPageAfterDelete();
      }, err => {
        this.baseService.spinnerHide();
        this.baseService.handleError(err);        
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
  onSortChange(column) {
    if (column.dataType != 'boolean') {
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.portIndexs = this.sortService.sort(this.portIndexs, property, this.isDesc);
    }
  }
  async export(){
    var portIndexes = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.query,this.criteria);
    console.log(portIndexes);
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.ENGLISH_API){
      portIndexes = lodash.map(portIndexes,function(pi,index){
        return [
          index+1,
          pi['code'],
          pi['name_EN'],
          pi['name_VN'],
          pi['countryNameEN'],
          pi['areaNameEN'],
          pi['modeOfTransport'],
          (pi['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      }); 
    }

    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.VIETNAM_API){
      portIndexes = lodash.map(portIndexes,function(pi,index){
        return [
          index+1,
          pi['code'],
          pi['name_EN'],
          pi['name_VN'],
          pi['countryNameVN'],
          pi['areaNameVN'],
          pi['modeOfTransport'],
          (pi['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }

    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "PortIndex List";
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.author = currrently_user;
    exportModel.header = [
      {name:"No.",width:10},
      {name:"Code",width:20},
      {name:"Name EN",width:20},
      {name:"Name VN",width:20},
      {name:"Country",width:20},
      {name:"Zone",width:20},
      {name:"Mode",width:20},
      {name:"Inactive",width:20}
    ]
    exportModel.data = portIndexes;
    exportModel.fileName = "PortIndex";
    
    this.excelService.generateExcel(exportModel);
   
    
  }

}
