import { Component, OnInit, ViewChild } from '@angular/core';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { CommodityGroup } from 'src/app/shared/models/catalogue/commonity-group.model';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { COMMODITYGROUPCOLUMNSETTING } from './commonity-group.column';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { Commodity } from 'src/app/shared/models/catalogue/commodity.model';
import { COMMODITYCOLUMNSETTING } from './commodity.column';
import { SelectComponent } from 'ng2-select';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { ExcelService } from 'src/app/shared/services/excel.service';
import * as lodash from 'lodash';
import { SystemConstants } from 'src/constants/system.const';
declare var $:any;

@Component({
  selector: 'app-commodity',
  templateUrl: './commodity.component.html',
  styleUrls: ['./commodity.component.sass']
})
export class CommodityComponent implements OnInit {

  /*
  declare variable
  */
  @ViewChild(PaginationComponent) child; 
  @ViewChild('formCommodity') formCommodity: NgForm;
  @ViewChild('formGroupCommodity') formGroupCommodity: NgForm;
  @ViewChild('chooseGroup') public groupSelect: SelectComponent;
  commodities: Array<Commodity>;
  commodity: Commodity;
  commodityGroups: Array<CommodityGroup>;
  commodityGroup: CommodityGroup;
  pager: PagerSetting = PAGINGSETTING;
  commoditySettings: ColumnSetting[] = COMMODITYCOLUMNSETTING;
  commodityGroupSettings: ColumnSetting[] = COMMODITYGROUPCOLUMNSETTING;
  groups: any[];
  criteria: any = {};
  keyCommoditySortDefault = "commodityNameEn";
  keygroupSortDefault = "groupNameEn";
  nameGroupModal = "edit-commodity-group-modal";
  nameCommodityModal = "edit-commodity-modal";
  titleAddGroupModal = "Add New Commodity Group";
  titleEditGroupModal = "Edit Commodity Group";
  titleAddCommodityModal = "Add New Commodity";
  titleEditCommodityModal = "Edit Commodity";
  tabName = {
    commodity: "commodity",
    commodityGroup: "commodityGroup"
  };
  activeTab: string = this.tabName.commodity;
  groupActive: any;
  addCommodityButtonSetting: ButtonModalSetting = {
    dataTarget: this.nameCommodityModal,
    typeButton: ButtonType.add
  };
  addGroupButtonSetting: ButtonModalSetting = {
    dataTarget: this.nameGroupModal,
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
  /*
  end declare variable
  */

  constructor(private baseService: BaseService,
    private excelService: ExcelService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
    this.getCommodities(this.pager);
    this.getGroups();
  }
  async getGroups(){
    const response = await this.baseService.getAsync(this.api_menu.Catalogue.CommodityGroup.getAllByLanguage, false, false);
    if(response){
      this.groups = response.map(x=>({"text":x.groupName,"id":x.id}));
    }
  }
  async setPage(pager: PagerSetting){
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    if(this.activeTab == this.tabName.commodityGroup){
      await this.getGroupCommodities(pager);
    }
    if(this.activeTab == this.tabName.commodity){
      await this.getCommodities(pager);
    }
  }
  tabSelect(tabName: string){
    this.activeTab = tabName;
    this.pager.currentPage = 1;
    this.pager.pageSize = 15;
    if(tabName == this.tabName.commodityGroup){
      this.getGroupCommodities(this.pager);
    }
    if(tabName == this.tabName.commodity){
      this.getCommodities(this.pager);
    }
  }
  async getCommodities(pager: PagerSetting) {
    const responses = await this.baseService.postAsync(this.api_menu.Catalogue.Commodity.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, true, true);
    if(responses){
      this.commodities = responses.data;
      this.pager.totalItems = responses.totalItems;
    }
  }

  async getGroupCommodities(pager: PagerSetting){
    const responses = await this.baseService.postAsync(this.api_menu.Catalogue.CommodityGroup.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, true, true);
    if(responses){
      this.commodityGroups = responses.data;
      this.pager.totalItems = responses.totalItems;
    }
  }
  onSearch(event, tabName){
    if(tabName == this.tabName.commodityGroup){
      this.searchCommodityGroup(event);
    }
    if(tabName == this.tabName.commodity){
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
    this.pager.currentPage = 1;
    this.getCommodities(this.pager);
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
    this.pager.currentPage = 1;
    this.getGroupCommodities(this.pager);
  }
  resetSearch(event){
    this.criteria = {};
  }
  
  onSortChange(column) {
    if(column.dataType != 'boolean'){
      let property = column.primaryKey;
      
      this.isDesc = !this.isDesc;
      if(this.activeTab == this.tabName.commodity){
        this.commodities = this.sortService.sort(this.commodities, property, this.isDesc);
      }
      if(this.activeTab ==  this.tabName.commodityGroup){
        this.commodityGroups = this.sortService.sort(this.commodityGroups, property, this.isDesc);
      }
    }
  }
  async showDetail(item, tabName){
    if(tabName == this.tabName.commodityGroup){
      this.commodityGroup = item;
      const response = await this.baseService.getAsync(this.api_menu.Catalogue.CommodityGroup.getById + item.id,false, false);
      this.commodityGroup = response;
    }
    if(tabName == this.tabName.commodity){
      this.commodity = item;
      const response = await this.baseService.getAsync(this.api_menu.Catalogue.Commodity.getById + item.id,false, false);
      this.commodity = response;
      this.groupActive = this.groups.find(x => x.id == this.commodity.commodityGroupId);
    }
  }
  async onDelete(event){
    if (event) {
      if(this.commodityGroup){
        await this.deleteGroupCommodity();
      }
      if(this.commodity){
        await this.deleteCommodity();
      }
    }
  }
  async deleteCommodity() {
    await this.baseService.deleteAsync(this.api_menu.Catalogue.Commodity.delete + this.commodity.id, true, false);
    //await this.getCommodities(this.pager);
    this.setPageAfterDelete();
  }
  async deleteGroupCommodity(){
    const response = await this.baseService.deleteAsync(this.api_menu.Catalogue.CommodityGroup.delete + this.commodityGroup.id, true, false);
    if(response){
      this.getGroups();
      //await this.getGroupCommodities(this.pager);
      this.setPageAfterDelete();
    }
  }
  setPageAfterDelete(){
    this.pager.totalItems = this.pager.totalItems -1;
    let totalPages = Math.ceil(this.pager.totalItems / this.pager.pageSize);
    if (totalPages < this.pager.totalPages) {
      this.pager.currentPage = totalPages;
    }
    this.child.setPage(this.pager.currentPage);
  }
  setPageAfterAdd() {
    this.child.setPage(this.pager.currentPage);
    if (this.pager.currentPage < this.pager.totalPages) {
      this.pager.currentPage = this.pager.totalPages;
      this.child.setPage(this.pager.currentPage);
    }
  }
  showAdd(tabName){
    if(tabName == this.tabName.commodityGroup){
      this.commodityGroup = new CommodityGroup();
    }
    if(tabName == this.tabName.commodity){
      this.commodity = new Commodity();
    }
  }
  onSubmit(){
    if(this.formGroupCommodity){
      this.saveGroupCommodity();
    }
    if(this.formCommodity){
      this.saveCommodity();
    }
  }
  saveCommodity(): any {
    if(this.formCommodity.valid && this.commodity.commodityGroupId != null){
      if(this.commodity.id == null){
        this.addNewCommodity();
      }
      else{
        this.updateCommodity();
      }
    }
  }
  async saveGroupCommodity(){
    if(this.formGroupCommodity.valid){
      if(this.commodityGroup.id == null){
        await this.addNewGroup();
      }
      else{
        await this.updateGroup();
      }
    }
  }
  updateCommodity(): any {
    this.baseService.spinnerShow();
    this.baseService.put(this.api_menu.Catalogue.Commodity.update + this.commodity.id, this.commodity).subscribe((response: any) => {
      this.groupSelect.active = [];
      this.formCommodity.onReset();
      $('#' + this.nameCommodityModal).modal('hide');
      this.setPage(this.pager);
      this.baseService.successToast(response.message);
      this.baseService.spinnerHide();
    }, err => {
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  async addNewCommodity() {
      const res = await this.baseService.postAsync(this.api_menu.Catalogue.Commodity.add, this.commodity);
      if(res){
        this.formCommodity.onReset();
        this.commodity = new Commodity();
        $('#' + this.nameCommodityModal).modal('hide');
        await this.getCommodities(this.pager);
        this.setPageAfterAdd();
      }
  }
  updateGroup(): any {
    this.baseService.spinnerShow();
    this.baseService.put(this.api_menu.Catalogue.CommodityGroup.update + this.commodityGroup.id, this.commodityGroup).subscribe((response: any) => {
    if (response.status == true){
      $('#' + this.nameGroupModal).modal('hide');
      this.baseService.successToast(response.message);
      this.baseService.spinnerHide();
      this.setPage(this.pager);
    }
  }, err=>{
     this.baseService.spinnerHide();
     this.baseService.handleError(err);
  });
  }
  async addNewGroup() {
    const response = await this.baseService.postAsync(this.api_menu.Catalogue.CommodityGroup.add, this.commodityGroup, true, false);
    if (response){
      this.getGroups();
      //await this.getGroupCommodities(this.pager);
      this.pager.totalItems = this.pager.totalItems + 1;
      this.pager.currentPage = 1;
      this.child.setPage(this.pager.currentPage);
      this.formGroupCommodity.onReset();
      this.commodityGroup = new CommodityGroup();
      $('#' + this.nameGroupModal).modal('hide');
      // this.child.setPage(this.pager.currentPage);
      // this.setPageAfterAdd();
    }
  }
  showConfirmDelete(item, tabName) {
    if(tabName == this.tabName.commodityGroup){
      this.commodityGroup = item;
    }
    if(tabName == this.tabName.commodity){
      this.commodity = item;
    }
  }
  onCancel(tabName){
    if(tabName == this.tabName.commodityGroup){
      this.commodityGroup = new CommodityGroup();
      this.formGroupCommodity.onReset();
      this.setPage(this.pager);
    }
    if(tabName == this.tabName.commodity){
      this.commodity = new Commodity();
      this.formCommodity.onReset();
      this.setPage(this.pager);
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

  async importCom(){

  }
  async exportCom(){
    var commodities = await this.baseService.postAsync(this.api_menu.Catalogue.Commodity.query,this.criteria);
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.ENGLISH_API){
      commodities = lodash.map(commodities,function(com,index){
        return [
          index+1,
          com['code'],
          com['commodityNameEn'],
          com['commodityNameVn'],
          com['commonityGroupNameEn'],
          (com['inactive']===true)?"Inactive":"Active"
        ]
      }); 
    }

    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.VIETNAM_API){
      commodities = lodash.map(commodities,function(com,index){
        return [
          index+1,
          com['code'],
          com['commodityNameEn'],
          com['commodityNameVn'],
          com['commonityGroupNameVn'],
          (com['inactive']===true)?"Ngưng Hoạt Động":"Đang Hoạt Động"
        ]
      });
    }
    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Commodity List";
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.author = currrently_user;
    exportModel.header = [
      {name:"STT",width:10},
      {name:"Code",width:20},
      {name:"Name EN",width:20},
      {name:"Name VN",width:20},
      {name:"Commodity Group",width:30},
      {name:"Inactive",width:20}
    ]
    exportModel.data = commodities;
    exportModel.fileName = "Commodity";
    
    this.excelService.generateExcel(exportModel);

  }

  async importComGroup(){
    
  }

  async exportComGroup(){
    var commodities_group = await this.baseService.postAsync(this.api_menu.Catalogue.CommodityGroup.query,this.criteria);
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.ENGLISH_API){
      commodities_group = lodash.map(commodities_group,function(com_group,index){
        return [
          index+1,         
          com_group['groupNameEn'],
          com_group['groupNameVn'],         
          (com_group['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      }); 
    }

    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)===SystemConstants.LANGUAGES.VIETNAM_API){      
      commodities_group = lodash.map(commodities_group,function(com_group,index){
        return [
          index+1,         
          com_group['groupNameEn'],
          com_group['groupNameVn'],        
          (com_group['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }

    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Commodity Group List";
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.author = currrently_user;
    exportModel.header = [
      {name:"No.",width:10},      
      {name:"Name EN",width:20},
      {name:"Name Local",width:20},   
      {name:"Inactive",width:20}
    ]
    exportModel.data = commodities_group;
    exportModel.fileName = "Commodity Group";
    
    this.excelService.generateExcel(exportModel);

  }

}
