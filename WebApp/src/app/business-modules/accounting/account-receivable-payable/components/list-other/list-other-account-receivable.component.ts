import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { catchError, finalize } from 'rxjs/operators';
import { SortService } from '@services';

import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo, ExportRepo } from '@repositories';





@Component({
    selector: 'list-other-account-receivable',
    templateUrl: './list-other-account-receivable.component.html',
})
export class AccountReceivableListOtherComponent extends AppList implements OnInit {

    //
    otherList: any[] = [];

    constructor(private _sortService: SortService,
        private _progressService: NgProgress,
        private _accountingRepo: AccountingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortOtherList;
        this.requestList = this.getPagingList;
    }
    ngOnInit() {
        this.headers = [
            { title: 'Partner Id', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerNameAbbr', sortable: true },

            { title: 'Debit Amount', field: 'debitAmount', sortable: true },

            { title: 'Billing', field: 'billingAmount', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paidAmount', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },

            { title: 'Status', field: 'agreementStatus', sortable: true },
        ];
        //seed page

    }
    sortOtherList(sortField: string, order: boolean) {
        this.otherList = this._sortService.sort(this.otherList, sortField, order);
    }
    getPagingList() {
        this._progressRef.start();
        this.isLoading = true;
        //call api
        this._accountingRepo.receivablePaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.isLoading = false;
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.otherList = res.data || [];
                    console.log("data trả về: ", res.data);

                    this.totalItems = res.totalItems;
                },
            );
    }
}