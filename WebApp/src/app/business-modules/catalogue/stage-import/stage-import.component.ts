import { Component, OnInit } from '@angular/core';
import { PagingService } from 'src/app/shared/common/pagination/paging-service';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { SystemConstants } from 'src/constants/system.const';

@Component({
  selector: 'app-stage-import',
  templateUrl: './stage-import.component.html',
  styleUrls: ['./stage-import.component.scss']
})
export class StageImportComponent implements OnInit {
  data:any[];
  pagedItems: any[]=[];
  inValidItems: any[]=[];
  totalValidRows: number = 0;
  totalInValidRows: number = 0;
  totalRows: number = 0;
  isShowInvalid:boolean = true;
  pager:PagerSetting = PAGINGSETTING;
  inProgress:boolean = false;

  constructor(
    private pagingService: PagingService,
    private baseService: BaseService,
    private menu_api: API_MENU,
    private sortService: SortService
  ) { }

  ngOnInit() {
    this.pager.totalItems = 0;
  }

  chooseFile(file:Event){
   // if(!this.baseService.checkLoginSession()) return;
    if(file.target['files']==null) return;
    this.baseService.spinnerShow();
    this.baseService.uploadfile(this.menu_api.Catalogue.Stage_Management.uploadExel, file.target['files'], "uploadedFile")
        .subscribe(res=>{
          this.data = res['data'];
          this.pager.totalItems = this.data.length;
          this.totalValidRows = res['totalValidRows'];
          this.totalRows = this.data.length;
          this.totalInValidRows = this.totalRows - this.totalValidRows;
          this.pagingData(this.data);
          this.baseService.spinnerHide();
          console.log(this.data);
        },err=>{
          this.baseService.spinnerHide();
          this.baseService.handleError(err);
        })
  }

  pagingData(data: any[]){
    this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    console.log(this.pagedItems);
  }



}
