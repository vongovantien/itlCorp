import { Component, OnInit, ViewChild } from '@angular/core';
import { SortService } from 'src/app/shared/services/sort.service';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { PagingService } from 'src/app/shared/services/paging-service';
import { SystemConstants } from 'src/constants/system.const';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { AppPage } from 'src/app/app.base';
import { InfoPopupComponent } from 'src/app/shared/common/popup';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-warehouse-import',
    templateUrl: './warehouse-import.component.html'
})
export class WarehouseImportComponent extends AppPage implements OnInit {
    @ViewChild(InfoPopupComponent, { static: false }) importAlert: InfoPopupComponent;
    @ViewChild(AppPaginationComponent, { static: false }) child: any;
    data: any[];
    pagedItems: any[] = [];
    inValidItems: any[] = [];
    totalValidRows: number = 0;
    totalRows: number = 0;
    isShowInvalid: boolean = true;
    pager: PagerSetting = PAGINGSETTING;

    isDesc = true;
    sortKey: string;
    closeButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel,
        buttonAttribute: {
            titleButton: "close",
            classStyle: "btn m-btn--square m-btn--icon m-btn--uppercase",
            icon: "la la-ban"
        }
    };

    constructor(private _catalogueRepo: CatalogueRepo,
        private pagingService: PagingService,
        private sortService: SortService,
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
        this._catalogueRepo.upLoadPlaceFile(file.target['files'], PlaceTypeEnum.Warehouse)
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
        this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
        this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
        this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
        this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
        console.log(this.pager);
    }

    downloadSample() {
        this._catalogueRepo.downloadPlaceExcel(PlaceTypeEnum.Warehouse)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "WarehouseImportTemplate.xlsx");
                },
            );
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

    import(element) {
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

    sort(property: string) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
    }

    setPage(pager: PagerSetting) {
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
            if (this.inValidItems.length === 0) {
                this.pager.totalItems = 1;
            }
        }
    }

    reset(element) {
        this.data = null;
        this.pagedItems = null;
        element.value = "";
        this.pager.totalItems = 0;
    }
}
