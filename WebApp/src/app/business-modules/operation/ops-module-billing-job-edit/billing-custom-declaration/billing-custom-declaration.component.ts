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
  checkAllNotImported = false;
  checkAllImported = false;

  constructor(private baseServices: BaseService,
    private api_menu: API_MENU,
    private pagerService: PagingService,
    private cdr: ChangeDetectorRef) { }

  async ngOnInit() {
    await this.stateChecking();
  }

  async getCustomClearanesOfJob(id: string) {
    this.importedData = await this.baseServices.getAsync(this.api_menu.ToolSetting.CustomClearance.getByJob + id, false, true);
    this.setPageMaster(this.pager);
  }

  stateChecking() {
    setTimeout(() => {
      this.baseServices.dataStorage.subscribe(data => {
        if(data["CurrentOpsTransaction"] != null){
          this.currentJob = data["CurrentOpsTransaction"];
          
          if(this.currentJob != null){
            this.getCustomClearanesOfJob(this.currentJob.id);
            this.getCustomClearancesNotImported();
          }
        }
      }); 
    }, 1000);
  }
  showPopupAdd(){
    this.setPage(this.pager);
  }
  async getCustomClearancesNotImported() {
    this.notImportedData = await this.baseServices.postAsync(this.api_menu.ToolSetting.CustomClearance.query, { "imPorted": false }, false, true);
    if(this.notImportedData != null){
      this.notImportedData.forEach(element => {
        element.isChecked = false;
      });
    }
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
      if(responses == true){
        this.checkAllNotImported = false;
        this.changeAllNotImported();
      }
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
    this.getCustomClearancesNotImported();
    this.pager.totalItems = 0;
    this.pager.currentPage = 1;
    this.setPage(this.pager);
  }
  setPageMaster(pager: PagerSetting){
    this.pagerMaster = this.pagerService.getPager(this.importedData.length, pager.currentPage, this.pagerMaster.pageSize, this.pagerMaster.totalPageBtn);
    this.pagerMaster.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pagerMaster.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.customClearances = this.importedData.slice(this.pagerMaster.startIndex, this.pagerMaster.endIndex + 1);
  }
  setPage(pager: PagerSetting){
    this.pager = this.pagerService.getPager(this.notImportedData.length, pager.currentPage, this.pager.pageSize, this.pager.totalPageBtn);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.notImportedCustomClearances = this.notImportedData.slice(this.pager.startIndex, this.pager.endIndex + 1);
  }
  searchClearanceNotImported(event){
    this.pager.totalItems = 0;
    let keySearch = this.searchString.trim().toLocaleLowerCase();
    if(keySearch!==null && keySearch.length<3 && keySearch.length>0){
      return 0;
    }
    var data = this.notImportedData.filter(item => item.clearanceNo.includes(keySearch)
                                                || item.datetimeCreated.toString().includes(keySearch)
                                          );
    this.pager = this.pagerService.getPager(data.length, 1, this.pager.pageSize, this.pager.totalPageBtn);
    this.notImportedCustomClearances = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    
    this.cdr.detectChanges();
  }
}
