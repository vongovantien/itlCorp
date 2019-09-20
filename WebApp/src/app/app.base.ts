import { OnInit, OnDestroy, OnChanges, DoCheck, AfterContentChecked, AfterContentInit, AfterViewChecked, AfterViewInit } from "@angular/core";
import { Observable, Subject, throwError } from "rxjs";
import { HttpErrorResponse } from "@angular/common/http";

import { UtilityHelper } from "src/helper";
import { NgProgressRef } from "@ngx-progressbar/core";
import { ButtonModalSetting } from "./shared/models/layout/button-modal-setting.model";
import { ButtonType } from "./shared/enums/type-button.enum";

export abstract class AppPage implements OnInit, OnDestroy, OnChanges, DoCheck, AfterContentChecked, AfterContentInit, AfterViewChecked, AfterViewInit {

    ngUnsubscribe: Subject<any> = new Subject();
    keyword: string = '';

    utility: UtilityHelper = new UtilityHelper();

    isLoading: boolean = false;
    isCheckAll: boolean = false;

    _progressRef: NgProgressRef;

    cancelButtonSetting: ButtonModalSetting = {
        buttonAttribute: {
            type: 'button',
            titleButton: "Cancel",
            classStyle: "btn btn-default m-btn--square m-btn--icon m-btn--uppercase",
            icon: "la la-ban"
        },
        typeButton: ButtonType.cancel,
    };

    searchlButtonSetting: ButtonModalSetting = {
        buttonAttribute: {
            type: 'button',
            titleButton: "Search",
            classStyle: "btn btn-brand m-btn--square m-btn--icon m-btn--uppercase",
            icon: "la la-search"
        },
        typeButton: ButtonType.search,
    };

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

    handleError(errors?: any, callBack?: Function) {
        let message: string = 'Has Error Please Check Again !';
        let title: string = '';
        if (errors instanceof HttpErrorResponse) {
            message = errors.message;
            title = errors.statusText;
        }
        return callBack({ message, title });
        // this._toastService.error(message, title, { positionClass: 'toast-bottom-right' });
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

