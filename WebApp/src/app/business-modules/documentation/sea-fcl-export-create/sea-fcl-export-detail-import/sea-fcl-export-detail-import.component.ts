import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';

@Component({
  selector: 'app-sea-fcl-export-detail-import',
  templateUrl: './sea-fcl-export-detail-import.component.html',
  styleUrls: ['./sea-fcl-export-detail-import.component.scss']
})
export class SeaFclExportDetailImportComponent implements OnInit {
  shipments: any[] = [];
  pager: PagerSetting = PAGINGSETTING;
  criteria: any = {
  };
  selectFilter: string = 'Job ID';
  searchString: string = '';
  
  constructor(private baseServices: BaseService,
    private api_menu: API_MENU) { 
    this.keepCalendarOpeningWithRange = true;
    this.selectedDate = Date.now();
    this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
  }

  async ngOnInit() {
    this.criteria.fromDate = this.selectedRange.startDate;
    this.criteria.toDate = this.selectedRange.endDate;
    await this.getShipments();
  }
  searchShipment(){
    this.criteria.jobNo ='';
    this.criteria.mawb = '';
    this.criteria.supplierName = '';
    this.criteria.agentName = '';
    this.criteria.hwbNo = '';
    this.criteria.fromDate = this.selectedRange.startDate;
    this.criteria.toDate = this.selectedRange.endDate;
    if(this.selectFilter === 'Job ID'){
        this.criteria.jobNo = this.searchString;
    }
    if(this.selectFilter === 'MBL No'){
        this.criteria.mawb = this.searchString;
    }
    if(this.selectFilter === 'Supplier'){
        this.criteria.supplierName = this.searchString;
    }
    this.getShipments();
}
  async getShipments(){
    let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransaction.paging+"?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.criteria,true, true);
    this.shipments = responses.data;
    this.pager.totalItems = responses.totalItems;
  }
  async setPage(pager: PagerSetting) {
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    await this.getShipments();
  }
  /**
     * Daterange picker
     */
    selectedRange: any;
    selectedDate: any;
    keepCalendarOpeningWithRange: true;
    maxDate: moment.Moment = moment();
    ranges: any = {
        Today: [moment(), moment()],
        Yesterday: [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
        'Last 7 Days': [moment().subtract(6, 'days'), moment()],
        'Last 30 Days': [moment().subtract(29, 'days'), moment()],
        'This Month': [moment().startOf('month'), moment().endOf('month')],
        'Last Month': [
            moment()
                .subtract(1, 'month')
                .startOf('month'),
            moment()
                .subtract(1, 'month')
                .endOf('month')
        ]
    };

    /**
   * ng2-select
   */
  searchFilters: Array<string> = ['Job ID', 'MBL No', 'Supplier'];
  searchFilterActive = ['Job ID'];

  public items: Array<string> = ['Option 1', 'Option 2', 'Option 3', 'Option 4',
  'Option 5', 'Option 6', 'Option 7', 'Option 8', 'Option 9', 'Option 10',];

  private _disabledV: string = '0';
  public disabled: boolean = false;


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
  }

}
