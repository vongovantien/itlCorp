import { Component, OnInit, ViewChild } from '@angular/core';
import { AccountingRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { AppList } from 'src/app/app.list';
import { PagingService, SortService } from '@services';
import { InfoPopupComponent } from '@common';
import { ToastrService } from 'ngx-toastr';

import cloneDeep from 'lodash/cloneDeep';
import { finalize, catchError } from 'rxjs/operators';

@Component({
    selector: 'app-import-obh-account-receivable-payable',
    templateUrl: './import-obh-account-receivable-payable.component.html'
})

export class AccountReceivablePayableImportOBHPaymentComponent extends AppList implements OnInit {
    @ViewChild(InfoPopupComponent) invaliDataAlert: InfoPopupComponent;

    data: IImportOBH[];
    pagedItems: IImportOBH[] = [];

    totalRows: number = 0;
    totalValidRows: number = 0;
    startIndex: number = 0;

    isShowInvalid: boolean = true;

    constructor(
        private _accountingRepo: AccountingRepo,
        private _progressService: NgProgress,
        private _pagingService: PagingService,
        private _sortService: SortService,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortList;
        this.requestList = this.onPaging;
    }

    ngOnInit(): void {
        this.data = [];

        this.headers = [
            { title: 'Soa No', field: 'soaNo', sortable: true },
            { title: 'Partner Id', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Payment Amount', field: 'paymentAmount', sortable: true },
            { title: 'CurrencyId', field: 'currencyId', sortable: true },
            { title: 'ExchangeRate', field: 'ExchangeRate', sortable: true },
            { title: 'Payment Method', field: 'paymentMethod', sortable: true },
            { title: 'Paid Date', field: 'paidDate', sortable: true },
            { title: 'Payment Type', field: 'paymentType', sortable: true }
        ];
    }


    changeFile(file: Event) {
        if (file.target['files'] == null) { return; }
        this._progressRef.start();
        this._accountingRepo.getOBHPaymentImport(file.target['files'])
            .pipe(
                finalize(() => {
                    this._progressRef.complete();
                })
            )
            .subscribe((response: CommonInterface.IResponseImport) => {
                this.data = response.data;
                this.pagedItems = cloneDeep(this.data);

                this.page = 1;
                this.totalItems = this.data.length;

                this.totalValidRows = response.totalValidRows;
                this.totalRows = this.data.length;

                this.pagingData(this.data.length, this.page);
            }, () => { });

    }

    pagingData(totalItem: number, currentPage: number) {
        const dataPaging: {
            startIndex: number,
            endIndex: number,
            pageSize: number,
            totalItems: number
        } = this._pagingService.getPager(totalItem, currentPage, this.pageSize);

        this.pagedItems = this.data.slice(dataPaging.startIndex, dataPaging.endIndex + 1);
        this.totalItems = dataPaging.totalItems;
        this.startIndex = dataPaging.startIndex;
    }

    hideInvalid() {
        if (!this.data.length) { return; }
        this.isShowInvalid = !this.isShowInvalid;

        if (this.isShowInvalid) {
            this.pagedItems = cloneDeep(this.data);
            this.totalItems = this.pagedItems.length;

        } else {
            const invalidData = this.data.filter(x => !x.isValid);
            this.pagedItems.length = 0;
            this.pagedItems = invalidData;
            this.totalItems = invalidData.length;
        }
    }

    onPaging() {
        this.pagingData(this.data.length, this.page);
    }

    sortList() {
        this.pagedItems = this._sortService.sort(this.pagedItems, this.sort, this.order);
    }

    downloadSample() {
        this._accountingRepo.downloadOBHPaymentFile()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "OBHPaymentImportFile.xlsx");
                },
            );
    }

    import() {
        if (this.data == null) { return; }
        if (this.data.length === 0) {
            this.invaliDataAlert.show();
            this._progressRef.complete();
            return;
        }
        if (this.totalRows - this.totalValidRows > 0) {
            this.invaliDataAlert.show();
            this._progressRef.complete();
        } else {
            const data = this.data.filter(x => x.isValid);
            this._progressRef.start();
            this._accountingRepo.importOBHPayment(data)
                .pipe(
                    finalize(() => {
                        this._progressRef.complete();
                    })
                )
                .subscribe(
                    (res) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
    }

}
interface IImportOBH {
    soaNo: string;
    partnerId: string;
    partnerName: string;
    paymentAmount: number;
    paidDate: string;
    paymentType: string;
    isValid: boolean;
    currencyId: string;
    paymentMethod: string;
    exchangeRate: number;
    soaNoError: string;
    partnerAccountError: string;
    currencyIdError: string;
    paymentMethodError: string;
}