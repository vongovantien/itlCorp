import { ChangeDetectorRef, Component, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { ActivatedRoute, Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { AbstractControl } from '@angular/forms';
import { NgProgress } from '@ngx-progressbar/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

import { DocumentationRepo } from '@repositories';
import { ShareBussinessSellingChargeComponent, ShareBussinessContainerListPopupComponent } from '@share-bussiness';
import { ConfirmPopupComponent, InfoPopupComponent, SubHeaderComponent } from '@common';
import { OpsTransaction, CsTransactionDetail, CsTransaction, Container } from '@models';
import { CommonEnum } from '@enums';
import { OPSTransactionGetDetailSuccessAction } from '../store';
import { InjectViewContainerRefDirective } from '@directives';
import { RoutingConstants } from '@constants';
import { ICanComponentDeactivate } from '@core';
import { AppForm } from '@app';

import { JobManagementFormEditComponent } from './components/form-edit/form-edit.component';
import { PlSheetPopupComponent } from './pl-sheet-popup/pl-sheet.popup';

import { catchError, finalize, map, takeUntil, tap, switchMap } from 'rxjs/operators';
import { combineLatest, Observable, of } from 'rxjs';
import * as fromShareBussiness from './../../share-business/store';


import _groupBy from 'lodash/groupBy';
import isUUID from 'validator/lib/isUUID';
@Component({
    selector: 'app-ops-module-billing-job-edit',
    templateUrl: './job-edit.component.html',
})
export class OpsModuleBillingJobEditComponent extends AppForm implements OnInit, ICanComponentDeactivate {

    @ViewChild(PlSheetPopupComponent) plSheetPopup: PlSheetPopupComponent;
    @ViewChild(ShareBussinessSellingChargeComponent) sellingChargeComponent: ShareBussinessSellingChargeComponent;
    @ViewChild(ShareBussinessContainerListPopupComponent) containerPopup: ShareBussinessContainerListPopupComponent;

    @ViewChild(JobManagementFormEditComponent) editForm: JobManagementFormEditComponent;
    @ViewChild(SubHeaderComponent) headerComponent: SubHeaderComponent;

    @ViewChild(InjectViewContainerRefDirective) public confirmContainerRef: InjectViewContainerRefDirective;
    @ViewChild('advSettleContainer', { read: ViewContainerRef }) public advSettleContainerRef: ViewContainerRef;

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

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private _ngProgressService: NgProgress,
        private _documentRepo: DocumentationRepo,
        private _router: Router,
        private _toastService: ToastrService,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _actionStoreSubject: ActionsSubject,
        protected _cd: ChangeDetectorRef,
        private readonly _viewContainerRef: ViewContainerRef,
    ) {
        super();

        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.subscriptionParamURLChange();

        this.subscriptionSaveContainerChange();

    }

    subscriptionParamURLChange() {
        combineLatest([
            this.route.params,
            this.route.queryParams
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams })),
            tap((param) => {
                this.jobId = param.id;
                this.tab = !!param.tab ? param.tab : 'job-edit';
                if (param.action) {
                    this.isDuplicate = param.action.toUpperCase() === 'COPY';
                    this.selectedTabSurcharge = 'BUY';
                } else {
                    this.isDuplicate = false;
                }
            }),
            switchMap(() => of(this.jobId))
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
        let sumCbm = 0;
        let sumPackages = 0;
        let sumContainers = 0;
        let sumNetWeight = 0;
        let sumGrossWeight = 0;
        let containerDescription = '';

        lstMasterContainers.forEach(x => {
            sumCbm = sumCbm + x.cbm;
            sumPackages = sumPackages + x.packageQuantity;
            sumContainers = sumContainers + x.quantity;
            sumNetWeight = sumNetWeight + x.nw;
            sumGrossWeight = sumGrossWeight + x.gw;
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

        this.editForm.formEdit.controls['sumCbm'].setValue(sumCbm === 0 ? null : sumCbm);
        this.editForm.formEdit.controls['sumPackages'].setValue(sumPackages === 0 ? null : sumPackages);
        this.editForm.formEdit.controls['sumContainers'].setValue(sumContainers === 0 ? null : sumContainers);
        this.editForm.formEdit.controls['sumNetWeight'].setValue(sumNetWeight === 0 ? null : sumNetWeight);
        this.editForm.formEdit.controls['sumGrossWeight'].setValue(sumGrossWeight === 0 ? null : sumGrossWeight);
        this.editForm.formEdit.controls['containerDescription'].setValue(containerDescription);
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
            this.updateShipment();
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
        this.opsTransaction.sumCbm = form.sumCbm === 0 ? null : form.sumCbm;
        this.opsTransaction.containerDescription = form.containerDescription;
        this.opsTransaction.note = form.note;

        this.opsTransaction.shipmentMode = form.shipmentMode;
        this.opsTransaction.productService = form.productService;
        this.opsTransaction.serviceMode = form.serviceMode;
        this.opsTransaction.packageTypeId = form.packageTypeId;
        this.opsTransaction.commodityGroupId = form.commodityGroupId;
        this.opsTransaction.shipmentType = form.shipmentType;

        if (this.editForm.shipmentNo !== this.opsTransaction.serviceNo && form.shipmentMode === 'Internal' && (form.productService.indexOf('Sea') > -1 || form.productService === 'Air')) {
            this.isSaveLink = true;
        } else {
            this.opsTransaction.serviceNo = null;
            this.opsTransaction.serviceHblId = null;
        }
    }

    updateShipment() {
        if (this.isSaveLink) {
            this._documentRepo.getASTransactionInfo(this.opsTransaction.mblno, this.opsTransaction.hwbno, this.opsTransaction.productService, this.opsTransaction.serviceMode)
                .pipe(catchError(this.catchError))
                .subscribe((res: any) => {
                    if (!!res) {
                        this.opsTransaction.serviceNo = res.jobNo;
                        this.opsTransaction.serviceHblId = res.id;
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
                                }
                            );
                    }
                });
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
                    }
                );
        }
    }

    insertDuplicateJob() {
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
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (response: any) => {
                    if (response != null) {
                        this.opsTransaction = new OpsTransaction(response);
                        this.hblid = this.opsTransaction.hblid;

                        this.getListContainersOfJob();
                        this.getSurCharges(CommonEnum.SurchargeTypeEnum.BUYING_RATE);
                        this.editForm.opsTransaction = this.opsTransaction;
                        const hbl = new CsTransactionDetail(this.opsTransaction);
                        hbl.id = this.opsTransaction.hblid;
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
                            mawb: this.opsTransaction.mblno
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
        if (type === CommonEnum.SurchargeTypeEnum.BUYING_RATE) {
            this._store.dispatch(new fromShareBussiness.GetBuyingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.BUYING_RATE, hblId: this.opsTransaction.hblid }));
        }
        if (type === CommonEnum.SurchargeTypeEnum.SELLING_RATE) {
            this._store.dispatch(new fromShareBussiness.GetSellingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.SELLING_RATE, hblId: this.opsTransaction.hblid }));
        }
        if (type === CommonEnum.SurchargeTypeEnum.OBH) {
            this._store.dispatch(new fromShareBussiness.GetOBHSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.OBH, hblId: this.opsTransaction.hblid }));
        }
    }

    selectTab(tabName: string) {
        this.tab = tabName;
        if (tabName === 'job-edit') {
            this.getShipmentDetails(this.jobId);
            this.getSurCharges(CommonEnum.SurchargeTypeEnum.BUYING_RATE);
        }

        if (tabName === 'advance-settle') {
            this.getAdvanceSettleInfoComponent();
        } else {
            this._viewContainerRef.clear();
        }
    }

    onOpePLPrint() {
        this.plSheetPopup.show();
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

    confirmDuplicate() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.confirmContainerRef.viewContainerRef, {
            body: 'The system will open the Job Create Screen. Do you want to leave ?',
            title: 'Duplicate OPS detail',
            labelConfirm: 'Yes'
        },
            () => {
                this.onSubmitDuplicateConfirm();
            })
    }

    onSubmitDuplicateConfirm() {
        this.editForm.isSubmitted = false;
        // this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_DETAIL}/${this.jobId}`], {
        //     queryParams: { action: 'copy' }
        // });
        this.tab = 'job-edit'
        this.isDuplicate = true;
        this.editForm.isJobCopy = this.isDuplicate;
        this.editForm.setFormValue();

        if (this.isDuplicate) {
            this.editForm.getBillingOpsId();
            this.headerComponent.resetBreadcrumb("Create Job");
        }
    }

    async getAdvanceSettleInfoComponent() {
        const { ShareBusinessAdvanceSettlementInforComponent } = await import('./../../share-business/components/advance-settlement-info/advance-settlement-info.component');
        this.renderDynamicComponent(ShareBusinessAdvanceSettlementInforComponent, this.advSettleContainerRef)
    }
}
