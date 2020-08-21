import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingRepo, ExportRepo } from '@repositories';
import { SortService } from '@services';
import { Store } from '@ngrx/store';
import { getMenuUserSpecialPermissionState, IAppState } from '@store';
import { SystemConstants } from 'src/constants/system.const';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { ConfirmPopupComponent, InfoPopupComponent } from '@common';


import { PaymentModel, AccountingPaymentModel } from '@models';



@Component({
    selector: 'list-trial-official-account-receivable',
    templateUrl: './list-trial-official-account-receivable.component.html',
})
export class AccountReceivableListTrialOfficialComponent extends AppList implements OnInit {
    //seedData
    seedData: any[] = [
        {
            partnerId: '111111',
            partnerName: 'Cristian',
            creditLimited: 1000000000,
            debitAmount: 1200000000,
            debitRate: 120,
            billing: 900000000,
            billingUnpaid: 800000000,
            paid: 100000000,
            obhAmount: 200000000,
            between1_15days: 400000000,
            between16_30days: 300000000,
            over30days: 100000000,
            contractNo: 'CT00929',
            contractType: 'Offical',
            expiredDate: '30/06/2020',
            expiredDays: 8,
            status: false,
        },
        {
            partnerId: '222222',
            partnerName: 'Maple',
            creditLimited: 1200000000,
            debitAmount: 1500000000,
            debitRate: 50,
            billing: 1000000000,
            billingUnpaid: 900000000,
            paid: 120000000,
            obhAmount: 220000000,
            between1_15days: 410000000,
            between16_30days: 390000000,
            over30days: 150000000,
            contractNo: 'TT00929',
            contractType: 'Demo',
            expiredDate: '22/06/2020',
            expiredDays: 10,
            status: true,
        },
        {
            partnerId: '333333',
            partnerName: 'Maple',
            creditLimited: 1200000000,
            debitAmount: 1500000000,
            debitRate: 60,
            billing: 1000000000,
            billingUnpaid: 900000000,
            paid: 120000000,
            obhAmount: 220000000,
            between1_15days: 410000000,
            between16_30days: 390000000,
            over30days: 150000000,
            contractNo: 'TT00929',
            contractType: 'Demo',
            expiredDate: '22/06/2020',
            expiredDays: 10,
            status: true,
        },
        {
            partnerId: '444444',
            partnerName: 'Maple',
            creditLimited: 1200000000,
            debitAmount: 1500000000,
            debitRate: 80,
            billing: 1000000000,
            billingUnpaid: 900000000,
            paid: 120000000,
            obhAmount: 220000000,
            between1_15days: 410000000,
            between16_30days: 390000000,
            over30days: 150000000,
            contractNo: 'TT00929',
            contractType: 'Demo',
            expiredDate: '22/06/2020',
            expiredDays: 10,
            status: true,
        },
        {
            partnerId: '555555',
            partnerName: 'Maple',
            creditLimited: 1200000000,
            debitAmount: 1500000000,
            debitRate: 150,
            billing: 1000000000,
            billingUnpaid: 900000000,
            paid: 120000000,
            obhAmount: 220000000,
            between1_15days: 410000000,
            between16_30days: 390000000,
            over30days: 150000000,
            contractNo: 'TT00929',
            contractType: 'Demo',
            expiredDate: '22/06/2020',
            expiredDays: 10,
            status: true,
        },
    ];

    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _router: Router,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortTrialOfficalList;
        this.requestList = this.getPagingTrialOffical;
    }
    ngOnInit() {

        this.headers = [
            { title: 'PartnerId', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Credit Limited', field: 'creditLimited', sortable: true },
            { title: 'Debit Amount', field: 'debitAmount', sortable: true },
            { title: 'Debit Rate (%)', field: 'debitRate', sortable: true },
            { title: 'Billing', field: 'billing', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paid', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'between1_15days', sortable: true },
            { title: 'Over 16-30 days', field: 'between16_30days', sortable: true },
            { title: 'Over 30 Days', field: 'over30days', sortable: true },
            { title: 'Contract No', field: 'contractNo', sortable: true },
            { title: 'Contract Type', field: 'contractType', sortable: true },
            { title: 'Expired Date', field: 'expiredDate', sortable: true },
            { title: 'Expired Days', field: 'expiredDays', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
        ];
        //seed paging
        this.totalItems = this.seedData.length;
        this.page = 1;
        this.pageSize = 15;
    }

    sortTrialOfficalList(sortField: string, order: boolean) {
        this.seedData = this._sortService.sort(this.seedData, sortField, order);
    }

    getPagingTrialOffical() {
        //this._progressRef.start();
        //this.isLoading = true;
        //call api
    }

    //
    viewDetail(id: string) {
        this._router.navigate([`/home/accounting/account-receivable-payable/detail/${id}`]);
    }
}