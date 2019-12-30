import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { DocumentationRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';


import * as fromShareBussiness from './../../../../../share-business/store';
import { ConfirmPopupComponent, InfoPopupComponent } from '@common';
import { catchError, finalize, skip, takeUntil } from 'rxjs/operators';
import { formatDate } from '@angular/common';
import { HouseBill, DeliveryOrder } from '@models';

import _merge from 'lodash/merge';
import { AirImportHBLFormCreateComponent } from '../components/form-create-house-bill-air-import/form-create-house-bill-air-import.component';
import { ShareBusinessArrivalNoteComponent, ShareBusinessDeliveryOrderComponent } from '@share-bussiness';
import { HBLArrivalNote } from 'src/app/shared/models/document/arrival-note-hbl';
@Component({
    selector: 'app-create-hbl-air-import',
    templateUrl: './create-house-bill.component.html',
})
export class AirImportCreateHBLComponent extends AppForm implements OnInit {
    @ViewChild(AirImportHBLFormCreateComponent, { static: false }) formCreateHBLComponent: AirImportHBLFormCreateComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ShareBusinessArrivalNoteComponent, { static: true, }) arrivalNoteComponent: ShareBusinessArrivalNoteComponent;
    @ViewChild(ShareBusinessDeliveryOrderComponent, { static: true }) deliveryComponent: ShareBusinessDeliveryOrderComponent;
    jobId: string;
    hblDetail: any = {};

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
                    this.getDetailShipment();
                }
            });

    }

    getDetailShipment() {
        this._store.select(fromShareBussiness.getTransactionDetailCsTransactionState)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (!!res) {
                        this.hblDetail = res;

                        const objArrival = {
                            arrivalNo: this.hblDetail.jobNo + "-A01",
                            arrivalFirstNotice: new Date()
                        };
                        this.arrivalNoteComponent.hblArrivalNote = new HBLArrivalNote(objArrival);

                        const objDelivery = {
                            deliveryOrderNo: this.hblDetail.jobNo + "-D01",
                            deliveryOrderPrintedDate: { startDate: new Date(), endDate: new Date() }
                        };
                        this.deliveryComponent.deliveryOrder = new DeliveryOrder(objDelivery);
                    }
                },
            );
    }

    getDataForm() {
        const form: any = this.formCreateHBLComponent.formCreate.getRawValue();
        console.log(form);
        const formData = {
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            etd: !!form.etd && !!form.etd.startDate ? formatDate(form.etd.startDate, 'yyyy-MM-dd', 'en') : null,
            issueHBLDate: !!form.issueHBLDate ? formatDate(form.issueHBLDate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightDate: !!form.flightDate ? formatDate(form.flightDate.startDate, 'yyyy-MM-dd', 'en') : null,

            originBlnumber: !!form.originBlnumber && !!form.originBlnumber.length ? form.originBlnumber[0].id : null,
            freightPayment: !!form.freightPayment && !!form.freightPayment.length ? form.freightPayment[0].id : null,
            hbltype: !!form.hbltype && !!form.hbltype.length ? form.hbltype[0].id : null,

            customerId: form.customer,
            saleManId: form.saleMan,
            shipperId: form.shipper,
            consigneeId: form.consignee,
            pol: form.pol,
            pod: form.pod,
            forwardingAgentId: form.forwardingAgent,
        };

        const houseBill = new HouseBill(_merge(form, formData));
        console.log(houseBill);
        return houseBill;
    }

    saveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;

        // if (!this.checkValidateForm()) {
        //     this.infoPopup.show();
        //     return;
        // }

        const houseBill: HouseBill = this.getDataForm();
        houseBill.jobId = this.jobId;

        // this.createHbl(modelAdd);

    }

    checkValidateForm() {
        let valid: boolean = true;

        this.setError(this.formCreateHBLComponent.hbltype);
        this.setError(this.formCreateHBLComponent.originBlnumber);
        this.setError(this.formCreateHBLComponent.freightPayment);


        if (!this.formCreateHBLComponent.formCreate.valid) {
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
        this._router.navigate([`home/documentation/air-import/${this.jobId}/hbl`]);
    }

}
