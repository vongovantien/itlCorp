import { Component, OnInit } from '@angular/core';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagingService } from 'src/app/shared/common/pagination/paging-service';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { SystemConstants } from 'src/constants/system.const';

@Component({
  selector: 'app-location-import',
  templateUrl: './location-import.component.html',
  styleUrls: ['./location-import.component.scss']
})
export class LocationImportComponent implements OnInit {
  data: any[];
  pagedItems: any[] = [];
  inValidItems: any[] = [];
  totalValidRows: number = 0;
  totalInValidRows: number = 0;
  totalRows: number = 0;
  isShowInvalid: boolean = true;
  pager: PagerSetting = PAGINGSETTING;
  inProgress: boolean = false;
  constructor(
    private pagingService: PagingService,
    private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  chooseFile(file: Event){
    if(!this.baseService.checkLoginSession()) return;
    if(file.target['files'] == null) return;
    this.baseService.spinnerShow();
    this.baseService.uploadfile(this.api_menu.Catalogue.Country.uploadExel, file.target['files'], "uploadedFile")
      .subscribe((response: any) => {
        this.data = response.data;
        this.pager.totalItems = this.data.length;
        this.totalValidRows = response.totalValidRows;
        this.totalRows = this.data.length;
        this.totalInValidRows = this.totalRows - this.totalValidRows;
        this.pagingData(this.data);
        this.baseService.spinnerHide();
        console.log(this.data);
      },err=>{
        this.baseService.handleError(err);
      });
  }
  pagingData(data: any[]){
    this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
  }
}
