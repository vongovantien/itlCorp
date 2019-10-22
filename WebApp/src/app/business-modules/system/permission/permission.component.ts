import { Component, OnInit, ViewChild } from '@angular/core';
import { Permission } from 'src/app/shared/models';
import { SystemRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services';
import { AppList } from 'src/app/app.list';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'app-permission',
    templateUrl: './permission.component.html',
})
export class PermissionComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) configmDeletePopup: ConfirmPopupComponent;

    headers: CommonInterface.IHeaderTable[];
    permissions: Permission[] = [];

    dataSearch: any = {};

    selectedPermission: Permission;
    constructor(
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
        private _router: Router,
        private _toastService: ToastrService,
        private _sortService: SortService,
    ) {
        super();
        this._progressRef = this._progressService.ref();

        this.requestList = this.searchPermission;
        this.requestSort = this.sortPermission;
    }

    ngOnInit() {
        this.headers = [
            { field: 'name', title: 'Permission Name', sortable: true },
            { field: 'type', title: 'Type', sortable: true },
            { field: 'role', title: 'Role', sortable: true },
            { field: 'status', title: 'Status', sortable: true },
        ];

        this.searchPermission(this.dataSearch);
    }

    searchPermission(dataSearch?: any) {
        this.permissions.push(new Permission());
    }

    sortPermission() {
        this.permissions = this._sortService.sort(this.permissions, this.sort, this.order);
    }

    deletePermission(permission: Permission) {
        this.selectedPermission = new Permission(permission);
        this.configmDeletePopup.show();
    }

    onDeletePermission() {

    }

}
