import { Component, OnInit, ViewChild } from '@angular/core';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
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

@Component({
  selector: 'app-warehouse-import',
  templateUrl: './warehouse-import.component.html',
  styleUrls: ['./warehouse-import.component.scss']
})
export class WarehouseImportComponent implements OnInit {
  file: File;
  data: any[];
  pagedItems: any[];
  validRows: number = 0;
  inValidRows: number = 0;
  totalRows: number = 0;
  WarehouseImportSettings: ColumnSetting[] = WAREHOUSEIMPORTENCOLUMNSETTING;
  pager: PagerSetting = PAGINGSETTING;
  @ViewChild(PaginationComponent) child;

  constructor(private spinnerService: Ng4LoadingSpinnerService,
    private pagingService: PagingService,
    private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  chooseFile(file: Event){
    this.file = file.target['files'];
    this.baseService.uploadfile(this.api_menu.Catalogue.CatPlace.uploadExel, this.file, "uploadedFile")
      .subscribe((response: any) => {
        this.data = response.data;
        this.pager.totalItems = this.data.length;
        this.validRows = response.validRows;
        this.totalRows = this.data.length;
        this.inValidRows = this.totalRows - this.validRows;
        this.pagingData();
        console.log(this.data);
      });
  }
  pagingData(){
    this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage);
    this.pager.pageSize = SystemConstants.OPTIONS_PAGE_SIZE;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pagedItems = this.data.slice(this.pager.startIndex, this.pager.endIndex + 1);
  }
  downloadSample(){
    this.baseService.downloadfile(this.api_menu.Catalogue.CatPlace.downloadExcel + "?type=12")
    .subscribe(
      response => {
        saveAs(response, 'WarehouseTemplate.xlsx');
      }
    )
  }
  async import(){
    await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.import, this.data, false, true);
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
    this.pagingData();
  }
}
