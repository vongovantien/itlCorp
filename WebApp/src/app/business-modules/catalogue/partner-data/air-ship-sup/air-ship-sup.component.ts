import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';

import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
import * as lodash from 'lodash';

@Component({
  selector: 'app-air-ship-sup',
  templateUrl: './air-ship-sup.component.html',
  styleUrls: ['./air-ship-sup.component.scss']
})
export class AirShipSupComponent implements OnInit {
  airShips: Array<Partner>;
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  criteria: any = { partnerGroup: PartnerGroupEnum.AIRSHIPSUP };
  isDesc: boolean = false;
  keySortDefault: string = "id";
  @ViewChild(PaginationComponent, { static: false }) child;
  @Output() deleteConfirm = new EventEmitter<any>();
  @Output() detail = new EventEmitter<any>();
  constructor(private baseService: BaseService,
    private excelService: ExcelService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  setPage(pager: PagerSetting): any {
    this.getPartnerData(pager, this.criteria);
  }
  async getPartnerData(pager: PagerSetting, criteria?: any) {
    if (criteria != undefined) {
      this.criteria = criteria;
    }
    let responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, false, true);
    this.airShips = responses.data;
    this.pager.totalItems = responses.totalItems;
  }
  onSortChange(column) {
    if (column.dataType != 'boolean') {
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.airShips = this.sortService.sort(this.airShips, property, this.isDesc);
    }
  }

  showConfirmDelete(item) {
    //this.partner = item;
    this.deleteConfirm.emit(item);
  }
  showDetail(item) {
    this.detail.emit(item);
  }

  async exportAirShipSup() {
    var airShipSup = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query, this.criteria);
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
      airShipSup = lodash.map(airShipSup, function (ashs, index) {
        return [
          index + 1,
          ashs['id'],
          ashs['partnerNameEn'],
          ashs['shortName'],
          ashs['addressEn'],
          ashs['taxCode'],
          ashs['tel'],
          ashs['fax'],
          ashs['userCreatedName'],
          ashs['datetimeModified'],
          (ashs['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
      airShipSup = lodash.map(airShipSup, function (ashs, index) {
        return [
          index + 1,
          ashs['id'],
          ashs['partnerNameVn'],
          ashs['shortName'],
          ashs['addressVn'],
          ashs['taxCode'],
          ashs['tel'],
          ashs['fax'],
          ashs['userCreatedName'],
          ashs['datetimeModified'],
          (ashs['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }


    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Partner Data - Air Ship Sup";
    exportModel.sheetName = "Air-Ship-Sup"
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
    exportModel.data = airShipSup;
    exportModel.fileName = "Partner Data - air ship sup";
    this.excelService.generateExcel(exportModel);
  }
}
