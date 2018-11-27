import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';

@Component({
  selector: 'app-exchange-rate',
  templateUrl: './exchange-rate.component.html',
  styleUrls: ['./exchange-rate.component.scss']
})
export class ExchangeRateComponent implements OnInit {

  constructor() {
    this.keepCalendarOpeningWithRange = true;
    this.selectedDate = Date.now();
    this.selectedRange = {startDate: moment().startOf('month'), endDate: moment().endOf('month')};
  }

  ngOnInit() {
  }
  
  /**
   * Daterange picker
   */
  selectedRange: any;
  selectedDate:any;
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

  dateRangeChanged(e) {
    $('.reset-date').show();
  }
  defaultHistory() {
    this.selectedRange = {startDate: moment().startOf('month'), endDate: moment().endOf('month')};
  }

  /**
   * ng2-select
   */
  public items: Array<string> = ['USD', 'JPY', 'SGD', 'EUR', 'GBP', 'HKD',];
  private value: any = {};
  private _disabledV: string = '0';
  private disabled: boolean = false;
  
  private get disabledV():string {
    return this._disabledV;
  }
 
  private set disabledV(value:string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }
 
  public selected(value:any):void {
    console.log('Selected value is: ', value);
  }
 
  public removed(value:any):void {
    console.log('Removed value is: ', value);
  }
 
  public typed(value:any):void {
    console.log('New search input: ', value);
  }
 
  public refreshValue(value:any):void {
    this.value = value;
  }

}
