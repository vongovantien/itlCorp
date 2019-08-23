import { OnInit, OnDestroy, OnChanges, DoCheck, AfterContentChecked, AfterContentInit, AfterViewChecked, AfterViewInit } from "@angular/core";
import { Observable, Subject, throwError } from "rxjs";
import { HttpErrorResponse } from "@angular/common/http";

import moment from "moment";
import { UtilityHelper } from "src/helper";
import { NgProgressRef } from "@ngx-progressbar/core";

export abstract class AppPage implements OnInit, OnDestroy, OnChanges, DoCheck, AfterContentChecked, AfterContentInit, AfterViewChecked, AfterViewInit {

  ngUnsubscribe: Subject<any> = new Subject();
  keyword: string = '';
  ranges: any = {
    // "Today": [moment(), moment()],
    // Yesterday: [moment().subtract(1, "days"), moment().subtract(1, "days")],
    // "Last 7 Days": [moment().subtract(6, "days"), moment()],
    // "Last 30 Days": [moment().subtract(29, "days"), moment()],
    // "This Month": [moment().startOf("month"), moment().endOf("month")],
    // "Last Month": [moment().subtract(1, "month").startOf("month"), moment().subtract(1, "month").endOf("month")]
    "Today": [new Date(), new Date()],
    "Yesterday": [new Date(new Date().setDate(new Date().getDate() - 1)), new Date(new Date().setDate(new Date().getDate() - 1))],
    "Last 7 Days": [new Date(new Date().setDate(new Date().getDate() - 6)), new Date()],
    "Last 30 Days": [new Date(new Date().setDate(new Date().getDate() - 29)), new Date()],
    "This Month": [new Date(new Date().getFullYear(), new Date().getMonth(), 1), new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)],
    "Last Month": [new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1), new Date(new Date().getFullYear(), new Date().getMonth(), 0)]
  };
  maxDate: any = moment();
  minDate: any = moment();

  utility: UtilityHelper = new UtilityHelper();

  isLoading: boolean = false;
  isCheckAll: boolean = false;

  _progressRef: NgProgressRef;

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

  removed(value: any): void {
  }

  refreshValue(value: any): void {
  }

  saveAs(blob: Blob, filename: string) {
    const anchorElem = document.createElement("a");
    anchorElem.id = "down";

    if (window.navigator.msSaveBlob) {
      window.navigator.msSaveOrOpenBlob(blob, filename);
    } else {
      const url = window.URL.createObjectURL(blob);
      // const el = document.getElementById('down');
      const el = document.getElementById(anchorElem.id);

      anchorElem.setAttribute('href', url);
      anchorElem.setAttribute('download', filename);
      anchorElem.style.display = 'none';
      anchorElem.click();

      setTimeout(function () {
        document.body.removeChild(anchorElem);
        window.URL.revokeObjectURL(url);
      }, 500);
    }
  }
}

