import { Component, OnInit } from '@angular/core';
import { Warehouse } from '../../../shared/models/ware-house';
import { ColumnSetting } from '../../../shared/models/layout/column-setting.model';
import { WAREHOUSEDATA } from './fakedata';
import { SortService } from '../../../shared/services/sort.service';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';

@Component({
  selector: 'app-warehouse',
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.sass']
})
export class WarehouseComponent implements OnInit {
  warehouses: Array<Warehouse>;
  breadcums: string[] = ["Dashboard", "Catalog", "Warehouse"];
  criteria: any = {};
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
  nameEditModal = "edit-ware-house-modal";
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

  constructor(private sortService: SortService) { }

  ngOnInit() {
    this.warehouses = this.getProjects();
    console.log(this.warehouses);
  }
  getProjects(): Warehouse[] {
    return WAREHOUSEDATA;
  }
  onSortChange(property){
    this.isDesc = !this.isDesc; 
    this.warehouses = this.sortService.sort(this.warehouses, property, this.isDesc);
  }
  onEdit(item){
    console.log(item);
  }
  onDelete(item){
    console.log(item);
  }
  searchTypeChange(){

  }
}
