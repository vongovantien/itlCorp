import { Component, OnInit, ViewChild } from '@angular/core';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { API_MENU } from 'src/constants/api-menu.const';
import { SystemConstants } from 'src/constants/system.const';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { language } from 'src/languages/language.en';
import { ActivatedRoute } from '@angular/router';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { NgProgressComponent } from '@ngx-progressbar/core';
import { PagingService, BaseService, SortService } from 'src/app/shared/services';
import { InfoPopupComponent } from 'src/app/shared/common/popup';

@Component({
  selector: 'app-location-import',
  templateUrl: './location-import.component.html'
})
export class LocationImportComponent implements OnInit {
  data: any[];
  pagedItems: any[] = [];
  inValidItems: any[] = [];
  totalValidRows: number = 0;
  totalRows: number = 0;
  isShowInvalid: boolean = true;
  pager: PagerSetting = PAGINGSETTING;
  type: string;
  inputFile: string;
  isDesc = true;
  sortKey: string;

  @ViewChild('form', { static: false }) form: any;
  @ViewChild(PaginationComponent, { static: false }) child: any;
  @ViewChild(NgProgressComponent, { static: false }) progressBar: NgProgressComponent;
  @ViewChild(InfoPopupComponent, { static: false }) invaliDataAlert: InfoPopupComponent;
  constructor(
    private pagingService: PagingService,
    private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService,
    private route: ActivatedRoute
  ) { }

  ngOnInit() {
    this.pager.totalItems = 0;
    this.route.queryParams.subscribe(prams => {
      console.log(prams["type"]);
      if (prams.type !== undefined) {
        this.type = prams.type;
      }
    });
  }
  chooseFile(file: Event) {
    if (file.target['files'] == null) { return; }
    this.progressBar.start();
    let url = '';
    if (this.type === 'province') {
      url = this.api_menu.Catalogue.CatPlace.uploadExel + "?type=" + PlaceTypeEnum.Province;
    }
    if (this.type === 'district') {
      url = this.api_menu.Catalogue.CatPlace.uploadExel + "?type=" + PlaceTypeEnum.District;
    }
    if (this.type === 'ward') {
      url = this.api_menu.Catalogue.CatPlace.uploadExel + "?type=" + PlaceTypeEnum.Ward;
    }
    if (this.type === 'country') {
      url = this.api_menu.Catalogue.Country.uploadExel;
    }
    this.baseService.uploadfile(url, file.target['files'], "uploadedFile")
      .subscribe((response: any) => {
        this.data = response.data;
        this.pager.totalItems = this.data.length;
        this.totalValidRows = response.totalValidRows;
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
  }
  async downloadSample() {
    let url = '';
    let fileName = 'ImportTemplate.xlsx';
    if (this.type === 'province') {
      url = this.api_menu.Catalogue.CatPlace.downloadExcel + "?type=" + PlaceTypeEnum.Province;
      fileName = "Province" + fileName;
    }
    if (this.type === 'district') {
      url = this.api_menu.Catalogue.CatPlace.downloadExcel + "?type=" + PlaceTypeEnum.District;
      fileName = "District" + fileName;
    }
    if (this.type === 'ward') {
      url = this.api_menu.Catalogue.CatPlace.downloadExcel + "?type=" + PlaceTypeEnum.Ward;
      fileName = "Ward" + fileName;
    }
    if (this.type === 'country') {
      url = this.api_menu.Catalogue.Country.downloadExcel;
      fileName = "Country" + fileName;
    }
    await this.baseService.downloadfile(url, fileName);
  }
  async setPage(pager: PagerSetting) {
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    if (this.isShowInvalid) {
      this.pager = this.pagingService.getPager(this.data.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
      this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
      this.pagedItems = this.data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    } else {
      this.pager = this.pagingService.getPager(this.inValidItems.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
      this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
      this.pagedItems = this.inValidItems.slice(this.pager.startIndex, this.pager.endIndex + 1);

      if (this.inValidItems.length === 0) {
        this.pager.totalItems = 1;
      }
    }
  }
  hideInvalid() {
    if (this.data == null) { return; }
    this.isShowInvalid = !this.isShowInvalid;
    if (this.isShowInvalid) {
      this.pager.totalItems = this.data.length;
    } else {
      this.inValidItems = this.data.filter(x => !x.isValid);
    }
    this.child.setPage(this.pager.currentPage);
  }
  async import() {
    if (this.data == null) { return; }
    if (this.totalRows - this.totalValidRows > 0) {
      this.invaliDataAlert.show();
    } else {
      const data = this.data.filter(x => x.isValid);
      let url = '';
      if (this.type === 'province') {
        url = this.api_menu.Catalogue.CatPlace.import + "?type=" + PlaceTypeEnum.Province;
      }
      if (this.type === 'district') {
        url = this.api_menu.Catalogue.CatPlace.import + "?type=" + PlaceTypeEnum.District;
      }
      if (this.type === 'ward') {
        url = this.api_menu.Catalogue.CatPlace.import + "?type=" + PlaceTypeEnum.Ward;
      }
      if (this.type === 'country') {
        url = this.api_menu.Catalogue.Country.import;
      }
      const response = await this.baseService.postAsync(url, data);
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
    this.inputFile = null;
    this.pager.totalItems = 0;
  }
  sort(property: string) {
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
  }
}
