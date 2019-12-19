import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgProgressComponent } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';

import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { SystemConstants } from 'src/constants/system.const';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { language } from 'src/languages/language.en';
import { PagingService, SortService } from 'src/app/shared/services';
import { InfoPopupComponent } from 'src/app/shared/common/popup';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { AppList } from 'src/app/app.list';
import { CommonEnum } from 'src/app/shared/enums/common.enum';

import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'app-location-import',
    templateUrl: './location-import.component.html'
})
export class LocationImportComponent extends AppList implements OnInit {

    @ViewChild(AppPaginationComponent, { static: false }) child: any;
    @ViewChild(NgProgressComponent, { static: false }) progressBar: NgProgressComponent;
    @ViewChild(InfoPopupComponent, { static: false }) invaliDataAlert: InfoPopupComponent;

    data: any[];
    pagedItems: any[] = [];
    inValidItems: any[] = [];

    totalValidRows: number = 0;
    totalRows: number = 0;

    isShowInvalid: boolean = true;
    pager: PagerSetting = PAGINGSETTING;

    type: string;
    inputFile: string;

    isDesc = true;
    sortKey: string;

    constructor(
        private pagingService: PagingService,
        private sortService: SortService,
        private route: ActivatedRoute,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit() {
        this.pager.totalItems = 0;
        this.route.queryParams.subscribe(prams => {
            if (prams.type !== undefined) {
                this.type = prams.type;
            }
        });
    }

    chooseFile(file: Event) {
        if (file.target['files'] == null) { return; }

        this.progressBar.start();
        let placeType: number = CommonEnum.PlaceTypeEnum.Province;

        if (this.type === 'country') {
            this._catalogueRepo.uploadCountry(file.target['files'])
                .pipe(catchError(this.catchError), finalize(() => this.progressBar.complete()))
                .subscribe(
                    (res: { data: any[], totalValidRows: number }) => {
                        this.data = res.data;
                        this.pager.totalItems = this.data.length;
                        this.totalValidRows = res.totalValidRows;
                        this.totalRows = this.data.length;
                        this.pagingData(this.data);
                    }
                );
        } else {
            switch (this.type) {
                case 'province':
                    placeType = CommonEnum.PlaceTypeEnum.Province;
                    break;
                case 'district':
                    placeType = CommonEnum.PlaceTypeEnum.District;
                    break;
                case 'ward':
                    placeType = CommonEnum.PlaceTypeEnum.Ward;
                    break;
                default:
                    break;
            }
            this._catalogueRepo.upLoadPlaceFile(file.target['files'], placeType)
                .pipe(catchError(this.catchError), finalize(() => this.progressBar.complete()))
                .subscribe(
                    (res: { data: any[], totalValidRows: number }) => {
                        this.data = res.data;
                        this.pager.totalItems = this.data.length;
                        this.totalValidRows = res.totalValidRows;
                        this.totalRows = this.data.length;
                        this.pagingData(this.data);
                    }
                );
        }

    }

    pagingData(data: any[]) {
        this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
        this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
        this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
        this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }

    downloadSample() {
        let fileName = 'ImportTemplate.xlsx';
        let placeType = CommonEnum.PlaceTypeEnum.Province;

        if (this.type === 'country') {
            fileName = "Country" + fileName;
            this._catalogueRepo.downloadExcelTemplateCountry()
                .subscribe(
                    (res: any) => {
                        this.downLoadFile(res, SystemConstants.FILE_EXCEL, fileName);
                    }
                );
        } else {
            switch (this.type) {
                case 'province':
                    fileName = "Province" + fileName;
                    break;
                case 'district':
                    fileName = "District" + fileName;
                    placeType = CommonEnum.PlaceTypeEnum.District;
                    break;
                case 'ward':
                    fileName = "Ward" + fileName;
                    placeType = CommonEnum.PlaceTypeEnum.Ward;
                    break;
                default:
                    break;
            }

            this._catalogueRepo.downloadPlaceExcel(placeType)
                .subscribe(
                    (res: any) => {
                        this.downLoadFile(res, SystemConstants.FILE_EXCEL, fileName);
                    }
                );
        }
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

    hideInvalid() {
        if (this.data == null) { return; }

        this.isShowInvalid = !this.isShowInvalid;

        if (this.isShowInvalid) {
            this.inValidItems = this.data.filter(x => !x.isValid);
            this.pager.totalItems = this.inValidItems.length;
        } else {
            this.inValidItems = this.data.filter(x => !x.isValid);
        }

        this.child.setPage(this.pager.currentPage);
    }

    import() {
        if (this.data == null) { return; }
        if (this.totalRows - this.totalValidRows > 0) {
            this.invaliDataAlert.show();
        } else {
            const data = this.data.filter(x => x.isValid);
            let placeType: number = CommonEnum.PlaceTypeEnum.Province;

            if (this.type === 'country') {
                this._catalogueRepo.importCountry(data)
                    .subscribe(
                        (res: any) => {
                            if (res.success) {
                                this._toastService.success(language.NOTIFI_MESS.IMPORT_SUCCESS);
                                this.pager.totalItems = 0;
                                this.reset();
                            }
                        }
                    );
            } else {
                switch (this.type) {
                    case 'province':
                        break;
                    case 'district':
                        placeType = CommonEnum.PlaceTypeEnum.District;
                        break;
                    case 'ward':
                        placeType = CommonEnum.PlaceTypeEnum.Ward;
                        break;
                    default:
                        break;
                }

                this._catalogueRepo.importPlace(data)
                    .subscribe(
                        (res: any) => {
                            if (res.success) {
                                this._toastService.success(language.NOTIFI_MESS.IMPORT_SUCCESS);
                                this.pager.totalItems = 0;
                                this.reset();
                            }
                        }
                    );
            }
        }
    }

    reset() {
        this.data = null;
        this.pagedItems = null;
        this.inputFile = null;
        this.pager.totalItems = 0;
    }

    sortData(property: string) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
    }
}
