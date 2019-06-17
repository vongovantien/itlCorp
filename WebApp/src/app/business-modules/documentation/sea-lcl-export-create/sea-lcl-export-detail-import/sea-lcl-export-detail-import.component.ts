import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';

@Component({
    selector: 'app-sea-lcl-export-detail-import',
    templateUrl: './sea-lcl-export-detail-import.component.html',
    styleUrls: ['./sea-lcl-export-detail-import.component.scss']
})
export class SeaLclExportDetailImportComponent implements OnInit {

    selectFilter: string = 'Job ID';

    constructor() {
        this.keepCalendarOpeningWithRange = true;
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment(Date.now()) };
    }

    ngOnInit() {
    }

    /**
     * Daterange picker
     */
    selectedRange: any;
    //selectedDate: any;
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
    searchFilters: Array<string> = ['Job ID', 'MBL No', 'Supplier'];
    searchFilterActive = ['Job ID'];

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
