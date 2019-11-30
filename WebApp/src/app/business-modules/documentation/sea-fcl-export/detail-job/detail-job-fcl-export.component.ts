import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { SeaFCLExportCreateJobComponent } from '../create-job/create-job-fcl-export.component';
import { DocumentationRepo } from 'src/app/shared/repositories';

import { combineLatest, of } from 'rxjs';
import { tap, map, switchMap, catchError, takeUntil, skip, take } from 'rxjs/operators';

import * as fromShareBussiness from './../../../share-business/store';
import { ReportPreviewComponent } from 'src/app/shared/common';

type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL';

@Component({
    selector: 'app-detail-job-fcl-export',
    templateUrl: './detail-job-fcl-export.component.html'
})

export class SeaFCLExportDetailJobComponent extends SeaFCLExportCreateJobComponent implements OnInit {
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;

    jobId: string;
    selectedTab: TAB | string = 'SHIPMENT';
    action: any = {};

    shipmentDetail: any;
    dataReport: any = null;
    constructor(
        private _store: Store<fromShareBussiness.TransactionActions>,
        protected _toastService: ToastrService,
        protected _documenRepo: DocumentationRepo,
        protected _router: Router,
        protected _actionStoreSubject: ActionsSubject,
        protected _cd: ChangeDetectorRef,
        protected _activedRoute: ActivatedRoute

    ) {
        super(_toastService, _documenRepo, _router, _actionStoreSubject, _cd);
    }

    ngAfterViewInit() {
        combineLatest([
            this._activedRoute.params,
            this._activedRoute.queryParams
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
        ).subscribe(
            (jobId: string) => {
                if (!!jobId) {
                    this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));
                    this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: jobId }));
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(jobId));

                    this.getListContainer();
                    this.getDetailSeaFCLImport();
                }
            }
        );
    }

    getDetailSeaFCLImport() {
        this._store.select<any>(fromShareBussiness.getTransactionDetailCsTransactionState)
            .pipe(
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.shipmentDetail = res;

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

    onSaveJob() {
        this.formCreateComponent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        if (!this.containers.length) {
            this._toastService.warning('Please add container to create new job');
            return;
        }

        const modelAdd = this.onSubmitData();
        modelAdd.csMawbcontainers = this.containers; // * Update containers model

        //  * Update field
        modelAdd.csMawbcontainers = this.containers;
        modelAdd.id = this.jobId;
        modelAdd.branchId = this.shipmentDetail.branchId;
        modelAdd.transactionType = this.shipmentDetail.transactionType;
        modelAdd.jobNo = this.shipmentDetail.jobNo;
        modelAdd.datetimeCreated = this.shipmentDetail.datetimeCreated;
        modelAdd.userCreated = this.shipmentDetail.userCreated;

        this.saveJob(modelAdd);
    }

    saveJob(body: any) {
        this._documenRepo.updateCSTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * get detail & container list.
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

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

                    console.log(this.containers);
                }
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'hbl':
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}/hbl`]);
                break;
            case 'shipment':
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action) });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
        }
    }

    previewPLsheet(currency: string) {
        this._documenRepo.previewSIFPLsheet(this.jobId, currency)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        setTimeout(() => {
                            this.previewPopup.frm.nativeElement.submit();
                            this.previewPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }
}
