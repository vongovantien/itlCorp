import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PaginationComponent } from 'ngx-bootstrap';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';

import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
import * as lodash from 'lodash';

@Component({
  selector: 'app-shipper',
  templateUrl: './shipper.component.html',
  styleUrls: ['./shipper.component.scss']
})
export class ShipperComponent implements OnInit {
  shippers: Array<Partner>;
  shipper: Partner;
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  criteria: any = { partnerGroup: PartnerGroupEnum.SHIPPER };
  isDesc: boolean = false;
  keySortDefault: string = "id";
  @ViewChild(PaginationComponent) child; 
  @Output() deleteConfirm = new EventEmitter<any>();
  constructor(private baseService: BaseService, 
    private excelService: ExcelService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  getPartnerData(pager: PagerSetting, criteria?: any): any {
    this.baseService.spinnerShow();
    if(criteria != undefined){
      this.criteria = criteria;
    }
    this.baseService.post(this.api_menu.Catalogue.PartnerData.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.baseService.spinnerHide();
      this.shippers = response.data.map(x=>Object.assign({},x));
      console.log(this.shippers);
      this.pager.totalItems = response.totalItems;
      return this.pager.totalItems;
    });
  }
  onSortChange(column) {
    if(column.dataType != 'boolean'){
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.shippers = this.sortService.sort(this.shippers, property, this.isDesc);
    }
  }
  showConfirmDelete(item) {
    this.shipper = item;
    this.deleteConfirm.emit(this.shipper);
  }
  showDetail(item) {
    this.shipper = item;
  }

  async exportShippers(){
    var shippers = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query,this.criteria);
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API){
      shippers = lodash.map(shippers,function(ship,index){
        return [
          index+1,
          ship['id'],
          ship['partnerNameEn'],
          ship['shortName'],
          ship['addressEn'],
          ship['taxCode'],
          ship['tel'],
          ship['fax'],
          ship['userCreatedName'],
          ship['datetimeModified'],
          (ship['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API){
      shippers = lodash.map(shippers,function(ship,index){
        return [
          index+1,
          ship['id'],
          ship['partnerNameVn'],
          ship['shortName'],
          ship['addressVn'],
          ship['taxCode'],
          ship['tel'],
          ship['fax'],
          ship['userCreatedName'],
          ship['datetimeModified'],
          (ship['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }
    

    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Partner Data - Shippers";
    exportModel.sheetName = "Shippers"
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
    exportModel.data = shippers;
    exportModel.fileName = "Partner Data - Shippers";
    this.excelService.generateExcel(exportModel);
  }
}
