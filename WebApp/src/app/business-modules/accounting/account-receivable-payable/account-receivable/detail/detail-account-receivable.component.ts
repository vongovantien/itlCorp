import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';

import { SortService } from '@services';
import { Router, ActivatedRoute, Params } from '@angular/router';

import { CatalogueRepo, AccountingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { DataService } from '@services';
import UUID from 'validator/lib/isUUID';
import _merge from 'lodash/merge';
import { of } from 'rxjs';
import { NgProgress } from '@ngx-progressbar/core';
import { takeUntil, map, switchMap, tap, catchError } from 'rxjs/operators';
import { AccReceivableDetailModel, AccReceivableOfficesDetailModel } from '@models';

@Component({
    selector: 'detail-account-receivable',
    templateUrl: 'detail-account-receivable.component.html'
})



export class AccountReceivableDetailComponent extends AppList implements OnInit {
    subTab: string;

    accReceivableDetail: AccReceivableDetailModel = new AccReceivableDetailModel();
    accReceivableMoreDetail: AccReceivableOfficesDetailModel[] = [];
    subHeaders: any[];
    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _accoutingRepo: AccountingRepo,
        private _activedRoute: ActivatedRoute,
        private _router: Router,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortDetailList;
        this.requestList = this.getPagingGuaranteed;
    }
    ngOnInit() {
        this.initHeaders();
        this._activedRoute.queryParams
            .pipe(
                takeUntil(this.ngUnsubscribe),

                switchMap((p: Params) => {
                    this.subTab = p.subTab;
                    if (!!p.agreementId) {
                        return this._accoutingRepo.getDetailReceivableByArgeementId(p.agreementId);
                    }
                    return this._accoutingRepo.getDetailReceivableByPartnerId(p.partnerId);
                }),
            ).subscribe(
                (data: any) => {

                    this.accReceivableDetail = new AccReceivableDetailModel(data.accountReceivable);
                    this.accReceivableMoreDetail = (data.accountReceivableGrpOffices || [])
                        .map((item: AccReceivableOfficesDetailModel) => new AccReceivableOfficesDetailModel(item));

                }
            );


    }
    //
    initHeaders() {
        this.headers = [
            { title: 'Office (branch)', field: 'officeName', sortable: true },
            { title: 'Debit Amount', field: 'totalDebitAmount', sortable: true },
            { title: 'Billing', field: 'totalBillingAmount', sortable: true },
            { title: 'Billing Unpaid', field: 'totalBillingUnpaid', sortable: true },
            { title: 'Paid', field: 'totalPaidAmount', sortable: true },
            { title: 'OBH Amount', field: 'totalObhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'totalOver1To15Day', sortable: true },
            { title: 'Over 16-30 days', field: 'totalOver15To30Day', sortable: true },
            { title: 'Over 30 Days', field: 'totalOver30Day', sortable: true },

        ];
        this.subHeaders = [
            { title: 'Service', field: 'serviceName', sortable: true },
            { title: 'Debit Amount', field: 'debitAmount', sortable: true },
            { title: 'Billing', field: 'billingAmount', sortable: true },
            { title: 'Billing Unpaid', field: 'billingUnpaid', sortable: true },
            { title: 'Paid', field: 'paidAmount', sortable: true },
            { title: 'OBH Amount', field: 'obhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'over1To15Day', sortable: true },
            { title: 'Over 16-30 days', field: 'over16To30Day', sortable: true },
            { title: 'Over 30 Days', field: 'over30Day', sortable: true },
        ];
    }
    sortDetailList(sortField: string, order: boolean) {
        this.accReceivableMoreDetail = this._sortService.sort(this.accReceivableMoreDetail, sortField, order);
    }

    sortDetailMoreGuaranteed(item: any, sortField: string, order: boolean) {

        item.accountReceivableGrpServices = this._sortService.sort(item.accountReceivableGrpServices, sortField, order);
    }

    getPagingGuaranteed() {

    }

    goBack() {


        //window.history.back();

        this._router.navigate(['/home/accounting/account-receivable-payable/receivable'],
            { queryParams: { subTab: this.subTab } });
    }
}