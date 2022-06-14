import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from '@app';

import { SortService } from '@services';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { AccountingRepo } from '@repositories';
import { AccReceivableDetailModel, AccReceivableOfficesDetailModel, AccReceivableServicesDetailModel } from '@models';
import { RoutingConstants } from '@constants';

import { takeUntil, switchMap, switchMapTo, tap, catchError, finalize } from 'rxjs/operators';
import { ConfirmPopupComponent } from '@common';
import { InjectViewContainerRefDirective } from '@directives';
import { of } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { IAppState, getMenuUserSpecialPermissionState } from '@store';
import { Store } from '@ngrx/store';
import { AccReceivableDebitDetailPopUpComponent } from '../../components/popup/account-receivable-debit-detail-popup.component';
import { NgProgress } from '@ngx-progressbar/core';
@Component({
    selector: 'detail-account-receivable',
    templateUrl: 'detail-account-receivable.component.html',
})
export class AccountReceivableDetailComponent extends AppList implements OnInit {
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;
    @ViewChild(AccReceivableDebitDetailPopUpComponent) debitDetailPopupComponent: AccReceivableDebitDetailPopUpComponent;
    subTab: string;

    accReceivableDetail: AccReceivableDetailModel = new AccReceivableDetailModel();
    accReceivableMoreDetail: AccReceivableOfficesDetailModel[] = [];
    subHeaders: any[];
    agreementId: string;
    partnerId: string;
    salemanId: string;

    constructor(
        private readonly _sortService: SortService,
        private readonly _accoutingRepo: AccountingRepo,
        private readonly _activedRoute: ActivatedRoute,
        private readonly _router: Router,
        private readonly _toastService: ToastrService,
        private readonly _store: Store<IAppState>,
        private _progressService: NgProgress,

    ) {
        super();
        this.requestSort = this.sortDetailList;
        this._progressRef = this._progressService.ref();
    }
    ngOnInit() {
        this.initHeaders();
        this._activedRoute.queryParams
            .pipe(
                takeUntil(this.ngUnsubscribe),
                tap((p: Params) => {
                    this.agreementId = p['agreementId'];
                    this.partnerId = p['partnerId'];
                    this.salemanId = p['salemanId'];
                }),
                switchMap((p: Params) => {
                    this.subTab = p.subTab;
                    if (!!p.agreementId) {
                        return this._accoutingRepo.getDetailReceivableByArgeementId(p.agreementId);
                    }
                    return this._accoutingRepo.getDetailReceivableByPartnerId(p.partnerId,p.salemanId);
                }),
            ).subscribe(
                (data: any) => {
                    if(data.accountReceivable ===null){
                        this._toastService.info("Chưa có data công nợ");
                    }
                    this.accReceivableDetail = new AccReceivableDetailModel(data.accountReceivable);
                    this.accReceivableMoreDetail = (data.accountReceivableGrpOffices || [])
                        .map((item: AccReceivableOfficesDetailModel) => new AccReceivableOfficesDetailModel(item));
                }
            );

        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);

    }

    initHeaders() {
        this.headers = [
            { title: 'Office (ABBR)', field: 'officeNameAbbr', sortable: true },
            { title: 'Debit Amount', field: 'totalDebitAmount', sortable: true },
            { title: 'Billing (Unpaid)', field: 'totalBillingAmount', sortable: true },
            { title: 'Paid', field: 'totalPaidAmount', sortable: true },
            { title: 'OutStanding Balance', field: 'totalBillingUnpaid', sortable: true },
            // { title: 'OBH Amount', field: 'totalObhAmount', sortable: true },
            { title: 'Over 1-15 days', field: 'totalOver1To15Day', sortable: true },
            { title: 'Over 16-30 days', field: 'totalOver15To30Day', sortable: true },
            { title: 'Over 30 Days', field: 'totalOver30Day', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },

        ];
        this.subHeaders = [
            { title: '', field: '' },
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

    onClickCalculateService(serviceData: AccReceivableServicesDetailModel, officeData: AccReceivableOfficesDetailModel) {
        console.log({ serviceData, officeData });
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Calculate Accounts Receivable Payable',
            body: 'Are you sure to recalculate data',
        }, () => {
            const body: { partnerId: string, office: string, service: string } = {
                partnerId: this.accReceivableDetail.partnerId,
                office: officeData.officeId,
                service: this.utility.mappingServiceType(serviceData.serviceName),
            };

            console.log({ body });
            this._accoutingRepo.insertOrUpdateReceivable([body])
                .pipe(
                    switchMap((res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            if (this.agreementId) {
                                return this._accoutingRepo.getDetailReceivableByArgeementId(this.agreementId);
                            }
                            return this._accoutingRepo.getDetailReceivableByPartnerId(this.partnerId,this.salemanId);
                        }
                        return of(false);
                    })
                )
                .subscribe((data: any) => {
                    if (!!data) {
                        this.accReceivableDetail = new AccReceivableDetailModel(data.accountReceivable);
                        this.accReceivableMoreDetail = (data.accountReceivableGrpOffices || [])
                            .map((item: AccReceivableOfficesDetailModel) => new AccReceivableOfficesDetailModel(item));
                    }
                });
        })
    }

    showDebitDetail(option,officeId,serviceCode) {
        var argeementId = this.accReceivableDetail.agreementId;
        if(argeementId !==null){
            this._accoutingRepo.getDataDebitDetail(argeementId, option,officeId,serviceCode)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.debitDetailPopupComponent.dataDebitList = res || [];
                        this.debitDetailPopupComponent.dataSearch= {argeementId, option,officeId,serviceCode};
                        this.debitDetailPopupComponent.calculateTotal();
                        this.debitDetailPopupComponent.show();
                    }
                },
            );
        }else{
            this._accoutingRepo.getDataDebitDetailByPartnerId(this.accReceivableDetail.partnerId, option,officeId,serviceCode,this.salemanId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.debitDetailPopupComponent.dataDebitList = res || [];
                        this.debitDetailPopupComponent.dataSearch= {argeementId, option,officeId,serviceCode};
                        this.debitDetailPopupComponent.calculateTotal();
                        this.debitDetailPopupComponent.show();
                    }
                },
            );
        }


    }
}
