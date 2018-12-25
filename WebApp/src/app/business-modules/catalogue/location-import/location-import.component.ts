import { Component, OnInit, ViewChild } from '@angular/core';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagingService } from 'src/app/shared/common/pagination/paging-service';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { SystemConstants } from 'src/constants/system.const';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { language } from 'src/languages/language.en';
import { ActivatedRoute } from '@angular/router';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';

declare var $:any;
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
  type: string;

  @ViewChild('form') form;
  @ViewChild(PaginationComponent) child;
  constructor(
    private pagingService: PagingService,
    private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService,
    private route:ActivatedRoute
    ) { }

  ngOnInit() {
    this.pager.totalItems = 0;
    this.route.queryParams.subscribe(prams => {
      console.log(prams["type"]);
      if(prams.type != undefined){
        this.type = prams.type;
      }
    });
  }
  chooseFile(file: Event){
    if(!this.baseService.checkLoginSession()) return;
    if(file.target['files'] == null) return;
    this.baseService.spinnerShow();
    let url = '';
    if(this.type == 'province'){
      url = this.api_menu.Catalogue.CatPlace.uploadExel + "?type=" + PlaceTypeEnum.Province;
    }
    if(this.type == 'district'){
      url = this.api_menu.Catalogue.CatPlace.uploadExel + "?type=" + PlaceTypeEnum.District;
    }
    if(this.type == 'ward'){
      url = this.api_menu.Catalogue.CatPlace.uploadExel + "?type=" + PlaceTypeEnum.Ward;
    }
    if(this.type == 'country'){
      url = this.api_menu.Catalogue.Country.uploadExel;
    }
    this.baseService.uploadfile(url, file.target['files'], "uploadedFile")
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
        this.baseService.spinnerHide();
        this.baseService.handleError(err);
      });
  }
  pagingData(data: any[]){
    this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
  }
  downloadSample(){
    let url = '';
    if(this.type == 'province'){
      url = this.api_menu.Catalogue.CatPlace.downloadExcel + "?type=" + PlaceTypeEnum.Province;
    }
    if(this.type == 'district'){
      url = this.api_menu.Catalogue.CatPlace.downloadExcel + "?type=" + PlaceTypeEnum.District;
    }
    if(this.type == 'ward'){
      url = this.api_menu.Catalogue.CatPlace.downloadExcel + "?type=" + PlaceTypeEnum.Ward;
    }
    if(this.type == 'country'){
      url = this.api_menu.Catalogue.Country.downloadExcel;
    }
    this.baseService.downloadfile(url)
    .subscribe(
      response => {
        saveAs(response, this.type + 'ImportTemplate.xlsx');
      }
    ),err=>{
      this.baseService.handleError(err);
    }
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
  hideInvalid(){
    if(this.data == null) return;
    this.isShowInvalid = !this.isShowInvalid;
    //this.sortKey = '';
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
    if(this.totalInValidRows > 0){
      $('#upload-alert-modal').modal('show');
    }
    else{
      this.inProgress = true;
      let data = this.data.filter(x => x.isValid);
      if(!this.baseService.checkLoginSession()) return;
      let url = '';
    if(this.type != 'province'){
      url = this.api_menu.Catalogue.Country.downloadExcel + "?type=" + PlaceTypeEnum.Province;
    }
    if(this.type != 'district'){
      url = this.api_menu.Catalogue.Country.downloadExcel + "?type=" + PlaceTypeEnum.District;
    }
    if(this.type != 'ward'){
      url = this.api_menu.Catalogue.Country.downloadExcel + "?type=" + PlaceTypeEnum.Ward;
    }
    if(this.type != 'country'){
      url = this.api_menu.Catalogue.Country.downloadExcel;
    }
      var response = await this.baseService.postAsync(url, data, true, false);
      if(response){
        this.baseService.successToast(language.NOTIFI_MESS.EXPORT_SUCCES);
        this.inProgress = false;
        this.pager.totalItems = 0;
        this.reset();
      }
      console.log(response);
    }
  }
  
  reset(){
    this.data = null;
    this.pagedItems = null;
    $("#inputFile").val('');
    //this.form.nativeElement.onReset();
    this.pager.totalItems = 0;
  }
  isDesc = true;
  sortKey: string;
  sort(property){
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
  }
}
