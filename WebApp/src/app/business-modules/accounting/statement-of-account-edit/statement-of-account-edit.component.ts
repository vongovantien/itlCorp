import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';

@Component({
    selector: 'app-statement-of-account-edit',
    templateUrl: './statement-of-account-edit.component.html',
    styleUrls: ['./statement-of-account-edit.component.scss']
})
export class StatementOfAccountEditComponent implements OnInit {

    constructor() {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    ngOnInit() {
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
    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4', 'option 5', 'option 6', 'option 7'];

    dateMode: Array<string> = ['Created Date', 'Service Date', 'Invoice Issued Date'];

    statusSOA: Array<string> = ['New', 'Request Confirmed', 'Confirmed', 'Need Revise', 'Done'];

    currencyList: Array<string> = ['USD', 'VND', 'BOX', 'CNTS'];

    servicesList: Array<string> = ['All', 'Logistic (operation)', 'Air Import', 'Air Export', 'Sea FCL Export', 'Sea LCL Export', 'Sea FCL Import', 'Sea LCL Import', 'Sea Consol Export', 'Sea Consol Import', 'Trucking Inland'];

    userList: Array<string> = [];
    currentUser = ['Thor'];

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
