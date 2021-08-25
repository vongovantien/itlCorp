import { Component, OnInit } from '@angular/core';
import { AppList } from '@app';

import { SortService } from '@services';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { AccountingRepo } from '@repositories';
import { AccReceivableDetailModel, AccReceivableOfficesDetailModel } from '@models';
import { RoutingConstants } from '@constants';

import { takeUntil, switchMap } from 'rxjs/operators';
@Component({
    selector: 'detail-account-receivable',
    templateUrl: 'detail-account-receivable.component.html',
})
export class AccountReceivableDetailComponent extends AppList implements OnInit {
    subTab: string;

    accReceivableDetail: AccReceivableDetailModel = new AccReceivableDetailModel();
    accReceivableMoreDetail: AccReceivableOfficesDetailModel[] = [];
    subHeaders: any[];

    constructor(
        private _sortService: SortService,
        private _accoutingRepo: AccountingRepo,
        private _activedRoute: ActivatedRoute,
        private _router: Router,
    ) {
        super();
        this.requestSort = this.sortDetailList;
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

    initHeaders() {
        this.headers = [
            { title: 'Office (Branch)', field: 'officeName', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Debit Amount', field: 'totalDebitAmount', sortable: true },
            { title: 'Billing (Unpaid)', field: 'totalBillingAmount', sortable: true },
            { title: 'Paid', field: 'totalPaidAmount', sortable: true },
            { title: 'OutStanding Balance', field: 'totalBillingUnpaid', sortable: true },
            // { title: 'OBH Amount', field: 'totalObhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'totalOver1To15Day', sortable: true },
            { title: 'Over 16-30 days', field: 'totalOver15To30Day', sortable: true },
            { title: 'Over 30 Days', field: 'totalOver30Day', sortable: true },

        ];
        this.subHeaders = [
            { title: 'Service', field: 'serviceName', sortable: true },
            { title: 'Debit Amount', field: 'debitAmount', sortable: true },
            { title: 'Billing (Unpaid)', field: 'billingAmount', sortable: true },
            { title: 'Paid', field: 'paidAmount', sortable: true },
            { title: 'OutStanding Balance', field: 'billingUnpaid', sortable: true },
            // { title: 'OBH Amount', field: 'obhAmount', sortable: true },
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

    goBack() {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/summary`],
            { queryParams: { subTab: this.subTab } });
    }
}