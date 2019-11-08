import { Component, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { AppForm } from 'src/app/app.form';
import { formatDate } from '@angular/common';
import { catchError, takeUntil } from 'rxjs/operators';
import { finalize } from 'rxjs/internal/operators/finalize';
import { FormAddHouseBillComponent } from '../components/form-add-house-bill/form-add-house-bill.component';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { FCLImportAddModel } from 'src/app/shared/models';
import { ActionsSubject, Store } from '@ngrx/store';
import * as fromStore from '../../../store/index';
import { InfoPopupComponent, ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { SeaFCLImportShipmentGoodSummaryComponent } from '../../../components/shipment-good-summary/shipment-good-summary.component';
import { ImportHouseBillDetailComponent } from '../popup/import-house-bill-detail/import-house-bill-detail.component';

@Component({
    selector: 'app-create-house-bill',
    templateUrl: './create-house-bill.component.html',
    styleUrls: ['./create-house-bill.component.scss']
})
export class CreateHouseBillComponent extends AppForm {
    @ViewChild(FormAddHouseBillComponent, { static: false }) formHouseBill: FormAddHouseBillComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmCreatePopup: ConfirmPopupComponent;
    @ViewChild(SeaFCLImportShipmentGoodSummaryComponent, { static: false }) shipmentGoodSummaryComponent: SeaFCLImportShipmentGoodSummaryComponent;
    @ViewChild(ImportHouseBillDetailComponent, { static: false }) importHouseBillPopup: ImportHouseBillDetailComponent;

    fclImportAddModel: FCLImportAddModel = new FCLImportAddModel();
    jobId: string = '';
    shipmentDetail: any = {};

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
    }
    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.id) {
                this.jobId = param.id;
            }
        });
        this._actionStoreSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: fromStore.ContainerAction) => {
                    if (action.type === fromStore.ContainerActionTypes.SAVE_CONTAINER) {
                        this.fclImportAddModel.csMawbcontainers = [];
                        this.fclImportAddModel.csMawbcontainers = action.payload;


                        console.log("list container add success", this.fclImportAddModel.csMawbcontainers);
                    }
                });

        this._store.select(fromStore.seaFCLImportTransactionState)
            .subscribe(
                (res: any) => {
                    console.log(res);
                    this.shipmentDetail = res;
                }
            );
    }

    ngAfterViewInit() {
        this.shipmentGoodSummaryComponent.initContainer();
        this.formHouseBill.mtBill.setValue(this.shipmentDetail.mawb);

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
        if (this.formHouseBill.PortChargeLikePortLoading === true) {
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
            id: "00000000-0000-0000-0000-000000000000",
            jobId: this.jobId,
            mawb: this.formHouseBill.mtBill.value,
            saleManId: !!this.formHouseBill.selectedSaleman.value ? this.formHouseBill.selectedSaleman.value : null,
            shipperId: !!this.formHouseBill.selectedShipper.value ? this.formHouseBill.selectedShipper.value : null,
            shipperDescription: this.formHouseBill.shipperdescriptionModel,
            consigneeId: this.formHouseBill.selectedConsignee.value,
            consigneeDescription: this.formHouseBill.consigneedescriptionModel,
            notifyPartyId: !!this.formHouseBill.selectedNotifyParty.value ? this.formHouseBill.selectedNotifyParty.value : null,
            notifyPartyDescription: this.formHouseBill.notifyPartyModel,
            alsoNotifyPartyId: !!this.formHouseBill.selectedAlsoNotifyParty.value ? this.formHouseBill.selectedAlsoNotifyParty.value : null,
            alsoNotifyPartyDescription: this.formHouseBill.alsoNotifyPartyDescriptionModel,
            hwbno: this.formHouseBill.hwbno.value,
            hbltype: this.formHouseBill.hbltype.value.value,
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
            documentDate: !!this.formHouseBill.documentDate.value ? formatDate(this.formHouseBill.documentDate.value.startDate !== undefined ? this.formHouseBill.documentDate.value.startDate : this.formHouseBill.documentDate.value, 'yyyy-MM-dd', 'en') : null,
            documentNo: this.formHouseBill.documentNo.value,
            etawarehouse: !!this.formHouseBill.etawarehouse.value.startDate ? formatDate(this.formHouseBill.etawarehouse.value.startDate !== undefined ? this.formHouseBill.etawarehouse.value.startDate : this.formHouseBill.etawarehouse.value, 'yyyy-MM-dd', 'en') : null,
            warehouseNotice: this.formHouseBill.warehouseNotice.value,
            shippingMark: this.formHouseBill.shippingMark.value,
            remark: this.formHouseBill.remark.value,
            issueHBLPlace: !!this.formHouseBill.selectedPlaceOfIssued.value ? this.formHouseBill.selectedPlaceOfIssued.value : null,
            issueHBLDate: !!this.formHouseBill.issueHBLDate.value ? formatDate(this.formHouseBill.issueHBLDate.value.startDate !== undefined ? this.formHouseBill.issueHBLDate.value.startDate : this.formHouseBill.issueHBLDate.value, 'yyyy-MM-dd', 'en') : null,
            originBLNumber: this.formHouseBill.originBLNumber.value.value,
            referenceNo: this.formHouseBill.referenceNo.value,
            customerId: this.formHouseBill.selectedCustomer.value,
            oceanVoyNo: this.formHouseBill.oceanVoyNo.value,
            csMawbcontainers: this.fclImportAddModel.csMawbcontainers,
            commodity: this.shipmentGoodSummaryComponent.commodities,
            packageContainer: this.shipmentGoodSummaryComponent.containerDetail,
            desOfGoods: this.shipmentGoodSummaryComponent.description,
            cbm: this.shipmentGoodSummaryComponent.totalCBM,
            grossWeight: this.shipmentGoodSummaryComponent.grossWeight,
            netWeight: this.shipmentGoodSummaryComponent.netWeight
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
}
