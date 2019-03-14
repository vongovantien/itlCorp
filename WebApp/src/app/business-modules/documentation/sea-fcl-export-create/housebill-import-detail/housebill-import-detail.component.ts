import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
declare var $: any;

@Component({
    selector: 'app-housebill-import-detail',
    templateUrl: './housebill-import-detail.component.html',
    styleUrls: ['./housebill-import-detail.component.scss']
})
export class HousebillImportDetailComponent implements OnInit {

    pager: PagerSetting = PAGINGSETTING;
    constructor() {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
     }

    ngOnInit() {
    }

    importHousebillDetail() {
        $('#import-housebill-detail-modal').modal('hide');
        console.log('import');
    }
    closeImportHousebillDetail() {
        $('#import-housebill-detail-modal').modal('hide');
        console.log('close');
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
    searchFilters: Array<string> = ['HBL', 'MBL', 'Customer', 'Saleman'];
    searchFilterActive = ['HBL'];
}
