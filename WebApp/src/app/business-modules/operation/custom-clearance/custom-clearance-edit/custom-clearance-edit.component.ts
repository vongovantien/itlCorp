import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';

@Component({
  selector: 'app-custom-clearance-edit',
  templateUrl: './custom-clearance-edit.component.html',
  styleUrls: ['./custom-clearance-edit.component.scss']
})
export class CustomClearanceEditComponent implements OnInit {

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
 
     statusClearance: Array<string> = ['All', 'Imported', 'Not imported'];
     typeClearance: Array<string> = ['All', 'Export', 'Imported'];
 
     serveiceType: Array<string> = ['Air', 'Sea', 'Cross border','Warehouse', 'Inland', 'Railway', 'Express'];
 
     packagesUnit: Array<string> = ['PKG', 'PCS', 'BOX', 'CNTS'];
     packagesUnitActive = ['PKG'];
 
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
