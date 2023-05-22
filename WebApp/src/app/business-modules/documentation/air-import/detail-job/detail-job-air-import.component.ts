import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Store } from '@ngrx/store';
import { Router, ActivatedRoute, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from '@common';
import { DIM } from '@models';
import { AirImportCreateJobComponent } from '../create-job/create-job-air-import.component';
import { ICanComponentDeactivate } from '@core';
import { RoutingConstants } from '@constants';
import { ICrystalReport } from '@interfaces';

import * as fromShareBussiness from '../../../share-business/store';

import { combineLatest, of, Observable } from 'rxjs';
import { tap, map, switchMap, catchError, takeUntil, concatMap } from 'rxjs/operators';

import isUUID from 'validator/es/lib/isUUID';
import { HttpErrorResponse } from '@angular/common/http';

type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL';

@Component({
    selector: 'app-detail-job-air-import',
    templateUrl: './detail-job-air-import.component.html'
})

export class AirImportDetailJobComponent extends AirImportCreateJobComponent implements OnInit, ICanComponentDeactivate, ICrystalReport {

    confirmUpdateFlightInfo: string = 'Do you want to sync Flight No, Flight Date, ETD, ETA to HAWB ?';
    params: any;
    tabList: string[] = ['SHIPMENT', 'CDNOTE', 'ASSIGNMENT', 'FILES', 'ADVANCE-SETTLE'];
    jobId: string;
    selectedTab: TAB | string = 'SHIPMENT';
    action: any = {};

    dimensionDetails: DIM[];
    isCancelFormPopupSuccess: boolean = false;

    nextState: RouterStateSnapshot;

    constructor(
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _toastService: ToastrService,
        protected _documenRepo: DocumentationRepo,
        protected _router: Router,
        protected _cd: ChangeDetectorRef,
        protected _activedRoute: ActivatedRoute,
        protected _exportRepo: ExportRepo,
        protected _fileMngtRepo: SystemFileManageRepo

    ) {
        super(_toastService, _documenRepo, _router, _store, _cd, _exportRepo, _fileMngtRepo);
        this.requestCancel = this.handleCancelForm;
    }

    ngAfterViewInit() {
        this.subscription = combineLatest([
            this._activedRoute.params,
            this._activedRoute.queryParams
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams })),
            tap((param: any) => {
                this.params = param;
                this.selectedTab = (!!param.tab && this.tabList.includes(param.tab.toUpperCase())) ? param.tab.toUpperCase() : 'SHIPMENT';
                this.jobId = !!param.jobId ? param.jobId : '';
                if (param.action) {
                    this.ACTION = param.action.toUpperCase();
                    this.isDuplicate = this.ACTION === 'COPY';
                } else {
                    this.ACTION = null;
                    this.isDuplicate = false;
                }

                this._cd.detectChanges();
            }),
            switchMap(() => of(this.jobId)),
        ).subscribe(
            (jobId: string) => {
                if (isUUID(jobId)) {
                    if (this.selectedTab === this.tabList[0]) {
                        this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));
                        this._store.dispatch(new fromShareBussiness.GetDimensionAction(jobId));
                    }

                    this.getDetailShipment(jobId);
                } else {
                    this.gotoList();
                }
            }
        );
    }

    getDetailShipment(jobId: string) {
        this._documenRepo.getDetailTransaction(jobId)
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.shipmentDetail = res;
                        this.formCreateComponent.isUpdate = true;
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailSuccessAction(res));

                        // * reset field duplicate
                        if (this.ACTION === "COPY") {
                            this.formCreateComponent.getUserLogged();
                            this.headerComponent.resetBreadcrumb("Create Job");
                        } else {
                            this.headerComponent.resetBreadcrumb("Job Detail");
                        }
                    }
                },
            );
    }

    onSaveJob() {
        this.formCreateComponent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                title: 'Cannot Update Job',
                body: this.invalidFormText
            });
            return;
        }
        const modelAdd = this.onSubmitData();
        modelAdd.dimensionDetails = this.formCreateComponent.dimensionDetails;

        for (const item of modelAdd.dimensionDetails) {
            item.mblid = this.shipmentDetail.id;
        }
        //  * Update field
        modelAdd.id = this.jobId;
        modelAdd.branchId = this.shipmentDetail.branchId;
        modelAdd.transactionType = this.shipmentDetail.transactionType;
        modelAdd.jobNo = this.shipmentDetail.jobNo;
        modelAdd.datetimeCreated = this.shipmentDetail.datetimeCreated;
        modelAdd.userCreated = this.shipmentDetail.userCreated;
        modelAdd.currentStatus = this.shipmentDetail.currentStatus;
        modelAdd.isLocked = this.shipmentDetail.isLocked;

        if (this.ACTION === 'COPY') {
            this.duplicateJob(modelAdd);
        } else {
            this.saveJob(modelAdd);
        }
    }

    duplicateJob(body: any) {
        this._documenRepo.importCSTransaction(body)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success("Duplicate data successfully");
                        this.jobId = res.data.id;

                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                        this.ACTION = "SHIPMENT";

                        this.isDuplicate = true;

                    } else {
                        //this._toastService.error(res.message);
                        if (res.data?.errorCode === 453) {
                            this.showHBLsInvalid(res.message);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                }
            );
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

                        // * get detail.
                        // this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                        this.getDetailShipment(this.jobId);
                    } else {
                        if (res.data?.errorCode === 452) {
                            this.showHBLsInvalid(res.message);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                },
                (error: HttpErrorResponse) => {
                    if (error.error?.data?.errorCode) {
                        this.formCreateComponent.formGroup.controls[error.error?.data?.errorCode].setErrors({ existed: true });
                    }
                }
            );
    }

    showHBLsInvalid(message: string) {
        this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Warning',
            body: `You cannot change shipment type because contract on HBL is Cash - Nominated with following: ${message.slice(0, -2)}`,
            class: 'bg-danger'
        });
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'hbl':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}/hbl`]);
                break;
            case 'shipment':
                if (this.ACTION === 'COPY') {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}`], { queryParams: Object.assign({}, { action: 'copy' }) });
                } else {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}`]);
                }
                break;
            case 'cdNote':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}`], { queryParams: { tab: 'CDNOTE', view: this.params.view, export: this.params.export } });
                break;
            case 'assignment':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
            case 'files':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}`], { queryParams: { tab: 'FILES' } });
                break;
            case 'advance-settle':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}`], { queryParams: { tab: 'ADVANCE-SETTLE' } });
                break;
        }
    }

    prepareDeleteJob() {
        this._documenRepo.checkPermissionAllowDeleteShipment(this.jobId)
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
                    this.showPopupDynamicRender(Permission403PopupComponent, this.viewContainerRef.viewContainerRef, {});
                    return;
                }
                if (value === 200) {
                    this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                        title: 'Alert',
                        body: this.confirmDeleteJob,
                        labelConfirm: 'Yes'
                    }, () => { this.onDeleteJob(); });
                    return;
                } else {
                    this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                        body: 'You are not allowed to delete this job',
                    });
                    return;
                }
            });
    }

    onDeleteJob() {
        this._documenRepo.deleteMasterBill(this.jobId)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {

                        this._toastService.success(respone.message, 'Delete Success !');

                        this.gotoList();
                    }
                },
            );
    }

    showDuplicateConfirm() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Duplicate job detail',
            body: 'The system will open the Job Create Screen. Are you sure you want to leave?',
            labelConfirm: 'Yes'
        }, () => { this.duplicateConfirm(); });
    }

    duplicateConfirm() {
        this._documenRepo.getPartnerForCheckPointInShipment(this.jobId, 'AI')
            .pipe(
                takeUntil(this.ngUnsubscribe),
                switchMap((partnerIds: string[]) => {
                    if (!!partnerIds.length) {
                        const criteria: DocumentationInterface.ICheckPointCriteria = {
                            data: partnerIds,
                            transactionType: 'DOC',
                            settlementCode: null,
                        };
                        return this._documenRepo.validateCheckPointMultiplePartner(criteria)
                    }
                    return of({ data: null, message: null, status: true });
                })
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.action = { action: 'copy' };
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}`], {
                            queryParams: this.action
                        });
                    }
                }
            )

    }

    lockShipment() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: this.confirmLockShipment,
            labelConfirm: 'Yes'
        }, () => { this.onLockShipment() });
    }

    onLockShipment() {
        this._documenRepo.LockCsTransaction(this.jobId)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (r: CommonInterface.IResult) => {
                    if (r.status) {
                        this._toastService.success(r.message);
                        // this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                        this.getDetailShipment(this.jobId);
                    } else {
                        this._toastService.error(r.message);
                    }
                },
            );
    }

    showSyncHBL() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Sync HAWB',
            body: this.confirmSyncHBLText,
            labelConfirm: 'Yes'
        }, () => {
            this.onSyncHBL();
        })
    }

    onSyncHBL() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                title: 'Cannot Update Job',
                body: this.invalidFormText
            });
            return;
        }
        const modelAdd = this.onSubmitData();

        const bodySyncData: DocumentationInterface.IDataSyncHBL = {
            flightVesselName: modelAdd.flightVesselName,
            flightDate: modelAdd.flightDate,
            etd: modelAdd.etd,
            eta: modelAdd.eta,
            pol: modelAdd.pol,
            pod: modelAdd.pod,
            agentId: modelAdd.agentId,
            issuedBy: modelAdd.issuedBy,
            warehouseId: modelAdd.warehouseId,
            route: modelAdd.route,
            mblNo: modelAdd.mawb,

            packageQty: modelAdd.packageQty,
            grossWeight: modelAdd.grossWeight,
            hw: modelAdd.hw,
            chargeWeight: modelAdd.chargeWeight,
            cbm: modelAdd.cbm,
            polDescription: modelAdd.polDescription,
            podDescription: modelAdd.podDescription,

        };

        this._documenRepo.syncHBL(this.jobId, bodySyncData)
            .pipe(
                catchError(this.catchError)
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

    handleCancelForm() {
        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formGroup.getRawValue());
        if (isEdited) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: this.confirmCancelFormText,
                labelConfirm: 'Yes'
            }, () => {
                this.confirmCancel();
            })
        } else {
            this.isCancelFormPopupSuccess = true;
            this.gotoList();
        }
    }

    confirmCancel() {
        this.isCancelFormPopupSuccess = true;

        if (this.nextState) {
            this._router.navigate([this.nextState.url.toString()]);
        } else {
            this.gotoList();
        }
    }

    canDeactivate(currenctRoute: ActivatedRouteSnapshot, currentState: RouterStateSnapshot, nextState: RouterStateSnapshot): Observable<boolean> {
        this.nextState = nextState; // * Save nextState for Deactivate service.
        if (this.isCancelFormPopupSuccess || this.isDuplicate) {
            return of(true);
        }
        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formGroup.getRawValue());

        if (isEdited && !this.isCancelFormPopupSuccess && !this.isDuplicate) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: this.confirmCancelFormText,
                labelConfirm: 'Yes'
            }, () => {
                this.confirmCancel();
            })
            return;
        }
        return of(!isEdited);
    }

    showUpdateFlightInfo() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Update Flight Infor',
            body: this.confirmUpdateFlightInfo,
            labelConfirm: 'Yes'
        }, () => {
            this.updateFlightInfor();
        })
    }

    updateFlightInfor() {
        this._documenRepo.updateFlightInfo(this.jobId)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (r: any) => {
                    if (r.status) {
                        this._toastService.success(r.message);
                    } else {
                        this._toastService.error(r.message);
                    }
                },
            );
    }
}
