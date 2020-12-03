import { Component, OnInit, ViewChild } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';

import { AppForm } from '@app';
import { DocumentationRepo } from '@repositories';
import { ConfirmPopupComponent, InfoPopupComponent } from '@common';
import { HouseBill, DIM, CsTransactionDetail } from '@models';
import {
    ShareBusinessImportHouseBillDetailComponent,
    ShareBusinessAttachListHouseBillComponent,
    getDimensionVolumesState, getDetailHBlState,
    GetDetailHBLSuccessAction,
    getTransactionPermission
} from '@share-bussiness';
import { SystemConstants, RoutingConstants } from '@constants';
import { CommonEnum } from '@enums';


import * as fromShareBussiness from './../../../../../share-business/store';
import { AirExportHBLFormCreateComponent } from '../components/form-create-house-bill-air-export/form-create-house-bill-air-export.component';

import _merge from 'lodash/merge';
import { catchError, finalize, takeUntil, map, tap, mergeMap } from 'rxjs/operators';
import isUUID from 'validator/lib/isUUID';
import { forkJoin, merge } from 'rxjs';

@Component({
    selector: 'app-create-hbl-air-export',
    templateUrl: './create-house-bill.component.html',
})
export class AirExportCreateHBLComponent extends AppForm implements OnInit {

    @ViewChild(AirExportHBLFormCreateComponent, { static: true }) formCreateHBLComponent: AirExportHBLFormCreateComponent;
    @ViewChild('confirmSave') confirmPopup: ConfirmPopupComponent;
    @ViewChild('confirmSaveExistedHbl') confirmExistedHbl: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(ShareBusinessAttachListHouseBillComponent) attachListComponent: ShareBusinessAttachListHouseBillComponent;
    @ViewChild(ShareBusinessImportHouseBillDetailComponent) importHouseBillPopup: ShareBusinessImportHouseBillDetailComponent;

    jobId: string;
    selectedHbl: CsTransactionDetail;
    isImport: boolean = false;

    constructor(
        protected _progressService: NgProgress,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,

    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this._activedRoute.params
            .subscribe((param: Params) => {
                if (isUUID(param.jobId)) {
                    this.jobId = param.jobId;
                    this.generateHblNo(CommonEnum.TransactionTypeEnum.AirExport);
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                    this.permissionShipments = this._store.select(getTransactionPermission);
                } else {
                    this.gotoList();
                }
            });

        this.listenShortcutSaveHawb();
    }

    listenShortcutSaveHawb() {
        merge(
            this.createShortcut(['ControlLeft', 'ShiftLeft', 'KeyS']),
        ).pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                () => {
                    this.confirmSaveHBL();
                }
            );
    }

    generateHblNo(transactionType: number) {
        this._documentationRepo.generateHBLNo(transactionType)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.formCreateHBLComponent.hwbno.setValue(res.hblNo);
                }
            );
    }

    getDataForm() {
        const form: any = this.formCreateHBLComponent.formCreate.getRawValue();
        const formData = {
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            etd: !!form.etd && !!form.etd.startDate ? formatDate(form.etd.startDate, 'yyyy-MM-dd', 'en') : null,
            issueHbldate: !!form.issueHbldate && !!form.issueHbldate.startDate ? formatDate(form.issueHbldate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightDate: !!form.flightDate && !!form.flightDate.startDate ? formatDate(form.flightDate.startDate, 'yyyy-MM-dd', 'en') : null,

            customerId: form.customer,
            saleManId: form.saleMan,
            shipperId: form.shipper,
            consigneeId: form.consignee,
            forwardingAgentId: form.forwardingAgent,
            shipmentType: form.shipmenttype,
            rateCharge: form.asArranged ? form.rateChargeAs : form.rateCharge,

            cbm: this.formCreateHBLComponent.totalCBM,
            hw: this.formCreateHBLComponent.totalHeightWeight,
            attachList: this.attachListComponent.attachList.replace(form.hwbno, '[[HBLNo]]').replace(formatDate(form.etd.startDate, 'dd/MM/yyyy', 'en'), '[[Date]]'),
            dimensionDetails: form.dimensionDetails,
            hwConstant: this.formCreateHBLComponent.hwconstant,

        };

        const houseBill = new HouseBill(_merge(form, formData));
        return houseBill;
    }

    saveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;
        if (this.isImport) {
            this._documentationRepo.generateHBLNo(CommonEnum.TransactionTypeEnum.AirExport)
                .pipe(
                    mergeMap((res: any) => {
                        if (this.formCreateHBLComponent.hwbno.value == null || this.formCreateHBLComponent.hwbno.value == "") {
                            this.formCreateHBLComponent.hwbno.setValue(res.hblNo);
                        }
                        if (!this.checkValidateForm()) {
                            this.infoPopup.show();
                            return null;
                        }
                        return forkJoin([this._documentationRepo.checkExistedHawbNo(this.formCreateHBLComponent.hwbno.value, this.jobId, null)]);
                    }
                    )).subscribe(result => {
                        if (result[0]) {
                            this.confirmExistedHbl.show();
                        } else {
                            const houseBill: HouseBill = this.getDataForm();
                            this.setData(houseBill);
                            this.createHbl(houseBill);
                        }
                    }
                    );
        } else {
            if (!this.checkValidateForm()) {
                this.infoPopup.show();
                return;
            }
            this._documentationRepo.checkExistedHawbNo(this.formCreateHBLComponent.hwbno.value, this.jobId, null)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res) {
                            this.confirmExistedHbl.show();
                        } else {
                            const houseBill: HouseBill = this.getDataForm();
                            this.setData(houseBill);
                            this.createHbl(houseBill);
                        }
                    }
                );
        }

    }

    setData(houseBill: HouseBill) {
        houseBill.jobId = this.jobId;
        houseBill.otherCharges = this.formCreateHBLComponent.otherCharges;
        houseBill.otherCharges.forEach(c => {
            c.jobId = this.jobId;
            c.hblId = SystemConstants.EMPTY_GUID;
        });
    }

    confirmSaveHBL() {
        this.confirmPopup.show();
    }

    confirmSaveData() {
        this.confirmExistedHbl.hide();
        const houseBill: HouseBill = this.getDataForm();
        this.setData(houseBill);
        this.createHbl(houseBill);
    }

    checkValidateForm() {
        let valid: boolean = true;
        if (!this.formCreateHBLComponent.formCreate.valid
            || (!!this.formCreateHBLComponent.etd.value && !this.formCreateHBLComponent.etd.value.startDate)
            || (!!this.formCreateHBLComponent.issueHbldate.value && !this.formCreateHBLComponent.issueHbldate.value.startDate)
        ) {
            valid = false;
        }
        return valid;
    }

    createHbl(houseBill: HouseBill, hbId?: string) {
        this._progressRef.start();
        this._documentationRepo.createHousebill(houseBill)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        if (!res.data) {
                            this.gotoList();
                        } else {
                            if (!!hbId) {
                                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}${this.jobId}/hbl/${hbId}/separate`]);
                            } else {
                                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}/hbl/${res.data}`]);
                            }
                        }
                    }
                }
            );
    }

    showImportPopup() {
        const dataSearch = { jobId: this.jobId };
        dataSearch.jobId = this.jobId;
        this.importHouseBillPopup.typeFCL = 'Export';
        this.importHouseBillPopup.selected = - 1;
        this.importHouseBillPopup.getHourseBill(dataSearch);
        this.importHouseBillPopup.show();
    }

    onImport(selectedData: CsTransactionDetail) {
        this.selectedHbl = selectedData;
        this._store.dispatch(new GetDetailHBLSuccessAction(this.selectedHbl));
        this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.selectedHbl.id));

        if (!!this.selectedHbl) {
            this._store.select(getDimensionVolumesState)
                .pipe(
                    takeUntil(this.ngUnsubscribe),
                    map((dims: DIM[]) => dims.map(d => new DIM(d))),
                    tap((dims: DIM[]) => { this.formCreateHBLComponent.dims = dims; }),
                    mergeMap(() => this._store.select(getDetailHBlState))
                ).subscribe(
                    (hbl: HouseBill) => {
                        if (!!hbl && hbl.id !== SystemConstants.EMPTY_GUID) {
                            this.isImport = true;
                            this.formCreateHBLComponent.totalCBM = hbl.cbm;
                            this.formCreateHBLComponent.totalHeightWeight = hbl.hw;
                            this.formCreateHBLComponent.jobId = hbl.jobId;
                            this.formCreateHBLComponent.hblId = hbl.id;
                            this.formCreateHBLComponent.hwconstant = hbl.hwConstant;

                            this.formCreateHBLComponent.updateFormValue(hbl, true);
                            this.formCreateHBLComponent.hwbno.setValue(this.formCreateHBLComponent.hwbno.value);
                        }
                    }
                );
        }
    }

    gotoList() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}/hbl`]);
    }
}
