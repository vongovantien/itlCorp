import { Component, ViewChild } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { formatDate } from '@angular/common';
import { ActionsSubject, Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { DocumentationRepo } from 'src/app/shared/repositories';
import { AppForm } from 'src/app/app.form';
import { FormAddHouseBillComponent } from '../components/form-add-house-bill/form-add-house-bill.component';
import { InfoPopupComponent, ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ImportHouseBillDetailComponent } from '../popup/import-house-bill-detail/import-house-bill-detail.component';
import { Container } from 'src/app/shared/models/document/container.model';
import { SystemConstants } from 'src/constants/system.const';
import { ShareBussinessShipmentGoodSummaryComponent } from 'src/app/business-modules/share-business/components/shipment-good-summary/shipment-good-summary.component';

import { finalize } from 'rxjs/internal/operators/finalize';
import { catchError, takeUntil } from 'rxjs/operators';

import * as fromStore from '../../../store';
import * as fromShareBussiness from './../../../../../share-business/store';


@Component({
    selector: 'app-create-house-bill',
    templateUrl: './create-house-bill.component.html',
})
export class CreateHouseBillComponent extends AppForm {
    @ViewChild(FormAddHouseBillComponent, { static: false }) formHouseBill: FormAddHouseBillComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmCreatePopup: ConfirmPopupComponent;
    @ViewChild(ShareBussinessShipmentGoodSummaryComponent, { static: false }) shipmentGoodSummaryComponent: ShareBussinessShipmentGoodSummaryComponent;
    @ViewChild(ImportHouseBillDetailComponent, { static: false }) importHouseBillPopup: ImportHouseBillDetailComponent;

    jobId: string = '';

    shipmentDetail: any = {}; // TODO model.
    selectedHbl: any = {}; // TODO model.

    containers: Container[] = [];

    constructor(
        protected _progressService: NgProgress,
        protected _documentationRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _activedRoute: ActivatedRoute,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _store: Store<fromStore.ISeaFCLImportState>,


    ) {
        super();
        this._progressRef = this._progressService.ref();

        this._actionStoreSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: fromShareBussiness.ContainerAction) => {
                    if (action.type === fromShareBussiness.ContainerActionTypes.SAVE_CONTAINER) {

                        this.containers = action.payload;
                    }
                });
    }

    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.id) {
                this.jobId = param.id;
            }
        });


        this._store.select(fromStore.seaFCLImportTransactionState)
            .subscribe(
                (res: any) => {
                    this.shipmentDetail = res;
                }
            );
    }

    ngAfterViewInit() {
        this.shipmentGoodSummaryComponent.initContainer();
        this.shipmentGoodSummaryComponent.containerPopup.isAdd = true;

        this.formHouseBill.mtBill.setValue(this.shipmentDetail.mawb);
        if (this.shipmentDetail.typeOfService != null) {
            this.formHouseBill.servicetype.setValue(this.formHouseBill.serviceTypes.filter(i => i.value === this.shipmentDetail.typeOfService)[0]);

        }

    }

    checkValidateForm() {
        if (this.formHouseBill.selectedPortOfLoading.value !== undefined && this.formHouseBill.selectedPortOfDischarge.value !== undefined) {
            if (this.formHouseBill.selectedPortOfLoading.value === this.formHouseBill.selectedPortOfDischarge.value) {
                this.formHouseBill.PortChargeLikePortLoading = true;
            } else {
                this.formHouseBill.PortChargeLikePortLoading = false;
            }
        }
        let valid: boolean = true;
        if (!this.formHouseBill.formGroup.valid) {
            valid = false;
        }
        if (this.formHouseBill.PortChargeLikePortLoading === true || this.formHouseBill.selectedSaleman === null) {
            valid = false;
        }
        return valid;
    }

    oncreate() {
        this.confirmCreatePopup.hide();
        this.formHouseBill.isSubmited = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
        }
        else {
            const body = this.onsubmitData();
            this.createHbl(body);
        }

    }
    onImport(selectedData: any) {
        this.selectedHbl = selectedData;

        console.log(this.selectedHbl);
        if (!!this.selectedHbl) {
            this.formHouseBill.mtBill.setValue(this.selectedHbl.mawb);
            this.formHouseBill.consigneeDescription.setValue(this.selectedHbl.consigneeDescription);
            this.formHouseBill.shipperDescription.setValue(this.selectedHbl.shipperDescription);
            this.formHouseBill.notifyPartyDescription.setValue(this.selectedHbl.notifyPartyDescription);
            this.formHouseBill.referenceNo.setValue(this.selectedHbl.referenceNo);
            this.formHouseBill.localVessel.setValue(this.selectedHbl.localVessel);
            this.formHouseBill.localVoyNo.setValue(this.selectedHbl.localVoyNo);
            this.formHouseBill.oceanVessel.setValue(this.selectedHbl.oceanVessel);
            this.formHouseBill.oceanVoyNo.setValue(this.selectedHbl.oceanVoyNo);



            this.formHouseBill.originBLNumber.setValue(this.formHouseBill.numberOfOrigins.filter(i => i.value === this.selectedHbl.originBlnumber)[0]);
            this.formHouseBill.alsonotifyPartyDescription.setValue(this.selectedHbl.alsoNotifyPartyDescription);
            this.formHouseBill.selectedCustomer = { field: 'id', value: this.selectedHbl.customerId };
            this.formHouseBill.bindSalemanImport(this.selectedHbl.saleManId);
            this.formHouseBill.selectedShipper = { field: 'id', value: this.selectedHbl.shipperId };
            this.formHouseBill.selectedConsignee = { field: 'id', value: this.selectedHbl.consigneeId };
            this.formHouseBill.selectedNotifyParty = { field: 'id', value: this.selectedHbl.notifyPartyId };
            this.formHouseBill.selectedPortOfLoading = { field: 'id', value: this.selectedHbl.pol };
            this.formHouseBill.selectedPortOfDischarge = { field: 'id', value: this.selectedHbl.pod };
            this.formHouseBill.selectedAlsoNotifyParty = { field: 'id', value: this.selectedHbl.alsoNotifyPartyId };
            this.formHouseBill.hbltype.setValue(this.formHouseBill.hbOfladingTypes.filter(i => i.value === this.selectedHbl.hbltype)[0]);
            this.formHouseBill.selectedSupplier = { field: 'id', value: this.selectedHbl.coloaderId };

        }
    }

    showCreatePpoup() {
        this.confirmCreatePopup.show();
    }

    showImportPopup() {
        this.importHouseBillPopup.show();
    }

    combackToHBLList() {
        this._router.navigate([`/home/documentation/sea-fcl-import/${this.jobId}/hbl`]);

    }

    createHbl(body: any) {

        if (this.formHouseBill.formGroup.valid) {
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
                        } else {

                        }
                    }
                );
        }

    }

    onsubmitData() {
        const body: ITransactionDetail = {
            id: SystemConstants.EMPTY_GUID,
            jobId: this.jobId,
            mawb: this.formHouseBill.mtBill.value,
            saleManId: !!this.formHouseBill.selectedSaleman.id ? this.formHouseBill.selectedSaleman.id : null,
            shipperId: !!this.formHouseBill.selectedShipper.value ? this.formHouseBill.selectedShipper.value : null,
            shipperDescription: this.formHouseBill.shipperdescriptionModel !== undefined ? this.formHouseBill.shipperdescriptionModel : this.formHouseBill.shipperDescription.value,
            consigneeId: this.formHouseBill.selectedConsignee.value,
            consigneeDescription: this.formHouseBill.consigneedescriptionModel !== undefined ? this.formHouseBill.consigneedescriptionModel : this.formHouseBill.consigneeDescription.value,
            notifyPartyId: !!this.formHouseBill.selectedNotifyParty.value ? this.formHouseBill.selectedNotifyParty.value : null,
            notifyPartyDescription: this.formHouseBill.notifyPartyModel !== undefined ? this.formHouseBill.notifyPartyModel : this.formHouseBill.notifyPartyDescription.value,
            alsoNotifyPartyId: !!this.formHouseBill.selectedAlsoNotifyParty.value ? this.formHouseBill.selectedAlsoNotifyParty.value : null,
            alsoNotifyPartyDescription: this.formHouseBill.alsoNotifyPartyDescriptionModel !== undefined ? this.formHouseBill.alsoNotifyPartyDescriptionModel : this.formHouseBill.alsonotifyPartyDescription.value,
            hwbno: this.formHouseBill.hwbno.value,
            hbltype: this.formHouseBill.hbltype.value != null ? this.formHouseBill.hbltype.value.value : null,
            servicetype: this.formHouseBill.servicetype.value != null ? this.formHouseBill.servicetype.value.value : null,
            etd: !!this.formHouseBill.etd.value ? formatDate(this.formHouseBill.etd.value.startDate !== undefined ? this.formHouseBill.etd.value.startDate : this.formHouseBill.etd.value, 'yyyy-MM-dd', 'en') : null,
            eta: !!this.formHouseBill.eta.value ? formatDate(this.formHouseBill.eta.value.startDate !== undefined ? this.formHouseBill.eta.value.startDate : this.formHouseBill.eta.value, 'yyyy-MM-dd', 'en') : null,
            pickupPlace: this.formHouseBill.pickupPlace.value,
            pol: this.formHouseBill.selectedPortOfLoading.value,
            pod: this.formHouseBill.selectedPortOfDischarge.value,
            finalDestinationPlace: this.formHouseBill.finalDestinationPlace.value,
            coloaderId: this.formHouseBill.selectedNotifyParty.value !== undefined ? this.formHouseBill.selectedSupplier.value : null,
            localVessel: this.formHouseBill.localVessel.value,
            localVoyNo: this.formHouseBill.localVoyNo.value,
            oceanVessel: this.formHouseBill.oceanVessel.value,
            documentDate: !!this.formHouseBill.documentDate.value && this.formHouseBill.documentDate.value.startDate != null ? formatDate(this.formHouseBill.documentDate.value.startDate !== undefined ? this.formHouseBill.documentDate.value.startDate : this.formHouseBill.documentDate.value, 'yyyy-MM-dd', 'en') : null,
            documentNo: this.formHouseBill.documentNo.value,
            etawarehouse: !!this.formHouseBill.etawarehouse.value && this.formHouseBill.etawarehouse.value.startDate != null ? formatDate(this.formHouseBill.etawarehouse.value.startDate !== undefined ? this.formHouseBill.etawarehouse.value.startDate : this.formHouseBill.etawarehouse.value, 'yyyy-MM-dd', 'en') : null,
            warehouseNotice: this.formHouseBill.warehouseNotice.value,
            shippingMark: this.formHouseBill.shippingMark.value,
            remark: this.formHouseBill.remark.value,
            issueHBLPlace: !!this.formHouseBill.selectedPlaceOfIssued.value ? this.formHouseBill.selectedPlaceOfIssued.value : null,
            issueHBLDate: !!this.formHouseBill.issueHBLDate.value && this.formHouseBill.issueHBLDate.value.startDate != null ? formatDate(this.formHouseBill.issueHBLDate.value.startDate !== undefined ? this.formHouseBill.issueHBLDate.value.startDate : this.formHouseBill.issueHBLDate.value, 'yyyy-MM-dd', 'en') : null,
            originBLNumber: this.formHouseBill.originBLNumber.value.value,
            referenceNo: this.formHouseBill.referenceNo.value,
            customerId: this.formHouseBill.selectedCustomer.value,
            oceanVoyNo: this.formHouseBill.oceanVoyNo.value,
            csMawbcontainers: this.containers,
            commodity: this.shipmentGoodSummaryComponent.commodities,
            packageContainer: this.shipmentGoodSummaryComponent.containerDetail,
            desOfGoods: this.shipmentGoodSummaryComponent.description,
            cbm: this.shipmentGoodSummaryComponent.totalCBM,
            grossWeight: this.shipmentGoodSummaryComponent.grossWeight,
            netWeight: this.shipmentGoodSummaryComponent.netWeight,

            arrivalSecondNotice: null,
            arrivalNo: null,
            arrivalHeader: null,
            arrivalFooter: null,
            arrivalFirstNotice: null,
            deliveryOrderNo: null,
            deliveryOrderPrintedDate: null,
            dofooter: null,
            dosentTo1: null,
            dosentTo2: null,
        };
        return body;
    }

}


export interface ITransactionDetail {
    id: string;
    jobId: string;
    mawb: string;
    saleManId: string;
    oceanVoyNo: string;
    shipperId: string;
    shipperDescription: string;
    consigneeId: string;
    consigneeDescription: string;
    notifyPartyId: string;
    notifyPartyDescription: string;
    alsoNotifyPartyId: string;
    alsoNotifyPartyDescription: string;
    hwbno: string;
    hbltype: string;
    servicetype: string;
    etd: string;
    eta: string;
    pickupPlace: string;
    pol: string;
    pod: string;
    finalDestinationPlace: string;
    coloaderId: string;
    localVessel: string;
    localVoyNo: string;
    oceanVessel: string;
    documentDate: string;
    documentNo: string;
    etawarehouse: string;
    warehouseNotice: string;
    shippingMark: string;
    remark: string;
    issueHBLPlace: string;
    issueHBLDate: string;
    originBLNumber: number;
    referenceNo: string;
    customerId: string;
    csMawbcontainers: any[];
    commodity: string;
    packageContainer: string;
    desOfGoods: string;
    cbm: number;
    grossWeight: number;
    netWeight: number;
    arrivalSecondNotice: string;
    arrivalNo: string;
    arrivalHeader: string;
    arrivalFooter: string;
    arrivalFirstNotice: string;
    deliveryOrderNo: string;
    deliveryOrderPrintedDate: string;
    dofooter: string;
    dosentTo1: string;
    dosentTo2: string;
}
