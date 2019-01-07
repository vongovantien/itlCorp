import { Component, OnInit, ViewChild } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { saveAs } from 'file-saver';
import { SortService } from 'src/app/shared/services/sort.service';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { PagingService } from 'src/app/shared/common/pagination/paging-service';
import { SystemConstants } from 'src/constants/system.const';
import { language } from 'src/languages/language.en';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { NgProgressComponent } from '@ngx-progressbar/core';
declare var $:any;

@Component({
  selector: 'app-warehouse-import',
  templateUrl: './warehouse-import.component.html',
  styleUrls: ['./warehouse-import.component.scss']
})
export class WarehouseImportComponent implements OnInit {
  data: any[];
  pagedItems: any[] = [];
  inValidItems: any[] = [];
  totalValidRows: number = 0;
  totalRows: number = 0;
  isShowInvalid: boolean = true;
  pager: PagerSetting = PAGINGSETTING;
  inProgress: boolean = false;
  @ViewChild('form') form:any;
  @ViewChild(PaginationComponent) child:any;
  @ViewChild(NgProgressComponent) progressBar: NgProgressComponent;
  closeButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.cancel,
    buttonAttribute: {
      titleButton: "close",
      classStyle: "btn m-btn--square m-btn--icon m-btn--uppercase",
      icon: "la la-ban"
    }
  };

  constructor(
    private pagingService: PagingService,
    private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
    this.pager.totalItems = 0;
  }
  chooseFile(file: Event){
    if(!this.baseService.checkLoginSession()) return;
    if(file.target['files'] == null) return;
    this.progressBar.start();
    this.baseService.uploadfile(this.api_menu.Catalogue.CatPlace.uploadExel + "?type=" + PlaceTypeEnum.Warehouse, file.target['files'], "uploadedFile")
      .subscribe((response: any) => {
        this.data = response.data;
        this.pager.totalItems = this.data.length;
        this.totalValidRows = response.totalValidRows;
        this.totalRows = this.data.length;
        this.pagingData(this.data);
        this.progressBar.complete();
      },err=>{
        this.progressBar.complete();
        this.baseService.handleError(err);
      });
  }
  pagingData(data: any[]){
    this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    console.log(this.pager);
  }
  async downloadSample(){
    await this.baseService.downloadfile(this.api_menu.Catalogue.CatPlace.downloadExcel + "?type=12",'WarehouseImportTemplate.xlsx');
  }
  
  hideInvalid(){
    if(this.data == null) return;
    this.isShowInvalid = !this.isShowInvalid;
    this.sortKey = '';
    if(this.isShowInvalid){
      this.pager.totalItems = this.data.length;
    }
    else{
      this.inValidItems = this.data.filter(x => !x.isValid);
      this.pager.totalItems = this.inValidItems.length;
    }
    this.child.setPage(this.pager.currentPage);
  }
  async import(){
    if(this.data == null) return;
    if(this.totalRows - this.totalValidRows > 0){
      $('#upload-alert-modal').modal('show');
    }
    else{     
      let data = this.data.filter(x => x.isValid);
      if(!this.baseService.checkLoginSession()) return;
      var response = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.import, data);
      if(response){
        this.baseService.successToast(language.NOTIFI_MESS.IMPORT_SUCCESS);        
        this.pager.totalItems = 0;
        this.reset();
      }
    }
  }
  isDesc = true;
  sortKey: string;
  sort(property: string){
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
  }
  async setPage(pager:PagerSetting){
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    if(this.isShowInvalid){
      this.pager = this.pagingService.getPager(this.data.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
      this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
      this.pagedItems = this.data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }
    else{
      this.pager = this.pagingService.getPager(this.inValidItems.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
      this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
      this.pagedItems = this.inValidItems.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }
  }
  reset(){
    this.data = null;
    this.pagedItems = null;
    $("#inputFile").val('');
    this.form.onReset();
    this.pager.totalItems = 0;
  }
}
