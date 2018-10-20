import { Component, OnInit } from '@angular/core';
import { Warehouse } from '../../../shared/models/ware-house';
import { ColumnSetting } from '../../../shared/models/layout/column-setting.model';
import { SortService } from '../../../shared/services/sort.service';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';
import { PagerSetting } from '../../../shared/models/layout/pager-setting.model';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { PagingService } from 'src/app/shared/common/pagination/paging-service';
import { config } from 'rxjs';
import { CountryModel } from '../../../shared/models/catalogue/country.model';
import { ProviceModel } from '../../../shared/models/catalogue/province.model';
import { DistrictModel } from '../../../shared/models/catalogue/district.model';

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
  warehouse: Warehouse;
  breadcums: string[] = ["Dashboard", "Catalog", "Warehouse"];
  criteria: any = {};
  pager: PagerSetting = {
    currentPage: 1,
    pageSize: 15,
    numberToShow: [3,5,10,15, 30, 50],
    numberPageDisplay: 7
  };
  addButtonSetting: ButtonModalSetting = {
    // buttonAttribute: 
    // {
    //   titleButton: "add new",
    //   classStyle: "btn btn-success m-btn--square m-btn--icon m-btn--uppercase",
    //   targetModal: "add-ware-house-modal",
    //   icon: "icon-plus7"
    // }
    //,
    dataTarget: "add-ware-house-modal",
    typeButton: ButtonType.add
  };
  importButtonSetting: ButtonModalSetting = {
    // buttonAttribute: {
    //   titleButton: "import",
    //   classStyle: "btn btn-brand m-btn--square m-btn--icon m-btn--uppercase",
    //   icon: "la la-download"
    // },
    typeButton: ButtonType.export
  };
  exportButtonSetting: ButtonModalSetting = {
    // buttonAttribute: {
    //   titleButton: "export",
    //   classStyle: "btn btn-danger m-btn--square m-btn--icon m-btn--uppercase",
    //   icon: "la la-upload"
    // },
    typeButton: ButtonType.import
  };
  saveButtonSetting: ButtonModalSetting = {
    // buttonAttribute: {
    //   titleButton: "export",
    //   classStyle: "btn btn-danger m-btn--square m-btn--icon m-btn--uppercase",
    //   icon: "la la-upload"
    // },
    typeButton: ButtonType.save
  };

  cancelButtonSetting: ButtonModalSetting = {
    // buttonAttribute: {
    //   titleButton: "export",
    //   classStyle: "btn btn-danger m-btn--square m-btn--icon m-btn--uppercase",
    //   icon: "la la-upload"
    // },
    typeButton: ButtonType.cancel
  };
  resetButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.reset
  }
  nameEditModal = "edit-ware-house-modal";
  titleConfirmDelete = "You want to delete this warehouse";
  postSettings: ColumnSetting[] =
    [
      {
        primaryKey: 'id',
        header: 'Id',
        dataType: "number"
      },
      {
        primaryKey: 'code',
        header: 'Code',
        isShow: true
      },
      {
        primaryKey: 'name',
        header: 'Name',
        isShow: true
      },
      {
        primaryKey: 'countryName',
        header: 'Country',
        isShow: true
      },
      {
        primaryKey: 'provinceName',
        header: 'City/ Province',
        isShow: true
      },
      {
        primaryKey: 'districtName',
        header: 'District',
        isShow: true
      },
      {
        primaryKey: 'address',
        header: 'Address',
        isShow: true
      }
    ];
  isDesc: boolean = false;
  
  constructor(private sortService: SortService, private baseService: BaseService,private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService, private pagingService: PagingService) { }

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
  searchTypeChange() {

  }


  setPage(pager) {
    this.getWarehouses(pager);
  }
  resetSearch(){
    
  }
}
