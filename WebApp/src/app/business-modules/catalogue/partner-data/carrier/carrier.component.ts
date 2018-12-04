import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { BaseService } from 'src/services-base/base.service';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';

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
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  getPartnerData(pager: PagerSetting, criteria?: any): any {
    this.spinnerService.show();
    if(criteria != undefined){
      this.criteria = criteria;
    }
    this.baseService.post(this.api_menu.Catalogue.PartnerData.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.spinnerService.hide();
      this.carriers = response.data.map(x=>Object.assign({},x));
      console.log(this.carriers);
      this.pager.totalItems = response.totalItems;
      return this.pager.totalItems;
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
}
