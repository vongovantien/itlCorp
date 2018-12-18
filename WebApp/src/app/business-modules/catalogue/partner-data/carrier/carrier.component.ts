import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';

import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
import * as lodash from 'lodash';

@Component({
  selector: 'app-carrier',
  templateUrl: './carrier.component.html',
  styleUrls: ['./carrier.component.scss']
})
export class CarrierComponent implements OnInit {
  carriers: Array<Partner>;
  //carrier: Partner;
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  criteria: any = { partnerGroup: PartnerGroupEnum.CARRIER };
  isDesc: boolean = false;
  keySortDefault: string = "id";
  
  @ViewChild(PaginationComponent) child; 
  @Output() deleteConfirm = new EventEmitter<any>();
  @Output() detail = new EventEmitter<any>();
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
      this.carriers = response.data.map(x=>Object.assign({},x));
      console.log(this.carriers);
      this.pager.totalItems = response.totalItems;
      return this.pager.totalItems;
    },err=>{
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }
  onSortChange(column) {
    if(column.dataType != 'boolean'){
      let property = column.primaryKey;
      this.isDesc = !this.isDesc;
      this.carriers = this.sortService.sort(this.carriers, property, this.isDesc);
    }
  }
  
  showConfirmDelete(item) {
    //this.partner = item;
    this.deleteConfirm.emit(item);
  }
  showDetail(item) {
    this.detail.emit(item);
  }

  async exportCarriers(){
    var carriers = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query,this.criteria);
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API){
      carriers = lodash.map(carriers,function(carr,index){
        return [
          index+1,
          carr['id'],
          carr['partnerNameEn'],
          carr['shortName'],
          carr['addressEn'],
          carr['taxCode'],
          carr['tel'],
          carr['fax'],
          carr['userCreatedName'],
          carr['datetimeModified'],
          (carr['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API){
      carriers = lodash.map(carriers,function(carr,index){
        return [
          index+1,
          carr['id'],
          carr['partnerNameVn'],
          carr['shortName'],
          carr['addressVn'],
          carr['taxCode'],
          carr['tel'],
          carr['fax'],
          carr['userCreatedName'],
          carr['datetimeModified'],
          (carr['inactive']===true)?SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }
    

    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Partner Data - Carriers";
    exportModel.sheetName = "Carriers"
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
    exportModel.data = carriers;
    exportModel.fileName = "Partner Data - Carriers";
    this.excelService.generateExcel(exportModel);
  }
}
