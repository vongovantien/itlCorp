import { Component, OnInit, AfterViewInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { DocumentationRepo } from 'src/app/shared/repositories';
import { SeaFCLExportCreateHBLComponent } from '../create/create-house-bill.component';
import { CsTransactionDetail } from 'src/app/shared/models';

import * as fromShareBussiness from './../../../../../share-business/store';
import { catchError, finalize, skip, takeUntil } from 'rxjs/operators';
import { Crystal } from 'src/app/shared/models/report/crystal.model';
import { ReportPreviewComponent } from 'src/app/shared/common';

@Component({
    selector: 'app-detail-hbl-fcl-export',
    templateUrl: './detail-house-bill.component.html'
})

export class SeaFCLExportDetailHBLComponent extends SeaFCLExportCreateHBLComponent implements OnInit, AfterViewInit {
    @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;

    hblId: string;

    hblDetail: CsTransactionDetail;

    dataReport: Crystal;

    constructor(
        protected _progressService: NgProgress,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _cd: ChangeDetectorRef

    ) {
        super(
            _progressService,
            _activedRoute,
            _store,
            _documentationRepo,
            _toastService,
            _actionStoreSubject,
            _router,
            _cd
        );
    }

    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.hblId) {
                this.hblId = param.hblId;
                this.jobId = param.jobId;
                this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

                this.getDetailHbl();

            } else {
                // TODO handle error. 
            }
        });

        this.isLocked = this._store.select(fromShareBussiness.getTransactionLocked);
    }

    ngAfterViewInit() {

    }

    getDetailHbl() {
        this._store.select(fromShareBussiness.getDetailHBlState)
            .pipe(
                skip(1),
                catchError(this.catchError),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CsTransactionDetail) => {
                    if (!!res) {
                        this.hblDetail = res;
                        // this.goodSummaryComponent.containerDetail = this.hblDetail.packageContainer;
                        // this.goodSummaryComponent.commodities = this.hblDetail.commodity;
                        // this.goodSummaryComponent.description = this.hblDetail.desOfGoods;
                        // this.goodSummaryComponent.grossWeight = this.hblDetail.grossWeight;
                        // this.goodSummaryComponent.netWeight = this.hblDetail.netWeight;
                        // this.goodSummaryComponent.totalChargeWeight = this.hblDetail.chargeWeight;
                        // this.goodSummaryComponent.totalCBM = this.hblDetail.cbm;
                    }

                    // * Dispatch to save containers.
                    this._store.dispatch(new fromShareBussiness.SaveContainerAction(res.csMawbcontainers || []));

                    // * Get container to update model
                    this.getListContainer();
                },
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
                }
            );
    }

    onSaveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const modelUpdate = this.getDataForm();
        modelUpdate.id = this.hblId;
        modelUpdate.jobId = this.jobId;

        this.updateHbl(modelUpdate);
    }

    updateHbl(body: any) {
        this._progressRef.start();
        this._documentationRepo.updateHbl(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._router.navigate([`/home/documentation/sea-fcl-export/${this.jobId}/hbl`]);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    preview(reportType: string) {
        this._documentationRepo.previewSeaHBLOfLanding(this.hblId, reportType)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

}
