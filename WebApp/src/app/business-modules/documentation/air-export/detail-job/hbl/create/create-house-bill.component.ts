import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { DocumentationRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';


import * as fromShareBussiness from './../../../../../share-business/store';
import { AirExportHBLFormCreateComponent } from '../components/form-create-house-bill-air-export/form-create-house-bill-air-export.component';
import { ConfirmPopupComponent, InfoPopupComponent } from '@common';
import { catchError, finalize } from 'rxjs/operators';
import { formatDate } from '@angular/common';
import { HouseBill } from '@models';

import _merge from 'lodash/merge';
import { AbstractControl } from '@angular/forms';
import { AirExportHBLAttachListComponent } from '../components/attach-list/attach-list-house-bill-air-export.component';
@Component({
    selector: 'app-create-hbl-air-export',
    templateUrl: './create-house-bill.component.html',
})
export class AirExportCreateHBLComponent extends AppForm implements OnInit {

    @ViewChild(AirExportHBLFormCreateComponent, { static: false }) formCreateHBLComponent: AirExportHBLFormCreateComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(AirExportHBLAttachListComponent, { static: false }) attachListComponent: AirExportHBLAttachListComponent;

    jobId: string;

    constructor(
        protected _progressService: NgProgress,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _cd: ChangeDetectorRef

    ) {
        super();
        this._progressRef = this._progressService.ref();

    }

    ngOnInit() {
        this._activedRoute.params
            .subscribe((param: Params) => {
                if (param.jobId) {
                    this.jobId = param.jobId;
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                }
            });
    }

    getDataForm() {
        const form: any = this.formCreateHBLComponent.formCreate.getRawValue();
        console.log(form);
        const formData = {
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            etd: !!form.etd && !!form.etd.startDate ? formatDate(form.etd.startDate, 'yyyy-MM-dd', 'en') : null,
            issueHbldate: !!form.issueHbldate ? formatDate(form.issueHbldate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightDate: !!form.flightDate ? formatDate(form.flightDate.startDate, 'yyyy-MM-dd', 'en') : null,

            originBlnumber: !!form.originBlnumber && !!form.originBlnumber.length ? form.originBlnumber[0].id : null,
            freightPayment: !!form.freightPayment && !!form.freightPayment.length ? form.freightPayment[0].id : null,
            hbltype: !!form.hbltype && !!form.hbltype.length ? form.hbltype[0].id : null,
            currencyId: !!form.currencyId && !!form.currencyId.length ? form.currencyId[0].id : null,
            wtorValpayment: !!form.wtorValpayment && !!form.wtorValpayment.length ? form.wtorValpayment[0].id : null,
            otherPayment: !!form.otherPayment && !!form.otherPayment.length ? form.otherPayment[0].id : null,

            customerId: form.customer,
            saleManId: form.saleMan,
            shipperId: form.shipper,
            consigneeId: form.consignee,
            pol: form.pol,
            pod: form.pod,
            forwardingAgentId: form.forwardingAgent,

            cbm: this.formCreateHBLComponent.totalCBM,
            hw: this.formCreateHBLComponent.totalHeightWeight,
            attachList: this.attachListComponent.attachList,
            dimensionDetails: this.formCreateHBLComponent.dims,
            hwConstant: 2323
        };

        const houseBill = new HouseBill(_merge(form, formData));
        return houseBill;
    }

    saveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const houseBill: HouseBill = this.getDataForm();
        houseBill.jobId = this.jobId;

        this.createHbl(houseBill);
    }

    checkValidateForm() {
        let valid: boolean = true;

        [this.formCreateHBLComponent.hbltype,
        this.formCreateHBLComponent.otherPayment,
        this.formCreateHBLComponent.originBlnumber,
        this.formCreateHBLComponent.currencyId,
        this.formCreateHBLComponent.freightPayment,
        this.formCreateHBLComponent.wtorValpayment].forEach((control: AbstractControl) => this.setError(control));;

        if (!this.formCreateHBLComponent.formCreate.valid
            || (!!this.formCreateHBLComponent.etd.value && !this.formCreateHBLComponent.etd.value.startDate)
        ) {
            valid = false;
        }
        return valid;
    }

    createHbl(houseBill: HouseBill) {
        this._progressRef.start();
        this._documentationRepo.createHousebill(houseBill)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.gotoList();
                    } else {

                    }
                }
            );

    }

    gotoList() {
        this._router.navigate([`home/documentation/air-export/${this.jobId}/hbl`]);
    }

}
