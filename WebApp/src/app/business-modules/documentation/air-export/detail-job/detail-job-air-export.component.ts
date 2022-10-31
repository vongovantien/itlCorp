import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from '@common';
import { ICanComponentDeactivate } from '@core';
import { CsTransaction, DIM } from '@models';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { AirExportCreateJobComponent } from '../create-job/create-job-air-export.component';

import { combineLatest, merge, Observable, of } from 'rxjs';
import { catchError, concatMap, map, switchMap, takeUntil, tap } from 'rxjs/operators';

import { HttpErrorResponse } from '@angular/common/http';
import { RoutingConstants } from '@constants';
import { ICrystalReport } from '@interfaces';
import isUUID from 'validator/lib/isUUID';
import * as fromShareBussiness from '../../../share-business/store';

type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL' | 'FILES' | 'ADVANCE-SETTLE';

@Component({
    selector: 'app-detail-job-air-export',
    templateUrl: './detail-job-air-export.component.html'
})

export class AirExportDetailJobComponent extends AirExportCreateJobComponent implements OnInit, ICanComponentDeactivate, ICrystalReport {

    params: any;
    tabList: string[] = ['SHIPMENT', 'CDNOTE', 'ASSIGNMENT', 'FILES', 'ADVANCE-SETTLE'];
    selectedTab: TAB | string = 'SHIPMENT';
    action: any = {};

    dimensionDetails: DIM[];

    isCancelFormPopupSuccess: boolean = false;

    errHasHBL: boolean = false;

    nextState: RouterStateSnapshot;
    confirmSyncHBLText: string = `
    Do you want to sync
    <span class='font-italic'>ETD, Port, Issue By, Agent, Flight No, Flight Date, Warehouse, Route, MBL, GW, CW, VW, Qty to HAWB ?<span>
    `;

    confirmUpdateFlightInfo: string = 'Do you want to sync Flight No, Flight Date, ETD, ETA to HAWB ?';
    constructor(
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _toastService: ToastrService,
        protected _documentRepo: DocumentationRepo,
        protected _router: Router,
        protected _cd: ChangeDetectorRef,
        protected _activedRoute: ActivatedRoute,
        protected _exportRepo: ExportRepo,
        protected _fileMngtRepo: SystemFileManageRepo

    ) {
        super(_toastService, _documentRepo, _router, _store, _exportRepo, _fileMngtRepo);
        this.requestCancel = this.handleCancelForm;
    }

    ngOnInit() {
        this.listenShortcutKey();
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
            switchMap((params) => of(params.jobId)),
        ).subscribe(
            (jobId: string) => {
                if (isUUID(jobId)) {
                    if (this.selectedTab === this.tabList[0]) {
                        this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));
                    }
                    this.getDetailShipment(this.jobId);
                } else {
                    this.gotoList();
                }
            }
        );
    }

    listenShortcutKey() {
        merge(this.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit1'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('shipment'); });
        merge(this.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit2'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('hbl'); });
        merge(this.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit3'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('cdNote'); });
        merge(this.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit4'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('assignment'); });
        merge(this.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit5'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('files'); });
        merge(this.createShortcut(['ControlLeft', 'KeyM'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}` + '/manifest']); });
        merge(this.createShortcut(['ControlLeft', 'KeyB'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}` + '/mawb']); });
    }

    getDetailShipment(jobId: string) {
        this._documentRepo.getDetailTransaction(jobId)
            .subscribe(
                (res: CsTransaction) => {
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailSuccessAction(res));
                    this.shipmentDetail = res;
                    // * reset field duplicate
                    if (this.ACTION === "COPY") {
                        this.formCreateComponent.getUserLogged();
                        this.headerComponent.resetBreadcrumb("Create Job");
                    } else {
                        this.headerComponent.resetBreadcrumb("Job Detail");
                        //this.attachV2.isJobLock = res.isLocked;
                    }
                }
            )
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
            item.airWayBillId = null;
            item.hblid = null;
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
        this._documentRepo.importCSTransaction(body)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success("Duplicate data successfully");
                        this.jobId = res.data.id;

                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                        this.ACTION = "SHIPMENT";
                        this.isDuplicate = true;

                    } else {
                        //this._toastService.error(res.message);

                        if (res.data.errorCode = 453) {
                            this.showHBLsInvalid(res.message);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                }
            );
    }

    saveJob(body: any) {
        this._documentRepo.updateCSTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    console.log(res);

                    if (res.status) {
                        this._toastService.success(res.message);

                        // * get detail.
                        this.getDetailShipment(this.jobId);
                        // this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                    } else {
                        if (res.data.errorCode = 452) {
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

    prepareDeleteJob() {
        this._documentRepo.checkPermissionAllowDeleteShipment(this.jobId)
            .pipe(
                concatMap((isAllowDelete: boolean) => {
                    if (isAllowDelete) {
                        return this._documentRepo.checkMasterBillAllowToDelete(this.jobId);
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
                        body: 'You you sure you want to delete this Job?',
                        title: 'Alert',
                        labelConfirm: 'Yes',
                    }, () => {
                        this.onDeleteJob();
                    });
                    return;
                } else {
                    this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                        body: 'You are not allowed to delete this job?',
                    });
                }
            });
    }

    onDeleteJob() {
        this._documentRepo.deleteMasterBill(this.jobId)
            .pipe(
                catchError(this.catchError),
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
        }, () => {
            this.duplicateConfirm();
        });
    }

    duplicateConfirm() {
        this._documentRepo.getPartnerForCheckPointInShipment(this.jobId, 'AE')
            .pipe(
                takeUntil(this.ngUnsubscribe),
                switchMap((partnerIds: string[]) => {
                    if (!!partnerIds.length) {
                        const criteria: DocumentationInterface.ICheckPointCriteria = {
                            data: partnerIds,
                            transactionType: 'DOC',
                            type: 5,
                            settlementCode: null,
                        };
                        return this._documentRepo.validateCheckPointMultiplePartner(criteria)
                    }
                    return of({ data: null, message: null, status: true });
                })
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.action = { action: 'copy' };
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`], {
                            queryParams: this.action
                        });
                    }
                }
            )
    }

    lockShipment() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Do you want to lock this shipment ?',
            labelConfirm: 'Yes'
        }, () => {
            this.onLockShipment();
        });
    }

    onLockShipment() {
        this._documentRepo.LockCsTransaction(this.jobId)
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

    showUpdateFlightInfo() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Update Flight Infor',
            body: this.confirmUpdateFlightInfo,
            labelConfirm: 'Yes'
        }, () => {
            this.updateFlightInfor();
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

            // * CR 14501
            packageQty: modelAdd.packageQty,
            grossWeight: modelAdd.grossWeight,
            hw: modelAdd.hw,
            chargeWeight: modelAdd.chargeWeight,
            cbm: modelAdd.cbm,
            polDescription: modelAdd.polDescription,
            podDescription: modelAdd.podDescription,
        };

        this._documentRepo.syncHBL(this.jobId, bodySyncData)
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
                body: 'All entered data will be discard. Are you sure you want to leave?',
                labelConfirm: 'Yes'
            }, () => {
                this.confirmCancel();
            });
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

        // * Trường hợp user duplicate thành công đi từ màn hình job detail - job detail khác hoặc nhấn confirm cancel.
        if (this.isCancelFormPopupSuccess || this.isDuplicate) {
            return of(true);
        }
        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formGroup.getRawValue());

        // * Trường hợp user confirm cancel
        if (isEdited && !this.isCancelFormPopupSuccess && !this.isDuplicate) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: 'All entered data will be discard. Are you sure you want to leave?',
                labelConfirm: 'Yes'
            }, () => {
                this.confirmCancel();
            })
            return;
        }
        return of(!isEdited);
    }

    updateFlightInfor() {
        this._documentRepo.updateFlightInfo(this.jobId)
            .subscribe(
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

