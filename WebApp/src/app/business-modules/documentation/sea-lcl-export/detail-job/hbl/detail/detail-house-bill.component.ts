import { Component, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { DocumentationRepo, CatalogueRepo } from '@repositories';
import { CsTransactionDetail } from '@models';
import { ChargeConstants, RoutingConstants } from '@constants';

import { SeaLCLExportCreateHBLComponent } from '../create/create-house-bill.component';
import * as fromShareBussiness from './../../../../../share-business/store';

import { catchError, skip, switchMap, takeUntil, tap } from 'rxjs/operators';
import isUUID from 'validator/es/lib/isUUID';
import { delayTime } from '@decorators';
import { formatDate } from '@angular/common';
import { ConfirmPopupComponent, InfoPopupComponent } from '@common';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';

@Component({
    selector: 'app-detail-hbl-lcl-export',
    templateUrl: './detail-house-bill.component.html'
})

export class SeaLCLExportDetailHBLComponent extends SeaLCLExportCreateHBLComponent implements OnInit, AfterViewInit {

    hblId: string;
    hblDetail: CsTransactionDetail;
    shipmentType: string;

    constructor(
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
                        // this._store.dispatch(new fromShareBussiness.SaveContainerAction(res.csMawbcontainers || []));
                        this._store.dispatch(new fromShareBussiness.GetContainersHBLSuccessAction(this.hblDetail.csMawbcontainers));

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
        this.formCreateHBLComponent.isSubmitted = true;
        this.goodSummaryComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: this.invalidFormText
            });
            return;
        }

        const modelUpdate: any = this.getDataForm();
        modelUpdate.id = this.hblId;
        modelUpdate.jobId = this.jobId;
        modelUpdate.userCreated = this.hblDetail.userCreated;
        modelUpdate.shipmentType = this.shipmentType;

        this.updateHbl(modelUpdate);

    }

    showCreatepoup() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Save HBL',
            body: this.confirmUpdateHblText,
            labelCancel: 'No',
            labelConfirm: 'Yes'
        }, () => { this.onSaveHBL() });
    }

    updateHbl(body: any) {
        body.transactionType = ChargeConstants.SLE_CODE;
        const deliveryDate = {
            deliveryDate: !!this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate && !!this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate.startDate ? formatDate(this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate.startDate, 'yyyy-MM-dd', 'en') : this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate.startDate == null ? null : this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate,
        };
        body.deliveryPerson = this.proofOfDeliveryComponent.proofOfDelievey.deliveryPerson;
        body.note = this.proofOfDeliveryComponent.proofOfDelievey.note;
        body.referenceNoProof = this.proofOfDeliveryComponent.proofOfDelievey.referenceNo;
        const checkPoint = {
            partnerId: body.customerId,
            salesmanId: body.saleManId,
            transactionType: 'DOC',
            type: 8,
            hblId: this.hblId
        };
        this._documentationRepo.validateCheckPointContractPartner(checkPoint)
            .pipe(
                switchMap(
                    (res: CommonInterface.IResult) => {
                        if (!res.status) {
                            this._toastService.warning(res.message);
                            return of(false);
                        }
                        return this._documentationRepo.updateHbl(Object.assign({}, body, deliveryDate))
                            .pipe(
                                tap(() => {
                                    if (this.proofOfDeliveryComponent.fileList !== null && this.proofOfDeliveryComponent.fileList.length !== 0 && this.proofOfDeliveryComponent.files === null) {
                                        this.proofOfDeliveryComponent.uploadFilePOD();
                                    }
                                }),
                                catchError(this.catchError),
                            )
                    }
                )
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));

                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    sendMail(type: any) {
        this._documentationRepo.validateCheckPointContractPartner({
            partnerId: this.hblDetail.customerId,
            hblId: this.hblId,
            transactionType: 'DOC',
            type: 7,
            salesmanId: this.hblDetail.saleManId
        }, 'false')
            .pipe(
                catchError((err: HttpErrorResponse) => {
                    if (!!err.error.message) {
                        this._toastService.error("Can not Send mail. " + err.error.message + ". Please recheck again.");
                    }
                    return throwError(err.error.message);
                })
            ).subscribe(
                (res: any) => {
                    if (res.status) {
                        switch (type) {
                            case 'Pre-Alert':
                                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}/hbl/${this.hblId}/manifest`]);
                                break;
                            case 'POD':
                                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}/hbl/${this.hblId}/proofofdelivery`]);
                                break;
                            case 'HBL':
                                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}/hbl/${this.hblId}/sendhbl`]);
                                break;
                        }
                    }
                },
            );
    }
    // preview(reportType: string) {
    //     this._documentationRepo.previewSeaHBLOfLanding(this.hblId, reportType)
    //         .pipe(
    //             catchError(this.catchError),
    //         )
    //         .subscribe(
    //             (res: any) => {
    //                 this.dataReport = res;
    //                 if (this.dataReport.dataSource.length > 0) {
    //                     this.showReport();
    //                 } else {
    //                     this._toastService.warning('There is no data to display preview');
    //                 }
    //             },
    //         );
    // }

    // previewAttachList() {
    //     this._documentationRepo.previewAirAttachList(this.hblId)
    //         .pipe(
    //             catchError(this.catchError),
    //         )
    //         .subscribe(
    //             (res: any) => {
    //                 this.dataReport = res;
    //                 if (this.dataReport.dataSource.length > 0) {
    //                     this.showReport();
    //                 } else {
    //                     this._toastService.warning('There is no data to display preview');
    //                 }
    //             },
    //         );
    // }

    // @delayTime(1000)
    // showReport(): void {
    //     this.reportPopup.frm.nativeElement.submit();
    //     this.reportPopup.show();
    // }
}
