import { Component, OnInit, ViewChild } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { saveAs } from 'file-saver';
import { WAREHOUSEIMPORTENCOLUMNSETTING } from './warehouse-import.columns';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { PagingService } from 'src/app/shared/common/pagination/paging-service';
import { SystemConstants } from 'src/constants/system.const';
declare var $:any;

@Component({
  selector: 'app-warehouse-import',
  templateUrl: './warehouse-import.component.html',
  styleUrls: ['./warehouse-import.component.scss']
})
export class WarehouseImportComponent implements OnInit {
  file: File;
  data: any[];
  pagedItems: any[] = [];
  inValidItems: any[] = [];
  validRows: number = 0;
  inValidRows: number = 0;
  totalRows: number = 0;
  isShowInvalid: boolean = true;
  WarehouseImportSettings: ColumnSetting[] = WAREHOUSEIMPORTENCOLUMNSETTING;
  pager: PagerSetting = PAGINGSETTING;
  inProgress: boolean = false;
  @ViewChild('form') form;
  @ViewChild(PaginationComponent) child;

  constructor(
    private pagingService: PagingService,
    private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
    this.pager.totalItems = 0;
  }
  chooseFile(file: Event){
    this.file = file.target['files'];
    this.baseService.spinnerShow();
    this.baseService.uploadfile(this.api_menu.Catalogue.CatPlace.uploadExel, this.file, "uploadedFile")
      .subscribe((response: any) => {
        this.data = response.data;
        this.pager.currentPage = 1;
        this.pager.totalItems = this.data.length;
        this.validRows = response.validRows;
        this.totalRows = this.data.length;
        this.inValidRows = this.totalRows - this.validRows;
        this.pagingData(this.data);
        this.baseService.spinnerHide();
        console.log(this.data);
      });
  }
  pagingData(data: any[]){
    //this.pager.pageSize = SystemConstants.OPTIONS_PAGE_SIZE;
    this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    //this.pager.totalItems = responses.totalItems;
  }
  downloadSample(){
    this.baseService.downloadfile(this.api_menu.Catalogue.CatPlace.downloadExcel + "?type=12")
    .subscribe(
      response => {
        saveAs(response, 'WarehouseTemplate.xlsx');
      }
    )
  }
  hideInvalid(){
    if(this.data == null) return;
    this.isShowInvalid = !this.isShowInvalid;
    this.sortKey = '';
    if(this.isShowInvalid){
      this.pagingData(this.data);
    }
    else{
      this.inValidItems = this.data.filter(x => !x.isValid);
      this.pager.totalItems = this.inValidItems.length;
      this.pagingData(this.inValidItems);
    }
  }
  async import(){
    if(this.data == null) return;
    if(this.inValidRows > 0){
      $('#upload-alert-modal').modal('show');
    }
    else{
      this.inProgress = true;
      let validItems = this.data.filter(x => x.isValid);
      var response = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.import, validItems, true, false);
      if(response.success){
        this.inProgress = false;
        this.pager.totalItems = 0;
        this.reset();
      }
      console.log(response);
    }
  }
  isDesc = true;
  sortKey: string;
  sort(property){
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
  }
  async setPage(pager:PagerSetting){
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pagedItems = this.data.slice(this.pager.startIndex, this.pager.endIndex + 1);
  }
  reset(){
    this.data = null;
    this.pagedItems = null;
    $("#inputFile").val('');
    this.form.onReset();
    this.pager.totalItems = 0;
  }
}
