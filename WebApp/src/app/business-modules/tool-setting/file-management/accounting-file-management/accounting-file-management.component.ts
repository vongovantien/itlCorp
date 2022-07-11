import { listAnimation } from './../../../../shared/animations/index';
import { Component, OnChanges, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { SettingRepo } from "@repositories";
import { ToastrService } from "ngx-toastr";
import { catchError, finalize } from "rxjs/operators";
import { AppList } from "src/app/app.list";

export interface IFileItem {
    name: string;
    dateCreated: string;
    userModified: string;
    folderName: string;
    url: string;
    classIcon: string;
    objectId: string;
    classColor: string;
}
@Component({
    selector: "accounting-file-management",
    templateUrl: "./accounting-file-management.component.html",
})
export class AccountingFileManagementComponent extends AppList implements OnInit, OnChanges {
    itemsDefault: IFileItem[];
    dataDefault: IFileItem[] = [
        {
            name: "SOA Folder",
            dateCreated: "19/03/2022",
            userModified: "22/03/2022",
            folderName: "SOA",
            classIcon: "la la-folder",
            url: "",
            classColor: "text-info",
            objectId: "",
        },
        {
            name: "Settlement Folder",
            dateCreated: "19/03/2022",
            userModified: "22/03/2022",
            folderName: "Settlement",
            classIcon: "la la-folder",
            classColor: "text-info",
            url: "",
            objectId: "",
        },
        {
            name: "Advance Folder",
            dateCreated: "19/03/2022",
            userModified: "22/03/2022",
            folderName: "Advance",
            classIcon: "la la-folder",
            url: "",
            classColor: "text-info",
            objectId: "",
        },
    ];

    listFolderName: IFileItem | any = [];
    stringBreadcrumb: string;
    folderName: string;
    folderChild: any;
    isActiveClick: boolean = false;
    isDisplayFolderParent: boolean = false;
    isActiveSearch: boolean = false;
    itemSelect: string;
    listBreadcrumb: Array<object> = [];
    constructor(
        private _settingRepo: SettingRepo,
        private readonly _toastService: ToastrService,
        private _router: Router
    ) {
        super();
        this.requestList = this.getListFolderName;
    }

    ngOnInit() {
        this.itemsDefault = this.dataDefault;
    }
    ngOnChanges(): void {
    }
    pushTypeForItem(items: any) {
        for (let item of items) {
            let arr = item.name.split(".");
            switch (arr[arr.length - 1]) {
                case "pdf":
                    item.classIcon = "la la-file-pdf-o";
                    item.classColor = "text-danger"
                    break;
                case "xlsx":
                    item.classIcon = "la la-file-excel-o";
                    item.classColor = "text-success"
                    break;
                default:
                    item.classIcon = "la la-file-image-o";
                    item.classColor = "text-primary"
            }
        }
        this.itemsDefault = items;
    }

    onGetFolderItems(data: any) {
        if (this.isActiveClick == false) {
            this.isActiveClick = true;
            this.isDisplayFolderParent = false;
            this.isActiveSearch = false;
            this.getDetailFileManagement(this.folderName, data.objectId);
        }
        this.listBreadcrumb.push(data);
    }

    getFolderFileManagement() {
        this._settingRepo
            .getListFileByFolderName(this.page, this.pageSize, this.dataSearch)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe((res: any) => {
                this.totalItems = res.totalItems || 0;
                this.pushTypeForItem(res.data);
            });
    }

    getDetailFileManagement(folderName: string, objectId: string) {
        this._settingRepo.getDetailFileManagement(folderName, objectId).pipe(
            catchError(this.catchError),
            finalize(() => { })
        )
            .subscribe((res: any) => {
                this.pushTypeForItem(res);
            });
    }

    getListFolderName() {
        this._settingRepo
            .getListFolderName(this.folderName, this.page, this.pageSize)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe((res: any) => {
                this.totalItems = res.totalItems || 0;
                this.listFolderName = res.data;
            });
    }

    onSelectFile(item: string) {
        this.itemSelect = item;
    }

    onRedirectLink(item: string) {
        window.open(`${item}`, "_blank");
    }

    onSearchValue($event) {
        console.log($event)
        console.log(this.dataSearch)
        if ($event === undefined) {
            this.dataSearch.name = "";
        }
        else {
            this.dataSearch.name = $event.name;

        }

        if (this.folderName != null && this.folderName != undefined) {
            this.dataSearch.folder = this.folderName;
        }
        console.log(this.dataSearch)

        this._settingRepo
            .searchListFolderName(this.folderName, this.dataSearch.name, this.page, this.pageSize)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe((res: any) => {
                this.totalItems = res.totalItems || 0;
                this.listFolderName = res.data;
            });
    }

    onDisplayListFolder(item: any) {
        this.isDisplayFolderParent = true;
        this.isActiveSearch = true;
        this.stringBreadcrumb = this.folderName;
        this.folderName = item.folderName
        this.getListFolderName();
        this.listBreadcrumb.push({ folderName: this.folderName })
    }

    onDisplayDefaultFolder() {
        this.isDisplayFolderParent = true;
        console.log()
    }

    getValueBreadcrumb($event: any) {
        if ($event === "Accounting") {
            this.isDisplayFolderParent = false;
            this.itemsDefault = this.dataDefault;
            this.isActiveSearch = false;
            this.isActiveClick = false;
        }
        else {
            this.isDisplayFolderParent = true;
            this.getListFolderName();
            this.isActiveSearch = true;
            this.isActiveClick = false;
        }
    }
}
