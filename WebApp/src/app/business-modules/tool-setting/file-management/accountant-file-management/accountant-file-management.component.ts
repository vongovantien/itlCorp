import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { SettingRepo } from '@repositories';
import { catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { EDocFile } from 'src/app/shared/models/tool-setting/edoc-file';

@Component({
    selector: 'app-accountant-file-management',
    templateUrl: './accountant-file-management.component.html',
})
export class AccountantFileManagementComponent extends AppList implements OnInit {

    listBreadcrumb: Array<object> = [];
    isDisplayFolderParent: boolean = false;
    edocFiles: EDocFile[] = [];
    headers: CommonInterface.IHeaderTable[] = [];
    dataSearch: any = null;
    constructor(
        private _settingRepo: SettingRepo,
        private _router: Router
    ) {
        super();
    }

    ngOnInit() {
    }
    onSearchFile(body: any) {
        this.dataSearch = body;
        body.isAcc = true;
        this._settingRepo.getEdocManagement(Object.assign({}, body))
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (res: any) => {
                    this.edocFiles = res || [];
                    console.log(this.edocFiles);
                },
            );
    }
    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'General':
                this._router.navigate([`${RoutingConstants.TOOL.FILE_MANAGMENT}/general-file-management`]);
                break;
            default:
                break;
        }
    }

    ReloadEDoc() {
        this.onSearchFile(this.dataSearch);
    }
}
