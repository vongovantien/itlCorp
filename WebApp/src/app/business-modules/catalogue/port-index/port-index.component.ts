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
  keySortDefault: string = "code";
  nameModal = "edit-port-index-modal";
  titleAddModal = "Add Port Index";
  titleEditModal = "Edit Port Index";
  addButtonSetting: ButtonModalSetting = {
    dataTarget: this.nameModal,
    typeButton: ButtonType.add
  };
  selectedFilter = "All";
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
    selectedFilter: this.selectedFilter,
    settingFields: this.portIndexSettings,
    typeSearch: TypeSearch.outtab
  };
  @ViewChild('formAddEdit') form: NgForm;
  @ViewChild('chooseCountry') public ngSelectCountry: SelectComponent;
  @ViewChild('chooseArea') public ngSelectArea: SelectComponent;
  @ViewChild('chooseMode') public ngSelectMode: SelectComponent;
  @ViewChild(PaginationComponent) child;
  countries: any[];
  areas: any[];
  modes: any[];
  countryActive: any;
  areaActive: any;
  isDesc: boolean = false;
  titleConfirmDelete = "You want to delete this port index";
  modeActive: any;

  constructor(private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService,
    private excelService: ExcelService,) { }

  ngOnInit() {
    this.pager.totalItems = 0;
    this.getPortIndexs(this.pager);
    this.getDataCombobox();
  }
  setPage(pager: PagerSetting): any {
    this.pager.currentPage = pager.currentPage;
    this.pager.totalPages = pager.totalPages;
    this.pager.pageSize = pager.pageSize;
    this.getPortIndexs(pager);
  }
  getPortIndexs(pager: PagerSetting): any {
    this.baseService.spinnerShow();
    this.baseService.post(this.api_menu.Catalogue.CatPlace.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.baseService.spinnerHide();
      this.portIndexs = response.data.map(x => Object.assign({}, x));
      this.pager.totalItems = response.totalItems;
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  onSearch(event) {
    if (event.field == "All") {
      this.criteria.all = event.searchString;
    }
    else {
      this.criteria.all = null;
      let language = localStorage.getItem(SystemConstants.CURRENT_LANGUAGE);
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
      if (event.field == "nameEN") {
        this.criteria.nameEN = event.searchString;
      }
      if (event.field == "nameVN") {
        this.criteria.nameVN = event.searchString;
      }
      if (event.field == "modeOfTransport") {
        this.criteria.modeOfTransport = event.searchString;
      }
    }
    this.pager.currentPage = 1;
    this.setPage(this.pager);
  }
  resetSearch(event) {
    this.criteria = {
      placeType: 8
    };
    this.onSearch(event);
  }
  showAdd() {
    this.initPortIndex();
    this.ngSelectCountry.active = [];
    this.ngSelectArea.active = [];
    this.ngSelectMode.active = [];
  }
  initPortIndex() {
    this.portIndex = new PortIndex();
    this.portIndex.placeType = 8;
    this.modeActive = null;
  }
  onSubmit() {
    if (this.form.valid) {
      if (this.portIndex.id == null) {
        this.addNew();
      }
      else {
        this.update();
      }
    }
  }

  update(): any {
    this.baseService.spinnerShow();
    this.baseService.put(this.api_menu.Catalogue.CatPlace.update + this.portIndex.id, this.portIndex).subscribe((response: any) => {
      
      this.form.onReset();
      this.initPortIndex();
      $('#' + this.nameModal).modal('hide');
      this.baseService.successToast(response.message);
      this.baseService.spinnerHide();
      this.setPage(this.pager);
    }, err => {
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }

  addNew(): any {
    this.baseService.spinnerShow();
    this.baseService.post(this.api_menu.Catalogue.CatPlace.add, this.portIndex).subscribe((response: any) => {
      this.baseService.spinnerHide();
      this.baseService.successToast(response.message);
      this.form.onReset();
      this.initPortIndex();
      $('#' + this.nameModal).modal('hide');
      this.pager.totalItems = this.pager.totalItems + 1;
      this.pager.currentPage = 1;
      this.child.setPage(this.pager.currentPage);
    }, err => {
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }


  onCancel() {
    this.form.onReset();
    this.initPortIndex();
    this.setPage(this.pager);
  }

  getDataCombobox() {
    this.getCountries();
    this.getAreas();
    this.getModeOfTransport();
  }
  getModeOfTransport(): any {
    this.baseService.spinnerShow();
    this.baseService.get(this.api_menu.Catalogue.CatPlace.getModeOfTransport).subscribe((response: any) => {
      this.baseService.spinnerHide();
      if (response != null) {
        this.modes = response.map(x => ({ "text": x.name, "id": x.id }));
      }
      else {
        this.modes = [];
      }
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  getAreas(): any {
    this.baseService.spinnerShow();
    this.baseService.get(this.api_menu.Catalogue.Area.getAllByLanguage).subscribe((response: any) => {
      this.baseService.spinnerHide();
      if (response != null) {
        this.areas = response.map(x => ({ "text": x.name, "id": x.id }));
      }
      else {
        this.areas = [];
      }
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  getCountries() {
    this.baseService.spinnerShow();
    this.baseService.get(this.api_menu.Catalogue.Country.getAllByLanguage).subscribe((response: any) => {
      this.baseService.spinnerHide();
      if (response != null) {
        this.countries = response.map(x => ({ "text": x.name, "id": x.id }));
      }
      else {
        this.countries = [];
      }
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  valueCountry: any = {};
  valueArea: any = {};
  valueMode: any = {};
  refreshCountryValue(value: any): void {
    this.valueCountry = value;
  }
  refreshAreaValue(value: any): void {
    this.valueArea = value;
  }
  refreshModeValue(value: any): void{
    this.valueMode = value;
  }
  public removed(value: any): void {
    console.log('Removed value is: ', value);
  }
  public typed(value: any): void {
    console.log('New search input: ', value);
  }
  onCountrychange(country) {
    this.portIndex.countryID = country.id;
  }
  onAreachange(area) {
    this.portIndex.areaID = area.id;
  }
  onModechange(mode){
    this.portIndex.modeOfTransport = mode.id;
  }
  showConfirmDelete(item) {
    this.portIndex = item;
  }
  showDetail(item) {
    this.portIndex = item;
    this.countryActive = this.countries.find(x => x.id == this.portIndex.countryID);
    this.areaActive = this.areas.find(x => x.id == this.portIndex.areaID);
    this.modeActive = this.modes.find(x => x.id == this.portIndex.modeOfTransport);
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
