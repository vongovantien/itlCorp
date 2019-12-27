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
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-create-hbl-air-export',
    templateUrl: './create-house-bill.component.html',
})
export class AirExportCreateHBLComponent extends AppForm implements OnInit {
    @ViewChild(AirExportHBLFormCreateComponent, { static: false }) formCreateHBLComponent: AirExportHBLFormCreateComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
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
            id: SystemConstants.EMPTY_GUID,
            jobId: this.jobId,
            mawb: form.mawb,
            shipperDescription: form.shipperDescription,
            consigneeDescription: form.consigneeDescription,
            hwbno: form.hwbno,
            pickupPlace: form.placeReceipt,
            deliveryPlace: form.placeDelivery,
            placeFreightPay: form.placeFreightPay,
            issueHblplaceAndDate: form.issueHblplaceAndDate,
            issueHbldate: new Date(),
            issueHblplace: form.issueHblplaceAndDate,
            forwardingAgentDescription: form.forwardingAgentDescription,

            originBlnumber: !!form.originBlnumber ? form.originBlnumber[0].id : null,
            freightPayment: !!form.freightPayment ? form.freightPayment[0].id : null,
            hbltype: !!form.hbltype ? form.hbltype[0].id : null,

            customerId: form.customer,
            saleManId: form.saleMan,
            shipperId: form.shipper,
            consigneeId: form.consignee,
            pol: form.pol,
            pod: form.pod,
            forwardingAgentId: form.forwardingAgent,
        };

        return formData;
    }




    saveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;

        // if (!this.checkValidateForm()) {
        //     this.infoPopup.show();
        //     return;
        // }

        const modelAdd = this.getDataForm();
        // this.createHbl(modelAdd);

    }

    checkValidateForm() {
        let valid: boolean = true;

        this.setError(this.formCreateHBLComponent.hbltype);
        this.setError(this.formCreateHBLComponent.otherPayment);
        this.setError(this.formCreateHBLComponent.originBLNumber);

        if (!this.formCreateHBLComponent.formCreate.valid) {
            valid = false;
        }
        return valid;
    }

    createHbl(body: any) {
        this._progressRef.start();
        this._documentationRepo.createHousebill(body)
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
