import { formatDate } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { ActivatedRoute, ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { ActionsSubject, Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { getCurrentUserState, getMenuUserSpecialPermissionState } from './../../../store/reducers/index';

import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { ConfirmPopupComponent, InfoPopupComponent, ReportPreviewComponent, SubHeaderComponent } from '@common';
import { CommonEnum } from '@enums';
import { InjectViewContainerRefDirective } from '@directives';
import { RoutingConstants, JobConstants, SystemConstants } from '@constants';
import { ICanComponentDeactivate } from '@core';
import { AppForm } from '@app';
import { Container, Crystal, CsTransaction, CsTransactionDetail, OpsTransaction } from '@models';
import { ShareBussinessContainerListPopupComponent, ShareBussinessSellingChargeComponent } from '@share-bussiness';
import { OPSTransactionGetDetailSuccessAction } from '../store';

import { ILinkAirSeaInfoModel, JobManagementFormEditComponent } from './components/form-edit/form-edit.component';

import { combineLatest, Observable, of } from 'rxjs';
import { catchError, concatMap, map, switchMap, takeUntil, tap, mergeMap, finalize } from 'rxjs/operators';
import * as fromShareBussiness from './../../share-business/store';


import _groupBy from 'lodash-es/groupBy';
import isUUID from 'validator/es/lib/isUUID';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { ICrystalReport } from '@interfaces';
import { delayTime } from '@decorators';
@Component({
    selector: 'app-ops-module-billing-job-edit',
    templateUrl: './job-edit.component.html',
})
export class OpsModuleBillingJobEditComponent extends AppForm implements OnInit, ICanComponentDeactivate, ICrystalReport {

    @ViewChild(ShareBussinessSellingChargeComponent) sellingChargeComponent: ShareBussinessSellingChargeComponent;
    @ViewChild(ShareBussinessContainerListPopupComponent) containerPopup: ShareBussinessContainerListPopupComponent;

    @ViewChild(JobManagementFormEditComponent) editForm: JobManagementFormEditComponent;
    @ViewChild(SubHeaderComponent) headerComponent: SubHeaderComponent;

    @ViewChild(InjectViewContainerRefDirective) public confirmContainerRef: InjectViewContainerRefDirective;

    opsTransaction: OpsTransaction = null;
    lstMasterContainers: any[];

    tab: string = '';
    tabCharge: string = '';
    jobId: string = '';
    hblid: string = '';

    deleteMessage: string = '';
    isSaveLink: boolean = false;

    nextState: RouterStateSnapshot;
    isCancelFormPopupSuccess: boolean = false;
    selectedTabSurcharge: string = 'BUY';
    allowLinkFeeSell: boolean = true;
    isReplicate: boolean = false;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private _documentRepo: DocumentationRepo,
        private _router: Router,
        private _toastService: ToastrService,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _actionStoreSubject: ActionsSubject,
        protected _cd: ChangeDetectorRef,
        private _exportRepo: ExportRepo,
        private _fileMngtRepo: SystemFileManageRepo
    ) {
        super();
    }

    ngOnInit() {
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this.subscriptionParamURLChange();
        this.subscriptionSaveContainerChange();

        this.currentUser$ = this._store.select(getCurrentUserState);
    }

    subscriptionParamURLChange() {
        this.subscription = combineLatest([
            this.route.params,
            this.route.queryParams,
            this.route.data
        ]).pipe(
            map(([params, qParams, dataParm]) => ({ ...params, ...qParams, ...dataParm })),
            tap((param) => {
                this.jobId = param.id;
                this.tab = !!param.tab ? (param.tab !== 'CDNOTE' ? 'job-edit' : param.tab) : 'job-edit';
                if (param.action) {
                    this.isDuplicate = param.action.toUpperCase() === 'COPY';
                    this.selectedTabSurcharge = 'BUY';
                } else {
                    this.isDuplicate = false;
                }
                this.selectedTabSurcharge = param.tabSurcharge ?? 'BUY';
                this.allowLinkFeeSell = param.allowLinkFee ?? true;
            }),
            switchMap(() => of(this.jobId)),
            takeUntil(this.ngUnsubscribe)
        ).subscribe((jobId: string) => {
            if (isUUID(jobId)) {
                this.tabCharge = 'buying';
                this.getShipmentDetails(jobId);
            } else {
                this.gotoList();
            }
        });
    }

    subscriptionSaveContainerChange() {
        this._actionStoreSubject
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (action: fromShareBussiness.ContainerAction) => {

                    if (action.type === fromShareBussiness.ContainerActionTypes.SAVE_CONTAINER) {
                        this.lstMasterContainers = action.payload;
                        this.updateData(this.lstMasterContainers);
                    }
                }
            );
    }

    updateData(lstMasterContainers: any[]) {
        let dataSum = {
            sumCbm: 0,
            sumPackages: 0,
            sumContainers: 0,
            sumNetWeight: 0,
            sumGrossWeight: 0
        }
        let containerDescription = '';

        lstMasterContainers.forEach(x => {
            dataSum.sumCbm = dataSum.sumCbm + x.cbm;
            dataSum.sumPackages = dataSum.sumPackages + x.packageQuantity;
            dataSum.sumContainers = dataSum.sumContainers + x.quantity;
            dataSum.sumNetWeight = dataSum.sumNetWeight + x.nw;
            dataSum.sumGrossWeight = dataSum.sumGrossWeight + x.gw;
        });
        const contData = [];
        for (const item of Object.keys(_groupBy(lstMasterContainers, 'containerTypeName'))) {
            contData.push({
                cont: item,
                quantity: _groupBy(lstMasterContainers, 'containerTypeName')[item].map(i => i.quantity).reduce((a: any, b: any) => a += b)
            });
        }

        if (!!contData.length && contData.length < 2) {
            containerDescription = `${contData[0].quantity}x${contData[0].cont}`;
        } else {
            for (const item of contData) {
                containerDescription = containerDescription + item.quantity + "x" + item.cont + ";";
            }
        }
        containerDescription = containerDescription.replace(/;$/, "");

        ['sumCbm', 'sumPackages', 'sumNetWeight', 'sumGrossWeight'].forEach(c => {
            if (dataSum[c] !== 0) {
                this.editForm.formEdit.controls[c].setValue(dataSum[c]);
            }
        })
        // this.editForm.formEdit.controls['sumCbm'].setValue(sumCbm === 0 ? null : sumCbm);
        // this.editForm.formEdit.controls['sumPackages'].setValue(sumPackages === 0 ? null : sumPackages);
        // this.editForm.formEdit.controls['sumNetWeight'].setValue(sumNetWeight === 0 ? null : sumNetWeight);
        // this.editForm.formEdit.controls['sumGrossWeight'].setValue(sumGrossWeight === 0 ? null : sumGrossWeight);

        this.editForm.formEdit.controls['containerDescription'].setValue(containerDescription);
        this.editForm.formEdit.controls['sumContainers'].setValue(dataSum.sumContainers === 0 ? null : dataSum.sumContainers);
    }

    getListContainersOfJob() {
        this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblId: this.jobId }));
        this._store.dispatch(new fromShareBussiness.GetContainersHBLAction({ hblid: this.opsTransaction.hblid }));

        this._store.select<any>(fromShareBussiness.getContainerSaveState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (containers: any) => {
                    this.lstMasterContainers = containers || [];
                }
            );
    }

    checkDelete() {
        this._documentRepo.checkShipmentAllowToDelete(this.opsTransaction.id)
            .subscribe(
                (respone: boolean) => {
                    if (respone === true) {
                        this.deleteMessage = `Do you want to delete job No <span class="font-weight-bold">${this.opsTransaction.jobNo}</span>?`;

                        this.showPopupDynamicRender(ConfirmPopupComponent, this.confirmContainerRef.viewContainerRef, {
                            body: this.deleteMessage
                        }, () => { this.onDeleteJob(); })
                    } else {

                        this.showPopupDynamicRender(InfoPopupComponent, this.confirmContainerRef.viewContainerRef, {
                            body: 'You are not allow to delete this job',
                        })
                    }
                }
            );
    }

    onDeleteJob() {
        this._documentRepo.deleteShipment(this.opsTransaction.id)
            .subscribe(
                (response: CommonInterface.IResult) => {
                    if (response.status) {
                        this.router.navigate([`${RoutingConstants.LOGISTICS.JOB_MANAGEMENT}`]);
                    }
                }
            );
    }

    onCancelUpdateJob() {
        this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_MANAGEMENT}`]);
    }

    confirmCancelJob() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.confirmContainerRef.viewContainerRef, {
            body: 'Unsaved data will be lost. Are you sure want to leave?',
            labelConfirm: 'Yes'
        }, () => {
            this.confirmCancel();
        })
    }

    saveShipment() {
        this.lstMasterContainers.forEach((c: Container) => {
            c.mblid = this.jobId;
            c.hblid = this.hblid;
        });
        this.opsTransaction.csMawbcontainers = this.lstMasterContainers;
        this.editForm.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.showPopupDynamicRender(InfoPopupComponent, this.confirmContainerRef.viewContainerRef, {
                body: this.invalidFormText,
                title: 'Cannot Create Job'
            })
            return;
        }

        this.onSubmitData();
        if (this.isDuplicate) {
            this.insertDuplicateJob();
        } else {
            const checkPoint = {
                partnerId: this.opsTransaction.customerId,
                salesmanId: this.opsTransaction.salemanId,
                transactionType: 'CL',
                type: 8,
                hblId: this.opsTransaction.hblid
            };
            this._documentRepo.validateCheckPointContractPartner(checkPoint)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (!res.status) {
                            this._toastService.error(res.message);
                            return;
                        }
                        this.updateShipment();
                    }
                )
        }
    }

    checkValidateForm() {
        [this.editForm.commodityGroupId,
        this.editForm.packageTypeId,
        ].forEach((control: AbstractControl) => this.setError(control));
        let valid: boolean = true;
        if (!this.editForm.formEdit.valid
            || (!!this.editForm.serviceDate.value && !this.editForm.serviceDate.value.startDate)
            || (!!this.editForm.finishDate.value.startDate && this.editForm.serviceDate.value.startDate > this.editForm.finishDate.value.startDate)
            || (this.editForm.hwbno.value === null || this.editForm.hwbno.value?.length === 0)
            || (this.editForm.mblno.value === null || this.editForm.mblno.value?.length === 0)
        ) {
            valid = false;
        }
        return valid;
    }

    onSubmitData() {
        const form: any = this.editForm.formEdit.getRawValue();
        this.opsTransaction.serviceDate = !!form.serviceDate && !!form.serviceDate.startDate ? formatDate(form.serviceDate.startDate, 'yyyy-MM-dd', 'en') : null;
        this.opsTransaction.finishDate = !!form.finishDate && !!form.finishDate.startDate ? formatDate(form.finishDate.startDate, 'yyyy-MM-dd', 'en') : null;

        this.opsTransaction.hwbno = form.hwbno;
        this.opsTransaction.mblno = form.mblno;
        this.opsTransaction.customerId = form.customerId;
        this.opsTransaction.pol = form.pol;
        this.opsTransaction.pod = form.pod;
        this.opsTransaction.supplierId = form.supplierId;
        this.opsTransaction.flightVessel = form.flightVessel;
        this.opsTransaction.agentId = form.agentId;
        this.opsTransaction.warehouseId = form.warehouseId;
        this.opsTransaction.invoiceNo = form.invoiceNo;
        this.opsTransaction.purchaseOrderNo = form.purchaseOrderNo;
        this.opsTransaction.salemanId = form.salemansId;
        this.opsTransaction.fieldOpsId = form.fieldOpsId;
        this.opsTransaction.billingOpsId = form.billingOpsId;
        this.opsTransaction.clearanceLocation = form.clearanceLocation;
        this.opsTransaction.shipper = form.shipper;
        this.opsTransaction.consignee = form.consignee;
        this.opsTransaction.sumGrossWeight = form.sumGrossWeight === 0 ? null : form.sumGrossWeight;
        this.opsTransaction.sumNetWeight = form.sumNetWeight === 0 ? null : form.sumNetWeight;
        this.opsTransaction.sumPackages = form.sumPackages === 0 ? null : form.sumPackages;
        this.opsTransaction.sumContainers = form.sumContainers === 0 ? null : form.sumContainers;
        this.opsTransaction.sumChargeWeight = (this.lstMasterContainers || []).reduce((acc, curr) => acc += curr.chargeAbleWeight, 0);
        this.opsTransaction.sumCbm = form.sumCbm === 0 ? null : form.sumCbm;
        this.opsTransaction.containerDescription = form.containerDescription;
        this.opsTransaction.note = form.note;

        this.opsTransaction.shipmentMode = form.shipmentMode;
        this.opsTransaction.productService = form.productService;
        this.opsTransaction.serviceMode = form.serviceMode;
        this.opsTransaction.packageTypeId = form.packageTypeId;
        this.opsTransaction.commodityGroupId = form.commodityGroupId;
        this.opsTransaction.shipmentType = form.shipmentType;
        this.opsTransaction.noProfit = form.noProfit;
        this.opsTransaction.eta = !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null;
        this.opsTransaction.deliveryDate = !!form.deliveryDate && !!form.deliveryDate.startDate ? formatDate(form.deliveryDate.startDate, 'yyyy-MM-dd', 'en') : null;
        this.opsTransaction.clearanceDate = !!form.clearanceDate && !!form.clearanceDate.startDate ? formatDate(form.clearanceDate.startDate, 'yyyy-MM-dd', 'en') : null;
        this.opsTransaction.suspendTime = form.suspendTime;
        this.opsTransaction.serviceNo = this.editForm.shipmentInfo;

        if ((!!this.editForm.shipmentNo || !!this.opsTransaction.serviceNo) && form.shipmentMode === 'Internal'
            && (form.productService.indexOf('Sea') > -1 || form.productService === 'Air')) {
            this.isSaveLink = true;
        } else {
            // this.opsTransaction.serviceNo = null;
            // this.opsTransaction.serviceHblId = null;
        }

    }

    updateShipment() {
        if (this.isSaveLink) {
            this._documentRepo.getASTransactionInfo(this.opsTransaction.jobNo, this.opsTransaction.mblno, this.opsTransaction.hwbno, this.opsTransaction.productService, this.opsTransaction.serviceMode)
                .pipe(
                    catchError(this.catchError),
                    concatMap((res: ILinkAirSeaInfoModel) => {
                        if (!!res) {
                            this.opsTransaction.serviceNo = res.jobNo;
                            this.opsTransaction.serviceHblId = res.hblId;
                            this.opsTransaction.isLinkJob = true;
                        } else {
                            this.opsTransaction.serviceNo = null;
                            this.opsTransaction.serviceHblId = null;
                            this.opsTransaction.isLinkJob = false;
                        }
                        this.isSaveLink = false;
                        return this._documentRepo.updateShipment(this.opsTransaction);
                    })
                ).subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.getShipmentDetails(this.opsTransaction.id);
                        } else {
                            this._toastService.warning(res.message);
                        }
                    },
                    (error: HttpErrorResponse) => {
                        if (error.error?.data?.errorCode) {
                            this.editForm.formEdit.controls[error.error?.data?.errorCode].setErrors({ existed: true });
                        }
                    }
                );
        } else {
            this._documentRepo.updateShipment(this.opsTransaction)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.getShipmentDetails(this.opsTransaction.id);
                        } else {
                            this._toastService.warning(res.message);
                        }
                    },
                    (error: HttpErrorResponse) => {
                        if (error.error?.data?.errorCode) {
                            this.editForm.formEdit.controls[error.error?.data?.errorCode].setErrors({ existed: true });
                        }
                    }
                );
        }
    }

    insertDuplicateJob() {
        this.opsTransaction.isReplicate = this.isReplicate;
        // this.opsTransaction.isReplicate = !!this.opsTransaction.replicatedId;
        if (this.isSaveLink) {
            this._documentRepo.getASTransactionInfo(this.opsTransaction.jobNo, this.opsTransaction.mblno, this.opsTransaction.hwbno, this.opsTransaction.productService, this.opsTransaction.serviceMode)
                .pipe(catchError(this.catchError))
                .subscribe((res: ILinkAirSeaInfoModel) => {
                    if (!!res?.jobNo) {
                        this.opsTransaction.serviceNo = res.jobNo;
                        this.opsTransaction.serviceHblId = res.hblId;
                        this.opsTransaction.isLinkJob = true;
                        this.insertDuplicateShipment();
                    }
                });
        } else {
            this.insertDuplicateShipment();
        }
    }

    insertDuplicateShipment() {
        this._documentRepo.insertDuplicateShipment(this.opsTransaction)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        this.jobId = res.data.id;
                        this.opsTransaction.hblid = res.data.hblid;
                        // this.isDuplicate = false;
                        this.headerComponent.resetBreadcrumb("Detail Job");
                        this.editForm.isSubmitted = false;

                        this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_DETAIL}/`, this.jobId]);
                    } else {
                        this._toastService.warning(res.message);
                    }
                }
            );
    }

    lockShipment() {
        this.showPopupDynamicRender(ConfirmPopupComponent,
            this.confirmContainerRef.viewContainerRef,
            {
                body: 'Do you want to lock this shipment ?',
                labelConfirm: 'Yes'
            },
            () => {
                this.opsTransaction.isLocked = true;
                this.updateShipment();
            })
    }

    showListContainer() {
        this.containerPopup.mblid = this.jobId;
        this.containerPopup.show();
    }

    getShipmentDetails(id: any) {
        this._documentRepo.getDetailShipment(id)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (response: any) => {
                    if (response != null) {
                        this.opsTransaction = new OpsTransaction(response);
                        if (this.opsTransaction.linkSource == "Replicate") {
                            this.allowLinkFeeSell = false;
                        }

                        this.hblid = this.opsTransaction.hblid;

                        this.getListContainersOfJob();
                        if (this.selectedTabSurcharge == CommonEnum.SurchargeTypeEnum.SELLING_RATE)
                            this.getSurCharges(CommonEnum.SurchargeTypeEnum.SELLING_RATE);
                        else if (this.selectedTabSurcharge == CommonEnum.SurchargeTypeEnum.BUYING_RATE)
                            this.getSurCharges(CommonEnum.SurchargeTypeEnum.BUYING_RATE);

                        this.editForm.opsTransaction = this.opsTransaction;
                        const hbl = new CsTransactionDetail(this.opsTransaction);
                        hbl.id = this.opsTransaction.hblid;
                        hbl.saleManId = this.opsTransaction.salemanId;

                        this._store.dispatch(new fromShareBussiness.GetDetailHBLSuccessAction(hbl));

                        const csTransation: CsTransaction = new CsTransaction(Object.assign({}, response, {
                            grossWeight: this.opsTransaction.sumGrossWeight,
                            netWeight: this.opsTransaction.sumNetWeight,
                            cbm: this.opsTransaction.sumCbm,
                            chargeWeight: this.opsTransaction.sumChargeWeight,
                            packageQty: this.opsTransaction.sumPackages,
                            isLocked: this.opsTransaction.isLocked,
                            customerId: this.opsTransaction.customerId,
                            customerName: this.opsTransaction.customerName,
                            agentName: this.opsTransaction.agentName,
                            supplierName: this.opsTransaction.supplierName,
                            coloaderId: this.opsTransaction.supplierId,
                            mawb: this.opsTransaction.mblno,
                            noProfit: this.opsTransaction.noProfit,
                            eta: this.opsTransaction.eta,
                            deliveryDate: this.opsTransaction.deliveryDate,
                            suspendTime: this.opsTransaction.suspendTime,
                            clearanceDate: this.opsTransaction.clearanceDate,
                            serviceMode: this.opsTransaction.serviceMode,
                            productService: this.opsTransaction.productService
                        }));

                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailSuccessAction(csTransation));

                        // Tricking Update Transation Apply for isLocked..

                        this._store.dispatch(new OPSTransactionGetDetailSuccessAction(this.opsTransaction));
                        this._store.dispatch(new fromShareBussiness.GetProfitHBLAction(this.opsTransaction.hblid));

                        // this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: this.jobId }));
                        // this._store.dispatch(new fromShareBussiness.GetContainersHBLAction({ hblid: this.opsTransaction.hblid }));

                        this.editForm.isJobCopy = this.isDuplicate;
                        this.editForm.setFormValue();
                    }

                    // if (this.isDuplicate) {
                    //     this.editForm.getBillingOpsId();
                    //     this.headerComponent.resetBreadcrumb("Create Job");
                    // }
                },
            );
    }

    getSurCharges(type: 'BUY' | 'SELL' | 'OBH') {
        this.selectedTabSurcharge = type;
        if (type === CommonEnum.SurchargeTypeEnum.BUYING_RATE && this.opsTransaction != null) {
            this._store.dispatch(new fromShareBussiness.GetBuyingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.BUYING_RATE, hblId: this.opsTransaction.hblid }));
        }
        if (type === CommonEnum.SurchargeTypeEnum.SELLING_RATE && this.opsTransaction != null) {
            this._store.dispatch(new fromShareBussiness.GetSellingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.SELLING_RATE, hblId: this.opsTransaction.hblid }));
        }
        if (type === CommonEnum.SurchargeTypeEnum.OBH && this.opsTransaction != null) {
            this._store.dispatch(new fromShareBussiness.GetOBHSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.OBH, hblId: this.opsTransaction.hblid }));
        }
    }

    selectTab(tabName: string) {
        this.tab = tabName;
        if (tabName === 'job-edit') {
            this.getShipmentDetails(this.jobId);
            this.getSurCharges(CommonEnum.SurchargeTypeEnum.BUYING_RATE);
        }
    }

    onOpenPLPrint(currency: string) {
        this._documentRepo.previewPL(this.jobId, currency)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );

    }

    selectTabCharge(tabName: string) {
        this.tabCharge = tabName;
        switch (tabName) {
            case 'buying':
                this.getSurCharges(CommonEnum.SurchargeTypeEnum.BUYING_RATE);
                break;
            case 'selling':
                this.getSurCharges(CommonEnum.SurchargeTypeEnum.SELLING_RATE);
                break;
            case 'obh':
                this.getSurCharges(CommonEnum.SurchargeTypeEnum.OBH);
                break;
            default:
                break;
        }
    }

    gotoList() {
        this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_MANAGEMENT}`]);
    }

    handleCancelForm() {
        if (this.tab !== 'job-edit') {
            this.gotoList();
        }
        const isEdited = JSON.stringify(this.editForm.currentFormValue) !== JSON.stringify(this.editForm.formEdit.getRawValue());
        if (isEdited) {
            this.confirmCancelJob();
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

        if (this.tab !== 'job-edit') {
            return of(true);
        }
        const isEdited = JSON.stringify(this.editForm.currentFormValue) !== JSON.stringify(this.editForm.formEdit.getRawValue());
        if (this.isCancelFormPopupSuccess || this.isDuplicate) {
            return of(true);
        }
        if (isEdited && !this.isCancelFormPopupSuccess) {
            this.confirmCancelJob();
            return;
        }
        return of(!isEdited);
    }

    confirmDuplicate(isReplicate: boolean = false) {
        if (isReplicate && !this.opsTransaction.replicatedId) {
            this._toastService.warning('This Job dont have Job Replicate')
            return;
        }
        this.showPopupDynamicRender(ConfirmPopupComponent, this.confirmContainerRef.viewContainerRef, {
            body: 'The system will open the Job Create Screen. Do you want to leave ?',
            title: 'Duplicate OPS detail',
            labelConfirm: 'Yes'
        },
            () => {
                this.isReplicate = isReplicate;
                this.onSubmitDuplicateConfirm();
            })
    }

    onSubmitDuplicateConfirm() {
        this._documentRepo.getPartnerForCheckPointInShipment(this.opsTransaction.hblid, 'CL')
            .pipe(
                takeUntil(this.ngUnsubscribe),
                switchMap((partnerIds: string[]) => {
                    if (!!partnerIds.length) {
                        const criteria: DocumentationInterface.ICheckPointCriteria = {
                            data: partnerIds,
                            transactionType: 'CL',
                            settlementCode: null,
                        };
                        return this._documentRepo.validateCheckPointMultiplePartner(criteria)
                    }
                    return of({ data: null, message: null, status: true });
                })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.editForm.isSubmitted = false;
                        this.tab = 'job-edit'
                        this.isDuplicate = true;
                        this.editForm.isJobCopy = this.isDuplicate;
                        this.editForm.opsTransaction.serviceNo = null;
                        this.editForm.setFormValue();
                        if (this.isDuplicate) {
                            this.editForm.getBillingOpsId();
                            this.headerComponent.resetBreadcrumb("Create Job");
                            if (this.selectedTabSurcharge == CommonEnum.SurchargeTypeEnum.SELLING_RATE)
                                this.getSurCharges(CommonEnum.SurchargeTypeEnum.SELLING_RATE);
                        }
                    }
                }
            )

    }

    onFinishJob() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Do you want to finish this shipment ?',
            labelConfirm: 'Yes'
        }, () => {
            this.handleChangeStatusJob(JobConstants.FINISH);
        })
    }

    onReopenJob() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Do you want to reopen this shipment ?',
            labelConfirm: 'Yes'
        }, () => {
            this.handleChangeStatusJob(JobConstants.REOPEN);
        })

    }

    handleChangeStatusJob(status: string) {
        let body: any = {
            jobId: this.jobId,
            transactionType: JobConstants.OPSTRANSACTION,
            status
        }
        this._documentRepo.updateStatusJob(body).pipe(
            catchError(this.catchError),
            finalize(() => {
                this._progressRef.complete();
            })
        ).subscribe(
            (r: CommonInterface.IResult) => {
                if (r.status) {
                    this.getShipmentDetails(this.jobId);
                    this._toastService.success(r.message);
                } else {
                    this._toastService.error(r.message);
                }
            },
        );
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    renderAndShowReport() {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.confirmContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.confirmContainerRef.viewContainerRef.clear();
            });
        let sub = ((this.componentRef.instance) as ReportPreviewComponent).onConfirmEdoc
            .pipe(
                concatMap(() => this._exportRepo.exportCrystalReportPDF(this.dataReport, 'response', 'text')),
                mergeMap((res: any) => {
                    if ((res as HttpResponse<any>).status == SystemConstants.HTTP_CODE.OK) {
                        const body = {
                            url: (this.dataReport as Crystal).pathReportGenerate || null,
                            module: 'Document',
                            folder: 'Shipment',
                            objectId: this.opsTransaction.id,
                            hblId: this.opsTransaction.hblid,
                            templateCode: 'PLSheet',
                            transactionType: 'CL'
                        };
                        return this._fileMngtRepo.uploadPreviewTemplateEdoc([body]);
                    }
                    return of(false);
                }),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (!res) return;
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.success(res.message || "Upload fail");
                    }
                },
                (errors) => {
                    console.log("error", errors);
                },
                () => {
                    sub.unsubscribe();
                }
            );
    }
}
