import { Component, OnInit, ViewChild } from '@angular/core';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgProgress } from '@ngx-progressbar/core';
import { PagingService } from 'src/app/shared/services/paging-service';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { SystemConstants } from 'src/constants/system.const';
import { CatalogueRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { AppPage } from 'src/app/app.base';
import { InfoPopupComponent } from '@common';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-partner-data-import',
    templateUrl: './partner-data-import.component.html'
})
export class PartnerDataImportComponent extends AppPage implements OnInit {
    @ViewChild(InfoPopupComponent, { static: false }) importAlert: InfoPopupComponent;
    @ViewChild(AppPaginationComponent, { static: false }) child;
    data: any[];
    pagedItems: any[] = [];
    inValidItems: any[] = [];
    totalValidRows: number = 0;
    totalRows: number = 0;
    isShowInvalid: boolean = true;
    pager: PagerSetting = PAGINGSETTING;
    inProgress: boolean = false;
    isDesc = true;
    sortKey: string;

    constructor(
        private pagingService: PagingService,
        private sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.pager.totalItems = 0;
    }
    chooseFile(file: Event) {
        this.pager.totalItems = 0;
        if (file.target['files'] == null) { return; }
        this._progressRef.start();
        this._catalogueRepo.upLoadPartnerFile(file.target['files'])
            .pipe(
                finalize(() => {
                    this._progressRef.complete();
                })
            )
            .subscribe((response: any) => {
                this.data = response.data;
                this.pager.totalItems = this.data.length;
                this.totalValidRows = response.totalValidRows;
                this.totalRows = this.data.length;
                this.pagingData(this.data);
            }, () => {
            });
    }

    pagingData(data: any[]) {
        this.pager = this.pagingService.getPager(data.length, this.pager.currentPage, this.pager.pageSize);
        this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
        this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
        this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }

    async import(element) {
        this._progressRef.start();
        if (this.data == null) { return; }
        if (this.totalRows - this.totalValidRows > 0) {
            this.importAlert.show();
        } else {
            const data = this.data.filter(x => x.isValid);
            this._catalogueRepo.importPlace(data)
                .pipe(
                    finalize(() => {
                        this._progressRef.complete();
                    })
                )
                .subscribe(
                    (res) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.pager.totalItems = 0;
                            this.reset(element);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
    }
    sort(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
    }
    hideInvalid() {
        if (this.data == null) { return; }
        this.isShowInvalid = !this.isShowInvalid;
        this.sortKey = '';
        if (this.isShowInvalid) {
            this.pager.totalItems = this.data.length;
        } else {
            this.inValidItems = this.data.filter(x => !x.isValid);
            this.pager.totalItems = this.inValidItems.length;
        }
        this.child.setPage(this.pager.currentPage);
    }
    reset(element) {
        this.data = null;
        this.pagedItems = null;
        element.value = "";
        this.pager.totalItems = 0;
    }
    async setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.pageSize = pager.pageSize;
        this.pager.totalPages = pager.totalPages;
        if (this.isShowInvalid) {
            this.pager = this.pagingService.getPager(this.data.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
            this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
            this.pagedItems = this.data.slice(this.pager.startIndex, this.pager.endIndex + 1);
        } else {
            this.pager = this.pagingService.getPager(this.inValidItems.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
            this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
            this.pagedItems = this.inValidItems.slice(this.pager.startIndex, this.pager.endIndex + 1);
        }
    }
    downloadSample() {
        this._catalogueRepo.downloadPartnerExcel()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "PartnerImportTemplate.xlsx");
                },
            );
    }
}
