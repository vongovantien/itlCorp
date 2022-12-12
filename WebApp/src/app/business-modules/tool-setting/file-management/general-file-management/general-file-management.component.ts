import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { SettingRepo, SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { EDocFile } from 'src/app/shared/models/tool-setting/edoc-file';

@Component({
    selector: 'app-general-file-management',
    templateUrl: './general-file-management.component.html',
})
export class GeneralFileManagementComponent extends AppList implements OnInit {

    listBreadcrumb: Array<object> = [];
    isDisplayFolderParent: boolean = false;
    edocFiles: EDocFile[] = [];
    headers: CommonInterface.IHeaderTable[] = [];
    dataSearch: any = null;
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
        this.dataSearch = body;
        body.isAcc = false;
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


}
