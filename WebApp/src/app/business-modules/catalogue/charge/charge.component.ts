import { Component, OnInit, ViewChild } from '@angular/core';
import _map from 'lodash/map';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { Router } from '@angular/router';
import { SortService } from 'src/app/shared/services/sort.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
import { AppList } from 'src/app/app.list';
import { CatalogueRepo, ExportRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map, tap } from 'rxjs/operators';
import { Charge } from 'src/app/shared/models';
import { NgProgress } from '@ngx-progressbar/core';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-charge',
    templateUrl: './charge.component.html',
    styleUrls: ['./charge.component.scss']
})
export class ChargeComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: any;
    headers: CommonInterface.IHeaderTable[];
    dataSearch: any = {};

    constructor(
        private _progressService: NgProgress,
        private sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
        private _exportRepo: ExportRepo,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.searchCharge;
        this.requestSort = this.sortCharge;
    }
    pager: PagerSetting = PAGINGSETTING;
    ListCharges: any = [];
    idChargeToUpdate: any = "";
    idChargeToDelete: any = "";
    idChargeToAdd: any = "";
    searchKey: string = "";
    searchObject: any = {};

    ngOnInit() {
        this.headers = [
            { title: 'Code', field: 'code', sortable: true },
            { title: 'Name EN', field: 'chargeNameEn', sortable: true },
            { title: 'Name Local', field: 'chargeNameVn', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Status', field: 'active', sortable: true }
        ];
        this.searchCharge(this.dataSearch);
    }
    sortCharge(sort: string): void {
        this.ListCharges = this.sortService.sort(this.ListCharges, sort, this.order);
    }

    onSearchCharge(dataSearch: any) {
        this.dataSearch = {};
        if (dataSearch.type === 'All') {
            this.dataSearch.all = dataSearch.keyword;
        } else {
            this.dataSearch.all = null;
            if (dataSearch.type === 'code') {
                this.dataSearch.code = dataSearch.keyword;
            }
            if (dataSearch.type === 'chargeNameEn') {
                this.dataSearch.chargeNameEn = dataSearch.keyword;
            }
            if (dataSearch.type === 'chargeNameVn') {
                this.dataSearch.chargeNameVn = dataSearch.keyword;
            }
            if (dataSearch.type === 'type') {
                this.dataSearch.type = dataSearch.keyword;
            }
        }
        this.searchCharge(this.dataSearch);
    }

    searchCharge(dataSearch: any) {
        this.isLoading = true;
        this._progressRef.start();
        this._catalogueRepo.getListCharge(this.page, this.pageSize, Object.assign({}, dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((res: any) => {
                    return {
                        data: res.data != null ? res.data.map((item: any) => new Charge(item)) : [],
                        totalItems: res.totalItems,
                    };
                }),
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems;
                    this.ListCharges = res.data;
                },
            );
    }

    prepareDeleteCharge(id: any) {
        this.idChargeToDelete = id;
        this.confirmDeletePopup.show();
    }

    onDeleteCharge() {
        this.confirmDeletePopup.hide();
        this.isLoading = true;
        this._progressRef.start();
        this._catalogueRepo.deleteCharge(this.idChargeToDelete)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.searchCharge(this.dataSearch);
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }

    public itemsToString(value: Array<any> = []): string {
        return value
            .map((item: any) => {
                return item.text;
            }).join(',');
    }

    export() {
        this._exportRepo.exportCharge(this.dataSearch)
            .subscribe(
                (response: ArrayBuffer) => {
                    this.downLoadFile(response, "application/ms-excel", 'Charge.xlsx');
                },
                (errors: any) => {
                },
                () => { }
            );
    }
}
