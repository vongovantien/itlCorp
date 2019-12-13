import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';

import { AppList } from 'src/app/app.list';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { CsTransaction, CsTransactionDetail } from 'src/app/shared/models';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SortService } from 'src/app/shared/services';
import { CommonEnum } from 'src/app/shared/enums/common.enum';

import { catchError, finalize, takeUntil } from 'rxjs/operators';

import * as fromShare from './../../share-business/store';

@Component({
    selector: 'app-sea-lcl-import',
    templateUrl: './sea-lcl-import.component.html'
})
export class SeaLCLImportComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;

    headers: CommonInterface.IHeaderTable[];
    headerHouseBills: CommonInterface.IHeaderTable[];

    masterbills: CsTransaction[] = [];
    housebills: CsTransactionDetail[] = [];
    tmpHouseBills: CsTransactionDetail[] = [];
    tmpIndex: number = -1;

    selectedMasterBill: CsTransaction = null;
    deleteMessage: string = '';

    constructor(
        private _router: Router,
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _toastService: ToastrService,
        private _store: Store<fromShare.IShareBussinessState>
    ) {
        super();
        this._progressRef = this._progressService.ref();

        this.requestList = this.requestSearchShipment;
        this.requestSort = this.sortMasterBills;

        this.isLoading = <any>this._store.select(fromShare.getTransationLoading);

    }

    ngOnInit() {
        this.headers = [
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'MBL No.', field: 'mawb', sortable: true },
            { title: 'ETA', field: 'eta', sortable: true },
            { title: 'Shipping Line', field: 'supplierName', sortable: true },
            { title: 'Agent', field: 'agentName', sortable: true },
            { title: 'POL', field: 'polName', sortable: true },
            { title: 'POD', field: 'podName', sortable: true },
            { title: "Cont Qty", field: 'sumCont', sortable: true },
            { title: "Package Qty", field: 'sumPackage', sortable: true },
            { title: 'G.W', field: 'grossWeight', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Creator', field: 'creatorName', sortable: true },
            { title: 'Modified Date', field: 'datetimeModified', sortable: true },
        ];

        this.headerHouseBills = [
            { title: 'HBL No', field: 'hwbno', sortable: true },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'SalesMan', field: 'saleManName', sortable: true },
            { title: 'Notify Party', field: 'notifyParty', sortable: true },
            { title: 'Destination', field: 'finalDestinationPlace', sortable: true },
            { title: 'Containers', field: 'containers', sortable: true },
            { title: 'Packages', field: 'packages', sortable: true },
            { title: 'G.W', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
        ];
        this.dataSearch = {
            transactionType: CommonEnum.TransactionTypeEnum.SeaLCLImport,
            //fromDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
            //toDate: new Date(),
        };

        this.requestSearchShipment();
        this.getShipments();
    }

    getShipments() {
        this._store.select(fromShare.getTransactionListShipment)
            .pipe(
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe(
                (res: CommonInterface.IResponsePaging | any) => {
                    this.masterbills = res.data || [];
                    this.totalItems = res.totalItems;
                }
            );
    }

    requestSearchShipment() {
        this._store.dispatch(new fromShare.TransactionLoadListAction({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
    }

    showHblList(jobId: string, index: number) {
        if (this.tmpIndex === index) {
            this.housebills = this.tmpHouseBills;
        } else {
            this._progressRef.start();
            this._documentationRepo.getListHouseBillOfJob({ jobId: jobId })
                .pipe(
                    catchError(this.catchError),
                    finalize(() => {
                        this._progressRef.complete();
                    })
                ).subscribe(
                    (res: CsTransactionDetail[]) => {
                        this.housebills = (res || []).map((item: CsTransactionDetail) => new CsTransactionDetail(item));
                        this.tmpHouseBills = this.housebills;
                        this.tmpIndex = index;
                    },
                );
        }
    }

    gotoCreateJob() {
        this._router.navigate(["home/documentation/sea-lcl-import/new"]);
    }

    onSearchMasterBills(data: any) {
        this.page = 1; // reset page.
        data.transactionType = CommonEnum.TransactionTypeEnum.SeaLCLImport;

        this.dataSearch = data;
        this.requestSearchShipment();
    }

    sortMasterBills(sort: string): void {
        this.masterbills = this._sortService.sort(this.masterbills, sort, this.order);
    }

    sortHouseBills(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.housebills = this._sortService.sort(this.housebills, sortData.sortField, sortData.order);
        }
    }

    checkDeleteMasterBill(masterbill: CsTransaction) {
        this._progressRef.start();
        this._documentationRepo.checkMasterBillAllowToDelete(masterbill.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.selectedMasterBill = new CsTransaction(masterbill);
                        this.deleteMessage = `Are you sure you want to delete this Job?`;
                        this.confirmDeleteJobPopup.show();
                    } else {
                        this.canNotDeleteJobPopup.show();
                    }
                },
            );
    }

    onDeleteMasterBill() {
        this._progressRef.start();
        this._documentationRepo.deleteMasterBill(this.selectedMasterBill.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.confirmDeleteJobPopup.hide();
                })
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {
                        this._toastService.success(respone.message, 'Delete Success !');
                        this.requestSearchShipment();
                    }
                },
            );
    }
}
