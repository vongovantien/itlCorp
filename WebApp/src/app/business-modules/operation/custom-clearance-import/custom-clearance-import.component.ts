import { Component, OnInit, ViewChild } from '@angular/core';
import { PagingService } from 'src/app/shared/common/pagination/paging-service';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgProgressComponent } from '@ngx-progressbar/core';
import { SystemConstants } from 'src/constants/system.const';

@Component({
  selector: 'app-custom-clearance-import',
  templateUrl: './custom-clearance-import.component.html',
  styleUrls: ['./custom-clearance-import.component.scss']
})
export class CustomClearanceImportComponent implements OnInit {
  data: any[];
  pagedItems: any[] = [];
  inValidItems: any[] = [];
  totalValidRows: number = 0;
  totalRows: number = 0;
  isShowInvalid: boolean = true;
  pager: PagerSetting = PAGINGSETTING;
  inProgress: boolean = false;
  @ViewChild(PaginationComponent, { static: false }) child;
  @ViewChild(NgProgressComponent, { static: false }) progressBar: NgProgressComponent;
  constructor(
    private pagingService: PagingService,
    private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
    this.pager.totalItems = 0;
  }

  chooseFile(file: Event) {
    if (file.target['files'] == null) return;
    this.progressBar.start();
    this.baseService.uploadfile(this.api_menu.ToolSetting.CustomClearance.uploadExel, file.target['files'], "uploadedFile")
      .subscribe((response: any) => {
        console.log(response);
        this.data = response.data;
        this.pager.totalItems = this.data.length;
        this.totalValidRows = response.totalValidRows;
        this.totalRows = this.data.length;
        this.pagingData(this.data);
        this.progressBar.complete();
        console.log(this.data);
        localStorage.setItem("listData", JSON.stringify(this.data));
      }, err => {
        this.progressBar.complete();
        this.baseService.handleError(err);
      });
  }

  pagingData(data: any[]) {
    this.pager = this.pagingService.getPager(data.length, this.pager.currentPage, this.pager.pageSize);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
  }

  setPage(pager: PagerSetting) {
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    console.log(JSON.parse(localStorage.getItem("listData")));
    this.pagingData(JSON.parse(localStorage.getItem("listData")));
  }

  async downloadSample() {
    await this.baseService.downloadfile(this.api_menu.ToolSetting.CustomClearance.downloadExcel, 'CustomClearanceImportTemplate.xlsx');
  }
}
