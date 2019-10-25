import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
import { User } from 'src/app/shared/models/system/user';
import { map } from 'rxjs/internal/operators/map';
import { NgProgress } from '@ngx-progressbar/core';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services';
import { Router } from '@angular/router';
import { ExportRepo } from 'src/app/shared/repositories';

@Component({
    selector: 'app-user-management',
    templateUrl: './user-management.component.html'
})
export class UserManagementComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    criteria: any = {};
    users: User[] = [];
    selectedUser: User;
    headers: CommonInterface.IHeaderTable[];
    exportButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.export
    };
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };
    importButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.import
    };

    cancelButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel
    };
    addButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.add
    };

    constructor(
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _router: Router,
        private _exportRepo: ExportRepo

    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.searchUser;
        this.requestSort = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'User Name', field: 'username', sortable: true, width: 100 },
            { title: 'Name EN', field: 'employeeNameEn', sortable: true },
            { title: 'Title', field: 'title', sortable: true },
            { title: 'User Type', field: 'userType', sortable: true },
            { title: 'Role', field: 'role', sortable: true },
            { title: 'Level permission', field: '', sortable: true },
            { title: 'Company', field: 'company', sortable: true },
            { title: 'Office', field: 'office', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
        ];
        this.dataSearch = {
            type: 'All'
        };
        this.criteria.all = null;
        this.searchUser();
    }

    sortLocal(sort: string): void {
        this.users = this._sortService.sort(this.users, sort, this.order);
    }


    onSearchUser(dataSearch: any) {
        this.dataSearch = dataSearch;
        this.criteria = {};
        if (this.dataSearch.type === 'All') {
            this.criteria.all = this.dataSearch.keyword;

        }
        else {
            this.criteria.all = null;
        }
        if (this.dataSearch.type === 'username') {

            this.criteria.username = this.dataSearch.keyword;
        }
        if (this.dataSearch.type === 'employeeNameEn') {

            this.criteria.employeeNameEn = this.dataSearch.keyword;
        }
        if (this.dataSearch.type === 'active') {
            this.criteria.active = this.dataSearch.keyword === "Active" ? true : false;
            // this.criteria.active = this.dataSearch.keyword;
        }
        this.searchUser();
    }


    searchUser() {
        this.isLoading = true;
        this._progressRef.start();
        this._systemRepo.getUser(this.page, this.pageSize, Object.assign({}, this.criteria))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((data: any) => {
                    if (data.data != null) {
                        return {
                            data: data.data.map((item: any) => new User(item)),
                            totalItems: data.totalItems,
                        };
                    }
                    return {
                        data: new User(),
                        totalItems: 0,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.users = res.data;
                    console.log(this.users);
                },
            );
    }

    showDeletePopup(user: User) {
        this.confirmDeletePopup.show();
        this.selectedUser = user;
    }

    onDeleteUser() {
        this.confirmDeletePopup.hide();
        this.deleteUser(this.selectedUser.id);
    }

    deleteUser(id: string) {
        this.isLoading = true;
        this._progressRef.start();
        this._systemRepo.deleteUser(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.searchUser();
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }

    gotoDetailUser(id: number) {
        this._router.navigate([`home/system/user-management/${id}`]);
    }

    export() {
        this._exportRepo.exportUser(this.criteria)
            .subscribe(
                (response: ArrayBuffer) => {
                    this.downLoadFile(response, "application/ms-excel", 'User.xlsx');
                },
                (errors: any) => {
                },
                () => { }
            );
    }



}
