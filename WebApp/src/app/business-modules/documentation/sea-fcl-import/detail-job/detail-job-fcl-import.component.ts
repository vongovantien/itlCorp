import { Component, ViewChild, ChangeDetectorRef, OnInit } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { Router, ActivatedRoute, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { SeaFCLImportCreateJobComponent } from '../create-job/create-job-fcl-import.component';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from 'src/app/shared/common/popup';
import { ReportPreviewComponent, SubHeaderComponent } from 'src/app/shared/common';

import { combineLatest, of, Observable } from 'rxjs';
import { map, tap, switchMap, skip, catchError, takeUntil, finalize, concatMap } from 'rxjs/operators';

import * as fromShareBussiness from './../../../share-business/store';

type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL';

import isUUID from 'validator/lib/isUUID';
import { CsTransaction } from '@models';
import { ICanComponentDeactivate } from '@core';

@Component({
    selector: 'app-detail-job-fcl-import',
    templateUrl: './detail-job-fcl-import.component.html',
})
export class SeaFCLImportDetailJobComponent extends SeaFCLImportCreateJobComponent implements OnInit, ICanComponentDeactivate {

    @ViewChild(SubHeaderComponent, { static: false }) headerComponent: SubHeaderComponent;
    @ViewChild("deleteConfirmTemplate", { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild("duplicateconfirmTemplate", { static: false }) confirmDuplicatePopup: ConfirmPopupComponent;
    @ViewChild("confirmLockShipment", { static: false }) confirmLockShipmentPopup: ConfirmPopupComponent;
    @ViewChild("confirmCancelPopup", { static: false }) confirmCancelPopup: ConfirmPopupComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild(Permission403PopupComponent, { static: false }) permissionPopup: Permission403PopupComponent;

    jobId: string;
    selectedTab: TAB | string = 'SHIPMENT';
    ACTION: CommonType.ACTION_FORM | string = 'UPDATE';

    fclImportDetail: CsTransaction;
    action: any = {};
    nextState: RouterStateSnapshot;

    isCancelFormPopupSuccess: boolean = false;

    constructor(
        protected _router: Router,
        protected _documentRepo: DocumentationRepo,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.ITransactionState>,
        protected _actionStoreSubject: ActionsSubject,
        protected _toastService: ToastrService,
        protected cdr: ChangeDetectorRef,
        private _ngProgressService: NgProgress
    ) {
        super(_router, _documentRepo, _actionStoreSubject, _toastService, cdr);

        this._progressRef = this._ngProgressService.ref();
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
                if (param.action) {
                    this.ACTION = param.action.toUpperCase();
                } else {
                    this.ACTION = null;
                }

                this.cdr.detectChanges();
            }),
            switchMap(() => of(this.jobId))
        ).subscribe(
            (jobId: string) => {
                if (isUUID(jobId)) {
                    this.jobId = jobId;
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(jobId));
                    this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: jobId }));
                    this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));

                    this.getDetailSeaFCLImport();
                    this.getListContainer();
                } else {
                    this.gotoList();
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
                    this.fclImportDetail = res; // TODO Model.
                    // * reset field duplicate
                    if (this.ACTION === "COPY") {
                        this.formCreateComponent.getUserLogged();
                        this.headerComponent.resetBreadcrumb("Create Job");
                    }
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

                    this.shipmentGoodSummaryComponent.containers = this.containers;
                    if (this.ACTION === 'COPY') {
                        this.containers.forEach(item => {
                            item.sealNo = null;
                            item.containerNo = null;
                            item.markNo = null;
                        });
                    }
                }
            );
    }

    onUpdateShipmentDetail() {
        this.formCreateComponent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        const modelUpdate = this.onSubmitData();

        //  * Update field

        modelUpdate.csMawbcontainers = this.containers;

        modelUpdate.csMawbcontainers.forEach(c => {
            c.mblid = this.jobId;
        });

        modelUpdate.id = this.jobId;
        modelUpdate.branchId = this.fclImportDetail.branchId;
        modelUpdate.transactionType = this.fclImportDetail.transactionType;
        modelUpdate.jobNo = this.fclImportDetail.jobNo;
        modelUpdate.datetimeCreated = this.fclImportDetail.datetimeCreated;
        modelUpdate.userCreated = this.fclImportDetail.userCreated;
        modelUpdate.currentStatus = this.fclImportDetail.currentStatus;

        if (this.ACTION === 'COPY') {
            this.duplicateJob(modelUpdate);
        } else {
            this.updateJob(modelUpdate);
        }
    }

    duplicateJob(body: any) {
        this._documenRepo.importCSTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success("Duplicate data successfully");
                        this.jobId = res.data.id;

                        // * get detail & container list.
                        this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                        this.ACTION = 'SHIPMENT';
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    updateJob(body: any) {
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

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'hbl':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}/hbl`]);
                break;
            case 'shipment':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action) });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            case 'assignment':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
        }
    }

    prepareDeleteJob() {
        this._documentRepo.checkPermissionAllowDeleteShipment(this.jobId)
            .pipe(
                concatMap((isAllowDelete: boolean) => {
                    if (isAllowDelete) {
                        return this._documenRepo.checkMasterBillAllowToDelete(this.jobId);
                    }
                    return of(403);
                }),
                concatMap((isValid) => {
                    if (isValid) {
                        return of(200);
                    }
                    return of(201);
                })
            )
            .subscribe((value: number) => {
                if (value === 403) {
                    this.permissionPopup.show();
                    return;
                }
                if (value === 200) {
                    this.confirmDeletePopup.show();
                    return;
                } else {
                    this.canNotDeleteJobPopup.show();
                }
            });
    }

    onDeleteJob() {
        this._progressRef.start();
        this._documenRepo.deleteMasterBill(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.confirmDeletePopup.hide();
                })
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {

                        this._toastService.success(respone.message, 'Delete Success !');

                        this.gotoList();
                    }
                },
            );
    }

    lockShipment() {
        this.confirmLockShipmentPopup.show();
    }

    onLockShipment() {
        this.confirmLockShipmentPopup.hide();

        this._progressRef.start();
        this._documenRepo.LockCsTransaction(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                })
            )
            .subscribe(
                (r: CommonInterface.IResult) => {
                    if (r.status) {
                        this._toastService.success(r.message);
                    } else {
                        this._toastService.error(r.message);
                    }
                },
            );
    }

    onSyncHBL() {
        this.formCreateComponent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        const modelAdd = this.onSubmitData();

        const bodySyncData: DocumentationInterface.IDataSyncHBL = {
            flightVesselName: modelAdd.flightVesselName,
            etd: modelAdd.etd,
            eta: modelAdd.eta,
            pol: modelAdd.pol,
            pod: modelAdd.pod,
            bookingNo: modelAdd.bookingNo,
            voyNo: modelAdd.voyNo,
            mblNo: modelAdd.mawb
        };

        this._progressRef.start();
        this._documenRepo.syncHBL(this.jobId, bodySyncData)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                })
            ).subscribe(
                (r: CommonInterface.IResult) => {
                    if (r.status) {
                        this._toastService.success(r.message);
                    } else {
                        this._toastService.error(r.message);
                    }
                },
            );
    }
    showDuplicateConfirm() {
        this.confirmDuplicatePopup.show();
    }

    duplicateConfirm() {
        this.action = { action: 'copy' };
        this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], {
            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action)
        });
        this.confirmDuplicatePopup.hide();
    }

    gotoList() {
        this._router.navigate(["home/documentation/sea-fcl-import"]);
    }

    previewPLsheet(currency: string) {
        const hblid = "00000000-0000-0000-0000-000000000000";
        this._documenRepo.previewSIFPLsheet(this.jobId, hblid, currency)
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

    previewShipmentCoverPage() {
        this._documenRepo.previewShipmentCoverPage(this.jobId)
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

    handleCancelForm() {
        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formCreate.getRawValue());
        if (isEdited) {
            this.confirmCancelPopup.show();
        } else {
            this.isCancelFormPopupSuccess = true;
            this.gotoList();
        }
    }

    confirmCancel() {
        this.confirmCancelPopup.hide();
        this.isCancelFormPopupSuccess = true;

        if (this.nextState) {
            this._router.navigate([this.nextState.url.toString()]);
        } else {
            this.gotoList();
        }
    }

    canDeactivate(currenctRoute: ActivatedRouteSnapshot, currentState: RouterStateSnapshot, nextState: RouterStateSnapshot): Observable<boolean> {
        this.nextState = nextState; // * Save nextState for Deactivate service.

        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formCreate.getRawValue());
        if (this.isCancelFormPopupSuccess) {
            return of(true);
        }
        if (isEdited && !this.isCancelFormPopupSuccess) {
            this.confirmCancelPopup.show();
            return;
        }
        return of(!isEdited);
    }
}
