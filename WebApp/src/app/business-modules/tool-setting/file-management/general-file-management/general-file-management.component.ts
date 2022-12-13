import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { SettingRepo, SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { ListFileManagementComponent } from '../components/list-file-management/list-file-management.component';

@Component({
    selector: 'app-general-file-management',
    templateUrl: './general-file-management.component.html',
})
export class GeneralFileManagementComponent extends AppList implements OnInit {

    @ViewChild(ListFileManagementComponent) listFile: ListFileManagementComponent;
    listBreadcrumb: Array<object> = [];
    isDisplayFolderParent: boolean = false;
    edocFiles: fileManagePaging = {
        data: [],
        page: 0,
        size: 0,
        totalItems: 0
    };
    headers: CommonInterface.IHeaderTable[] = [];
    dataSearch: any = null;
    TotalItems: number = 0;
    constructor(
        private _settingRepo: SettingRepo,
        private _systemFileRepo: SystemFileManageRepo,
        private _router: Router,
        private _toast: ToastrService,
    ) {
        super();
    }

    ngOnInit() {
    }

    onSearchFile(body: any) {
        body.Size = this.pageSize;
        body.Page = this.page;
        this.dataSearch = body;
        body.isAcc = false;
        this._settingRepo.getEdocManagement(Object.assign({}, body))
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (res: any) => {
                    this.edocFiles.data = res.data || [];
                    this.listFile.totalItems = res.totalItems;
                    this.listFile.page = res.page;
                    this.listFile.pageSize = res.size;
                }
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'Acc':
                this._router.navigate([`${RoutingConstants.TOOL.FILE_MANAGMENT}/accountant-file-management`]);
                break;
            default:
                break;
        }
    }

    ReloadEDoc() {
        this.onSearchFile(this.dataSearch);
    }

    RepageEDoc(event) {
        console.log(event);
        this.page = event.page;
        this.pageSize = event.size;
        this.onSearchFile(this.dataSearch);
    }

}

export interface fileManagePaging {
    data: any[];
    page: number;
    size: number;
    totalItems: number;
}
