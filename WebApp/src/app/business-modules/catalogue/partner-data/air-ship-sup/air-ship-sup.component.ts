import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';

@Component({
  selector: 'app-air-ship-sup',
  templateUrl: './air-ship-sup.component.html',
  styleUrls: ['./air-ship-sup.component.scss']
})
export class AirShipSupComponent implements OnInit {
  airShips: Array<Partner>;
  //airShip: Partner;
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  criteria: any = { partnerGroup: PartnerGroupEnum.AIRSHIPSUP };
  isDesc: boolean = false;
  @ViewChild(PaginationComponent) child; 
  @Output() deleteConfirm = new EventEmitter<any>();
  @Output() detail = new EventEmitter<any>();
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
      this.airShips = response.data.map(x=>Object.assign({},x));
      console.log(this.airShips);
      this.pager.totalItems = response.totalItems;
    });
  }
  onSortChange(property) {
    this.isDesc = !this.isDesc;
    this.airShips = this.sortService.sort(this.airShips, property, this.isDesc);
  }
  
  showConfirmDelete(item) {
    //this.partner = item;
    this.deleteConfirm.emit(item);
  }
  showDetail(item) {
    this.detail.emit(item);
  }
}
