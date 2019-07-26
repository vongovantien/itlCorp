import { OnInit, OnDestroy, OnChanges, DoCheck, AfterContentChecked, AfterContentInit, AfterViewChecked, AfterViewInit } from "@angular/core";
import { Observable, Subject, throwError } from "rxjs";
import { HttpErrorResponse } from "@angular/common/http";

import moment from "moment";
import { UtilityHelper } from "src/helper";

export abstract class AppPage implements OnInit, OnDestroy, OnChanges, DoCheck, AfterContentChecked, AfterContentInit, AfterViewChecked, AfterViewInit {

  ngUnsubscribe: Subject<any> = new Subject();
  keyword: string = '';
  ranges: any = {
    Today: [moment(), moment()],
    Yesterday: [moment().subtract(1, "days"), moment().subtract(1, "days")],
    "Last 7 Days": [moment().subtract(6, "days"), moment()],
    "Last 30 Days": [moment().subtract(29, "days"), moment()],
    "This Month": [moment().startOf("month"), moment().endOf("month")],
    "Last Month": [moment().subtract(1, "month").startOf("month"), moment().subtract(1, "month").endOf("month")]
  };
  maxDate: any = moment();

  utility: UtilityHelper = new UtilityHelper();
  isLoading: boolean = false;
  
  constructor() { 
  }
  ngOnInit(): void { }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  ngDoCheck(): void { }

  ngOnChanges(changes: any): void { }

  ngAfterContentInit(): void { }

  ngAfterContentChecked(): void { }

  ngAfterViewInit(): void { }

  ngAfterViewChecked(): void { }

  trackByFn(index: number, item: any) {
    return !!item.id ? item.id : !!item.code ? item.code : index;
  }

  back() {
    window.history.back();
  }

  catchError(error: HttpErrorResponse): Observable<any> {
    return throwError(error || 'Có lỗi xảy, Vui lòng kiểm tra lại !');
  }

  removed(value:any):void {
  }
 
  refreshValue(value:any):void {
  }
}

  // config for <app-combo-grid-virtual-scroll>
export interface IComboGirdConfig {
  placeholder: string;
  displayFields: any[];
  dataSource: any[];
  selectedDisplayFields: any[];
}
