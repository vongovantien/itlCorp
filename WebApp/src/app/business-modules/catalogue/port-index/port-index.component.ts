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
  addButtonSetting: ButtonModalSetting = {
    dataTarget: "add-port-index-modal",
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
    settingFields: this.portIndexSettings
  };
  @ViewChild('formAddEdit') form: NgForm;
  countries: any[];
  areas: any[];
  modes: any[];

  constructor(private baseService: BaseService,
    private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU) { }

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
  onSearch(event){}
  resetSearch(event){}
  showAdd(){
  }
  initPortIndex(){
    this.portIndex = { 
      id: null,
      code: null,
      name: null,
      countryID: null,
      areaID: null,
      countryName: null,
      areaName: null,
      modeOfTransport: null,
      placeType: 8
    }
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
    throw new Error("Method not implemented.");
  }
  addNew(): any {
    this.baseService.post(this.api_menu.Catalogue.CatPlace.add, this.portIndex).subscribe((response: any) => {
      if (response.status == true){
        this.toastr.success(response.message);
        this.form.onReset();
        this.initPortIndex();
        this.setPage(this.pager);
        $('#' + this.addButtonSetting.dataTarget).modal('hide');
      }
      else{
        this.toastr.error(response.message);
      }
    }, error => this.baseService.handleError(error));
  }
  onCancel(){
    this.form.onReset();
    this.initPortIndex();
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
}
