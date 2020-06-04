import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccAccountingManagementResult } from 'src/app/shared/models/accouting/accounting-management';
import { AccountingRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize, map } from 'rxjs/operators';

@Component({
    selector: 'app-accounting-voucher',
    templateUrl: './accounting-voucher.component.html'
})

export class AccountingManagementVoucherComponent extends AppList implements OnInit {
    vouchers: AccAccountingManagementResult[] = [];
    constructor(
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _progressService: NgProgress,
        private _sortService: SortService,
        private _toastService: ToastrService,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.getListVoucher;
        this.requestSort = this.sortVoucher;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Voucher ID', field: 'VoucherId', sortable: true },
            { title: 'Partner Name', field: 'PartnerName', sortable: true },
            { title: 'Total Amount', field: 'totalAmount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Voucher Date', field: 'date', sortable: true },
            { title: 'Issued Date', field: 'datetimeCreated', sortable: true },
            { title: 'Creator', field: 'creatorName', sortable: true },
        ];
        this.dataSearch = {
            typeOfAcctManagement: 'Voucher'
        };
        this.getListVoucher();
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'vat': {
                this._router.navigate([`home/accounting/management/vat-invoice`]);
                break;
            }
            case 'cdi': {
                this._router.navigate([`home/accounting/management/cd-invoice`]);
                break;
            }
        }
    }

    getListVoucher() {
        this._progressRef.start();
        this._accountingRepo.getListAcctMngt(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                }),
                // tslint:disable-next-line: no-any
                map((data: any) => {
                    return {
                        // tslint:disable-next-line: no-any
                        data: data.data.map((item: any) => new AccAccountingManagementResult(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                // tslint:disable-next-line: no-any
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.vouchers = res.data;
                },
            );
    }

    // tslint:disable-next-line: no-any
    onSearchVoucher($event: any) {
        this.page = 1;
        this.dataSearch = $event;
        this.getListVoucher();
    }

    sortVoucher(sortField: string) {
        this.vouchers = this._sortService.sort(this.vouchers, sortField, this.order);
    }
}