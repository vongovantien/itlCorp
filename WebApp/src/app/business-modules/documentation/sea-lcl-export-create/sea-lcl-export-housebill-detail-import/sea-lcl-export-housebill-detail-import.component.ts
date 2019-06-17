import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';

@Component({
    selector: 'app-sea-lcl-export-housebill-detail-import',
    templateUrl: './sea-lcl-export-housebill-detail-import.component.html',
    styleUrls: ['./sea-lcl-export-housebill-detail-import.component.scss']
})
export class SeaLclExportHousebillDetailImportComponent implements OnInit {

    constructor() {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    ngOnInit() {
        this.selectFilter = 'HBL No';
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
