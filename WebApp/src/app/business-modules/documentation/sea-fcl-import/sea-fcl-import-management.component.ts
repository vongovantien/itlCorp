import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { TransactionTypeEnum } from 'src/app/shared/enums/transaction-type.enum';
import { SortService } from 'src/app/shared/services';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';

@Component({
    selector: 'app-sea-fcl-import-management',
    templateUrl: './sea-fcl-import-management.component.html',
})
export class SeaFCLImportManagementComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;

    tabs: any[] = [
        { title: 'Shipment Detail', content: 'Dynamic content 1' },
        { title: 'Dynamic Title 2', content: 'Dynamic content 2' },
        { title: 'Dynamic Title 3', content: 'Dynamic content 3', removable: true }
    ];
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
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.searchList;
        this.requestSort = this.sortMasterBills;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'MBL No.', field: 'mawb', sortable: true },
            { title: 'ETA', field: 'eta', sortable: true },
            { title: 'Supplier', field: 'supplierName', sortable: true },
            { title: 'Agent', field: 'agentName', sortable: true },
            { title: 'POL', field: 'polName', sortable: true },
            { title: 'POD', field: 'podName', sortable: true },
            { title: "Cont Qty", field: 'sumCont', sortable: true },
            { title: "Package Qty", field: 'sumPackage', sortable: true },
            { title: 'G.W', field: 'grossWeight', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Creator', field: 'creatorName', sortable: true },
            { title: 'Modified Date', field: 'modifiedDate', sortable: true },
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
            transactionType: TransactionTypeEnum.SeaFCLImport
        };
        this.searchList(this.dataSearch);
    }

    searchList(dataSearch?: any) {
        if (dataSearch == undefined) {
            dataSearch = {
                transactionType: TransactionTypeEnum.SeaFCLImport
            };
        }
        this._progressRef.start();
        this.isLoading = true;
        this._documentationRepo.getListShipmentDocumentation(this.page, this.pageSize, Object.assign({}, dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.isLoading = false;
                    this._progressRef.complete();
                }),
                map((data: any) => {
                    console.log(data);
                    return {
                        data: data.data.map((item: any) => new CsTransaction(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.masterbills = res.data;
                },
            );
    }

    showHblList(jobId: string, index: number) {
        if (this.tmpIndex == index) {
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
        this._router.navigate(['home/documentation/sea-fcl-import/new']);
    }

    onSearchMasterBills(data: any) {
        console.log(data);
        this.page = 1; // reset page.
        this.searchList(data);
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
                        this.searchList(this.dataSearch);
                    }
                },
            );

    }
}
