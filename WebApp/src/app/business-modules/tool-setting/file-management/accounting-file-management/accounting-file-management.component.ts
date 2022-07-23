import { ToastrService } from 'ngx-toastr';
import { Component, OnChanges, OnInit } from "@angular/core";
import { AccountingRepo, SettingRepo } from "@repositories";
import { SortService } from "@services";
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
    isActiveView: boolean;
    isActiveDownload: boolean;
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
        private _sortService: SortService,
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService,
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
                case "PDF":
                    item.classIcon = "la la-file-pdf-o";
                    item.classColor = "text-danger";
                    item.fileType = 'pdf';
                    break;
                case "xlsx":
                case "xls":
                    item.classIcon = "la la-file-excel-o";
                    item.classColor = "text-success";
                    item.fileType = 'xlsx';
                    break;
                case "doc":
                case "docx":
                    item.classIcon = "la la-file-word-o";
                    item.classColor = "text-primary";
                    item.fileType = 'doc';
                    break;
                case "zip":
                    item.classIcon = "la la-file-zip-o";
                    item.classColor = "text-warning";
                    item.fileType = 'zip';
                    break;
                case "png":
                case "jpg":
                case "jpeg":
                case "PNG":
                    item.classIcon = "la la-file-image-o";
                    item.classColor = "text-primary";
                    item.fileType = 'png';
                    break;
                default:
                    item.classIcon = "la la-folder";
                    item.classColor = "text-info";
                    item.fileType = 'folder';
                    break;
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
        this.listBreadcrumb.push(data.folderName);
    }

    getDetailFileManagement(folderName: string, objectId: string) {
        this.isLoading = true;
        this.folderChild = objectId;
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
                this.listFolderName = res.data
            });
    }

    onSelectFile(item: any) {
        switch (item.fileType) {
            case "png":
                this.isActiveDownload = true;
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
            case "doc":
            case "xlsx":
                this.isActiveDownload = true;
                this.isActiveView = true;
                break;
            default:
                this.isActiveDownload = false;
                this.isActiveView = false;
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
            if (item.fileType === 'png') {

            }
            window.open(`${item.url}`, '_blank');
        }
    }

    onDisplayListFolder(item: any) {
        this.isDisplayFolderParent = true;
        this.isActiveSearch = true;
        this.stringBreadcrumb = this.folderName;
        this.folderName = item.folderName;
        this.getListFolderName();
        this.listBreadcrumb.push(item.folderName)
    }

    onDisplayDefaultFolder() {
        this.isDisplayFolderParent = true;
    }

    getValueBreadcrumb($event: any) {
        this.onResetFilterValue()
        this.page = 1
        this.pageSize = this.numberToShow[0];

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

    chooseUploadFile($event: any) {
        const fileList = event.target['files'];
        if (fileList.length > 0) {
            let validSize: boolean = true;

            for (let i = 0; i <= fileList.length - 1; i++) {
                const fileSize: number = fileList[i].size / Math.pow(1024, 2);
                if (fileSize >= 100) {
                    validSize = false;
                    break;
                }
            }
            if (!validSize) {
                this._toastService.warning("maximum file size < 100Mb");
                return;
            }
            this._accountingRepo.uploadAttachedFiles(this.folderName, this.folderChild, fileList)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            this.getDetailFileManagement(this.folderName, this.folderChild);
                        }
                    }
                );
        }
    }

    onResetFilterValue() {
        this.keyword = '';
    }
}
