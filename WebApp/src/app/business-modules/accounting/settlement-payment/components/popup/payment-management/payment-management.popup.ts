import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize } from 'rxjs/operators';
import { SortService } from 'src/app/shared/services';

@Component({
    selector: 'payment-management-popup',
    templateUrl: './payment-management.popup.html'
})

export class SettlementPaymentManagementPopupComponent extends PopupBase {

    headerAdvance: CommonInterface.IHeaderTable[];
    headerSettlement: CommonInterface.IHeaderTable[];

    data: IPaymentManagement = null;

    constructor(
        private _accountingRepo: AccoutingRepo,
        private _progressService: NgProgress,
        private _sortService: SortService
    ) {
        super();

        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        // this.headerAdvance = [
        //     { title: 'Advance No', field: 'advanceNo', sortable: true },
        //     { title: 'Description', field: 'description', sortable: true },
        //     { title: 'Total Amount', field: 'totalAmount', sortable: true },
        //     { title: 'Currency', field: 'currency', sortable: true },
        //     { title: 'Advance Date', field: 'advanceDate', sortable: true },
        // ];

        // this.headerSettlement = [
        //     { title: 'Settlement No', field: 'AdvanceNo', sortable: true },
        //     { title: 'Charge Name', field: 'AdvanceNo', sortable: true },
        //     { title: 'Total Amount', field: 'AdvanceNo', sortable: true },
        //     { title: 'Currency', field: 'AdvanceNo', sortable: true },
        //     { title: 'Settlement Date', field: 'AdvanceNo', sortable: true },
        //     { title: 'OBH Partner', field: 'AdvanceNo', sortable: true },
        // ];

        this.data = {
            jobId: '',
            hbl: '',
            mbl: '',
            balance: '',
            totalSettlement: '',
            totalAdvance: '',
            advancePayment: [],
            settlementPayment: []
        };
    }

    getDataPaymentManagement(jobId: string, hbl: string, mbl: string) {
        this._progressRef.start();
        this._accountingRepo.getPaymentManagement(jobId, mbl, hbl)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (response: IPaymentManagement) => {
                    this.data = response;
                }
            );
    }

    sortAdvancePayment(dataSort: CommonInterface.ISortData) {
        this.data.advancePayment = this._sortService.sort(this.data.advancePayment, dataSort.sortField, dataSort.order);
    }

    sortSettlementPayment(dataSort: CommonInterface.ISortData) {
        this.data.settlementPayment = this._sortService.sort(this.data.settlementPayment, dataSort.sortField, dataSort.order);
    }


}

interface IPaymentManagement {
    jobId: string;
    hbl: string;
    mbl: string;
    balance: string;
    totalAdvance: string;
    totalSettlement: string;
    settlementPayment: any[];
    advancePayment: any[];
}
