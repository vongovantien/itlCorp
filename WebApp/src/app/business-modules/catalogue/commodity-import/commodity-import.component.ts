import { Component, OnInit, ViewChild } from '@angular/core';
import { NgProgress, NgProgressComponent } from '@ngx-progressbar/core';
import { PagingService } from 'src/app/shared/common/pagination/paging-service';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { SystemConstants } from 'src/constants/system.const';
import { language } from 'src/languages/language.en';
declare var $: any;

@Component({
  selector: 'app-commodity-import',
  templateUrl: './commodity-import.component.html',
  styleUrls: ['./commodity-import.component.scss']
})
export class CommodityImportComponent implements OnInit {
  data: any[];
  pagedItems: any[] = [];
  inValidItems: any[] = [];
  totalValidRows: number = 0;
  totalRows: number = 0;
  isShowInvalid: boolean = true;
  pager: PagerSetting = PAGINGSETTING;
  constructor(
    public ngProgress: NgProgress,
    private pagingService: PagingService,
    private baseService: BaseService,
    private menu_api: API_MENU,
    private sortService: SortService
  ) { }

  @ViewChild(PaginationComponent,{static:false}) child: any;
  @ViewChild('form',{static:false}) form: any;
  @ViewChild(NgProgressComponent,{static:false}) progressBar: NgProgressComponent;
  ngOnInit() {

  }
  chooseFile(file: Event) {
    if (!this.baseService.checkLoginSession()) return;
    if (file.target['files'] == null) return;
    this.progressBar.start();
    this.baseService.uploadfile(this.menu_api.Catalogue.Commodity.uploadFile, file.target['files'], "uploadedFile")
      .subscribe(res => {
        this.data = res['data'];
        this.pager.totalItems = this.data.length;
        this.totalValidRows = res['totalValidRows'];
        this.totalRows = this.data.length;
        this.pagingData(this.data);
        this.progressBar.complete();
      }, err => {
        this.progressBar.complete();
        this.baseService.handleError(err);
      });
  }

  pagingData(data: any[]) {
    this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    console.log(this.pagedItems);
  }

  async setPage(pager: PagerSetting) {
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    if (this.isShowInvalid) {
      this.pager = this.pagingService.getPager(this.data.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
      this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
      this.pagedItems = this.data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }
    else {
      this.pager = this.pagingService.getPager(this.inValidItems.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
      this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
      this.pagedItems = this.inValidItems.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }
  }

  isDesc = true;
  sortKey: string;
  sort(property: string) {
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
  }

  hideInvalid() {
    if (this.data == null) return;
    this.isShowInvalid = !this.isShowInvalid;
    this.sortKey = '';
    if (this.isShowInvalid) {
      this.pager.totalItems = this.data.length;
    }
    else {
      this.inValidItems = this.data.filter(x => !x.isValid);
      this.pager.totalItems = this.inValidItems.length;
    }
    this.child.setPage(this.pager.currentPage);
  }


  async import() {
    if (this.data == null) return;
    if (this.totalRows - this.totalValidRows > 0) {
      $('#upload-alert-modal').modal('show');
    }
    else {     
      let validItems = this.data.filter(x => x.isValid);
      if (!this.baseService.checkLoginSession()) return;
      var response = await this.baseService.postAsync(this.menu_api.Catalogue.Commodity.import, validItems);
      if (response) {
        this.baseService.successToast(language.NOTIFI_MESS.IMPORT_SUCCESS);
        this.pager.totalItems = 0;
        this.reset();
      }
    }

  }


  reset() {
    this.data = null;
    this.pagedItems = null;
    $("#inputFile").val('');
    this.pager.totalItems = 0;
  }


  async downloadSample() {
    await this.baseService.downloadfile(this.menu_api.Catalogue.Commodity.downloadExcel,'CommodityTemplate.xlsx');
  }

}
