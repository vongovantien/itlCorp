import { Component, OnInit, ViewChild } from '@angular/core';
import { PagingService } from 'src/app/shared/services/paging-service';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgProgressComponent, NgProgress } from '@ngx-progressbar/core';
import { SystemConstants } from 'src/constants/system.const';
import { language } from 'src/languages/language.en';
import { ToastrService } from 'ngx-toastr';
import { OperationRepo } from '@repositories';
import { finalize } from 'rxjs/operators';
import { AppPage } from 'src/app/app.base';
declare var $: any;

@Component({
    selector: 'app-custom-clearance-import',
    templateUrl: './custom-clearance-import.component.html',
})
export class CustomClearanceImportComponent extends AppPage implements OnInit {
    data: any[];
    pagedItems: any[] = [];
    inValidItems: any[] = [];
    totalValidRows: number = 0;
    totalRows: number = 0;
    isShowInvalid: boolean = true;
    pager: PagerSetting = PAGINGSETTING;
    inProgress: boolean = false;
    @ViewChild(AppPaginationComponent, { static: false }) child;
    @ViewChild(NgProgressComponent, { static: false }) progressBar: NgProgressComponent;
    constructor(
        private pagingService: PagingService,
        private baseService: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService,
        private toastr: ToastrService,
        private _operationRepo: OperationRepo,
        private _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.pager.totalItems = 0;
    }

    chooseFile(file: Event) {
        if (file.target['files'] == null) { return; }
        this.progressBar.start();
        /**/
        this.resetBeforeSelecedFile();
        /**/
        this._operationRepo.upLoadClearanceFile(file.target['files'])
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
                this.progressBar.complete();
            }, () => {
            });
    }

    pagingData(data: any[]) {
        this.pager = this.pagingService.getPager(data.length, this.pager.currentPage, this.pager.pageSize);
        this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
        this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
        this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }

    async setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.pageSize = pager.pageSize;
        this.pager.totalPages = pager.totalPages;

        if (this.isShowInvalid) {
            this.pagingData(this.data);
        } else {
            this.pagingData(this.inValidItems);
        }
    }

    async downloadSample() {
        await this.baseService.downloadfile(this.api_menu.Operation.CustomClearance.downloadExcel, 'CustomClearanceImportTemplate.xlsx');
    }

    isDesc = true;
    sortKey: string;
    sort(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
    }

    hideInvalid() {
        if (this.data == null || this.data == undefined) return;
        this.isShowInvalid = !this.isShowInvalid;
        this.sortKey = '';
        if (this.isShowInvalid) {
            this.pager.totalItems = this.data.length;
            this.pagingData(this.data);
        } else {
            this.inValidItems = this.data.filter(x => !x.isValid);
            this.pager.totalItems = this.inValidItems.length;
            this.setPage(this.pager);
        }

        this.pager.currentPage = 1;
        if (this.inValidItems.length > 0) {
            this.child.setPage(this.pager.currentPage);
        }
    }

    reset() {
        this.data = null;
        this.pagedItems = null;
        $("#inputFile").val('');
        this.pager.totalItems = 0;
        this.totalRows = this.totalValidRows = 0;
    }

    resetBeforeSelecedFile() {
        this.pager.currentPage = 1;
        this.pager.totalItems = 0;
        this.totalRows = this.totalValidRows = 0;
        this.pagedItems = [];
        this.isShowInvalid = true;
    }

    async import() {
        if (this.data == null || this.data == undefined) {
            this.toastr.warning('Not selected import file', '', { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 5000 });
            return;
        }
        if (this.totalRows - this.totalValidRows > 0) {
            $('#upload-alert-modal').modal('show');
        }
        else {
            this.progressBar.start();
            console.log(this.data);
            var response = await this.baseService.postAsync(this.api_menu.Operation.CustomClearance.import, this.data, true, false);
            if (response.success) {
                this.baseService.successToast(language.NOTIFI_MESS.IMPORT_SUCCESS);
                this.reset();
                this.progressBar.complete();
            }
            else {
                this.progressBar.complete();
                this.baseService.handleError(response);
            }
            console.log(response);
        }
    }

}
