import { Component, ViewChild, ChangeDetectorRef, OnInit } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { Router, ActivatedRoute, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { SeaFCLImportCreateJobComponent } from '../create-job/create-job-fcl-import.component';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from 'src/app/shared/repositories';
import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from 'src/app/shared/common/popup';
import { ReportPreviewComponent } from 'src/app/shared/common';

import { combineLatest, of, Observable } from 'rxjs';
import { map, tap, switchMap, skip, catchError, takeUntil, concatMap } from 'rxjs/operators';

import * as fromShareBussiness from './../../../share-business/store';

type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL' | 'FILES';

import isUUID from 'validator/lib/isUUID';
import { CsTransaction } from '@models';
import { ICanComponentDeactivate } from '@core';
import { RoutingConstants } from '@constants';
import { ICrystalReport } from '@interfaces';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'app-detail-job-fcl-import',
    templateUrl: './detail-job-fcl-import.component.html',
})
export class SeaFCLImportDetailJobComponent extends SeaFCLImportCreateJobComponent implements OnInit, ICanComponentDeactivate, ICrystalReport {

    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;
    tabList: string[] = ['SHIPMENT', 'CDNOTE', 'ASSIGNMENT', 'ADVANCE-SETTLE', 'FILES'];
    selectedTab: TAB | string = 'SHIPMENT';

    shipmentDetail: CsTransaction;
    action: any = {};
    nextState: RouterStateSnapshot;

    isCancelFormPopupSuccess: boolean = false;

    constructor(
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _documentRepo: DocumentationRepo,
        protected _activedRoute: ActivatedRoute,
        protected _toastService: ToastrService,
        protected cdr: ChangeDetectorRef,
        protected _exportRepo: ExportRepo,
        protected _fileMngt: SystemFileManageRepo
    ) {
        super(_router, _documentRepo, _actionStoreSubject, _store, _toastService, cdr, _exportRepo, _fileMngt);
        this.requestCancel = this.handleCancelForm;
    }


    ngAfterViewInit() {
        this.subscription = this.subscription = combineLatest([
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

                this.cdr.detectChanges();
            }),
            switchMap(() => of(this.jobId))
        ).subscribe(
            (jobId: string) => {
                if (isUUID(jobId)) {
                    this.jobId = jobId;
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(jobId));
                    this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: jobId }));
                    if (this.selectedTab === this.tabList[0]) {
                        this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));
                    }
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
                    this.shipmentDetail = res; // TODO Model.
                    // * reset field duplicate
                    if (this.ACTION === "COPY") {
                        this.formCreateComponent.getUserLogged();
                        this.headerComponent.resetBreadcrumb("Create Job");
                    } else {
                        this.headerComponent.resetBreadcrumb("Job Detail");
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
            this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                title: 'Cannot Update Job',
                body: this.invalidFormText
            });
            return;
        }
        const modelUpdate = this.onSubmitData();

        //  * Update field

        modelUpdate.csMawbcontainers = this.containers;

        modelUpdate.csMawbcontainers.forEach(c => {
            c.mblid = this.jobId;
        });

        modelUpdate.id = this.jobId;
        modelUpdate.branchId = this.shipmentDetail.branchId;
        modelUpdate.transactionType = this.shipmentDetail.transactionType;
        modelUpdate.jobNo = this.shipmentDetail.jobNo;
        modelUpdate.datetimeCreated = this.shipmentDetail.datetimeCreated;
        modelUpdate.userCreated = this.shipmentDetail.userCreated;
        modelUpdate.currentStatus = this.shipmentDetail.currentStatus;
        modelUpdate.isLocked = this.shipmentDetail.isLocked;

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
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                        this.ACTION = 'SHIPMENT';

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
                        if (res.data.errorCode = 452) {
                            this.showHBLsInvalid(res.message);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                },
                (error: HttpErrorResponse) => {
                    if (error.error?.data?.errorCode) {
                        this.formCreateComponent.formCreate.controls[error.error?.data?.errorCode].setErrors({ existed: true });
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
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}/hbl`]);
                break;
            case 'shipment':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                break;
            case 'cdNote':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}`], { queryParams: { tab: 'CDNOTE', view: this.params.view, export: this.params.export } });
                break;
            case 'assignment':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
            case 'files':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}`], { queryParams: { tab: 'FILES' } });
                break;
            case 'advance-settle':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}`], { queryParams: { tab: 'ADVANCE-SETTLE' } });
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
                }),
                takeUntil(this.ngUnsubscribe)
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

    lockShipment() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Do you want to lock this shipment ?',
            labelConfirm: 'Yes'
        }, () => {
            this.onLockShipment();
        });
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
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
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
            etd: modelAdd.etd,
            eta: modelAdd.eta,
            pol: modelAdd.pol,
            pod: modelAdd.pod,
            bookingNo: modelAdd.bookingNo,
            voyNo: modelAdd.voyNo,
            mblNo: modelAdd.mawb,
            polDescription: modelAdd.polDescription,
            podDescription: modelAdd.podDescription
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

    showDuplicateConfirm() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Duplicate job detail',
            body: 'The system will open the Job Create Screen. Are you sure you want to leave?',
            labelConfirm: 'Yes'
        }, () => {
            this.duplicateConfirm();
        })
    }

    duplicateConfirm() {
        this._documenRepo.getPartnerForCheckPointInShipment(this.jobId, 'SIF')
            .pipe(
                takeUntil(this.ngUnsubscribe),
                switchMap((partnerIds: string[]) => {
                    if (!!partnerIds.length) {
                        const criteria: DocumentationInterface.ICheckPointCriteria = {
                            data: partnerIds,
                            transactionType: 'DOC',
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
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}`], {
                            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action)
                        });
                    }
                }
            )
    }

    handleCancelForm() {
        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formCreate.getRawValue());
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

        if (this.isCancelFormPopupSuccess || this.isDuplicate) {
            return of(true);
        }

        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formCreate.getRawValue());

        if (isEdited && !this.isCancelFormPopupSuccess) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: 'All entered data will be discard. Are you sure you want to leave?',
                labelConfirm: 'Yes'
            }, () => {
                this.confirmCancel();
            });
            return;
        }
        return of(!isEdited);
    }
}
