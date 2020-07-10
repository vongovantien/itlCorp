import { Component, OnInit, ViewChild } from '@angular/core';
import { AccountingRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { AppList } from 'src/app/app.list';
import { finalize, catchError } from 'rxjs/operators';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagingService } from '@services';
import { SystemConstants } from '@constants';
import { InfoPopupComponent } from '@common';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-import-obh-account-receivable-payable',
    templateUrl: './import-obh-account-receivable-payable.component.html'
})

export class AccountReceivablePayableImportOBHPaymentComponent extends AppList implements OnInit {
    @ViewChild(InfoPopupComponent, { static: false }) invaliDataAlert: InfoPopupComponent;
    headers: any = [];
    data: IImportOBH[];
    totalRows: number = 0;
    totalValidRows: number = 0;
    inValidItems: any[] = [];
    isShowInvalid: boolean = true;
    //
    pager: PagerSetting = PAGINGSETTING;
    pagedItems: any[] = [];

    constructor(
        private _accountingRepo: AccountingRepo,
        private _progressService: NgProgress,
        private _pagingService: PagingService,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit(): void {
        this.data = [];

        this.headers = [
            { title: 'Soa No', field: 'soaNo', sortable: true },
            { title: 'Partner Id', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Payment Amount', field: 'paymentAmount', sortable: true },
            { title: 'Paid Date', field: 'paidDate', sortable: true },
            { title: 'Payment Type', field: 'paymentType', sortable: true }
        ];
    }

    hideInvalid() {
        if (this.data == null) { return; }
        this.isShowInvalid = !this.isShowInvalid;
        if (this.isShowInvalid) {
            this.pager.totalItems = this.data.length;
            this.pagingData(this.data);
        } else {
            this.inValidItems = this.data.filter(x => !x.isValid);
            this.pagingData(this.inValidItems);
            this.pager.totalItems = this.inValidItems.length;
        }
    }

    pagingData(data: any[]) {
        this.pager = this._pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
        this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
        this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
        this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
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

                this.pager.currentPage = 1;
                this.pager.totalItems = this.data.length;

                this.totalValidRows = response.totalValidRows;
                this.totalRows = this.data.length;

                this.pagingData(this.data);
            }, () => { });

    }

    pageChanged(event: any): void {
        if (this.pager.currentPage !== event.page || this.pager.pageSize !== event.itemsPerPage) {
            this.pager.currentPage = event.page;
            this.pager.pageSize = event.itemsPerPage;

            this.pagingData(this.data);
        }
    }

    selectPageSize() {
        this.pager.currentPage = 1;
        if (this.isShowInvalid) {
            this.pager.totalItems = this.data.length;
            this.pagingData(this.data);

        } else {
            this.inValidItems = this.data.filter(x => !x.isValid);
            this.pagingData(this.inValidItems);
            this.pager.totalItems = this.inValidItems.length;
        }
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
    import(element) {
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
                            this.pager.totalItems = 0;
                            this.reset(element);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
    }
    reset(element) {
        this.data = null;
        this.pagedItems = null;
        element.value = "";
        this.pager.totalItems = 0;
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
}