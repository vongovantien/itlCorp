import { Component, OnInit, ViewChild } from '@angular/core';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagingService, BaseService, SortService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { ToastrService } from 'ngx-toastr';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { NgProgressComponent } from '@ngx-progressbar/core';
import { SystemConstants } from 'src/constants/system.const';
import { AppList } from 'src/app/app.list';
import { User } from 'src/app/shared/models';
import { language } from 'src/languages/language.en';
import { Employee } from 'src/app/shared/models/system/employee';
import { AppPaginationComponent, InfoPopupComponent } from 'src/app/shared/common';
import { SystemRepo } from '@repositories';
import { catchError } from 'rxjs/operators';

@Component({
    selector: 'app-user-management-import',
    templateUrl: './user-management-import.component.html',
})
export class UserManagementImportComponent extends AppList {
    constructor(
        private pagingService: PagingService,
        private baseService: BaseService,
        private api_menu: API_MENU,
        private _systemRepo:SystemRepo,
        private toastr: ToastrService) { super(); }
    data: any[];
    pagedItems: any[] = [];
    inValidItems: any[] = [];
    sysUser: User[] = [];
    totalValidRows: number = 0;
    totalRows: number = 0;
    isShowInvalid: boolean = true;
    pager: PagerSetting = PAGINGSETTING;
    inProgress: boolean = false;
    headers: CommonInterface.IHeaderTable[];
    @ViewChild(AppPaginationComponent, { static: false }) child;
    @ViewChild(NgProgressComponent, { static: false }) progressBar: NgProgressComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;

    isDesc = true;
    sortKey: string;


    ngOnInit() {
        this.headers = [
            { title: 'No', field: 'no', width: 100 },
            { title: 'Staff Code', field: 'staffCode', sortable: true },
            { title: 'User Name', field: 'username', sortable: true },
            { title: 'Full Name', field: 'employeeNameEn', sortable: true },
            { title: 'Name EN', field: 'employeeNameEn', sortable: true },
            { title: 'Title', field: 'title', sortable: true },
            { title: 'User Type', field: 'userType', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
            { title: 'Working Status', field: 'workingstatus', sortable: true },
            { title: 'Email', field: 'email', sortable: true },
            { title: 'Phone Number', field: 'tel', sortable: true },
            { title: 'Description', field: 'description', sortable: true }
        ];
    }

    resetBeforeSelecedFile() {
        this.pager.currentPage = 1;
        this.pager.totalItems = 0;
        this.totalRows = this.totalValidRows = 0;
        this.pagedItems = [];
        this.isShowInvalid = true;
    }

    chooseFile(file: Event) {
        if (file.target['files'] == null) return;
        this.progressBar.start();
        /**/
        this.resetBeforeSelecedFile();
        /**/
        this.baseService.uploadfile(this.api_menu.System.User_Management.uploadExel, file.target['files'], "uploadedFile")
            .subscribe((response: any) => {
                console.log(response);
                this.data = response.data;
                this.pager.totalItems = this.data.length;
                this.totalValidRows = response.totalValidRows;
                this.totalRows = this.data.length;
                this.pagingData(this.data);
                this.progressBar.complete();
                console.log(this.data);
            }, err => {
                this.progressBar.complete();
                this.baseService.handleError(err);
            });
    }

    pagingData(data: any[]) {
        this.pager = this.pagingService.getPager(data.length, this.pager.currentPage, this.pager.pageSize);
        this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
        this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
        this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }

    downloadSample() {
        this._systemRepo.downloadUserExcel()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "User-Import.xlsx");
                },
            );
    }

    setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.pageSize = pager.pageSize;
        this.pager.totalPages = pager.totalPages;

        if (this.isShowInvalid) {
            this.pagingData(this.data);
        } else {
            this.pagingData(this.inValidItems);
        }
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

    async import() {
        if (this.data == null || this.data == undefined) {
            this.toastr.warning('Not selected import file', '', { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 5000 });
            return;
        }
        if (this.totalRows - this.totalValidRows > 0) {
            this.infoPopup.show();
        } else {
            this.progressBar.start();
            console.log(this.data);
            this.data.forEach(element => {
                if (element.status === 'Active') {
                    element.active = true;
                } else {
                    element.active = false;
                }
            });
            const response = await this.baseService.postAsync(this.api_menu.System.User_Management.import, this.data, true, false);
            if (response.success) {
                this.baseService.successToast(language.NOTIFI_MESS.IMPORT_SUCCESS);
                this.reset();
                this.progressBar.complete();
            } else {
                this.progressBar.complete();
                this.baseService.handleError(response);
            }
            console.log(response);
        }
    }

    reset() {
        this.data = null;
        this.pagedItems = null;
        this.pager.totalItems = 0;
        this.totalRows = this.totalValidRows = 0;
    }
}
export interface IAddUser {
    sysEmployeeModel: Employee;
    username: string;
    userType: string;
    workingStatus: string;
    isLdap: boolean;
    active: boolean;
}
