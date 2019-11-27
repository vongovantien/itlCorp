import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SortService } from 'src/app/shared/services';
import { formatDate } from '@angular/common';
import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'import-job-detail-popup',
    templateUrl: './import-job-detail.popup.html'
})

export class ShareBusinessImportJobDetailPopupComponent extends PopupBase {
    @Output() onImport: EventEmitter<any> = new EventEmitter<any>();
    headers: CommonInterface.IHeaderTable[];
    dataSearch: any = {};
    shippments: any = [];
    jobId: string = '';
    selected = -1;
    selectedShipment: any = {};
    isCheckShipment: boolean = false;
    pageChecked: number = 0;
    constructor(
        private _documentRepo: DocumentationRepo,
        private _sortService: SortService,

    ) {
        super();
        this.requestList = this.getShippments;
        this.requestSort = this.sortLocal;
    }

    sortLocal(sort: string): void {
        this.shippments = this._sortService.sort(this.shippments, sort, this.order);
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'MBL No', field: 'mawb', sortable: true },
            { title: 'Supplier(Shipping Line)', field: 'supplierName', sortable: true },
            { title: 'Shipment Date', field: 'etd', sortable: true }
        ];
        this.getShippments(this.dataSearch);
    }

    onCancel() {
        this.hide();
    }

    onSelected(index: number, shiment: any) {
        this.selected = index;
        this.pageChecked = this.page;
        this.selectedShipment = shiment;

    }

    getShippments(data: any = {}) {
        this.isLoading = true;
        const date = new Date();

        if (data.all === undefined && Object.keys(this.dataSearch).length === 0) {
            data.fromDate = formatDate(new Date(date.getFullYear(), date.getMonth(), 1), 'yyyy-MM-dd', 'en');
            data.toDate = formatDate(new Date(), 'yyyy-MM-dd', 'en');
        } else {
            data = this.dataSearch;
        }
        data.transactionType = 7;
        this._documentRepo.getListShipmentDocumentation(this.page, this.pageSize, data).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false; }),
        ).subscribe(
            (res: any) => {
                if (!!res.data) {
                    this.shippments = res.data;
                    this.totalItems = res.totalItems || 0;
                    console.log(this.shippments);
                } else {
                    this.totalItems = 0;
                    this.shippments = [];
                }

            },
        );
    }

    onImportShippment() {
        if (this.selected === -1) {
            this.isCheckShipment = false;
            return;
        } else {
            if (this.pageChecked !== this.page) {
                return;
            }

            this.isCheckShipment = true;
            this.onImport.emit(this.selectedShipment);
            this.hide();
        }
    }

    onSearchShippment(dataSearch: any) {
        this.dataSearch = dataSearch;
        this.getShippments(this.dataSearch);
    }


}
