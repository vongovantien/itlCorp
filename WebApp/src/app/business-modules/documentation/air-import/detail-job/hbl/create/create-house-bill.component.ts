import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { DocumentationRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';
import { ConfirmPopupComponent, InfoPopupComponent } from '@common';
import { HouseBill, DeliveryOrder, CsTransaction, HBLArrivalNote } from '@models';

import { AirImportHBLFormCreateComponent } from '../components/form-create-house-bill-air-import/form-create-house-bill-air-import.component';
import { ShareBusinessDeliveryOrderComponent, ShareBusinessImportHouseBillDetailComponent, ShareBusinessArrivalNoteAirComponent, getTransactionPermission, IShareBussinessState, TransactionGetDetailAction, getTransactionDetailCsTransactionState } from '@share-bussiness';

import { forkJoin } from 'rxjs';
import _merge from 'lodash/merge';
import isUUID from 'validator/lib/isUUID';
import { DataService } from '@services';

import { catchError, finalize, skip, takeUntil, mergeMap } from 'rxjs/operators';


@Component({
    selector: 'app-create-hbl-air-import',
    templateUrl: './create-house-bill.component.html',
})
export class AirImportCreateHBLComponent extends AppForm implements OnInit {
    @ViewChild(AirImportHBLFormCreateComponent, { static: false }) formCreateHBLComponent: AirImportHBLFormCreateComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ShareBusinessArrivalNoteAirComponent, { static: true }) arrivalNoteComponent: ShareBusinessArrivalNoteAirComponent;
    @ViewChild(ShareBusinessDeliveryOrderComponent, { static: true }) deliveryComponent: ShareBusinessDeliveryOrderComponent;
    @ViewChild(ShareBusinessImportHouseBillDetailComponent, { static: false }) importHouseBillPopup: ShareBusinessImportHouseBillDetailComponent;
    @ViewChild('confirmSaveExistedHbl', { static: false }) confirmExistedHbl: ConfirmPopupComponent;

    jobId: string;
    shipmentDetail: CsTransaction;
    selectedHbl: any = {};
    isImport: boolean = false;

    activeTab: string = 'hawb';

    constructor(
        protected _progressService: NgProgress,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _dataService: DataService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this._activedRoute.params
            .subscribe((param: Params) => {
                if (isUUID(param.jobId)) {
                    this.jobId = param.jobId;

                    this._store.dispatch(new TransactionGetDetailAction(this.jobId));
                    this.getDetailShipment();

                    this.permissionShipments = this._store.select(getTransactionPermission);
                } else {
                    this.gotoList();
                }
            });
    }

    getDetailShipment() {
        this._store.select(getTransactionDetailCsTransactionState)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CsTransaction) => {
                    if (!!res) {
                        this.shipmentDetail = res;
                        const objArrival = {
                            arrivalNo: this.shipmentDetail.jobNo + "-A01",
                            arrivalFirstNotice: new Date()
                        };
                        this.arrivalNoteComponent.hblArrivalNote = new HBLArrivalNote(objArrival);

                        const objDelivery = {
                            deliveryOrderNo: this.shipmentDetail.jobNo + "-AL01",
                            deliveryOrderPrintedDate: { startDate: new Date(), endDate: new Date() },

                            // *  AUTO GENERATE SENT TO (1) WITH WAREHOUSENAME FROM POD
                            doheader1: !!this.shipmentDetail.warehousePOD ? this.shipmentDetail.warehousePOD.nameVn : null,
                            subAbbr: !!this.shipmentDetail.warehousePOD ? this.shipmentDetail.warehousePOD.nameAbbr : null
                        };
                        this.deliveryComponent.deliveryOrder = new DeliveryOrder(objDelivery);

                    }
                },
            );
    }

    onImport(selectedData: any) {
        this.isImport = true;
        this.selectedHbl = selectedData;
        this.selectedHbl.hwbno = null;
        this.formCreateHBLComponent.updateFormValue(this.selectedHbl);
    }

    showImportPopup() {
        const dataSearch = { jobId: this.jobId };
        dataSearch.jobId = this.jobId;
        this.importHouseBillPopup.typeFCL = 'Import';
        this.importHouseBillPopup.selected = - 1;
        this.importHouseBillPopup.getHourseBill(dataSearch);
        this.importHouseBillPopup.show();
    }

    getDataForm() {
        const form: any = this.formCreateHBLComponent.formCreate.getRawValue();
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
            arrivalDate: !!form.arrivalDate && !!form.arrivalDate.startDate ? formatDate(form.arrivalDate.startDate, 'yyyy-MM-dd', 'en') : null,
            forwardingAgentId: form.forwardingAgentId,
            pol: form.pol,
            pod: form.pod,
            warehouseId: form.warehouseId,
            route: form.route,
            flightNo: form.flightNo,
            flightDate: !!form.flightDate ? formatDate(form.flightDate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightNoOrigin: form.flightNoOrigin,
            flightDateOrigin: !!form.flightDateOrigin && form.flightDateOrigin.startDate !== undefined ? formatDate(form.flightDateOrigin.startDate, 'yyyy-MM-dd', 'en') : null,
            issueHBLDate: !!form.issueHBLDate ? formatDate(form.issueHBLDate.startDate, 'yyyy-MM-dd', 'en') : null,
            packageType: !!form.packageType && !!form.packageType.length ? form.packageType[0].id : null,
            freightPayment: !!form.freightPayment && !!form.freightPayment.length ? form.freightPayment[0].id : null,
            packageQty: form.packageQty,
            grossWeight: form.gw,
            chargeWeight: form.chargeWeight,
            poInvoiceNo: form.poinvoiceNo,
            desOfGoods: form.desOfGoods,
            finalPOD: form.finalPod,
        };

        const houseBill = new HouseBill(_merge(form, formData));
        return houseBill;
    }

    saveHBL() {
        this.confirmPopup.hide();

        this.formCreateHBLComponent.isSubmitted = true;
        this.arrivalNoteComponent.isSubmitted = true;
        this.deliveryComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.activeTab = 'hawb';
            this.infoPopup.show();
            return;
        }
        if (!this.arrivalNoteComponent.checkValidate() || !this.arrivalNoteComponent.hblArrivalNote.arrivalNo) {
            this.activeTab = 'arrival';
            this.infoPopup.show();
            return;
        }
        if (!this.deliveryComponent.deliveryOrder.deliveryOrderNo) {
            this.activeTab = 'authorize';
            this.infoPopup.show();
            return;
        }
        this._documentationRepo.checkExistedHawbNo(this.formCreateHBLComponent.hwbno.value, this.jobId, null)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    if (res) {
                        this.confirmExistedHbl.show();
                    } else {
                        const houseBill: HouseBill = this.getDataForm();
                        houseBill.jobId = this.jobId;
                        this.createHbl(houseBill);
                    }
                }
            );
    }

    confirmSaveData() {
        this.confirmExistedHbl.hide();

        const houseBill: HouseBill = this.getDataForm();
        houseBill.jobId = this.jobId;

        this.createHbl(houseBill);
    }

    checkValidateForm() {
        let valid: boolean = true;
        this.setError(this.formCreateHBLComponent.freightPayment);
        this.setError(this.formCreateHBLComponent.packageType);
        this.setError(this.formCreateHBLComponent.hbltype);

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
                            arrivalFirstNotice: !!this.arrivalNoteComponent.hblArrivalNote.arrivalFirstNotice && !!this.arrivalNoteComponent.hblArrivalNote.arrivalFirstNotice.startDate ? formatDate(this.arrivalNoteComponent.hblArrivalNote.arrivalFirstNotice.startDate, 'yyyy-MM-dd', 'en') : formatDate(new Date(), 'yyyy-MM-dd', 'en'),
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
                ).subscribe((res: CommonInterface.IResult) => {
                    if (!!res) {
                        this._toastService.success(res[1].message, '');
                        this._router.navigate([`/home/documentation/air-import/${this.jobId}/hbl/${this.arrivalNoteComponent.hblArrivalNote.hblid}`]);
                    }
                });
        }
    }

    onSelectTabAL() {
        this.deliveryComponent.deliveryOrder.doheader1 = this._dataService.getDataByKey("podName") || "";
    }

    gotoList() {
        this._router.navigate([`home/documentation/air-import/${this.jobId}/hbl`]);
    }
}
