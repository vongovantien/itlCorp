import { Component, EventEmitter, Output } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { SortService } from 'src/app/shared/services';
import { formatDate } from '@angular/common';
@Component({
    selector: 'import-house-bill-detail',
    templateUrl: './import-house-bill-detail.component.html'
})
export class ShareBusinessImportHouseBillDetailComponent extends PopupBase {
    @Output() onImport: EventEmitter<any> = new EventEmitter<any>();
    headers: CommonInterface.IHeaderTable[];
    dataSearch: any = {};
    houseBill: any = [];
    jobId: string = '';
    selected = -1;
    selectedHbl: any = {};
    isCheckHbl: boolean = false;
    pageChecked: number = 0;
    typeFCL: string = '';

    constructor(
        private _documentRepo: DocumentationRepo,
        private _sortService: SortService,
    ) {
        super();
        this.requestList = this.getHourseBill;
        this.requestSort = this.sortLocal;
    }

    sortLocal(sort: string): void {
        this.houseBill = this._sortService.sort(this.houseBill, sort, this.order);
    }

    ngOnInit() {
        this.dataSearch.jobId = this.jobId;
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true },
            { title: 'MBL No', field: 'mawb', sortable: true },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'SaleMan', field: 'saleManName', sortable: true },
            { title: 'Shipment Date', field: this.typeFCL === 'Export' ? 'etd' : 'eta', sortable: true }
        ];

    }

    onCancel() {
        this.hide();
    }

    getHourseBill(data: any = {}) {
        this.isLoading = true;
        const date = new Date();
        if (this.dataSearch !== undefined) {
            data = this.dataSearch;
        }
        if (data.fromDate === undefined) {
            data.fromDate = formatDate(new Date(date.getFullYear(), date.getMonth(), 1), 'yyyy-MM-dd', 'en');
            data.toDate = formatDate(new Date(), 'yyyy-MM-dd', 'en');
        }
        if (this.typeFCL !== '') {
            data.typeFCL = this.typeFCL;
        }
        this._documentRepo.getListHblPaging(this.page, this.pageSize, data).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false; }),
        ).subscribe(
            (res: any) => {
                if (!!res.data) {
                    this.houseBill = res.data;
                    this.totalItems = res.totalItems || 0;
                    console.log(this.houseBill);
                } else {
                    this.totalItems = 0;
                    this.houseBill = [];
                }

            },
        );
    }

    onSelected(index: number, hblSelected: any) {
        this.selected = index;
        this.pageChecked = this.page;
        this.selectedHbl = hblSelected;
    }

    onImportHbl() {
        if (this.selected === -1) {
            this.isCheckHbl = false;
            return;
        } else {
            if (this.pageChecked !== this.page && this.selected === -1) {
                return;
            }
            this.isCheckHbl = true;
            this.onImport.emit(this.selectedHbl);
            this.hide();
        }

    }

    onSearchHbl(dataSearch: any) {
        this.dataSearch = dataSearch;
        this.getHourseBill(this.dataSearch);
    }
}
