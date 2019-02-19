
import * as moment from 'moment';
import { Component, OnInit, ViewChild, Output, EventEmitter, Input, OnChanges, SimpleChange, SimpleChanges, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
// import { PaginationComponent } from 'ngx-bootstrap';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import * as lodash from 'lodash';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { timingSafeEqual } from 'crypto';

@Component({
  selector: 'app-housebill-list',
  templateUrl: './housebill-list.component.html',
  styleUrls: ['./housebill-list.component.scss']
})
export class HousebillListComponent implements OnInit,AfterViewInit{
  ngAfterViewInit(): void {
    this.cdr.detach();
  }

  HouseBillListData: any[] = [];
  ConstHouseBillListData: any[] = [];

  constructor(private cdr: ChangeDetectorRef) { }
  @Input() set houseBillList(lstHB: any[]) {
    this.HouseBillListData = lstHB;
    this.ConstHouseBillListData = lstHB;
  }

  ngOnInit() {
  }

  viewdata() {
    console.log({ "HB L": this.HouseBillListData });
    console.log({ "Const hbl ": this.ConstHouseBillListData });
  }

  searchHouseBill(key: any) {
    const search_key = key.toLowerCase();
    this.HouseBillListData = lodash.filter(this.ConstHouseBillListData, function (x) {
      return (
        x.data.hwbno.toLowerCase().includes(search_key) ||
        x.extend_data.customer_nameEn.toLowerCase().includes(search_key) ||
        x.extend_data.customer_nameVn.toLowerCase().includes(search_key) ||
        x.extend_data.saleman_nameEn.toLowerCase().includes(search_key) ||
        x.extend_data.notify_party_nameEn.toLowerCase().includes(search_key) ||
        x.extend_data.notify_party_nameVn.toLowerCase().includes(search_key) ||
        x.data.finalDestinationPlace.toLowerCase().includes(search_key)
      )
    });
  }



  /**
    * ng2-select
    */
  public items: Array<string> = ['Option 1', 'Option 2', 'Option 3', 'Option 4',
    'Option 5', 'Option 6', 'Option 7', 'Option 8', 'Option 9', 'Option 10',];

  private value: any = {};
  private _disabledV: string = '0';
  public disabled: boolean = false;

  private get disabledV(): string {
    return this._disabledV;
  }

  private set disabledV(value: string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }

  public selected(value: any): void {
    console.log('Selected value is: ', value);
  }

  public removed(value: any): void {
    console.log('Removed value is: ', value);
  }

  public typed(value: any): void {
    console.log('New search input: ', value);
  }

  public refreshValue(value: any): void {
    this.value = value;
  }
}
