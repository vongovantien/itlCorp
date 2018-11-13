import { Component, OnInit, ViewChild } from '@angular/core';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PORTINDEXCOLUMNSETTING } from './port-index.columns';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
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
declare var $:any;

@Component({
  selector: 'app-port-index',
  templateUrl: './port-index.component.html',
  styleUrls: ['./port-index.component.sass']
})
export class PortIndexComponent implements OnInit {

  portIndexSettings: ColumnSetting[] = PORTINDEXCOLUMNSETTING;
  portIndexs: Array<PortIndex>;
  portIndex: PortIndex= new PortIndex();
  pager: PagerSetting = PAGINGSETTING;
  criteria: any = { placeType: 8 };
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
    typeButton: ButtonType.export
  };
  exportButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.import
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
  @ViewChild(PaginationComponent) child; 
  countries: any[];
  areas: any[];
  modes: any[];
  countryActive: any;
  areaActive: any;
  isDesc: boolean = false;
  titleConfirmDelete = "You want to delete this port index";

  constructor(private baseService: BaseService,
    private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
    this.setPage(this.pager);
    this.getDataCombobox();
  }
  setPage(pager: PagerSetting): any {
    this.getPortIndexs(pager);
  }
  getPortIndexs(pager: PagerSetting): any {
    this.spinnerService.show();
    this.baseService.post(this.api_menu.Catalogue.CatPlace.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.spinnerService.hide();
      this.portIndexs = response.data.map(x=>Object.assign({},x));
      this.pager.totalItems = response.totalItems;
    });
  }
  onSearch(event){
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
        if(event.field == "areaName"){
          this.criteria.areaNameEN = event.searchString;
        }
      }
      else{
        if(event.field == "countryName"){
          this.criteria.countryNameVN = event.searchString;
        }
        if(event.field == "areaName"){
          this.criteria.areaNameVN = event.searchString;
        }
      }
      if(language == SystemConstants.LANGUAGES.VIETNAM){
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
      if(event.field == "modeOfTransport"){
        this.criteria.modeOfTransport = event.searchString;
      }
    }
    this.pager.currentPage = 1;
    this.setPage(this.pager);
  }
  resetSearch(event){
    this.criteria = {
      placeType: 8
    };
  }
  showAdd(){
    this.initPortIndex();
    this.ngSelectCountry.active = [];
    this.ngSelectArea.active = [];
  }
  initPortIndex(){
    this.portIndex = new PortIndex();
    this.portIndex.placeType = 8;
  }
  onSubmit(){
    if(this.form.valid){
      if(this.portIndex.id == null){
        this.addNew();
      }
      else{
        this.update();
      }
    }
  }
  update(): any {
    this.baseService.put(this.api_menu.Catalogue.CatPlace.update + this.portIndex.id, this.portIndex).subscribe((response: any) => {
    if (response.status == true){
      $('#' + this.nameModal).modal('hide');
      this.toastr.success(response.message);
      this.setPage(this.pager);
    }
  }, error => this.baseService.handleError(error));
  }
  addNew(): any {
    this.baseService.post(this.api_menu.Catalogue.CatPlace.add, this.portIndex).subscribe((response: any) => {
      if (response.status == true){
        this.toastr.success(response.message);
        this.getPortIndexs(this.pager);
        this.form.onReset();
        this.initPortIndex();
        $('#' + this.nameModal).modal('hide');
        setTimeout(() => {
          this.pager.currentPage = 1;
          this.child.setPage(this.pager.currentPage);
        }, 500);
      }
      else{
        this.toastr.error(response.message);
      }
    }, error => this.baseService.handleError(error));
  }
  onCancel(){
    this.form.onReset();
    this.initPortIndex();
    this.setPage(this.pager);
  }

  getDataCombobox(){
    this.getCountries();
    this.getAreas();
    this.getModeOfTransport();
  }
  getModeOfTransport(): any {
    this.baseService.get(this.api_menu.Catalogue.CatPlace.getModeOfTransport).subscribe((response: any) => {
      if(response != null){
        this.modes = response.map(x=>({"text":x.name,"id":x.id}));
      }
      else{
        this.modes = [];
      }
    });
  }
  getAreas(): any {
    this.baseService.get(this.api_menu.Catalogue.Area.getAllByLanguage).subscribe((response: any) => {
      if(response != null){
        this.areas = response.map(x=>({"text":x.name,"id":x.id}));
      }
      else{
        this.areas = [];
      }
    });
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
  valueCountry: any = {};
  valueArea: any = {};
  refreshCountryValue(value: any):void {
    this.valueCountry = value;
  }
  refreshAreaValue(value: any): void {
    this.valueArea = value;
  }
  public removed(value:any):void {
    console.log('Removed value is: ', value);
  }
  public typed(value:any):void {
    console.log('New search input: ', value);
  }
  onCountrychange(country){
    this.portIndex.countryID = country.id;
  }
  onAreachange(area){
    this.portIndex.areaID = area.id;
  }
  showConfirmDelete(item) {
    this.portIndex = item;
  }
  showDetail(item) {
    this.portIndex = item;
    this.countryActive = this.countries.find(x => x.id == this.portIndex.countryID);
    this.areaActive = this.areas.find(x => x.id == this.portIndex.areaID);
  }
  async onDelete(event) {
    console.log(event);
    if (event) {
      this.baseService.delete(this.api_menu.Catalogue.CatPlace.delete + this.portIndex.id).subscribe((response: any) => {
        if (response.status == true) {
          this.toastr.success(response.message);
          this.pager.currentPage = 1;
          this.getPortIndexs(this.pager);
          setTimeout(() => {
            this.child.setPage(this.pager.currentPage);
          }, 300);
         
        }
        if (response.status == false) {
          this.toastr.error(response.message);
        }
      }, error => this.baseService.handleError(error));
    }
  }
  onSortChange(column) {
    if(column.dataType != 'boolean'){
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.portIndexs = this.sortService.sort(this.portIndexs, property, this.isDesc);
    }
  }
}
