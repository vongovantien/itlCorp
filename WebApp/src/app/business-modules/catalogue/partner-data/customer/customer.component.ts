import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';

@Component({
  selector: 'app-customer',
  templateUrl: './customer.component.html',
  styleUrls: ['./customer.component.scss']
})
export class CustomerComponent implements OnInit {
  customers: any;
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  criteria: any = { partnerGroup: PartnerGroupEnum.CUSTOMER };
  @ViewChild(PaginationComponent) child; 
  @Output() deleteConfirm = new EventEmitter<Partner>();
  @Output() detail = new EventEmitter<any>();
  constructor(private baseService: BaseService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  getPartnerData(pager: PagerSetting, criteria?: any): any {
    this.baseService.spinnerShow();
    if(criteria != undefined){
      this.criteria = criteria;
    }
    this.baseService.post(this.api_menu.Catalogue.PartnerData.customerPaging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.baseService.spinnerHide();
      this.customers = response.data;
      console.log(this.customers);
      this.pager.totalItems = response.totalItems;
      return this.pager.totalItems;
    });
  }
  showConfirmDelete(item) {
    this.deleteConfirm.emit(item);
  }
  showDetail(item){
    this.detail.emit(item);
  }

  isDesc = true;
  sortKey: string = "id";
  sort(property){
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.customers.forEach(element => {
      element.catPartnerModels = this.sortService.sort(element.catPartnerModels, property, this.isDesc)
    });
  }
}
