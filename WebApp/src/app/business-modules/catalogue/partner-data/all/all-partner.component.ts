import { Component, OnInit, ViewChild, EventEmitter, Output } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { BaseService } from 'src/services-base/base.service';

@Component({
  selector: 'app-all-partner',
  templateUrl: './all-partner.component.html',
  styleUrls: ['./all-partner.component.scss']
})
export class AllPartnerComponent implements OnInit {
  partners: Array<Partner>;
  //partner: Partner;
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  criteria: any = { partnerGroup: PartnerGroupEnum.ALL };
  isDesc: boolean = false;
  @ViewChild(PaginationComponent) child; 
  @Output() deleteConfirm = new EventEmitter<Partner>();
  @Output() detail = new EventEmitter<Partner>();
  constructor(private baseService: BaseService,
    private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  setPageAll(pager: PagerSetting): any {
    console.log(pager);
    this.getPartnerData(pager, this.criteria);
  }
  getPartnerData(pager: PagerSetting, criteria?: any): any {
    this.spinnerService.show();
    if(criteria != undefined){
      this.criteria = criteria;
    }
    this.baseService.post(this.api_menu.Catalogue.PartnerData.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.spinnerService.hide();
      this.partners = response.data.map(x=>Object.assign({},x));
      console.log(this.partners);
      this.pager.totalItems = response.totalItems;
      return this.pager.totalItems;
    });
  }
  onSortChange(property) {
    this.isDesc = !this.isDesc;
    this.partners = this.sortService.sort(this.partners, property, this.isDesc);
  }
  showConfirmDelete(item) {
    this.deleteConfirm.emit(item);
  }
  showDetail(item) {
    this.detail.emit(item);
  }
}
