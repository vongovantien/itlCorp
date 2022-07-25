import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';

import { SeaLCLImportCreateJobComponent } from '../create-job/create-job-lcl-import.component';
import { DocumentationRepo } from '@repositories';
import { ICanComponentDeactivate } from '@core';
import { CsTransaction } from '@models';
import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent, ReportPreviewComponent, SubHeaderComponent } from '@common';

import { combineLatest, of, Observable } from 'rxjs';
import { switchMap, map, tap, skip, takeUntil, catchError, finalize, concatMap } from 'rxjs/operators';

import * as fromShareBussiness from './../../../share-business/store';
import isUUID from 'validator/lib/isUUID';
import { RoutingConstants, SystemConstants } from '@constants';
import { ICrystalReport } from '@interfaces';
import { delayTime } from '@decorators';
import { HttpErrorResponse } from '@angular/common/http';


type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL' | 'FILES';

@Component({
    selector: 'app-sea-lcl-import-detail-job',
    templateUrl: './detail-job-lcl-import.component.html'
})

export class SeaLCLImportDetailJobComponent extends SeaLCLImportCreateJobComponent implements OnInit, ICanComponentDeactivate, ICrystalReport {

    @ViewChild(SubHeaderComponent) headerComponent: SubHeaderComponent;

    params: any;
    tabList: string[] = ['SHIPMENT', 'CDNOTE', 'ASSIGNMENT', 'ADVANCE-SETTLE', 'FILES'];
    jobId: string;

    shipmentDetail: CsTransaction;

    selectedTab: TAB | string = 'SHIPMENT';
    ACTION: CommonType.ACTION_FORM | string = 'UPDATE';

    action: any = {};

    nextState: RouterStateSnapshot;
    isCancelFormPopupSuccess: boolean = false;

    constructor(
        protected _router: Router,
        protected _documenRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        private _activedRoute: ActivatedRoute,
        private _store: Store<any>,
        private _ngProgressService: NgProgress,
        private _cd: ChangeDetectorRef
    ) {
        super(_router, _documenRepo, _toastService);
        this._progressRef = this._ngProgressService.ref();
        this.requestCancel = this.handleCancelForm;
    }

    ngOnInit() { }

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
                    }
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(jobId));

                    this.getDetailSeaFCLImport();
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
                (res: CsTransaction) => {
                    if (!!res) {
                        this.shipmentDetail = res;

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
        [this.formCreateComponent.isSubmitted, this.shipmentGoodSummaryComponent.isSubmitted] = [true, true];

        if (!this.checkValidateForm()) {
            this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                title: 'Cannot Create Job',
                body: this.invalidFormText
            });
            return;
        }

        const modelAdd = this.onSubmitData();

        //  * Update field
        modelAdd.id = this.jobId;
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
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success("Duplicate data successfully");
                        this.jobId = res.data.id;

                        // * get detail & container list.
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                        this.ACTION = 'SHIPMENT';
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    saveJob(body: any) {
        this._documenRepo.updateCSTransaction(body)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * Dispatch action get detail.
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

                    } else {
                        this._toastService.error(res.message);
                    }
                },
                (error: HttpErrorResponse) => {
                    if (error.error?.data?.errorCode) {
                        this.formCreateComponent.formCreate.controls[error.error?.data?.errorCode].setErrors({ existed: true });
                    }
                }
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'hbl':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}/hbl`]);
                break;
            case 'shipment':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                break;
            case 'cdNote':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}`], { queryParams: { tab: 'CDNOTE', view: this.params.view, export: this.params.export } });
                break;
            case 'assignment':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
            case 'files':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}`], { queryParams: { tab: 'FILES' } });
                break;
            case 'advance-settle':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}`], { queryParams: { tab: 'ADVANCE-SETTLE' } });
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
                    })
                    return;
                } else {
                    this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                        body: 'You are not allowed to delete this job'
                    });
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

    previewPLsheet(currency: string) {
        const hblid = "00000000-0000-0000-0000-000000000000";
        this._documenRepo.previewSIFPLsheet(this.jobId, hblid, currency)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        this.renderAndShowReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
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
        this._documenRepo.getPartnerForCheckPointInShipment(this.jobId, 'SLE')
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
                        return this._documenRepo.validateCheckPointMultiplePartner(criteria)
                    }
                    return of({ data: null, message: null, status: true });
                })
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.action = { action: 'copy' };
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}`], {
                            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action)
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
        })
    }

    onLockShipment() {
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
                title: 'Cannot Create Job',
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

    previewShipmentCoverPage() {
        this._documenRepo.previewShipmentCoverPage(this.jobId)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        this.renderAndShowReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    handleCancelForm() {
        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formCreate.getRawValue());
        if (isEdited) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: 'All entered data will be discard. Are you sure you want to leave?',
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
        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formCreate.getRawValue());

        if (isEdited && !this.isCancelFormPopupSuccess) {
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

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    renderAndShowReport() {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.viewContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.viewContainerRef.viewContainerRef.clear();
            });
    }

}
