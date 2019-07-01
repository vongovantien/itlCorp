import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagingService } from 'src/app/shared/common/pagination/paging-service';
import { SystemConstants } from 'src/constants/system.const';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import * as lodash from 'lodash';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { SortService } from 'src/app/shared/services/sort.service';

@Component({
  selector: 'app-billing-custom-declaration',
  templateUrl: './billing-custom-declaration.component.html',
  styleUrls: ['./billing-custom-declaration.component.scss']
})
export class BillingCustomDeclarationComponent implements OnInit {
  currentJob: OpsTransaction;
  notImportedCustomClearances: any[];
  customClearances: any[];
  pager: PagerSetting = PAGINGSETTING;
  pagerMaster: PagerSetting = PAGINGSETTING;
  notImportedData: any[];
  importedData: any[];
  searchString: string = '';
  searchImportedString: string = '';
  checkAllNotImported = false;
  checkAllImported = false;
  dataNotImportedSearch: any[];
  dataImportedSearch: any[];

  constructor(private baseServices: BaseService,
    private api_menu: API_MENU,
    private pagerService: PagingService,
    private cdr: ChangeDetectorRef,
    private sortService: SortService) { }

  async ngOnInit() {
    this.pager.currentPage = 1;
    this.pager.totalItems = 0;
    await this.stateChecking();
  }

  async getCustomClearanesOfJob(jobNo: string) {
    this.importedData = await this.baseServices.getAsync(this.api_menu.ToolSetting.CustomClearance.getByJob + "?jobNo=" + jobNo, false, true);
    if(this.importedData != null){
      this.importedData.forEach(element => {
        element.isChecked = false;
      });
    }
    this.dataImportedSearch = this.importedData;
    this.setPageMaster(this.pager);
  }

  async stateChecking() {
    this.baseServices.spinnerShow();
    setTimeout(() => {
      this.baseServices.dataStorage.subscribe(data => {
        if(data["CurrentOpsTransaction"] != null){
          this.currentJob = data["CurrentOpsTransaction"];
          
          if(this.currentJob != null){
            this.getCustomClearanesOfJob(this.currentJob.jobNo);
            this.getCustomClearancesNotImported();
          }
        }
      }); 
    }, 1000);
    
    this.baseServices.spinnerHide();
  }
  removeChecked(){
    this.checkAllImported = false;
    let checkedData = this.importedData.filter(x => x.isChecked == true);
    if(checkedData.length >0){
      for(let i =0; i< checkedData.length; i++){
        let index = this.importedData.indexOf(x => x.id == checkedData[i].id);
        if(index > -1){
          this.importedData[index] = true;
        }
      }
    }
  }
  isDesc = true;
  sortKey: string = "";
  sort(property, isImported = true) {
      this.isDesc = !this.isDesc;
      this.sortKey = property;
      if(isImported){
        this.customClearances = this.sortService.sort(this.customClearances, property, this.isDesc);
      }
      else{
        this.notImportedCustomClearances = this.sortService.sort(this.notImportedCustomClearances, property, this.isDesc);
      }
  }
  async removeImported(){
    let dataToUpdate = this.importedData.filter(x => x.isChecked == true);
    if(dataToUpdate.length > 0){
      dataToUpdate.forEach(x =>{
        x.jobNo = null;
      });
      let responses = await this.baseServices.postAsync(this.api_menu.ToolSetting.CustomClearance.updateToAJob, dataToUpdate, false, true);
      if(responses.success == true){
        await this.getCustomClearanesOfJob(this.currentJob.jobNo);
        this.updateShipmentVolumn();
        this.getCustomClearancesNotImported();
      }
    }
  }
  closePopUp(){
    this.searchString = '';
    this.pager.totalItems = 0;
    this.pager.currentPage = 1;
    this.getCustomClearancesNotImported();
  }
  showPopupAdd(){
    this.pager.totalItems = 0;
    this.pager.currentPage = 1;
    if(this.dataNotImportedSearch != null){
      this.cdr.detectChanges();
      this.setPage(this.pager);
    }
  }
  async getCustomClearancesNotImported() {
    this.notImportedData = await this.baseServices.postAsync(this.api_menu.ToolSetting.CustomClearance.query, { "imPorted": false }, false, true);
    if(this.notImportedData != null){
      this.notImportedData.forEach(element => {
        element.isChecked = false;
      });
    }
    this.dataNotImportedSearch = this.notImportedData;
    this.setPage(this.pager);
  }
  changeAllImported(){
    if(this.checkAllImported){
      this.customClearances.forEach(x => {
        x.isChecked = true;
      });
    }
    else{
      this.customClearances.forEach(x => {
        x.isChecked = false;
      });
    }
    let checkedData = this.customClearances.filter(x => x.isChecked == true);
    if(checkedData.length >0){
      for(let i =0; i< checkedData.length; i++){
        let index = this.importedData.indexOf(x => x.id == checkedData[i].id);
        if(index > -1){
          this.importedData[index] = true;
        }
      }
    }
  }
  changeAllNotImported(){
    if(this.checkAllNotImported){
      this.notImportedCustomClearances.forEach(x =>{
          x.isChecked = true;
      });
    }
    else{
      this.notImportedCustomClearances.forEach(x =>{
          x.isChecked = false;
      });
    }
    let checkedData = this.notImportedCustomClearances.filter(x => x.isChecked == true);
    if(checkedData.length >0){
      for(let i =0; i< checkedData.length; i++){
        let index = this.notImportedData.indexOf(x => x.id == checkedData[i].id);
        if(index > -1){
          this.notImportedData[index] = true;
        }
      }
    }
  }
  async updateJobToClearance(){
    let dataToUpdate = this.notImportedData.filter(x => x.isChecked == true);
    if(dataToUpdate.length > 0){
      dataToUpdate.forEach(x =>{
        x.jobNo = this.currentJob.jobNo;
      });
      let responses = await this.baseServices.postAsync(this.api_menu.ToolSetting.CustomClearance.updateToAJob, dataToUpdate, false, true);
      if(responses.success == true){
        await this.getCustomClearanesOfJob(this.currentJob.jobNo);
        this.updateShipmentVolumn();
        this.getCustomClearancesNotImported();
        this.setPageMaster(this.pagerMaster);
      }
    }
  }
  async updateShipmentVolumn(){
    if(this.importedData.length > 0){
      this.currentJob.sumGrossWeight = 0;
      this.currentJob.sumNetWeight = 0;
      this.currentJob.sumCbm = 0;
      for(let i=0; i< this.importedData.length; i++){
        this.currentJob.sumGrossWeight = this.currentJob.sumGrossWeight + this.importedData[i].grossWeight==null?0: this.importedData[i].grossWeight;
        this.currentJob.sumNetWeight = this.currentJob.sumNetWeight + this.importedData[i].netWeight==null?0: this.importedData[i].netWeight;
        this.currentJob.sumCbm = this.currentJob.sumCbm + this.importedData[i].sumCbm==null?0: this.importedData[i].sumCbm;
      }
      await this.baseServices.putAsync(this.api_menu.Documentation.Operation.update, this.currentJob, false, false);
    }
  }
  removeAllChecked(){
    this.checkAllNotImported = false;
    let checkedData = this.notImportedCustomClearances.filter(x => x.isChecked == true);
    if(checkedData.length >0){
      for(let i =0; i< checkedData.length; i++){
        let index = this.notImportedData.indexOf(x => x.id == checkedData[i].id);
        if(index > -1){
          this.notImportedData[index] = true;
        }
      }
    }
  }
  refreshData(){
    this.searchString = '';
    this.getCustomClearancesNotImported();
    this.pager.totalItems = 0;
    this.pager.currentPage = 1;
    this.setPage(this.pager);
  }
  setPageMaster(pager: PagerSetting){
    this.pagerMaster = this.pagerService.getPager(this.dataImportedSearch.length, pager.currentPage, this.pagerMaster.pageSize, this.pagerMaster.totalPageBtn);
    this.pagerMaster.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pagerMaster.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.customClearances = this.dataImportedSearch.slice(this.pagerMaster.startIndex, this.pagerMaster.endIndex + 1);
  }
  setPage(pager: PagerSetting){
    this.pager = this.pagerService.getPager(this.dataNotImportedSearch.length, pager.currentPage, this.pager.pageSize, this.pager.totalPageBtn);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.notImportedCustomClearances = this.dataNotImportedSearch.slice(this.pager.startIndex, this.pager.endIndex + 1);
  }
  searchClearanceNotImported(event){
    this.pager.totalItems = 0;
    this.cdr.detectChanges();
    let keySearch = this.searchString.trim().toLocaleLowerCase();
    if(keySearch!==null && keySearch.length<2 && keySearch.length>0){
      return 0;
    }
    this.dataNotImportedSearch = this.notImportedData.filter(item => item.clearanceNo.includes(keySearch)
                                                || (item.hblid == null? '': item.hblid.toLocaleLowerCase()).includes(keySearch)
                                                || (item.exportCountryCode == null? '': item.exportCountryCode.toLocaleLowerCase()).includes(keySearch)
                                                || (item.importCountryCode == null? '': item.importCountryCode.toLocaleLowerCase()).includes(keySearch)
                                                || (item.commodityCode == null? '': item.commodityCode.toLocaleLowerCase()).includes(keySearch)
                                                || (item.firstClearanceNo == null? '': item.firstClearanceNo.toLocaleLowerCase()).includes(keySearch)
                                                || (item.qtyCont == null? '': item.qtyCont.toString()).includes(keySearch)
                                          );
    this.pager.currentPage = 1;
    this.pager.totalItems = this.dataNotImportedSearch.length;
    this.setPage(this.pager);
  }
  searchClearanceImported(event){
    this.pagerMaster.totalItems = 0;
    let keySearch = this.searchImportedString.trim().toLocaleLowerCase();
    if(keySearch!==null && keySearch.length<2 && keySearch.length>0){
      return 0;
    }
    this.dataImportedSearch = this.importedData.filter(item => item.clearanceNo.includes(keySearch)
      || (item.hblid == null? '': item.hblid.toLocaleLowerCase()).includes(keySearch)
      || (item.exportCountryCode == null? '': item.exportCountryCode.toLocaleLowerCase()).includes(keySearch)
      || (item.importCountryCode == null? '': item.importCountryCode.toLocaleLowerCase()).includes(keySearch)
      || (item.commodityCode == null? '': item.commodityCode.toLocaleLowerCase()).includes(keySearch)
      || (item.firstClearanceNo == null? '': item.firstClearanceNo.toLocaleLowerCase()).includes(keySearch)
      || (item.qtyCont == null? '': item.qtyCont.toString()).includes(keySearch));
    this.pagerMaster.currentPage = 1;
    this.pagerMaster.totalItems = this.dataImportedSearch.length;
    this.setPageMaster(this.pagerMaster);
  }
}
