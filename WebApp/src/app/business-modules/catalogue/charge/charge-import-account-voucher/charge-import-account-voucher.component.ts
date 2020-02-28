import { Component, OnInit, ViewChild } from '@angular/core';
import { SortService, PagingService } from '@services';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { SystemConstants } from 'src/constants/system.const';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgProgress, NgProgressComponent } from '@ngx-progressbar/core';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { AppPage } from 'src/app/app.base';
import { finalize, catchError } from 'rxjs/operators';
import { InfoPopupComponent } from '@common';

@Component({
    selector: 'app-charge-import-account-voucher',
    templateUrl: './charge-import-account-voucher.component.html'
})
export class ChargeImportAccountVoucherComponent extends AppPage implements OnInit {

    data: any[];
    pagedItems: any[] = [];
    inValidItems: any[] = [];
    totalValidRows: number = 0;
    totalRows: number = 0;
    isShowInvalid: boolean = true;
    pager: PagerSetting = PAGINGSETTING;
    isDesc = true;
    sortKey: string;
    @ViewChild(InfoPopupComponent, { static: false }) importAlert: InfoPopupComponent;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private pagingService: PagingService,
        private sortService: SortService,
        private _progressService: NgProgress,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }
    @ViewChild(AppPaginationComponent, { static: false }) child: any;
    @ViewChild('form', { static: false }) form: any;
    @ViewChild(NgProgressComponent, { static: false }) progressBar: NgProgressComponent;

    ngOnInit() {
        this.pager.totalItems = 0;
    }

    chooseFile(file: Event) {
        this.pager.totalItems = 0;
        if (file.target['files'] == null) { return; }
        this._progressRef.start();
        this._catalogueRepo.upLoadChargeVoucher(file.target['files'])
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
        }
    }


    sort(property: string) {
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


    import(element) {
        if (this.data == null) { return; }
        if (this.totalRows - this.totalValidRows > 0) {
            this.importAlert.show();
            this._progressRef.complete();
        } else {
            const data = this.data.filter(x => x.isValid);
            this._progressRef.start();
            this._catalogueRepo.importChargeVoucher(data)
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

    downloadSample() {
        this._catalogueRepo.downloadChargeVoucherExcel()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "VoucherTypeAccountTemplate.xlsx");
                },
            );
    }

    reset(element) {
        this.data = null;
        this.pagedItems = null;
        element.value = "";
        this.pager.totalItems = 0;
    }
}
