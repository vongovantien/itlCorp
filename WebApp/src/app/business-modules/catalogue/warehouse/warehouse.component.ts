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
import { CountryModel } from '../../../shared/models/catalogue/country.model';
import { ProviceModel } from '../../../shared/models/catalogue/province.model';
import { DistrictModel } from '../../../shared/models/catalogue/district.model';
import { NgForm } from '@angular/forms';
import { SystemConstants } from '../../../../constants/system.const';

@Component({
  selector: 'app-warehouse',
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.sass']
})
export class WarehouseComponent implements OnInit {
  warehouses: Array<Warehouse>;
  countries: Array<CountryModel>;
  provinces: Array<ProviceModel>;
  districts: Array<DistrictModel>;
  warehouse: Warehouse = new Warehouse();
  countryLookup: any = { 
    dataLookup: {},
    value: null,
    displayName: null
  };
  provinceLookup: any;
  districtLookup: any;
  criteria: any = {};
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
  resetButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.reset
  }
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
        primaryKey: 'name',
        header: 'Name',
        isShow: true,
        dataType: 'text',
        allowSearch: true,
        required: true,
        lookup: ''
      },
      {
        primaryKey: 'countryName',
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
        primaryKey: 'provinceName',
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
        primaryKey: 'districtName',
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
  
  constructor(private sortService: SortService, private baseService: BaseService,private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService) { }

  ngOnInit() {
    this.setPage(this.pager);
    this.getDataCombobox();
  }
  getDataCombobox(){
    this.getCountries();
    this.getProvinces();
    this.getDistricts();
  }
  getCountries(){
    this.baseService.get("http://localhost:44361/api/v1/1/CatCountry").subscribe((response: any) => {
      this.countries = response;
    });
  }
  getProvinces(id?: number){
    let url = "http://localhost:44361/api/v1/1/CatPlace/GetProvinces";
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
    let url = "http://localhost:44361/api/v1/1/CatPlace/GetDistricts";
    if(id != undefined){
      url = url + "?provinceId=" + id; 
    }
    this.baseService.get(url).subscribe((response: any) => {
      this.districts = response;
    });
  }
  getWarehouses(pager: PagerSetting) {
    this.spinnerService.show();
    this.baseService.post("http://localhost:44361/api/v1/1/CatPlace/Paging?page=" + pager.currentPage + "&size=" + pager.pageSize, {"placeType": 11}).subscribe((response: any) => {
      this.spinnerService.hide();
      this.warehouses = response.data;
      console.log(this.warehouses);
    });
  }
  onSortChange(property) {
    this.isDesc = !this.isDesc;
    this.warehouses = this.sortService.sort(this.warehouses, property, this.isDesc);
  }
  showDetail(item) {
    console.log(item);
    this.warehouse = item;
  }
  onDelete(event) {
    console.log(event);
    if (event) {
      //call api
    }
  }
  showConfirmDelete(item) {
    this.warehouse = item;
    console.log(item);
  }

  setPage(pager) {
    this.getWarehouses(pager);
  }
  resetSearch(){
    
  }
  onSubmit(event){
    // if(event.valid){
    //   console.log("submit success");
    //   event.onReset();
    // }
    // else{
    //   console.log("submit");
    // }
    console.log(this.warehouse);
    if(this.form.valid){
      this.form.onReset();
      console.log("submit success");
    }
    else{
      console.log("submit");
    }
  }
  onSearch(event){
    console.log(event);
  }
  onCancel(){
    this.form.onReset();
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
}
