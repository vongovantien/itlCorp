import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { NgForm, AbstractControl } from '@angular/forms';
import { NgProgress } from '@ngx-progressbar/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { formatDate } from '@angular/common';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { DocumentationRepo } from 'src/app/shared/repositories/documentation.repo';
import { ShareBussinessSellingChargeComponent, ShareBussinessContainerListPopupComponent } from '../../share-business';
import { ConfirmPopupComponent, InfoPopupComponent, SubHeaderComponent } from '@common';
import { OpsTransaction, CsTransactionDetail, CsTransaction, Container } from '@models';
import { CommonEnum } from '@enums';
import * as fromShareBussiness from './../../share-business/store';
import { OPSTransactionGetDetailSuccessAction } from '../store';

import { JobManagementFormEditComponent } from './components/form-edit/form-edit.component';
import { AppForm } from 'src/app/app.form';
import { ICanComponentDeactivate } from '@core';
import { combineLatest, Observable, of } from 'rxjs';
import { PlSheetPopupComponent } from './pl-sheet-popup/pl-sheet.popup';

import { catchError, finalize, map, takeUntil } from 'rxjs/operators';
import _groupBy from 'lodash/groupBy';
import { RoutingConstants } from '@constants';


@Component({
    selector: 'app-ops-module-billing-job-edit',
    templateUrl: './job-edit.component.html',
})
export class OpsModuleBillingJobEditComponent extends AppForm implements OnInit, ICanComponentDeactivate {

    @ViewChild(PlSheetPopupComponent) plSheetPopup: PlSheetPopupComponent;
    @ViewChild('confirmCancelUpdate') confirmCancelJobPopup: ConfirmPopupComponent;
    @ViewChild('notAllowDelete') canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild('confirmDelete') confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild('confirmLockShipment') confirmLockShipmentPopup: ConfirmPopupComponent;
    @ViewChild("duplicateconfirmTemplate") confirmDuplicatePopup: ConfirmPopupComponent;
    @ViewChild(ShareBussinessSellingChargeComponent) sellingChargeComponent: ShareBussinessSellingChargeComponent;
    @ViewChild(ShareBussinessContainerListPopupComponent) containerPopup: ShareBussinessContainerListPopupComponent;

    @ViewChild(JobManagementFormEditComponent) editForm: JobManagementFormEditComponent;
    @ViewChild('addOpsForm') formOps: NgForm;
    @ViewChild('notAllowUpdate') infoPoup: InfoPopupComponent;
    @ViewChild(SubHeaderComponent) headerComponent: SubHeaderComponent;

    opsTransaction: OpsTransaction = null;
    lstMasterContainers: any[];

    tab: string = '';
    tabCharge: string = '';
    jobId: string = '';
    hblid: string = '';

    deleteMessage: string = '';

    nextState: RouterStateSnapshot;
    isCancelFormPopupSuccess: boolean = false;
    selectedTabSurcharge: string = 'BUY';
    action: any = {};

    constructor(
        private _spinner: NgxSpinnerService,
        private route: ActivatedRoute,
        private router: Router,
        private _ngProgressService: NgProgress,
        private _documentRepo: DocumentationRepo,
        private _router: Router,
        private _toastService: ToastrService,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _actionStoreSubject: ActionsSubject,
        protected _cd: ChangeDetectorRef,
    ) {
        super();

        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        combineLatest([
            this.route.params,
            this.route.queryParams
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams }))
        )
            .subscribe((params: any) => {
                this.tab = !!params.tab ? params.tab : 'job-edit';
                this.tabCharge = 'buying';
                if (!!params) {
                    this.jobId = params.id;
                    if (!!params.action) {
                        this.isDuplicate = params.action.toUpperCase() === 'COPY';
                        this.selectedTabSurcharge = 'BUY';
                    }
                    this.getShipmentDetails(params.id);
                }

            });

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
                        this.confirmDeleteJobPopup.show();
                    } else {
                        this.canNotDeleteJobPopup.show();
                    }
                }
            );
    }

    onDeleteJob() {
        this._documentRepo.deleteShipment(this.opsTransaction.id)
            .subscribe(
                (response: CommonInterface.IResult) => {
                    if (response.status) {
                        this.confirmDeleteJobPopup.hide();
                        this.router.navigate([`${RoutingConstants.LOGISTICS.JOB_MANAGEMENT}`]);
                    }
                }
            );
    }

    onCancelUpdateJob() {
        this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_MANAGEMENT}`]);
    }

    confirmCancelJob() {
        this.confirmCancelJobPopup.show();
    }

    saveShipment() {
        this.lstMasterContainers.forEach((c: Container) => {
            c.mblid = this.jobId;
            c.hblid = this.hblid;
        });
        this.opsTransaction.csMawbcontainers = this.lstMasterContainers;
        this.editForm.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPoup.show();
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

        this.opsTransaction.shipmentMode = form.shipmentMode;
        this.opsTransaction.productService = form.productService;
        this.opsTransaction.serviceMode = form.serviceMode;
        this.opsTransaction.packageTypeId = form.packageTypeId;
        this.opsTransaction.commodityGroupId = form.commodityGroupId;
        this.opsTransaction.shipmentType = form.shipmentType;
    }

    updateShipment() {
        this._spinner.show();
        this._documentRepo.updateShipment(this.opsTransaction)
            .pipe(catchError(this.catchError), finalize(() => this._spinner.hide()))
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

    insertDuplicateJob() {
        this._spinner.show();
        this._documentRepo.insertDuplicateShipment(this.opsTransaction)
            .pipe(catchError(this.catchError), finalize(() => this._spinner.hide()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.jobId = res.data.id;
                        this.isDuplicate = false;
                        this.headerComponent.resetBreadcrumb("Detail Job");
                        this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_DETAIL}/`, this.jobId]);
                    } else {
                        this._toastService.warning(res.message);
                    }
                }
            );
    }

    lockShipment() {
        this.confirmLockShipmentPopup.show();
    }

    onLockShipment() {
        this.opsTransaction.isLocked = true;
        this.confirmLockShipmentPopup.hide();

        this.updateShipment();
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
                    this.opsTransaction = new OpsTransaction(response);
                    this.hblid = this.opsTransaction.hblid;
                    if (this.opsTransaction != null) {
                        this.getListContainersOfJob();
                        if (this.opsTransaction != null) {
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

                            this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: this.jobId }));
                            this._store.dispatch(new fromShareBussiness.GetContainersHBLAction({ hblid: this.opsTransaction.hblid }));

                            this.editForm.isJobCopy = this.isDuplicate;
                            this.editForm.setFormValue();
                        }

                        if (this.isDuplicate) {
                            this.editForm.getBillingOpsId();
                            this.headerComponent.resetBreadcrumb("Create Job");
                        }
                    }
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
            this.confirmCancelJobPopup.show();
        } else {
            this.isCancelFormPopupSuccess = true;
            this.gotoList();
        }
    }

    confirmCancel() {
        this.confirmCancelJobPopup.hide();
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
        if (this.isCancelFormPopupSuccess || !this.isDuplicate) {
            return of(true);
        }
        if (isEdited && !this.isCancelFormPopupSuccess) {
            this.confirmCancelJobPopup.show();
            return;
        }
        return of(!isEdited);
    }

    confirmDuplicate() {
        this.confirmDuplicatePopup.show();
    }

    onSubmitDuplicateConfirm() {
        this.action = { action: 'copy' };
        this.editForm.isSubmitted = false;
        this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_DETAIL}/${this.jobId}`], {
            queryParams: Object.assign({}, { tab: 'job-edit' }, this.action)
        });
        this.confirmDuplicatePopup.hide();
    }
}
