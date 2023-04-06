import { formatDate } from "@angular/common";
import { Component, ViewChild } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { InfoPopupComponent } from "@common";
import { ToastrService } from "ngx-toastr";

import { AppForm } from "@app";
import { RoutingConstants, SystemConstants } from "@constants";
import { LinkAirSeaModel, OpsTransaction } from "@models";
import { DocumentationRepo } from "@repositories";
import { JobManagementFormCreateComponent } from "../components/form-create/form-create-job.component";

import _merge from 'lodash/merge';
import { of } from "rxjs";
import { catchError, mergeMap, switchMap, takeUntil } from "rxjs/operators";
import { ActionsSubject, Store } from "@ngrx/store";
import _groupBy from 'lodash/groupBy';
import * as fromShareBussiness from '../../../share-business/store'
import { ShareBussinessContainerListPopupComponent } from '@share-bussiness';
@Component({
    selector: "app-job-mangement-create",
    templateUrl: "./create-job.component.html",
})
export class JobManagementCreateJobComponent extends AppForm {

    @ViewChild(JobManagementFormCreateComponent) formCreateComponent: JobManagementFormCreateComponent;
    @ViewChild(ShareBussinessContainerListPopupComponent) containerPopup: ShareBussinessContainerListPopupComponent;

    isSaveLink: boolean = false;
    listContainer: any[];
    transactionType: string = '';

    constructor(
        private _toaster: ToastrService,
        private _documentRepo: DocumentationRepo,
        private _router: Router,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _actionStoreSubject: ActionsSubject,
        private route: ActivatedRoute,
    ) {
        super();
        this.requestCancel = this.gotoList;
    }

    ngOnInit() {
        this.subscriptionJobOpsType();
        this.subscriptionSaveContainerChange();
    }

    subscriptionJobOpsType() {
        this.subscription =
            this.route.data
                .pipe(
                    takeUntil(this.ngUnsubscribe)
                ).subscribe((res: any) => {
                    console.log(res);
                    this.transactionType = res.transactionType;
                });
    }


    onSubmitData() {
        const form: any = this.formCreateComponent.formCreate.getRawValue();
        const formData = {
            serviceDate: !!form.serviceDate && !!form.serviceDate.startDate ? formatDate(form.serviceDate.startDate, 'yyyy-MM-dd', 'en') : null,
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            deliveryDate: !!form.deliveryDate && !!form.deliveryDate.startDate ? formatDate(form.deliveryDate.startDate, 'yyyy-MM-dd', 'en') : null,
            clearanceDate: !!form.clearanceDate && !!form.clearanceDate.startDate ? formatDate(form.clearanceDate.startDate, 'yyyy-MM-dd', 'en') : null,
            commodityGroupId: form.commodity,
        };
        const opsTransaction: OpsTransaction = new OpsTransaction(Object.assign(_merge(form, formData)));
        opsTransaction.transactionType = this.transactionType;
        opsTransaction.salemanId = form.salemansId;
        opsTransaction.csMawbcontainers = this.listContainer;
        if (!!this.formCreateComponent.jobLinkAirSeaNo
            && form.shipmentMode === 'Internal'
            && (form.productService.indexOf('Sea') > -1 || form.productService === 'Air')) {
            this.isSaveLink = true;

            opsTransaction.sumGrossWeight = this.formCreateComponent.jobLinkAirSeaInfo?.gw;
            opsTransaction.sumChargeWeight = this.formCreateComponent.jobLinkAirSeaInfo?.cw;
            opsTransaction.sumPackages = this.formCreateComponent.jobLinkAirSeaInfo?.packageQty;
            opsTransaction.csMawbcontainers = this.formCreateComponent.jobLinkAirSeaInfo?.containers || [];
            opsTransaction.containerDescription = this.formCreateComponent.jobLinkAirSeaInfo?.packageContainer
        }
        if (this.transactionType === 'TK') {
            opsTransaction.productService = 'Trucking';
        }

        return opsTransaction;
    }

    checkValidateForm() {
        let valid: boolean = true;
        if (!this.formCreateComponent.formCreate.valid || (!!this.formCreateComponent.serviceDate.value && !this.formCreateComponent.serviceDate.value.startDate)) {
            valid = false;
        }
        return valid;
    }

    onSave() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: this.invalidFormText,
                title: 'Cannot Create Job'
            });
            return;
        }

        const modelAdd: OpsTransaction = this.onSubmitData();
        this.saveJob(modelAdd);
    }

    saveJob(model: OpsTransaction) {
        let objUpdateData = null;

        if (this.isSaveLink) {
            objUpdateData = this._documentRepo.validateCheckPointContractPartner({
                partnerId: model.customerId,
                salesmanId: model.salemanId,
                hblId: SystemConstants.EMPTY_GUID,
                transactionType: 'CL',
                type: 1
            }).pipe(
                switchMap(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            return this._documentRepo.getASTransactionInfo(null, model.mblno, model.hwbno, model.productService, model.serviceMode)
                                .pipe(
                                    mergeMap((res: LinkAirSeaModel) => {
                                        model.serviceNo = res?.jobNo;
                                        model.serviceHblId = res?.hblId;
                                        return this._documentRepo.addOPSJob(model);
                                    }),
                                )
                        }
                        return of(res);
                    }
                )
            )

        } else {
            objUpdateData = this._documentRepo.validateCheckPointContractPartner({
                partnerId: model.customerId,
                salesmanId: model.salemanId,
                hblId: SystemConstants.EMPTY_GUID,
                transactionType: 'CL',
                type: 1
            }).pipe(
                switchMap(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            return this._documentRepo.addOPSJob(model);
                        }
                        return of(res);
                    }
                )
            )

        }
        if (objUpdateData != null) {
            objUpdateData
                .pipe(
                    takeUntil(this.ngUnsubscribe),
                    catchError(this.catchError),
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (!res.status) {
                            this._toaster.error(res.message);
                        } else {
                            this._toaster.success(res.message);
                            //this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_DETAIL}/${res.data}`]);
                            if (this.transactionType === 'TK') {
                                this._router.navigate([`${RoutingConstants.LOGISTICS.TRUCKING_INLAND_DETAIL}/${res.data}`]);
                            } else {
                                this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_DETAIL}/${res.data}`]);
                            }

                        }
                    }
                );
        }
    }

    gotoList() {
        if (this.transactionType === 'TK') {
            this._router.navigate([RoutingConstants.LOGISTICS.TRUCKING_INLAND]);
        }
        else {
            this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_MANAGEMENT}`]);
        }
    }

    subscriptionSaveContainerChange() {
        this._actionStoreSubject
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (action: fromShareBussiness.SaveContainerAction) => {
                    if (action.type === fromShareBussiness.ContainerActionTypes.SAVE_CONTAINER) {
                        this.listContainer = action.payload;
                        this.setDataValueGoodsInformation(this.listContainer);
                    }
                }
            );
    }

    setDataValueGoodsInformation(lstMasterContainers: any[]) {
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
                this.formCreateComponent.formCreate.controls[c].setValue(dataSum[c]);
            }
        })
        this.formCreateComponent.formCreate.controls['containerDescription'].setValue(containerDescription);
        this.formCreateComponent.formCreate.controls['sumContainers'].setValue(dataSum.sumContainers === 0 ? null : dataSum.sumContainers);
    }
}
