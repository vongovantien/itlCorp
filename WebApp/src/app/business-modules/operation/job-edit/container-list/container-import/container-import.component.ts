import { Component, OnInit, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { BaseService, SortService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { UploadAlertComponent } from 'src/app/shared/common/popup/upload-alert/upload-alert.component';

@Component({
    selector: 'app-container-import',
    templateUrl: './container-import.component.html'
})
export class ContainerImportComponent extends PopupBase implements OnInit {
    isDesc: string = '';
    sortKey: string = '';
    @Input() jobId: string;
    @ViewChild(UploadAlertComponent, { static: false }) importAlert: UploadAlertComponent;
    @Output() isImportSuccess = new EventEmitter();

    page: number = 1;
    totalItems: number = 0;
    numberToShow: number[] = [3, 15, 30, 50];
    pageSize: number = this.numberToShow[1];

    sort: string = null;
    order: any = false;
    keyword: string = '';
    requestList: any = null;
    requestSort: any = null;
    headers: CommonInterface.IHeaderTable[];
    data: any[];
    importedData: any[] = [];
    totalValidRow: number = 0;
    totalInvalidRow: number = 0;
    isShowInvalid: boolean = true;
    existedError: string = null;
    duplicatedError: string = null;

    constructor(
        private _progressService: NgProgress,
        private baseService: BaseService,
        private api_menu: API_MENU,
        private _documentRepo: DocumentationRepo,
        private _sortService: SortService) {
        super();
        this._progressRef = this._progressService.ref();

        this.requestSort = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Cont Type *', field: 'containerTypeName', sortable: true },
            { title: 'Cont Qty *', field: 'quantity', sortable: true },
            { title: 'Cont No', field: 'containerNo', sortable: true },
            { title: 'Seal No', field: 'sealNo', sortable: true },
            { title: 'GW', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'NW', field: 'nw', sortable: true },
            { title: 'Package Qty', field: 'packageQuantity', sortable: true },
            { title: 'Package Type', field: 'packageTypeName', sortable: true },
            { title: 'Mark No', field: 'markNo', sortable: true },
            { title: 'Description', field: 'description', sortable: true },
            { title: 'Commodity', field: 'commodityName', sortable: true },
            { title: 'Unit', field: 'unitOfMeasureName', sortable: true },
        ];
    }

    reset() {
        this.data = null;
        this.importedData = [];
        this.totalItems = 0;
        this.totalValidRow = 0;
        this.totalInvalidRow = 0;
        this.duplicatedError = null;
        this.existedError = null;
    }

    close() {
        this.reset();
        this.hide();
    }

    sortLocal(sort: string): void {
        this.importedData = this._sortService.sort(this.importedData, sort, this.order);
    }

    downloadFile() {
        this._documentRepo.downloadcontainerfileExcel()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "ContainerImportTemplate.xlsx");
                },
            );
    }

    import() {
        if (this.data == null) { return; }
        if (this.totalInvalidRow > 0) {
            this.importAlert.show();
        } else {
            this._progressRef.start();
            this.data.forEach(x => {
                x.mblid = this.jobId;
            });
            this._documentRepo.importContainerExcel(this.data)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.success) {
                            this.reset();
                            this._progressRef.complete();
                            this.isImportSuccess.emit(true);
                            this.hide();
                        }
                        console.log(res);
                    },
                    (errors: any) => { },
                    () => { }
                );
        }
    }

    hideInvalid() {
        this.page = 0;
        const end = this.page + (this.pageSize - 1);
        if (this.data == null) { return; }
        this.isShowInvalid = !this.isShowInvalid;
        if (this.isShowInvalid) {
            this.totalItems = this.data.length;
            this.importedData = this.data.slice(this.page, end);
        } else {
            const inValidItems = this.data.filter(x => !x.isValid);
            this.totalItems = inValidItems.length;
            this.importedData = inValidItems.slice(this.page, end);
        }
    }

    chooseFile(file: Event) {
        if (file.target['files'] == null) { return; }
        this._progressRef.start();
        this.baseService.uploadfile(this.api_menu.Documentation.CsMawbcontainer.uploadFileExcel, file.target['files'], "uploadedFile")
            .subscribe((response: any) => {
                if (response["list"] != null) {
                    console.log('read file');
                    this.data = response["list"];
                    this.totalItems = this.data.length;
                    this.totalValidRow = response['totalValidRows'];
                    this.duplicatedError = response['duplicatedError'];
                    this.existedError = response['existedError'];
                    this.totalInvalidRow = this.totalItems - this.totalValidRow;

                    if (this.data != null) {
                        this.getData();
                    } else {
                        this.reset();
                    }
                }
                this._progressRef.complete();
            }, err => {
            });
    }

    getData() {
        this.totalItems = this.data.length;
        this.page = 0;
        const end = this.page + (this.pageSize - 1);
        this.importedData = this.data.slice(this.page, end);
        console.log(this.importedData);
    }

    // app list
    setSortBy(sort?: string, order?: boolean): void {
        this.sort = sort ? sort : 'code';
        this.order = order;
    }

    sortBy(sort: string): void {
        if (!!sort) {
            this.setSortBy(sort, this.sort !== sort ? true : !this.order);

            if (typeof (this.requestSort) === 'function') {
                this.requestSort(this.sort, this.order);   // sort Local
            }
        }
    }

    sortClass(sort: string): string {
        if (!!sort) {
            let classes = 'sortable ';
            if (this.sort === sort) {
                classes += ('sort-' + (this.order ? 'asc' : 'desc') + ' ');
            }

            return classes;
        }
        return '';
    }
}
