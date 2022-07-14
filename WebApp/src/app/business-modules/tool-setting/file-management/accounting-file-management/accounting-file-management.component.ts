import { Component, OnChanges, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { SettingRepo } from "@repositories";
import { SortService } from "@services";
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
    itemsDefault: IFileItem[] = [];
    isActiveDownload: boolean;
    isActiveView: boolean;
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
    itemSelect: any;
    listBreadcrumb: Array<object> = [];
    constructor(
        private _settingRepo: SettingRepo,
        private _sortService: SortService
    ) {
        super();
        this.requestList = this.getListFolderName;
        this.requestSort = this.sortData;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Name', field: 'name', sortable: true },
            { title: 'Date Created', field: 'dateTimeCreated', sortable: true },
            { title: 'User Created', field: 'userCreated', sortable: true },
        ]
        this.itemsDefault = this.dataDefault;
    }

    sortData(sort: string): void {
        this.listFolderName = this._sortService.sort(this.listFolderName, sort, this.order);
    }

    pushTypeForItem(items: any) {
        for (let item of items) {
            let arr = item.name.split(".");
            switch (arr[arr.length - 1]) {
                case "pdf":
                    item.classIcon = "la la-file-pdf-o";
                    item.classColor = "text-danger"
                    item.fileType = 'pdf'
                    break;
                case "xlsx":
                    item.classIcon = "la la-file-excel-o";
                    item.classColor = "text-success"
                    item.fileType = 'xlsx'
                    break;
                case "doc":
                    item.classIcon = "la la-file-word-o";
                    item.classColor = "text-primary"
                    item.fileType = 'doc'
                    break;
                case "zip":
                    item.classIcon = "la la-file-zip-o";
                    item.classColor = "text-warning"
                    item.fileType = 'zip'
                    break;
                case "png":
                    item.classIcon = "la la-file-image-o";
                    item.classColor = "text-primary"
                    item.fileType = 'png'
                default:
                    item.classIcon = "la la-folder";
                    item.classColor = "text-info"
                    item.fileType = 'folder'
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

    getDetailFileManagement(folderName: string, objectId: string) {
        this.isLoading = true;
        this._settingRepo.getDetailFileManagement(folderName, objectId).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false })
        )
            .subscribe((res: any) => {
                this.pushTypeForItem(res || []);
            });
    }

    getListFolderName() {
        this.isLoading = true;
        const body = {
            folderName: this.folderName,
            keywords: []
        }
        this._settingRepo
            .getListFolderName(body, this.page, this.pageSize)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false })
            )
            .subscribe((res: any) => {
                this.totalItems = res.totalItems || 0;
                this.listFolderName = res.data;
            });
    }

    onSelectFile(item: any) {
        switch (item.fileType) {
            case "png":
                this.isActiveDownload = false;
                this.isActiveView = true;
                break;
            case "pdf":
                this.isActiveDownload = false;
                this.isActiveView = true;
                break;
            case "zip":
                this.isActiveDownload = true;
                this.isActiveView = false;
                break;
            default:
                this.isActiveDownload = true;
                this.isActiveView = true;
                break;
        }
        this.itemSelect = item;
    }

    onRedirectLink(item: any, action: string) {
        let allowedViewFiles = /(\.xlsx|\.doc)$/i;
        if (!allowedViewFiles.exec(item.fileType) && action === 'view') {
            if (item.fileType === "zip" || item.fileType === "pdf" || item.fileType === 'png' || item.fileType === 'jpg') {
                window.open(`${item.url}`, '_blank');
            } else {
                window.open(`https://gbc-excel.officeapps.live.com/op/view.aspx?src=${item.url}`, '_blank');
            }
        } else if (action === 'download') {
            window.open(`${item.url}`, '_blank');
        }
    }

    escapeRegExp(str) {
        // * Save Regex into System
        return str.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&");
    }

    onSearchValue($event) {
        let listTemp = this.listFolderName;
        let searches = $event.name.split(" ").filter(String)
        let s = "";
        searches.forEach(e => s = s + "(?=.*" + this.escapeRegExp(e) + ")");
        s = s + ".*"
        console.log(s);
        let matches = listTemp.filter((i) => {
            const r = new RegExp(s);
            return r.test(i.folderName);
        });
        this.listFolderName = matches;
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

    getValueSearch($event: any) {
        const body = { keyWords: $event, folderName: this.folderName };
        this._settingRepo
            .getListFolderName(body, this.page, this.pageSize)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe((res: any) => {
                this.totalItems = res.totalItems || 0;
                this.listFolderName = res.data;
            });
    }
}
