import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { AbstractControl } from '@angular/forms';

import { AppForm } from 'src/app/app.form';
import { InfoPopupComponent } from '@common';
import { DocumentationRepo } from '@repositories';
import { CsTransaction } from '@models';
import { CommonEnum } from '@enums';
import { RoutingConstants } from '@constants';
import {
    ShareBusinessImportJobDetailPopupComponent,
    ShareBusinessFormCreateAirComponent
} from 'src/app/business-modules/share-business';

import * as fromShareBusiness from './../../../share-business/store';

import { catchError } from 'rxjs/operators';
import _merge from 'lodash/merge';
@Component({
    selector: 'app-create-job-air-export',
    templateUrl: './create-job-air-export.component.html'
})

export class AirExportCreateJobComponent extends AppForm implements OnInit {

    @ViewChild(ShareBusinessFormCreateAirComponent, { static: false }) formCreateComponent: ShareBusinessFormCreateAirComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ShareBusinessImportJobDetailPopupComponent, { static: true }) formImportJobDetailPopup: ShareBusinessImportJobDetailPopupComponent;

    isImport: boolean = false;
    selectedJob: CsTransaction;

    constructor(
        protected _toastService: ToastrService,
        protected _documenRepo: DocumentationRepo,
        protected _router: Router,
        protected _store: Store<fromShareBusiness.IShareBussinessState>
    ) {
        super();
    }

    ngOnInit() {
        this._store.dispatch(new fromShareBusiness.TransactionGetDetailSuccessAction({}));
        this.formImportJobDetailPopup.service = 'air';
    }

    onSubmitData() {
        const form: any = this.formCreateComponent.formGroup.getRawValue();
        const formData = {
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            etd: !!form.etd && !!form.etd.startDate ? formatDate(form.etd.startDate, 'yyyy-MM-dd', 'en') : null,
            serviceDate: !!form.serviceDate && !!form.serviceDate.startDate ? formatDate(form.serviceDate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightDate: !!form.flightDate && !!form.flightDate.startDate ? formatDate(form.flightDate.startDate, 'yyyy-MM-dd', 'en') : null,

            shipmentType: !!form.shipmentType && !!form.shipmentType.length ? form.shipmentType[0].id : null,
            mbltype: !!form.mbltype && !!form.mbltype.length ? form.mbltype[0].id : null,
            paymentTerm: !!form.paymentTerm && !!form.paymentTerm.length ? form.paymentTerm[0].id : null,
            packageType: !!form.packageType && !!form.packageType.length ? form.packageType[0].id : null,
            commodity: !!form.commodity && !!form.commodity.length ? form.commodity.map(i => i.id).toString() : null,

            agentId: form.agentId,
            pol: form.pol,
            pod: form.pod,
            coloaderId: form.coloaderId,
            warehouseId: form.warehouseId,

            airlineInfo: form.airlineInfo,
        };
        const csTransaction: CsTransaction = new CsTransaction(Object.assign(_merge(form, formData)));
        csTransaction.transactionTypeEnum = CommonEnum.TransactionTypeEnum.AirExport;

        return csTransaction;
    }

    checkValidateForm() {
        [this.formCreateComponent.shipmentType,
        this.formCreateComponent.packageType,
        this.formCreateComponent.mbltype,
        this.formCreateComponent.commodity,
        this.formCreateComponent.paymentTerm].forEach((control: AbstractControl) => this.setError(control));

        let valid: boolean = true;
        if (!this.formCreateComponent.formGroup.valid || (!!this.formCreateComponent.etd.value && !this.formCreateComponent.etd.value.startDate)) {
            valid = false;
        }
        return valid;
    }

    onSaveJob() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const modelAdd = this.onSubmitData();
        modelAdd.dimensionDetails = this.formCreateComponent.dimensionDetails;

        modelAdd.dimensionDetails.forEach(d => {
            d.airWayBillId = null;
            d.hblid = null;
        });

        if (this.isImport === true) {
            modelAdd.id = this.selectedJob.id;
            this.importJob(modelAdd);
        } else {
            this.saveJob(modelAdd);
        }
    }

    saveJob(body: any) {
        console.log("test body: ", body);
        this._documenRepo.createTransaction(body)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    if (res.result.success) {
                        this._toastService.success("New data added");

                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${res.model.id}`]);
                    } else {
                        this._toastService.error("Opps", "Something getting error!");
                    }
                }
            );
    }

    onImport(selectedData: any) {
        this.selectedJob = selectedData;
        this.isImport = true;
        this.formCreateComponent.isUpdate = true;
        this.formCreateComponent.formGroup.controls['jobNo'].setValue(null);
        this.formCreateComponent.formGroup.controls['personIncharge'].setValue(this.formCreateComponent.userLogged.id);
        this._store.dispatch(new fromShareBusiness.GetDimensionAction(selectedData.id));
    }

    showImportPopup() {
        this.formImportJobDetailPopup.transactionType = CommonEnum.TransactionTypeEnum.AirExport;
        this.formImportJobDetailPopup.getShippments();
        this.formImportJobDetailPopup.selected = -1;
        this.formImportJobDetailPopup.selectedShipment = null;
        this.formImportJobDetailPopup.show();
    }


    importJob(body: any) {
        this._documenRepo.importCSTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        // TODO goto detail.
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${res.data.id}`]);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    gotoList() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}`]);
    }
}

