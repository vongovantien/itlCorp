import { Component, OnInit, ViewChild } from '@angular/core';
import { InfoPopupComponent, ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { CsTransactionDetail } from 'src/app/shared/models';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { Store } from '@ngrx/store';
import { SortService } from 'src/app/shared/services';
import { NgProgress } from '@ngx-progressbar/core';
import * as fromShare from './../../share-business/store';
import { AppList } from 'src/app/app.list';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { takeUntil, finalize, catchError } from 'rxjs/operators';
@Component({
    selector: 'app-sea-lcl-export',
    templateUrl: './sea-lcl-export.component.html',
})
export class SeaLCLExportComponent extends AppList {
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    headers: CommonInterface.IHeaderTable[];
    headersHBL: CommonInterface.IHeaderTable[];

    shipments: any[] = [];
    houseBills: CsTransactionDetail[] = [];

    itemToDelete: any = null;
    transactionService: number = CommonEnum.TransactionTypeEnum.SeaLCLExport;

    constructor(
        private _router: Router,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _ngProgessService: NgProgress,
        private _store: Store<fromShare.IShareBussinessState>
    ) {
        super();
        this._progressRef = this._ngProgessService.ref();

        this.requestList = this.requestSearchShipment;
        this.requestSort = this.sortShipment;

        this.isLoading = <any>this._store.select(fromShare.getTransationLoading);

        this.dataSearch = {
            transactionType: this.transactionService,
            //fromDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
            //toDate: new Date(),
        };
    }

    ngOnInit() {
        this.headers = [
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'MBL No.', field: 'mawb', sortable: true },
            { title: 'ETD', field: 'etd', sortable: true },
            { title: 'ETA', field: 'eta', sortable: true },
            { title: 'Supplier', field: 'supplierName', sortable: true },
            { title: 'Agent', field: 'agentName', sortable: true },
            { title: 'POL', field: 'polName', sortable: true },
            { title: 'POD', field: 'podName', sortable: true },
            { title: 'Total Pkgs', field: 'packageQty', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Creator', field: 'userCreated', sortable: true },
            { title: 'Created Date', field: 'datetimeCreated', sortable: true },
            { title: 'Modified Date', field: 'datetimeModified', sortable: true },
        ];
        this.headersHBL = [
            { title: 'HBL No', field: 'hwbno', sortable: true },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'Salesman', field: 'saleManName', sortable: true },
            { title: 'Notify Party', field: 'notifyParty', sortable: true },
            { title: 'Destination', field: 'finalDestinationPlace', sortable: true },
            { title: 'Containers', field: 'containers', sortable: true },
            { title: 'Packages', field: 'packages', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
        ];

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
                    this.shipments = res.data || [];
                    this.totalItems = res.totalItems;
                }
            );
    }

    sortShipment(sortField: string) {
        this.shipments = this._sortService.sort(this.shipments, sortField, this.order);
    }

    sortHBL(sortField: string, order: boolean) {
        this.houseBills = this._sortService.sort(this.houseBills, sortField, order);
    }

    getListHouseBill(jobId: any, index: number) {
        if (!!this.shipments[index].houseBillList && !!this.shipments[index].houseBillList.length) {
            this.houseBills = this.shipments[index].houseBillList || [];
        } else {
            this._progressRef.start();
            this._documentRepo.getListHouseBillOfJob({ jobId: jobId })
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: any) => {
                        this.houseBills = res || [];
                        console.log(this.houseBills);

                        this.shipments[index].houseBillList = res;
                    }
                );
        }
    }

    onSearchShipment($event: any) {
        $event.transactionType = this.transactionService;
        this.dataSearch = $event;
        this.requestSearchShipment();
    }

    requestSearchShipment() {
        this._store.dispatch(new fromShare.TransactionLoadListAction({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
    }

    confirmDelete(item: { id: string; }) {
        this.itemToDelete = item;
        this._progressRef.start();
        this._documentRepo.checkMasterBillAllowToDelete(item.id)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (respone: boolean) => {
                    if (respone === true) {
                        this.confirmDeletePopup.show();
                    } else {
                        this.infoPopup.show();
                    }
                }
            );
    }

    deleteJob() {
        this.confirmDeletePopup.hide();
        this._progressRef.start();

        this._documentRepo.deleteMasterBill(this.itemToDelete.id)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        this._store.dispatch(new fromShare.TransactionLoadListAction({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    gotoCreateJob() {
        this._router.navigate(['home/documentation/sea-lcl-export/new']);
    }

}
