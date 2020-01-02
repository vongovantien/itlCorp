import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { DocumentationRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';


import * as fromShareBussiness from './../../../../../share-business/store';
import { ConfirmPopupComponent, InfoPopupComponent } from '@common';
import { catchError, finalize, skip, takeUntil, mergeMap } from 'rxjs/operators';
import { formatDate } from '@angular/common';
import { HouseBill, DeliveryOrder } from '@models';

import _merge from 'lodash/merge';
import { AirImportHBLFormCreateComponent } from '../components/form-create-house-bill-air-import/form-create-house-bill-air-import.component';
import { ShareBusinessArrivalNoteComponent, ShareBusinessDeliveryOrderComponent } from '@share-bussiness';
import { HBLArrivalNote } from 'src/app/shared/models/document/arrival-note-hbl';
import { forkJoin } from 'rxjs';
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
                    this.deliveryComponent.isAir = true;
                    this.getDetailShipment();
                }
            });
    }

    ngAfterViewInit() {

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
            mawb: form.mawb,
            customerId: form.customer,
            saleManId: form.saleManId,
            shipperId: form.shipperId,
            consigneeId: form.consigneeId,
            notifyPartyId: form.notifyId,
            notifyPartyDescription: form.notifyDescription,
            hwbno: form.hawb,
            hbltype: !!form.hbltype && !!form.hbltype.length ? form.hbltype[0].id : null,
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            arrivalDate: !!form.arrivaldate && !!form.arrivaldate.startDate ? formatDate(form.arrivaldate.startDate, 'yyyy-MM-dd', 'en') : null,
            forwardingAgentId: form.forwardingAgentId,
            pol: form.pol,
            pod: form.pod,
            warehouseNotice: form.warehouseNotice,
            route: form.route,
            flightNo: form.flightNo,
            flightDate: !!form.flightDate ? formatDate(form.flightDate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightNoOrigin: form.flightNoOrigin,
            flightDateOrigin: !!form.flightDateOrigin ? formatDate(form.flightDateOrigin.startDate, 'yyyy-MM-dd', 'en') : null,
            issueHBLDate: !!form.issueHBLDate ? formatDate(form.issueHBLDate.startDate, 'yyyy-MM-dd', 'en') : null,
            packageType: !!form.unit && !!form.unit.length ? form.unit[0].id : null,
            freightPayment: !!form.freightPayment && !!form.freightPayment.length ? form.freightPayment[0].id : null,
            packageQty: form.packageQty,
            grossWeight: form.gw,
            chargeWeight: form.cw,
            poInvoiceNo: form.po,
            goodsDeliveryDescription: form.descriptionOfGood,
            finalPOD: form.finalPOD,
            desOfGoods: form.descriptionOfGood
        };

        const houseBill = new HouseBill(_merge(form, formData));
        console.log(houseBill);
        return houseBill;
    }

    saveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;
        if (!this.checkValidateForm() || !this.arrivalNoteComponent.checkValidate() || !this.deliveryComponent.deliveryOrder.deliveryOrderNo) {
            this.arrivalNoteComponent.isSubmitted = true;
            this.deliveryComponent.isSubmitted = true;
            this.infoPopup.show();
        } else {
            const houseBill: HouseBill = this.getDataForm();
            houseBill.jobId = this.jobId;
            this.createHbl(houseBill);
        }
    }

    checkValidateForm() {
        let valid: boolean = true;
        // this.setError(this.formCreateHBLComponent.hbltype);
        this.setError(this.formCreateHBLComponent.freightPayment);
        this.setError(this.formCreateHBLComponent.packageType);
        if (!this.formCreateHBLComponent.formCreate.valid) {
            valid = false;
        }
        return valid;
    }

    createHbl(houseBill: HouseBill) {
        if (this.formCreateHBLComponent.formCreate.valid) {
            this._progressRef.start();
            this._documentationRepo.createHousebill(houseBill)
                .pipe(
                    mergeMap((res: any) => {
                        const dateNotice = {
                            arrivalFirstNotice: !!this.arrivalNoteComponent.hblArrivalNote.arrivalFirstNotice && !!this.arrivalNoteComponent.hblArrivalNote.arrivalFirstNotice.startDate ? formatDate(this.arrivalNoteComponent.hblArrivalNote.arrivalFirstNotice.startDate, 'yyyy-MM-dd', 'en') : null,
                            arrivalSecondNotice: !!this.arrivalNoteComponent.hblArrivalNote.arrivalSecondNotice && <any>!!this.arrivalNoteComponent.hblArrivalNote.arrivalSecondNotice.startDate ? formatDate(this.arrivalNoteComponent.hblArrivalNote.arrivalSecondNotice.startDate, 'yyyy-MM-dd', 'en') : null,
                        };
                        this.arrivalNoteComponent.hblArrivalNote.hblid = res.data;
                        const arrival = this._documentationRepo.updateArrivalInfo(Object.assign({}, this.arrivalNoteComponent.hblArrivalNote, dateNotice));
                        const printedDate = {
                            deliveryOrderPrintedDate: !!this.deliveryComponent.deliveryOrder.deliveryOrderPrintedDate && !!this.deliveryComponent.deliveryOrder.deliveryOrderPrintedDate.startDate ? formatDate(this.deliveryComponent.deliveryOrder.deliveryOrderPrintedDate.startDate, 'yyyy-MM-dd', 'en') : null,
                        };
                        this.deliveryComponent.deliveryOrder.hblid = res.data;
                        const delivery = this._documentationRepo.updateDeliveryOrderInfo(Object.assign({}, this.deliveryComponent.deliveryOrder, printedDate));
                        return forkJoin([arrival, delivery]);
                    }),
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                ).subscribe(result => {
                    this._toastService.success(result[0].message, '');
                    this.gotoList();
                }
                );
        }
    }


    gotoList() {
        this._router.navigate([`home/documentation/air-import/${this.jobId}/hbl`]);
    }

}
