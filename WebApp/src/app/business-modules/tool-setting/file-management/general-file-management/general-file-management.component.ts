import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { SettingRepo } from '@repositories';
import { catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { ListFileManagementComponent } from '../components/list-file-management/list-file-management.component';

@Component({
    selector: 'app-general-file-management',
    templateUrl: './general-file-management.component.html'
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
        private _router: Router,
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
                    console.log(res.data);

                    this.edocFiles.data = res.data || [];
                    this.listFile.totalItems = res.totalItems;
                    this.listFile.page = res.page;
                    this.listFile.pageSize = res.size;
                    this.page = 1;
                }
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'Acc':
                this._router.navigate([`${RoutingConstants.TOOL.FILE_MANAGMENT}/accountant`]);
                break;
            default:
                break;
        }
    }

    reset(event: any) {
        this.page = 1;
        this.listFile.page = 1;
        this.onSearchFile(event);
    }

    ReloadEDoc() {
        this.onSearchFile(this.dataSearch);
    }

    RepageEDoc(event) {
        this.page = event.page;
        this.pageSize = event.pageSize;
        this.dataSearch.isSearch = false;
        this.onSearchFile(this.dataSearch);
    }

}

export interface fileManagePaging {
    data: any[];
    page: number;
    size: number;
    totalItems: number;
}
