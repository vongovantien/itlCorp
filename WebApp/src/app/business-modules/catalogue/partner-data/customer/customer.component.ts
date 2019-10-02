import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { NgProgress } from '@ngx-progressbar/core';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map, } from 'rxjs/operators';
import * as lodash from 'lodash';


@Component({
  selector: 'app-customer',
  templateUrl: './customer.component.html',
  styleUrls: ['./customer.component.scss']
})
export class CustomerComponent extends AppList {
  customers: Partner[] = [];
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  criteria: any = { partnerGroup: PartnerGroupEnum.CUSTOMER };
  saleMans: any[] = [];
  headerSaleman: CommonInterface.IHeaderTable[];
  @ViewChild(PaginationComponent, { static: false }) child;
  @Output() deleteConfirm = new EventEmitter<Partner>();
  @Output() detail = new EventEmitter<any>();
  constructor(
    private baseService: BaseService,
    private excelService: ExcelService,
    private api_menu: API_MENU,
    private _progressService: NgProgress,
    private sortService: SortService,
    private _catalogueRepo: CatalogueRepo,
  ) {
    super();
    this._progressRef = this._progressService.ref();
    this.requestSort = this.sortCustomers;

  }


  ngOnInit() {
    this.headerSaleman = [
      { title: 'service', field: 'service', sortable: true },
      { title: 'office', field: 'office', sortable: true },
      { title: 'company', field: 'company', sortable: true },
      { title: 'status', field: 'status', sortable: true },
      { title: 'createDate', field: 'createDate', sortable: true }
    ];
  }
  async getPartnerData(pager: PagerSetting, criteria?: any) {
    if (criteria != undefined) {
      this.criteria = criteria;
    }
    const responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, false, true);
    this.customers = (responses.data || []).map(i => new Partner(i));
    console.log(this.customers);
    this.pager.totalItems = responses.totalItems;
  }
  showConfirmDelete(item) {
    this.deleteConfirm.emit(item);
  }
  showDetail(item) {
    this.detail.emit(item);
  }

  showSaleman(partnerId: string, indexs: number) {
    if (!!this.customers[indexs].saleManRequests.length) {
      this.saleMans = this.customers[indexs].saleManRequests;
    } else {
      this._progressRef.start();
      this._catalogueRepo.getListSaleman(partnerId)
        .pipe(
          catchError(this.catchError),
          finalize(() => this._progressRef.complete())
        ).subscribe(
          (res: any[]) => {
            this.saleMans = res || [];
            console.log(this.saleMans);
            this.customers[indexs].saleManRequests = this.saleMans;
          },
        );
    }
  }


  async exportCustomers() {
    var customers = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query, this.criteria);
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
      customers = lodash.map(customers, function (cus, index) {
        return [
          index + 1,
          cus['id'],
          cus['partnerNameEn'],
          cus['shortName'],
          cus['addressEn'],
          cus['taxCode'],
          cus['tel'],
          cus['fax'],
          cus['userCreatedName'],
          cus['datetimeModified'],
          (cus['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
      customers = lodash.map(customers, function (cus, index) {
        return [
          index + 1,
          cus['id'],
          cus['partnerNameVn'],
          cus['shortName'],
          cus['addressVn'],
          cus['taxCode'],
          cus['tel'],
          cus['fax'],
          cus['userCreatedName'],
          cus['datetimeModified'],
          (cus['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }


    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Partner Data - Customers";
    exportModel.sheetName = "Customers"
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.author = currrently_user;
    exportModel.header = [
      { name: "No.", width: 10 },
      { name: "Partner ID", width: 20 },
      { name: "Full Name", width: 60 },
      { name: "Short Name", width: 20 },
      { name: "Billing Address", width: 60 },
      { name: "Tax Code", width: 20 },
      { name: "Tel", width: 30 },
      { name: "Fax", width: 30 },
      { name: "Creator", width: 30 },
      { name: "Modify", width: 30 },
      { name: "Inactive", width: 20 }
    ]
    exportModel.data = customers;
    exportModel.fileName = "Partner Data - Customers";
    this.excelService.generateExcel(exportModel);
  }


  sortCustomers(sort: string): void {
    this.customers = this.sortService.sort(this.customers, sort, this.order);
  }
}
