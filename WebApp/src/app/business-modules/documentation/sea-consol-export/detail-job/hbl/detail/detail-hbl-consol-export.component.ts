import { Component, OnInit, AfterViewInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { DocumentationRepo, CatalogueRepo } from '@repositories';
import { CsTransactionDetail } from '@models';
import { ReportPreviewComponent } from '@common';
import { ChargeConstants } from '@constants';
import { delayTime } from '@decorators';
import { ICrystalReport } from '@interfaces';

import { SeaConsolExportCreateHBLComponent } from '../create/create-hbl-consol-export.component';
import * as fromShareBussiness from './../../../../../share-business/store';

import { catchError, finalize, skip, takeUntil } from 'rxjs/operators';
import isUUID from 'validator/lib/isUUID';

@Component({
    selector: 'app-detail-hbl-consol-export',
    templateUrl: './detail-hbl-consol-export.component.html'
})

export class SeaConsolExportDetailHBLComponent extends SeaConsolExportCreateHBLComponent implements OnInit, AfterViewInit, ICrystalReport {

    @ViewChild(ReportPreviewComponent) reportPopup: ReportPreviewComponent;

    hblId: string;
    hblDetail: CsTransactionDetail;

    constructor(
        protected _progressService: NgProgress,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
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
            _catalogueRepo,
            _toastService,
            _actionStoreSubject,
            _router,
            _cd
        );
    }

    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.hblId && isUUID(param.hblId)) {
                this.hblId = param.hblId;
                this.jobId = param.jobId;

                this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

                this.permissionHblDetail = this._store.select(fromShareBussiness.getDetailHBlPermissionState);

                this.getDetailHbl();

            } else {
                this.gotoList();
            }
        });
        this.isLocked = this._store.select(fromShareBussiness.getTransactionLocked);
    }

    // ! Override ngAfterViewInit in SeaFCLExportCreateHBLComponent
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
                        // * Dispatch to save containers.
                        this._store.dispatch(new fromShareBussiness.GetContainersHBLSuccessAction(res.csMawbcontainers || []));

                        // * Get container to update model
                        this.getListContainer();
                    }
                },
            );
    }

    getListContainer() {
        this._store.select<any>(fromShareBussiness.getHBLContainersState)
            .pipe(takeUntil(this.ngUnsubscribe))
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

        const modelUpdate: any = this.getDataForm();
        modelUpdate.id = this.hblId;
        modelUpdate.jobId = this.jobId;
        modelUpdate.userCreated = this.hblDetail.userCreated;

        this._catalogueRepo.getSalemanIdByPartnerId(modelUpdate.customerId, this.jobId).subscribe((res: any) => {
            if (!!res.salemanId) {
                if (res.salemanId !== modelUpdate.saleManId) {
                    this._toastService.error('Not found contract information, please check!');
                    return;
                }
            }
            if (!!res.officeNameAbbr) {
                this._toastService.error('The selected customer not have any agreement for service in office ' + res.officeNameAbbr + '! Please check Again', 'Cannot Update House Bill!');
            } else {
                this.updateHbl(modelUpdate);
            }
        });
    }

    updateHbl(body: any) {
        this._progressRef.start();
        body.transactionType = body.transactionType = ChargeConstants.SCE_CODE;
        this._documentationRepo.updateHbl(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
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
                        this.showReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewAttachList() {
        this._documentationRepo.previewAirAttachList(this.hblId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport.dataSource.length > 0) {
                        this.showReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    @delayTime(1000)
    showReport(): void {
        this.reportPopup.frm.nativeElement.submit();
        this.reportPopup.show();
    }

}
