import { Component, OnInit, ViewChild } from '@angular/core';
import { Warehouse } from '../../../shared/models/catalogue/ware-house';
import { ColumnSetting } from '../../../shared/models/layout/column-setting.model';
import { SortService } from '../../../shared/services/sort.service';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';
import { PagerSetting } from '../../../shared/models/layout/pager-setting.model';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { NgForm } from '@angular/forms';
import { SystemConstants } from '../../../../constants/system.const';
import { API_MENU } from '../../../../constants/api-menu.const';
declare var $:any;

@Component({
  selector: 'app-warehouse',
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.sass']
})
export class WarehouseComponent implements OnInit {
  warehouses: Array<Warehouse>;
  countries: any[];
  provinces: any[];
  districts: any[];
  warehouse: Warehouse = new Warehouse();
  showModal: boolean = false;
  countryLookup: any = { 
    dataLookup: {},
    value: null,
    displayName: null
  };
  provinceLookup: any;
  districtLookup: any;
  criteria: any = { placeType: 12 };
  // @ViewChild('formAddEdit') form: NgForm;
  pager: PagerSetting = {
    currentPage: 1,
    pageSize: SystemConstants.OPTIONS_PAGE_SIZE,
    numberToShow: SystemConstants.ITEMS_PER_PAGE,
    numberPageDisplay: SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY
  };
  addButtonSetting: ButtonModalSetting = {
    dataTarget: "add-ware-house-modal",
    typeButton: ButtonType.add
  };
  resetWarehouse(){
    this.warehouse = {
      id: null,
      code: null,
      name: null,
      countryID: null,
      districtID: null,
      provinceID:null,
      countryName: null,
      provinceName: null,
      districtName: null,
      address: null,
      placeType: 12
    };
  }
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
  
  @ViewChild('formAddEdit') form: NgForm;
  nameEditModal = "edit-ware-house-modal";
  selectedFilter = "All";
  titleConfirmDelete = "You want to delete this warehouse";
  warehouseSettings: ColumnSetting[] =
    [
      {
        primaryKey: 'id',
        header: 'Id',
        dataType: "number",
        lookup: ''
      },
      {
        primaryKey: 'code',
        header: 'Code',
        isShow: true,
        allowSearch: true,
        dataType: "text",
        required: true,
        lookup:''
      },
      {
        primaryKey: 'displayName',
        header: 'Name',
        isShow: true,
        dataType: 'text',
        allowSearch: true,
        required: true,
        lookup: ''
      },
      {
        primaryKey: 'countryNameVN',
        header: 'Country',
        isShow: true,
        allowSearch: true,
        lookup: ''
      },
      {
        primaryKey: 'countryID',
        header: 'Country',
        isShow: false,
        required: true,
        lookup: 'countries'
      },
      {
        primaryKey: 'provinceNameVN',
        header: 'City/ Province',
        isShow: true,
        allowSearch: true,
        lookup: ''
      },
      {
        primaryKey: 'provinceID',
        header: 'City/ Province',
        isShow: false,
        required: true,
        lookup: 'provinces'
      },
      {
        primaryKey: 'districtNameVN',
        header: 'District',
        isShow: true,
        allowSearch: true,
        lookup: ''
      },
      {
        primaryKey: 'districtID',
        header: 'District',
        isShow: false,
        required: true,
        lookup: 'districts'
      },
      {
        primaryKey: 'address',
        header: 'Address',
        isShow: true,
        dataType: 'text',
        allowSearch: true,
        required: true,
        lookup: ''
      }
    ];
  isDesc: boolean = false;
  configSearch: any = {
    selectedFilter: this.selectedFilter,
    settingFields: this.warehouseSettings
  };
  
  constructor(private sortService: SortService, private baseService: BaseService,
    private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU) { }

  ngOnInit() {
    this.warehouse.placeType = 12;
    this.setPage(this.pager);
    this.getDataCombobox();
  }
  getDataCombobox(){
    this.getCountries();
    this.getProvinces();
    this.getDistricts();
  }
  getCountries(){
    this.baseService.get(this.api_menu.Catalogue.country.getAll).subscribe((response: any) => {
      this.countries = response;
    });
  }
  getProvinces(id?: number){
    let url = this.api_menu.Catalogue.CatPlace.getProvinces;
    if(id != undefined){
      url = url + "?countryId=" + id; 
    }
    this.baseService.get(url).subscribe((response: any) => {
      this.provinces = response;
      this.countryLookup.dataLookup = this.provinces;
      this.countryLookup.value = "id";
      this.countryLookup.displayName = "nameEn";
      console.log(this.provinces);
    });
  }
  getDistricts(id?: number){
    let url = this.api_menu.Catalogue.CatPlace.getDistricts;
    if(id != undefined){
      url = url + "?provinceId=" + id; 
    }
    this.baseService.get(url).subscribe((response: any) => {
      this.districts = response;
    });
  }
  getWarehouses(pager: PagerSetting) {
    this.spinnerService.show();
    this.baseService.post(this.api_menu.Catalogue.CatPlace.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.spinnerService.hide();
      this.warehouses = response.data.map(x=>Object.assign({},x));
      this.pager.totalItems = response.totalItems;
    });
  }
  onSortChange(property) {
    this.isDesc = !this.isDesc;
    this.warehouses = this.sortService.sort(this.warehouses, property, this.isDesc);
  }
  showDetail(item) {
    this.warehouse = item;
  }
  async onDelete(event) {
    console.log(event);
    if (event) {
      this.baseService.delete(this.api_menu.Catalogue.CatPlace.delete + this.warehouse.id).subscribe((response: any) => {
        if (response.status == true) {
          this.toastr.success(response.message);
          this.pager.currentPage = 1;
          this.getWarehouses(this.pager);
        }
        if (response.status == false) {
          this.toastr.error(response.message);
        }
      }, error => this.baseService.handleError(error));
    }
  }
  showConfirmDelete(item) {
    this.warehouse = item;
  }

  setPage(pager) {
    this.getWarehouses(pager);
  }
  onSubmit(){
    if(this.form.valid){
      if(this.warehouse.id == null){
        this.baseService.post(this.api_menu.Catalogue.CatPlace.add, this.warehouse).subscribe((response: any) => {
          if (response.status == true){
            this.toastr.success(response.message);
            this.resetWarehouse();
            this.form.onReset();
            this.getWarehouses(this.pager);
            $('#' + this.addButtonSetting.dataTarget).modal('hide');
          }
          else{
            this.toastr.error(response.message);
          }
        }, error => this.baseService.handleError(error));
      }
      else{
        this.baseService.put(this.api_menu.Catalogue.CatPlace.update + this.warehouse.id, this.warehouse).subscribe((response: any) => {
          if (response.status == true){
            $('#edit-ware-house-modal').modal('hide');
            this.toastr.success(response.message);
            this.getWarehouses(this.pager);
          }
        }, error => this.baseService.handleError(error));
      }
    }
    else{
      console.log("submit");
    }
  }
  addNew(){
    
  }
  resetSearch(event){
    this.criteria = {
      placeType: 12
    };
  }
  onSearch(event){
    console.log(event);
    if(event.field == "All"){
      this.criteria.all = event.searchString;
    }
    else{
      this.criteria.all = null;
      if(event.field == "code"){
        this.criteria.code = event.searchString;
      }
      if(event.field == "displayName"){
        this.criteria.displayName = event.searchString;
      }
      if(event.field == "countryNameVN"){
        this.criteria.countryNameEN = event.searchString;
      }
      if(event.field == "provinceNameVN"){
        this.criteria.provinceNameVN = event.searchString;
      }
      if(event.field == "districtNameVN"){
        this.criteria.districtNameVN = event.searchString;
      }
      if(event.field == "address"){
        this.criteria.address = event.searchString;
      }
    }
    this.pager.currentPage = 1;
    this.getWarehouses(this.pager);
  }
  onCancel(){
    this.form.onReset();
    this.getWarehouses(this.pager);
  }
  getColumn(field){
    return this.warehouseSettings.find(x => x.primaryKey == field);
  }
  onCountrychange(countryId){
    this.getProvinces(countryId);
  }
  onProvincechange(provinceId){
    this.getDistricts(provinceId);
  }
  showAdd(){
    this.showModal = true;
    this.form.onReset();
  }
}
