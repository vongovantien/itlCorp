import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { OperationRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { BaseService, SortService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';

@Component({
    selector: 'app-container-import',
    templateUrl: './container-import.component.html'
})
export class ContainerImportComponent extends PopupBase implements OnInit {

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

    constructor(private operationRepo: OperationRepo,
        private _progressService: NgProgress,
        private baseService: BaseService,
        private api_menu: API_MENU,
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
            { title: 'NW', field: 'nw', sortable: true },
            { title: 'Package Qty', field: 'packageQuantity', sortable: true },
            { title: 'Parentdoc', field: 'firstClearanceNo', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];
    }
    close() {
        this.data = null;
        this.importedData = [];
        this.hide();
    }
    sortLocal(sort: string): void {
        this.importedData = this._sortService.sort(this.importedData, sort, this.order);
    }
    downloadFile() {
        this.operationRepo.downloadcontainerfileExel()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    saveAs(res, "ContainerImportTemplate.xlsx");
                },
                (errors: any) => { },
                () => { }
            );
    }
    chooseFile(file: Event) {
        if (file.target['files'] == null) { return; }
        this._progressRef.start();
        this.baseService.uploadfile(this.api_menu.Documentation.CsMawbcontainer.uploadFileExcel, file.target['files'], "uploadedFile")
            .subscribe((response: any) => {
                if (response["list"] != null) {
                    console.log('read file');
                    this.data = response["list"];
                    if (this.data != null) {
                        this.getData();
                    } else {
                        this.importedData = [];
                    }
                }
                this._progressRef.complete();
                // console.log(response);
                // this.data = response.data;
                // this.pager.totalItems = this.data.length;
                // this.totalValidRows = response.totalValidRows;
                // this.totalRows = this.data.length;
                // this.pagingData(this.data);
                // this.progressBar.complete();
                // console.log(this.data);
            }, err => {
                // this.progressBar.complete();
                // this.baseService.handleError(err);
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
