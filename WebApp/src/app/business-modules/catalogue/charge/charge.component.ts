import { Component, OnInit, ViewChild } from '@angular/core';
import * as lodash from 'lodash';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { Router } from '@angular/router';
import { SortService } from 'src/app/shared/services/sort.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
// import {DataHelper} from 'src/helper/data.helper';
declare var $: any;

@Component({
  selector: 'app-charge',
  templateUrl: './charge.component.html',
  styleUrls: ['./charge.component.scss']
})
export class ChargeComponent implements OnInit {

  constructor(
    private baseServices: BaseService,
    private excelService: ExcelService,
    private api_menu: API_MENU,
    private router: Router,
    private sortService: SortService) {

     }

  listFilter = [
    { filter: "All", field: "all" }, { filter: "Code", field: "code" },
    { filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" }];
  selectedFilter = this.listFilter[0].filter;

  pager: PagerSetting = PAGINGSETTING;
  // ChargeToUpdate : CatChargeToAddOrUpdate ;
  // ChargeToAdd : CatChargeToAddOrUpdate ;
  ListCharges: any = [];
  idChargeToUpdate: any = "";
  idChargeToDelete: any = ""
  idChargeToAdd: any = "";
  searchKey: string = "";
  searchObject: any = {};


  @ViewChild(PaginationComponent,{static:false}) child:any;

  async ngOnInit() {
    await this.getCharges();
  }

  async searchCharge() {
    this.searchObject = {};
    if (this.selectedFilter == "All") {
      this.searchObject.All = this.searchKey;
    } else {
      this.searchObject = {};
      for (var i = 1; i < this.listFilter.length; i++) {
        if (this.selectedFilter == this.listFilter[i].filter) {
          eval("this.searchObject[this.listFilter[i].field]=this.searchKey");
        }

      }
    }
    this.pager.currentPage = 1;
    await this.getCharges();
  }

  async resetSearch() {
    this.searchKey = "";
    this.searchObject = {};
    this.pager = PAGINGSETTING;
    this.selectedFilter = this.listFilter[0].filter;
    await this.getCharges();
  }

  async setPage(pager: PagerSetting) {
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    await this.getCharges();
  }

  setPageAfterDelete() {
    this.child.setPage(this.pager.currentPage);
    if (this.pager.currentPage > this.pager.totalPages) {
      this.pager.currentPage = this.pager.totalPages;
      this.child.setPage(this.pager.currentPage);
    }
  }

  async getCharges() {
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.Charge.paging + "?pageNumber=" + this.pager.currentPage + "&pageSize=" + this.pager.pageSize, this.searchObject, false, true);
    if(response){
      this.ListCharges = response.data;
      console.log(this.ListCharges);
      this.pager.totalItems = response.totalItems;
    }
  }

  prepareDeleteCharge(id) {
    this.idChargeToDelete = id;
  }

  async deleteCharge() {
    await this.baseServices.deleteAsync(this.api_menu.Catalogue.Charge.delete + this.idChargeToDelete, true, true);
    await this.getCharges();
    this.setPageAfterDelete();
  }

  gotoEditPage(id) {
    this.router.navigate(["/home/catalogue/charge-edit", { id: id }]);
  }

  isDesc = true;
  sortKey: string = "code";
  sort(property: string) {
    this.sortKey = property;
    this.isDesc = !this.isDesc;
    const temp = this.ListCharges.map((x: any) => Object.assign({}, x));
    this.ListCharges = this.sortService.sort(this.ListCharges.map((x: { charge: any; }) => Object.assign({}, x.charge)), property, this.isDesc);
    // var getDept = this.getDepartmentname;
    // this.ListCharges = this.ListCharges.map(x=>({stage:x,deptName:getDept(x.id,temp)})); 
    this.ListCharges = this.ListCharges.map(x => ({ charge: x }));
  }
  getDepartmentname(stageId, ListCharges: any[]) {
    var inx = lodash.findIndex(ListCharges, function (o) { return o.charge.id === stageId });
    if (inx != -1) {
      return ListCharges[inx].deptName;
    }
  }



  /**
   * ng2-select multi
   */
  public items: Array<string> = ['Amsterdam', 'Antwerp', 'Athens', 'Barcelona',
    'Berlin', 'Birmingham', 'Bradford', 'Bremen', 'Brussels'];

  private value: any = ['Athens'];
  private _disabledV: string = '0';
  public disabled: boolean = false;

  public get disabledV(): string {
    return this._disabledV;
  }

  public set disabledV(value: string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }

  public selected(value: any): void {
    console.log('Selected value is: ', value);
  }

  public removed(value: any): void {
    console.log('Removed value is: ', value);
  }

  public refreshValue(value: any): void {
    this.value = value;
  }

  public itemsToString(value: Array<any> = []): string {
    return value
      .map((item: any) => {
        return item.text;
      }).join(',');
  }


  async import() {

  }

  async export() {
    var charges = await this.baseServices.postAsync(this.api_menu.Catalogue.Charge.query, this.searchObject);
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
      charges = lodash.map(charges, function (chrg, index) {
        return [
          index + 1,
          chrg['code'],
          chrg['chargeNameEn'],
          chrg['chargeNameVn'],
          chrg['type'],
          (chrg['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }

    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
      charges = lodash.map(charges, function (chrg, index) {
        return [
          index + 1,
          chrg['code'],
          chrg['chargeNameEn'],
          chrg['chargeNameVn'],
          chrg['type'],
          (chrg['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }
    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Charge List";
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.author = currrently_user;
    exportModel.header = [
      { name: "No.", width: 10 },
      { name: "Code", width: 20 },
      { name: "English Name", width: 20 },
      { name: "Local Name", width: 20 },
      { name: "Type", width: 20 },
      { name: "Inactive", width: 20 }
    ]
    exportModel.data = charges;
    exportModel.fileName = "Charges";

    this.excelService.generateExcel(exportModel);

  }

}
