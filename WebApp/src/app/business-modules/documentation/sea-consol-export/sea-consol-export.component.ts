import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { InfoPopupComponent, ConfirmPopupComponent, Permission403PopupComponent } from '@common';
import { CsTransactionDetail } from '@models';
import { CommonEnum } from '@enums';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SortService } from '@services';
import { DocumentationRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { Store } from '@ngrx/store';
import { takeUntil, catchError, finalize } from 'rxjs/operators';
import { IShareBussinessState, getTransationLoading, getTransactionListShipment, TransactionLoadListAction } from '@share-bussiness';

@Component({
    selector: 'app-sea-consol-export',
    templateUrl: './sea-consol-export.component.html',
})
export class SeaConsolExportComponent extends AppList implements OnInit {

    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(Permission403PopupComponent, { static: false }) permissionPopup: Permission403PopupComponent;

    headers: CommonInterface.IHeaderTable[];
    headersHBL: CommonInterface.IHeaderTable[];

    shipments: any[] = [];
    houseBills: CsTransactionDetail[] = [];
    tmpHouseBills: CsTransactionDetail[] = [];
    tmpIndex: number = -1;

    itemToDelete: any = null;
    transactionService: number = CommonEnum.TransactionTypeEnum.SeaConsolExport;

    _fromDate: Date = this.createMoment().startOf('month').toDate();
    _toDate: Date = this.createMoment().endOf('month').toDate();

    jobIdSelected: string = null;

    constructor(
        private _router: Router,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _ngProgessService: NgProgress,
        private _store: Store<IShareBussinessState>
    ) {
        super();

        this._progressRef = this._ngProgessService.ref();

        this.requestList = this.requestSearchShipment;
        this.requestSort = this.sortShipment;

        this.isLoading = <any>this._store.select(getTransationLoading);

        this.dataSearch = {
            transactionType: this.transactionService,
            fromDate: this._fromDate,
            toDate: this._toDate,
        };

    }

    ngOnInit() {
        this.headers = [
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'MBL No.', field: 'mawb', sortable: true },
            { title: 'ETD', field: 'etd', sortable: true },
            { title: 'ETA', field: 'eta', sortable: true },
            { title: 'Shipping Line', field: 'supplierName', sortable: true },
            { title: 'Agent', field: 'agentName', sortable: true },
            { title: 'POL', field: 'polName', sortable: true },
            { title: 'POD', field: 'podName', sortable: true },
            { title: 'Total Cont', field: 'sumCont', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Creator', field: 'creatorName', sortable: true },
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
        this._store.select(getTransactionListShipment)
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
        this.jobIdSelected = jobId;
        if (this.tmpIndex === index) {
            this.houseBills = this.tmpHouseBills;
        } else {
            this._progressRef.start();
            this._documentRepo.getListHouseBillAscHBLOfJob({ jobId: jobId, hwbno: this.dataSearch.hwbno, customerId: this.dataSearch.customerId, saleManId: this.dataSearch.saleManId, creditDebitNo: this.dataSearch.creditDebitNo, soaNo: this.dataSearch.soaNo })
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: any) => {
                        this.houseBills = res || [];
                        this.tmpHouseBills = this.houseBills;
                        this.tmpIndex = index;
                    }
                );
        }
    }

    onSearchShipment($event: any) {
        $event.transactionType = this.transactionService;
        this.dataSearch = $event;
        this.requestSearchShipment();
        this.loadListHouseBillExpanding();
    }

    onResetShipment($event: any) {
        this.page = 1;
        $event.transactionType = this.transactionService;
        $event.fromDate = this._fromDate;
        $event.toDate = this._toDate;
        this.dataSearch = $event;
        this.requestSearchShipment();
        this.loadListHouseBillExpanding();
    }

    requestSearchShipment() {
        this._store.dispatch(new TransactionLoadListAction({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
    }

    prepareDeleteShipment(item) {
        this._documentRepo.checkPermissionAllowDeleteShipment(item.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this.confirmDelete(item);
                } else {
                    this.permissionPopup.show();
                }
            });
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

                        this._store.dispatch(new TransactionLoadListAction({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    gotoCreateJob() {
        this._router.navigate(['home/documentation/sea-fcl-export/new']);
    }

    loadListHouseBillExpanding() {
        this.tmpIndex = -1;
        if (this.jobIdSelected !== null) {
            this.getListHouseBill(this.jobIdSelected, -2);
        }
    }

    viewDetail(id: string): void {
        this._documentRepo.checkDetailShippmentPermission(id)
            .subscribe((value: boolean) => {
                if (value) {


                    this._router.navigate(["/home/documentation/sea-fcl-export", id]);
                } else {
                    this.permissionPopup.show();
                }
            });
    }

}
