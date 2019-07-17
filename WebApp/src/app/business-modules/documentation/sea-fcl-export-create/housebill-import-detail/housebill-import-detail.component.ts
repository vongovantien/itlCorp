import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import moment from 'moment/moment';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { SortService } from 'src/app/shared/services/sort.service';
import { API_MENU } from 'src/constants/api-menu.const';
declare var $: any;

@Component({
    selector: 'app-housebill-import-detail',
    templateUrl: './housebill-import-detail.component.html',
    styleUrls: ['./housebill-import-detail.component.scss']
})
export class HousebillImportDetailComponent implements OnInit {
    @Output() shipmentImport = new EventEmitter<any>();
    @Input()shipmentDetails: any[];
    @Input()pager: PagerSetting = PAGINGSETTING;
    shipmentDetail: any;
    criteria: any = {
    };
    searchString: string = '';

    constructor(private baseServices: BaseService,
        private sortService: SortService,
        private api_menu: API_MENU) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
     }

    ngOnInit() {
        this.selectFilter = 'HBL No';
        $('#import-housebill-detail-modal').on('shown.bs.modal', function() {
            $('#searchTextHouseBill').trigger('focus');
          });
    }

    async getShipmentDetails(){
        let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransactionDetail.paging+"?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.criteria,true, true);
        this.shipmentDetails = responses.data;
        console.log(this.shipmentDetails);
        this.pager.totalItems = responses.totalItems;
    }
    async setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.pageSize = pager.pageSize;
        this.pager.totalPages = pager.totalPages;
        await this.getShipmentDetails();
      }
    importHousebillDetail() {
        if(this.shipmentDetail != null){
            this.shipmentImport.emit(this.shipmentDetail);
            $('#import-housebill-detail-modal').modal('hide');
          }
          else{
            this.baseServices.errorToast("Please choose a shipment detail");
          }
    }
    closeImportHousebillDetail() {
        this.shipmentDetail = null;
        this.resetSearchShipmentDetail();
        $('#import-housebill-detail-modal').modal('hide');
    }
    searchShipmentDetail(){
        this.criteria.hwbno ='';
        this.criteria.mawb = '';
        this.criteria.customerName = '';
        this.criteria.saleManName = '';
        this.criteria.all = null;
        this.criteria.fromDate = this.selectedRange.startDate;
        this.criteria.toDate = this.selectedRange.endDate;
        if(this.selectFilter === 'HBL No'){
            this.criteria.hwbno = this.searchString;
        }
        if(this.selectFilter === 'MBL No'){
            this.criteria.mawb = this.searchString;
        }
        if(this.selectFilter === 'Customer'){
            this.criteria.customerName = this.searchString;
        }
        if(this.selectFilter === 'Saleman'){
            this.criteria.saleManName = this.searchString;
        }
        if(this.selectFilter === null){
          this.criteria.all = this.searchString;
        }
        this.getShipmentDetails();
    }
    async resetSearchShipmentDetail(){
        this.pager.currentPage = 1;
        this.selectFilter = 'HBL No';
        this.searchString = null;
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
        this.searchShipmentDetail();
      }
    /**
     * Daterange picker
     */
    selectedRange: any;
    selectedDate: any;
    keepCalendarOpeningWithRange: true;
    //maxDate: moment.Moment = moment();
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
    searchFilters: Array<string> = ['HBL No', 'MBL No', 'Customer', 'Saleman'];
    searchFilterActive = ['HBL No'];
    selectFilter:any= null;

    public value: any = {};
    public _disabledV: string = '0';
    public disabled: boolean = false;
  
    private set disabledV(value: string) {
      this._disabledV = value;
      this.disabled = this._disabledV === '1';
    }
  
    public typed(value: any): void {
      console.log('New search input: ', value);
    }
  
    public refreshValue(value: any): void {
      this.value = value;
    }

}
