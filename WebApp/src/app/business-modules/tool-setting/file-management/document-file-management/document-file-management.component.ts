import { SystemFileManageRepo } from './../../../../shared/repositories/system-file-manage.repo';
import { ToastrService } from 'ngx-toastr';
import { Component, OnChanges, OnInit } from "@angular/core";
import { SettingRepo } from "@repositories";
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
    selector: "document-file-management",
    templateUrl: "./document-file-management.component.html",
})
export class DocumentFileManagementComponent extends AppList implements OnInit, OnChanges {
    itemsDefault: IFileItem[] = [];
    isActiveDownload: boolean;
    isActiveView: boolean;
    dataDefault: IFileItem[] = [
        {
            name: "Shipment Folder",
            dateCreated: "19/03/2022",
            userModified: "22/03/2022",
            folderName: "Shipment",
            classIcon: "la la-folder",
            url: "",
            classColor: "text-info",
            objectId: "",
        }
    ];

    listFolderName: IFileItem | any = [];
    stringBreadcrumb: string;
    folderName: string;
    folderChild: any;
    isActiveClick: boolean = true;
    isDisplayFolderParent: boolean = true;
    isActiveSearch: boolean = false;
    isActiveUpload: boolean = false;
    itemSelect: any;
    listBreadcrumb: Array<object> = [];
    folderType: string;
    isDisplayFolderType: boolean = true;
    constructor(
        private _settingRepo: SettingRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _systemFileManageRepo: SystemFileManageRepo
    ) {
        super();
        this.requestList = this.getListFolderByType;
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

    //Add type for item in list obj
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
                case "gif":
                    item.classIcon = "la la-file-image-o";
                    item.classColor = "text-primary";
                    item.fileType = 'png';
                    break;

                case "txt":
                    item.classIcon = "la la-file-text";
                    item.classColor = "text-dark";
                    item.fileType = 'txt';
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
        if (this.folderType !== null && this.isDisplayFolderType === true) {
            this.isActiveUpload = false;
            this.folderType = data.folderType
            this.isActiveSearch = true;
            this.getListFolderByType()
            this.listBreadcrumb.push(data.folderType);
        }
        else {
            this.isActiveSearch = false
            this.getDetailFileManagement(this.folderName, data.objectId);
            this.isDisplayFolderParent = true;
            this.listBreadcrumb.push(data.folderName);
        }
    }

    onDisplayListFolder(item: any) {
        this.isActiveUpload = false;
        this.isDisplayFolderParent = false;
        this.folderName = item.folderName;
        this.getListFolderName();
        this.listBreadcrumb.push(item.folderName)
    }

    getDetailFileManagement(folderName: string, objectId: string) {
        this.isLoading = true;
        this.folderChild = objectId;
        this.isActiveUpload = true;
        this._settingRepo.getDetailFileManagement(folderName, objectId).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false })
        )
            .subscribe((res: any) => {
                this.pushTypeForItem(res || []);
            });
    }

    getListFolderByType() {
        this.isLoading = true;
        this.isDisplayFolderType = false;
        const body = {
            folderName: this.folderName,
            folderType: this.folderType,
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
            });;
    }

    getListFolderName() {
        this.isLoading = true;
        this.isDisplayFolderType = true;
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

    //Config file actions
    onSelectFile(item: any) {
        switch (item.fileType) {
            case "png":
            case "gif":
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
            case "doc":
            case "xlsx":
                this.isActiveDownload = true;
                this.isActiveView = true;
                break;
            default:
                this.isActiveDownload = true;
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
            window.open(`${item.url}`, '_blank');
        }
    }

    getValueBreadcrumb($event: any) {
        this.onResetFilterValue()
        this.page = 1
        this.pageSize = this.numberToShow[0];
        
        if ($event === "Document") {
            this.isDisplayFolderParent = true;
            this.itemsDefault = this.dataDefault;
            this.isActiveSearch = false;
            this.isActiveClick = true;
        }
        if ($event === this.folderType) {
            this.isDisplayFolderParent = false;
            this.isDisplayFolderType = true;
            this.getListFolderByType();
            this.isActiveSearch = true;
            this.isActiveClick = true;
        }
        if ($event === this.folderName) {
            this.isDisplayFolderParent = false;
            this.getListFolderName();
            this.isActiveSearch = false;
            this.isActiveClick = true;
        }
        this.isActiveUpload = false;
    }

    getValueSearch($event: any) {
        const body = { keyWords: $event, folderName: this.folderName, folderType: this.folderType };
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
            this._systemFileManageRepo.uploadFileShipment(this.folderChild, fileList)
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
