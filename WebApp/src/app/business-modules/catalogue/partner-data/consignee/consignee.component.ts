import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
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

import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
import * as lodash from 'lodash';

@Component({
  selector: 'app-consignee',
  templateUrl: './consignee.component.html',
  styleUrls: ['./consignee.component.scss']
})
export class ConsigneeComponent implements OnInit {
  consignees: Array<Partner>;
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  criteria: any = { partnerGroup: PartnerGroupEnum.CONSIGNEE };
  isDesc: boolean = false;
  keySortDefault: string = "id";
  
  @ViewChild(PaginationComponent,{static:false}) child; 
  @Output() deleteConfirm = new EventEmitter<any>();
  @Output() detail = new EventEmitter<any>();
  constructor(private baseService: BaseService,
    private excelService: ExcelService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  async getPartnerData(pager: PagerSetting, criteria?: any) {
    if(criteria != undefined){
      this.criteria = criteria;
    }
    let responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, false, true);
    this.consignees = responses.data;
    this.pager.totalItems = responses.totalItems;
  }
  onSortChange(column) {
    if(column.dataType != 'boolean'){
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.consignees = this.sortService.sort(this.consignees, property, this.isDesc);
    }
  }
  
  showConfirmDelete(item) {
    //this.partner = item;
    this.deleteConfirm.emit(item);
  }
  showDetail(item) {
    this.detail.emit(item);
  }

  async exportConsignees(){
    var consignees = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query,this.criteria);
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API){
      consignees = lodash.map(consignees,function(consig,index){
        return [
          index+1,
          consig['id'],
          consig['partnerNameEn'],
          consig['shortName'],
          consig['addressEn'],
          consig['taxCode'],
          consig['tel'],
          consig['fax'],
          consig['userCreatedName'],
          consig['datetimeModified'],
          (consig['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API){
      consignees = lodash.map(consignees,function(consig,index){
        return [
          index+1,
          consig['id'],
          consig['partnerNameVn'],
          consig['shortName'],
          consig['addressVn'],
          consig['taxCode'],
          consig['tel'],
          consig['fax'],
          consig['userCreatedName'],
          consig['datetimeModified'],
          (consig['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }
    

    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Partner Data - Consignees";
    exportModel.sheetName = "Consignees"
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
    exportModel.data = consignees;
    exportModel.fileName = "Partner Data - Consignees";
    this.excelService.generateExcel(exportModel);
  }
}
