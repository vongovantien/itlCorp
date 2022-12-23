import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { SettingRepo } from '@repositories';
import { catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { ListFileManagementComponent } from '../components/list-file-management/list-file-management.component';
import { fileManagePaging } from '../general-file-management/general-file-management.component';

@Component({
    selector: 'app-accountant-file-management',
    templateUrl: './accountant-file-management.component.html',
})
export class AccountantFileManagementComponent extends AppList implements OnInit {

    @ViewChild(ListFileManagementComponent) listFile: ListFileManagementComponent;
    edocFiles: fileManagePaging = {
        data: [],
        page: 0,
        size: 0,
        totalItems: 0
    };
    headers: CommonInterface.IHeaderTable[] = [];

    constructor(
        private _settingRepo: SettingRepo,
        private _router: Router
    ) {
        super();
    }

    ngOnInit() {
    }

    onSearchFile(body: any) {
        body.Size = this.pageSize;
        body.Page = this.page;
        this.dataSearch = body;
        body.isAcc = true;
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


    directToGeneral() {
        this._router.navigate([`${RoutingConstants.TOOL.FILE_MANAGMENT}/general`]);
    }

    ReloadEDoc() {
        this.onSearchFile(this.dataSearch);
    }

    RepageEDoc(event) {
        this.page = event.page;
        this.pageSize = event.pageSize;
        this.onSearchFile(this.dataSearch);
    }

}

