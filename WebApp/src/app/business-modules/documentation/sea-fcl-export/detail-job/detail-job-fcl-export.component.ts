import { Component, OnInit } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { getParamsRouterState, getQueryParamsRouterState } from 'src/app/store';
import { SeaFCLExportCreateJobComponent } from '../create-job/create-job-fcl-export.component';
import { DocumentationRepo } from 'src/app/shared/repositories';

import { combineLatest, of } from 'rxjs';
import { tap, map, switchMap, take, catchError, takeUntil, skip } from 'rxjs/operators';

import * as fromShareBussiness from './../../../share-business/store';
import * as fromStore from './../store';

type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL';

@Component({
    selector: 'app-detail-job-fcl-export',
    templateUrl: './detail-job-fcl-export.component.html'
})

export class SeaFCLExportDetailJobComponent extends SeaFCLExportCreateJobComponent implements OnInit {

    jobId: string;
    selectedTab: TAB | string = 'SHIPMENT';
    action: any = {};
    constructor(
        private _store: Store<fromShareBussiness.TransactionActions>,
        protected _toastService: ToastrService,
        protected _documenRepo: DocumentationRepo,
        protected _router: Router,
        protected _actionStoreSubject: ActionsSubject,
    ) {
        super(_toastService, _documenRepo, _router, _actionStoreSubject);
    }

    ngAfterViewInit() {
        combineLatest([
            this._store.select(getParamsRouterState),
            this._store.select(getQueryParamsRouterState),
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams })),
            tap((param: any) => {
                this.selectedTab = !!param.tab ? param.tab.toUpperCase() : 'SHIPMENT';
                this.jobId = !!param.jobId ? param.jobId : '';
                // if (param.action) {
                //     this.ACTION = param.action.toUpperCase();
                // }

                // this.cdr.detectChanges();
            }),
            switchMap(() => of(this.jobId)),
            take(1)
        ).subscribe(
            (jobId: string) => {
                if (!!jobId) {
                    this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));
                    this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: jobId }));
                    this._store.dispatch(new fromStore.SeaFCLExportGetDetailAction(jobId));

                    this.getListContainer();
                    this.getDetailSeaFCLImport();
                }
            }
        );
    }

    getDetailSeaFCLImport() {
        this._store.select<any>(fromStore.getSeaFCLShipmentDetail)
            .pipe(
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        // * Update Good Summary.
                        this.shipmentGoodSummaryComponent.containerDetail = res.packageContainer;
                        this.shipmentGoodSummaryComponent.commodities = res.commodity;
                        this.shipmentGoodSummaryComponent.description = res.desOfGoods;
                        this.shipmentGoodSummaryComponent.grossWeight = res.grossWeight;
                        this.shipmentGoodSummaryComponent.netWeight = res.netWeight;
                        this.shipmentGoodSummaryComponent.totalChargeWeight = res.chargeWeight;
                        this.shipmentGoodSummaryComponent.totalCBM = res.cbm;
                    }
                },
            );
    }

    saveJob(body: any) {
        body.id = this.jobId;
        this._documenRepo.updateCSTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * get detail & container list.
                        this._store.dispatch(new fromStore.SeaFCLExportGetDetailFailAction(this.jobId));

                        this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: this.jobId }));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    getListContainer() {
        this._store.select<any>(fromShareBussiness.getContainerSaveState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (containers: any) => {
                    this.containers = containers || [];

                    this.shipmentGoodSummaryComponent.containers = this.containers;
                }
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            // case 'hbl':
            //     this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}/hbl`]);
            //     break;
            case 'shipment':
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action) });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            
        }
    }
}
