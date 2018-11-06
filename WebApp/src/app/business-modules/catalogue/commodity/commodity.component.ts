import { Component, OnInit, ViewChild } from '@angular/core';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { CommodityGroup } from 'src/app/shared/models/catalogue/commonity-group.model';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { COMMODITYGROUPCOLUMNSETTING } from './commonity-group.column';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { SystemConstants } from 'src/constants/system.const';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { Commodity } from 'src/app/shared/models/catalogue/commodity.model';
import { COMMODITYCOLUMNSETTING } from './commodity.column';
declare var $:any;

@Component({
  selector: 'app-commodity',
  templateUrl: './commodity.component.html',
  styleUrls: ['./commodity.component.sass']
})
export class CommodityComponent implements OnInit {
  commonities: Array<Commodity>;
  commodity: Commodity;
  commodityGroups: Array<CommodityGroup>;
  commodityGroup: CommodityGroup;
  pagerGroup: PagerSetting = PAGINGSETTING;
  pagerCommodity: PagerSetting = PAGINGSETTING;
  commoditySettings: ColumnSetting[] = COMMODITYCOLUMNSETTING;
  commodityGroupSettings: ColumnSetting[] = COMMODITYGROUPCOLUMNSETTING;
  groups: any[];
  criteria: any = {};
  @ViewChild(PaginationComponent) child; 
  @ViewChild('formCommodity') formCommodity: NgForm;
  @ViewChild('formGroupCommodity') formGroupCommodity: NgForm;
  nameGroupModal = "edit-commodity-group-modal";
  nameCommodityModal = "edit-commodity-modal";
  titleAddGroupModal = "Add New Commodity Group";
  titleEditGroupModal = "Edit Commodity Group";
  titleAddCommodityModal = "Add New Commodity";
  titleEditCommodityModal = "Edit Commodity";
  groupActive: any;
  //isTabGroup = false;
  addCommodityButtonSetting: ButtonModalSetting = {
    dataTarget: this.nameCommodityModal,
    typeButton: ButtonType.add
  };
  addGroupButtonSetting: ButtonModalSetting = {
    dataTarget: this.nameGroupModal,
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
  selectedFilter = "All";
  configSearchGroup: any = {
    selectedFilter: this.selectedFilter,
    settingFields: this.commodityGroupSettings,
    typeSearch: TypeSearch.intab
  };
  configSearchCommonity: any = {
    selectedFilter: this.selectedFilter,
    settingFields: this.commoditySettings,
    typeSearch: TypeSearch.intab
  };
  titleConfirmDelete = "You want to delete this Commodity Group";
  isDesc = false;
  constructor(private baseService: BaseService,
    private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
    this.setPage(this.pagerCommodity, 'commodity');
    this.getComboboxData();
  }
  getComboboxData(): any {
    this.baseService.get(this.api_menu.Catalogue.CommodityGroup.getAllByLanguage).subscribe((response: any) => {
    this.groups = response.map(x=>({"text":x.groupName,"id":x.id}));
  });
  }
  setPage(pager: PagerSetting, tabName): any {
    if(tabName == 'commodityGroup'){
      this.getGroupCommodities(pager);
    }
    if(tabName == 'commodity'){
      this.getCommodities(pager);
    }
  }
  getCommodities(pager: PagerSetting): any {this.spinnerService.show();
    this.baseService.post(this.api_menu.Catalogue.Commodity.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.spinnerService.hide();
      this.commonities = response.data.map(x=>Object.assign({},x));
      this.pagerCommodity.totalItems = response.totalItems;
    });
  }

  getGroupCommodities(pager: PagerSetting): any {
    this.spinnerService.show();
    this.baseService.post(this.api_menu.Catalogue.CommodityGroup.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.spinnerService.hide();
      this.commodityGroups = response.data.map(x=>Object.assign({},x));
      this.pagerGroup.totalItems = response.totalItems;
    });
  }
  onSearch(event, tabName){
    if(tabName == 'commodityGroup'){
      this.searchCommodityGroup(event);
    }
    if(tabName == 'commodity'){
      this.searchCommodity(event);
    }
  }
  searchCommodity(event: any): any {
    if(event.field == "All"){
      this.criteria.all = event.searchString;
    }
    else{
      this.criteria.all = null;
      if(event.field == "commodityNameEn"){
        this.criteria.commodityNameEn = event.searchString;
      }
      if(event.field == "commodityNameVn"){
        this.criteria.commodityNameVn = event.searchString;
      }
      if(event.field == "commonityGroupNameEn"){
        this.criteria.commonityGroupNameEn = event.searchString;
      }
    }
    this.pagerCommodity.currentPage = 1;
    this.getCommodities(this.pagerCommodity);
  }
  searchCommodityGroup(event: any): any {
    if(event.field == "All"){
      this.criteria.all = event.searchString;
    }
    else{
      this.criteria.all = null;
      if(event.field == "groupNameEn"){
        this.criteria.groupNameEn = event.searchString;
      }
      if(event.field == "groupNameVn"){
        this.criteria.groupNameVn = event.searchString;
      }
    }
    this.pagerGroup.currentPage = 1;
    this.getGroupCommodities(this.pagerGroup);
  }
  resetSearch(event){
    this.criteria = {};
  }
  
  onSortChange(property) {
    this.isDesc = !this.isDesc;
    this.commodityGroups = this.sortService.sort(this.commodityGroups, property, this.isDesc);
  }
  async showDetail(item, tabName){
    if(tabName == 'commodityGroup'){
      this.commodityGroup = item;
      var response = await this.baseService.getAsync(this.api_menu.Catalogue.CommodityGroup.getById + item.id,false, false);
      this.commodityGroup = response;
    }
    if(tabName == 'commodity'){
      this.commodity = item;
      var response = await this.baseService.getAsync(this.api_menu.Catalogue.Commodity.getById + item.id,false, false);
      this.commodity = response;
      this.groupActive = this.groups.find(x => x.id == this.commodity.commodityGroupId);
    }
  }
  async onDelete(event){
    if (event) {
      if(this.commodityGroup){
        this.deleteGroupCommodity();
      }
      if(this.commodity){
        this.deleteCommodity();
      }
    }
  }
  deleteCommodity(): any {
    
    this.baseService.delete(this.api_menu.Catalogue.Commodity.delete + this.commodity.id).subscribe((response: any) => {
      if (response.status == true) {
        this.toastr.success(response.message);
        this.pagerCommodity.currentPage = 1;
        this.getCommodities(this.pagerCommodity);
        setTimeout(() => {
          this.child.setPage(this.pagerCommodity.currentPage, 'commodity');
        }, 300);
      
      }
      if (response.status == false) {
        this.toastr.error(response.message);
      }
    }, error => this.baseService.handleError(error));
  }
  deleteGroupCommodity(): any {
    
    this.baseService.delete(this.api_menu.Catalogue.CommodityGroup.delete + this.commodityGroup.id).subscribe((response: any) => {
      if (response.status == true) {
        this.toastr.success(response.message);
        this.pagerGroup.currentPage = 1;
        this.getGroupCommodities(this.pagerGroup);
        setTimeout(() => {
          this.child.setPage(this.pagerCommodity.currentPage, 'commodityGroup');
        }, 300);
      
      }
      if (response.status == false) {
        this.toastr.error(response.message);
      }
    }, error => this.baseService.handleError(error));
  }
  showAdd(tabName){
    if(tabName == 'commodityGroup'){
      this.commodityGroup = new CommodityGroup();
    }
    if(tabName == 'commodity'){
      this.commodity = new Commodity();
    }
  }
  onSubmit(){
    if(this.formGroupCommodity){
      // if(this.formGroupCommodity.valid){
      //   if(this.commodityGroup.id == null){
      //     this.addNewGroup();
      //   }
      //   else{
      //     this.updateGroup();
      //   }
      // }
      this.saveGroupCommodity();
    }
    if(this.formCommodity){
      // if(this.formCommodity.valid){
      //   if(this.commodityGroup.id == null){
      //     this.addNewCommodity();
      //   }
      //   else{
      //     this.updateCommodity();
      //   }
      // }
      this.saveCommodity();
    }
  }
  saveCommodity(): any {
    if(this.formCommodity.valid){
      if(this.commodity.id == null){
        this.addNewCommodity();
      }
      else{
        this.updateCommodity();
      }
    }
  }
  saveGroupCommodity(){
    if(this.formGroupCommodity.valid){
      if(this.commodityGroup.id == null){
        this.addNewGroup();
      }
      else{
        this.updateGroup();
      }
    }
  }
  updateCommodity(): any {
    this.baseService.put(this.api_menu.Catalogue.Commodity.update + this.commodity.id, this.commodity).subscribe((response: any) => {
      if (response.status == true){
        $('#' + this.nameCommodityModal).modal('hide');
        this.toastr.success(response.message);
        this.setPage(this.pagerCommodity, 'commodity');
      }
    }, error => this.baseService.handleError(error));
  }
  addNewCommodity(): any {
    this.baseService.post(this.api_menu.Catalogue.Commodity.add, this.commodity).subscribe((response: any) => {
    if (response.status == true){
      this.toastr.success(response.message);
      this.getCommodities(this.pagerGroup);
      this.formCommodity.onReset();
      this.commodity = new Commodity();
      $('#' + this.nameCommodityModal).modal('hide');
      setTimeout(() => {
        this.pagerCommodity.currentPage = 1;
        this.child.setPage(this.pagerCommodity.currentPage);
      }, 500);
    }
    else{
      this.toastr.error(response.message);
    }
  }, error => this.baseService.handleError(error));
  }
  updateGroup(): any {
    this.baseService.put(this.api_menu.Catalogue.CommodityGroup.update + this.commodityGroup.id, this.commodityGroup).subscribe((response: any) => {
    if (response.status == true){
      $('#' + this.nameGroupModal).modal('hide');
      this.toastr.success(response.message);
      this.setPage(this.pagerGroup, true);
    }
  }, error => this.baseService.handleError(error));
  }
  addNewGroup(): any {
    this.baseService.post(this.api_menu.Catalogue.CommodityGroup.add, this.commodityGroup).subscribe((response: any) => {
      if (response.status == true){
        this.toastr.success(response.message);
        this.getGroupCommodities(this.pagerGroup);
        this.formCommodity.onReset();
        this.commodityGroup = new CommodityGroup();
        $('#' + this.nameGroupModal).modal('hide');
        setTimeout(() => {
          this.pagerGroup.currentPage = 1;
          this.child.setPage(this.pagerGroup.currentPage);
        }, 500);
      }
      else{
        this.toastr.error(response.message);
      }
    }, error => this.baseService.handleError(error));
  }
  showConfirmDelete(item, tabName) {
    if(tabName == 'commodityGroup'){
      this.commodityGroup = item;
    }
    if(tabName == 'commodity'){
      this.commodity = item;
    }
  }
  onCancel(tabName){
    if(tabName == 'commodityGroup'){
      this.commodityGroup = new CommodityGroup();
      this.formGroupCommodity.onReset();
      this.setPage(this.pagerGroup, tabName);
    }
    if(tabName == 'commodity'){
      this.commodity = new Commodity();
      this.formCommodity.onReset();
      this.setPage(this.pagerCommodity, tabName);
    }
  }
  public removed(value:any):void {
    console.log('Removed value is: ', value);
  }
  public typed(value:any):void {
    console.log('New search input: ', value);
  }
  value: any
  onGroupchange(value){
    this.commodity.commodityGroupId = value.id;
    console.log(this.commodity.commodityGroupId);
  }
  refreshGroupValue(value:any){
    this.value = value;
  }
}
