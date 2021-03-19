import { Component, OnInit, ChangeDetectorRef, ViewChild, ViewContainerRef } from '@angular/core';
import { Store } from '@ngrx/store';
import { Router, ActivatedRoute, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { AirExportCreateJobComponent } from '../create-job/create-job-air-export.component';
import { DocumentationRepo } from '@repositories';
import { ReportPreviewComponent, SubHeaderComponent, ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from '@common';
import { DIM, CsTransaction } from '@models';
import { ICanComponentDeactivate } from '@core';

import { combineLatest, of, Observable, merge } from 'rxjs';
import { tap, map, switchMap, catchError, takeUntil, skip, finalize, concatMap } from 'rxjs/operators';

import * as fromShareBussiness from '../../../share-business/store';
import isUUID from 'validator/lib/isUUID';
import { RoutingConstants } from '@constants';
import { ICrystalReport } from '@interfaces';
import { delayTime } from '@decorators';

type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL' | 'FILES' | 'ADVANCE-SETTLE';

@Component({
    selector: 'app-detail-job-air-export',
    templateUrl: './detail-job-air-export.component.html'
})

export class AirExportDetailJobComponent extends AirExportCreateJobComponent implements OnInit, ICanComponentDeactivate, ICrystalReport {

    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;
    @ViewChild('confirmDeleteJob') confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild("duplicateconfirmTemplate") confirmDuplicatePopup: ConfirmPopupComponent;
    @ViewChild("confirmLockShipment") confirmLockPopup: ConfirmPopupComponent;
    @ViewChild(SubHeaderComponent) headerComponent: SubHeaderComponent;
    @ViewChild('notAllowDelete') canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild('Permission403PopupComponent') permissionPopup: Permission403PopupComponent;
    @ViewChild('confirmCancelPopup') confirmCancelPopup: ConfirmPopupComponent;
    @ViewChild('advSettleContainer', { read: ViewContainerRef }) public advSettleContainerRef: ViewContainerRef;

    jobId: string;
    selectedTab: TAB | string = 'SHIPMENT';
    action: any = {};
    ACTION: CommonType.ACTION_FORM | string = 'UPDATE';

    shipmentDetail: CsTransaction;

    dimensionDetails: DIM[];

    isCancelFormPopupSuccess: boolean = false;

    nextState: RouterStateSnapshot;
    confirmSyncHBLText: string = `
    Do you want to sync
    <span class='font-italic'>ETD, Port, Issue By, Agent, Flight No, Flight Date, Warehouse, Route, MBL, GW, CW, VW, Qty to HAWB ?<span>
    `;
    constructor(
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _toastService: ToastrService,
        protected _documentRepo: DocumentationRepo,
        protected _router: Router,
        protected _cd: ChangeDetectorRef,
        protected _activedRoute: ActivatedRoute,
        private _ngProgressService: NgProgress,
        private readonly _viewContainerRef: ViewContainerRef

    ) {
        super(_toastService, _documentRepo, _router, _store);

        this._progressRef = this._ngProgressService.ref();
    }


    ngOnInit() {
        this.listenShortcutKey();
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
                    this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(jobId));

                    this.getDetailShipment();
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

    getDetailShipment() {
        this._store.select<any>(fromShareBussiness.getTransactionDetailCsTransactionState)
            .pipe(
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.shipmentDetail = res;
                        this.formCreateComponent.isUpdate = true;
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

    previewShipmentCoverPage() {
        this._documentRepo.previewShipmentCoverPage(this.jobId)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        this.showReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
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
                        this.ACTION = 'SHIPMENT';
                        this.isDuplicate = true;

                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    saveJob(body: any) {
        this._progressRef.start();
        this._documentRepo.updateCSTransaction(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * get detail.
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'hbl':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}/hbl`]);
                break;
            case 'shipment':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                break;
            case 'cdNote':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            case 'assignment':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
            case 'files':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`], { queryParams: { tab: 'FILES' } });
                break;
            case 'advance-settle':
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`], { queryParams: { tab: 'ADVANCE-SETTLE' } });
                break;
        }
        if (tabName !== 'advance-settle') {
            this._viewContainerRef.clear();
        }
    }

    previewPLsheet(currency: string) {
        const hblid = "00000000-0000-0000-0000-000000000000";
        this._documentRepo.previewSIFPLsheet(this.jobId, hblid, currency)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        this.showReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    gotoList() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}`]);
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
                    this.permissionPopup.show();
                    return;
                }
                if (value === 200) {
                    this.confirmDeleteJobPopup.show();
                    return;
                } else {
                    this.canNotDeleteJobPopup.show();
                }
            });
    }

    onDeleteJob() {
        this._progressRef.start();
        this._documentRepo.deleteMasterBill(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.confirmDeleteJobPopup.hide();
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

    showDuplicateConfirm() {
        this.confirmDuplicatePopup.show();
    }

    duplicateConfirm() {
        this.action = { action: 'copy' };
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`], {
            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action)
        });
        this.confirmDuplicatePopup.hide();
    }

    lockShipment() {
        this.confirmLockPopup.show();
    }

    onLockShipment() {
        this.confirmLockPopup.hide();

        this._progressRef.start();
        this._documentRepo.LockCsTransaction(this.jobId)
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

    onSyncHBL() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
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
        };

        this._progressRef.start();
        this._documentRepo.syncHBL(this.jobId, bodySyncData)
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

    handleCancelForm() {
        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formGroup.getRawValue());
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

        // * Trường hợp user duplicate thành công đi từ màn hình job detail - job detail khác hoặc nhấn confirm cancel.
        if (this.isCancelFormPopupSuccess || this.isDuplicate) {
            return of(true);
        }
        const isEdited = JSON.stringify(this.formCreateComponent.currentFormValue) !== JSON.stringify(this.formCreateComponent.formGroup.getRawValue());

        // * Trường hợp user confirm cancel
        if (isEdited && !this.isCancelFormPopupSuccess && !this.isDuplicate) {
            this.confirmCancelPopup.show();
            return;
        }
        return of(!isEdited);
    }

    @delayTime(1000)
    showReport(): void {
        this.previewPopup.frm.nativeElement.submit();
        this.previewPopup.show();
    }

    async getAdvanceSettleInfoComponent() {
        const { ShareBusinessAdvanceSettlementInforComponent } = await import('./../../../share-business/components/advance-settlement-info/advance-settlement-info.component');
        this.renderDynamicComponent(ShareBusinessAdvanceSettlementInforComponent, this.advSettleContainerRef)
    }

}

