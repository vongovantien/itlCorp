import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router'; import { Office } from '@models';
;
import { AccountingRepo, SettingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { listenerCount } from 'process';
import { catchError, finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

export interface IFileItem { name: string, dateCreated: string, userModified: string, folderName: string, url: string, classIcon: string, objectId: string }
@Component({
    selector: 'accouting-file-management',
    templateUrl: './accouting-file-management.component.html'
})

export class AccoutingFileManagementComponent extends AppList implements OnInit {
    itemsDefault: IFileItem[] = [{
        name: "SOA Folder",
        dateCreated: "19/03/2022",
        userModified: "22/03/2022",
        folderName: "SOA",
        classIcon: "la la-folder",
        url: "",
        objectId: ""
    },
    {
        name: "Settlement Folder",
        dateCreated: "19/03/2022",
        userModified: "22/03/2022",
        folderName: "Settlement",
        classIcon: "la la-folder",
        url: "",
        objectId: ""
    },
    {
        name: "Advance Folder",
        dateCreated: "19/03/2022",
        userModified: "22/03/2022",
        folderName: "Advance",
        classIcon: "la la-folder",
        url: "",
        objectId: ""
    }]

    isActiveClick: boolean = false;
    isActiveSearch: boolean = false;
    folderName: string;
    itemSelect: string;

    constructor(private _settingRepo: SettingRepo, private readonly _toastService: ToastrService, private _router: Router, private _accountingRepo: AccountingRepo) {
        super();
    }


    ngOnInit() {
    }

    pushTypeForItem(items: any) {
        for (let item of items) {
            let arr = item.name.split(".");
            switch (arr[arr.length - 1]) {
                case 'pdf':
                    item.classIcon = "la la-file-pdf-o"
                    break;
                case 'xlsx':
                    item.classIcon = "la la-file-excel-o"
                    break;
                default:
                    item.classIcon = "la la-file-image-o"
            }
        }
        this.itemsDefault = items;
    }

    onGetFolerItems(data: IFileItem) {
        if (this.isActiveClick == false) {
            this.isActiveClick = !this.isActiveClick;
            this.folderName = data.folderName;
            this.dataSearch = { folder: data.folderName };
            this.getFolderFileManagement();
        }

    }

    getFolderFileManagement() {
        this.isActiveSearch = true;
        this._settingRepo.getListFileManagement(this.page, this.pageSize, this.dataSearch)
            .pipe(catchError(this.catchError), finalize(() => { this.isLoading = false; this._progressRef.complete(); }),)
            .subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.pushTypeForItem(res.data);
                },
            );
    }
    getFolderFileByObjectIDs() {
        this.getFolderFileManagement();
        let datas;
        this.itemsDefault.map(s => datas.push(s.objectId));
        this._accountingRepo.getListSOANoByIds(datas);
    }

    onSelectFile(item: string) {
        this.itemSelect = item;
    }
    onRedirectLink(item: string) {
        window.open(`${item}`, "_blank");
    }

    onSearchValue(event: { field: string; searchString: any; }) {
        this.dataSearch = event;
        if (this.folderName != null && this.folderName != undefined) {
            this.dataSearch.folder = this.folderName;
        }
        this.getFolderFileManagement();
    }
}
