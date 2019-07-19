import { Component, OnInit, ViewChild, Output, EventEmitter } from 'node_modules/@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';

import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
import * as lodash from 'node_modules/lodash';

@Component({
  selector: 'app-agent',
  templateUrl: './agent.component.html',
  styleUrls: ['./agent.component.scss']
})
export class AgentComponent implements OnInit {
  agents: Array<Partner>;
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  criteria: any = { partnerGroup: PartnerGroupEnum.AGENT };
  isDesc: boolean = false;
  keySortDefault = "id";
  @ViewChild(PaginationComponent, { static: false }) child;
  @Output() deleteConfirm = new EventEmitter<any>();
  @Output() detail = new EventEmitter<any>();

  constructor(private baseService: BaseService,
    private excelService: ExcelService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  async getPartnerData(pager: PagerSetting, criteria?: any) {
    if (criteria != undefined) {
      this.criteria = criteria;
    }
    let responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, false, true);
    this.agents = responses.data;
    this.pager.totalItems = responses.totalItems;
  }
  onSortChange(column) {
    if (column.dataType != 'boolean') {
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.agents = this.sortService.sort(this.agents, property, this.isDesc);
    }
  }
  showConfirmDelete(item) {
    //this.partner = item;
    this.deleteConfirm.emit(item);
  }
  showDetail(item) {
    this.detail.emit(item);
  }

  async exportAgents() {
    var agents = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query, this.criteria);
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
      agents = lodash.map(agents, function (ag, index) {
        return [
          index + 1,
          ag['id'],
          ag['partnerNameEn'],
          ag['shortName'],
          ag['addressEn'],
          ag['taxCode'],
          ag['tel'],
          ag['fax'],
          ag['userCreatedName'],
          ag['datetimeModified'],
          (ag['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
      agents = lodash.map(agents, function (ag, index) {
        return [
          index + 1,
          ag['id'],
          ag['partnerNameVn'],
          ag['shortName'],
          ag['addressVn'],
          ag['taxCode'],
          ag['tel'],
          ag['fax'],
          ag['userCreatedName'],
          ag['datetimeModified'],
          (ag['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }


    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Partner Data - Agents";
    exportModel.sheetName = "Agents"
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
    exportModel.data = agents;
    exportModel.fileName = "Partner Data - Agents";
    this.excelService.generateExcel(exportModel);
  }



}
