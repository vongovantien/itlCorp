import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PaginationComponent } from 'ngx-bootstrap';
import { ToastrService } from 'ngx-toastr';
import { BaseService } from 'src/services-base/base.service';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';

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
  @ViewChild(PaginationComponent) child; 
  @Output() deleteConfirm = new EventEmitter<any>();
  constructor(private baseService: BaseService,
    private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
  }
  setPage(pager: PagerSetting): any {
    this.getPartnerData(pager, this.criteria);
  }
  getPartnerData(pager: PagerSetting, criteria?: any): any {
    this.spinnerService.show();
    if(criteria != undefined){
      this.criteria = criteria;
    }
    this.baseService.post(this.api_menu.Catalogue.PartnerData.paging+"?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria).subscribe((response: any) => {
      this.spinnerService.hide();
      this.shippers = response.data.map(x=>Object.assign({},x));
      console.log(this.shippers);
      this.pager.totalItems = response.totalItems;
    });
  }
  onSortChange(property) {
    this.isDesc = !this.isDesc;
    this.shippers = this.sortService.sort(this.shippers, property, this.isDesc);
  }
  showConfirmDelete(item) {
    this.shipper = item;
    this.deleteConfirm.emit(this.shipper);
  }
  showDetail(item) {
    this.shipper = item;
  }
}
