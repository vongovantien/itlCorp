import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { formatDate } from '@angular/common';

import { AppForm } from 'src/app/app.form';
import { InfoPopupComponent, ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { SeaFCLExportFormCreateHBLComponent } from '../components/form-create/form-create-hbl.component';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { ShareBussinessShipmentGoodSummaryComponent } from 'src/app/business-modules/share-business/components/shipment-good-summary/shipment-good-summary.component';
import { Container } from 'src/app/shared/models/document/container.model';
import { SystemConstants } from 'src/constants/system.const';

import { skip, catchError, finalize, takeUntil } from 'rxjs/operators';

import * as fromShareBussiness from './../../../../../share-business/store';
import { ShareBusinessImportHouseBillDetailComponent } from 'src/app/business-modules/share-business';


@Component({
    selector: 'app-create-hbl-fcl-export',
    templateUrl: './create-house-bill.component.html'
})

export class SeaFCLExportCreateHBLComponent extends AppForm {

    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;
    @ViewChild(SeaFCLExportFormCreateHBLComponent, { static: false }) formCreateHBLComponent: SeaFCLExportFormCreateHBLComponent;
    @ViewChild(ShareBussinessShipmentGoodSummaryComponent, { static: false }) goodSummaryComponent: ShareBussinessShipmentGoodSummaryComponent;
    @ViewChild(ShareBusinessImportHouseBillDetailComponent, { static: false }) importHouseBillPopup: ShareBusinessImportHouseBillDetailComponent;
    jobId: string;
    containers: Container[] = [];
    selectedHbl: any = {}; // TODO model.

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
        this._activedRoute.params
            .subscribe((param: Params) => {
                if (param.jobId) {
                    this.jobId = param.jobId;
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                }
            });

    }

    ngAfterViewInit() {
        this.importHouseBillPopup.typeFCL = 'Export';
        this.goodSummaryComponent.initContainer();
        this.goodSummaryComponent.containerPopup.isAdd = true;

        this._cd.detectChanges();
    }

    showCreatepoup() {

        this.confirmPopup.show();
    }

    onSaveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const modelAdd = this.getDataForm();
        this.createHbl(modelAdd);

    }

    getDataForm() {
        const form: any = this.formCreateHBLComponent.formCreate.getRawValue();
        const formData = {
            id: SystemConstants.EMPTY_GUID,
            jobId: this.jobId,
            sailingDate: !!form.sailingDate && !!form.sailingDate.startDate ? formatDate(form.sailingDate.startDate, 'yyyy-MM-dd', 'en') : null,
            closingDate: !!form.closingDate && !!form.closingDate.startDate ? formatDate(form.closingDate.startDate, 'yyyy-MM-dd', 'en') : null,

            mawb: form.mawb,

            shipperDescription: form.shipperDescription,
            consigneeDescription: form.consigneeDescription,
            notifyPartyDescription: form.notifyPartyDescription,
            hwbno: form.hwbno,
            customsBookingNo: form.bookingNo,
            localVoyNo: form.localVoyNo,
            oceanVoyNo: form.oceanVoyNo,
            pickupPlace: form.placeReceipt,
            deliveryPlace: form.placeDelivery,
            finalDestinationPlace: form.finalDestinationPlace,
            placeFreightPay: form.placeFreightPay,
            issueHblplaceAndDate: form.issueHblplaceAndDate,
            issueHbldate: new Date(),
            issueHblplace: form.issueHblplaceAndDate,
            referenceNo: form.referenceNo,
            exportReferenceNo: form.exportReferenceNo,
            goodsDeliveryDescription: form.goodsDeliveryDescription,
            forwardingAgentDescription: form.forwardingAgentDescription,
            purchaseOrderNo: form.purchaseOrderNo || null,
            shippingMark: form.shippingMark,
            inWord: form.inWord,
            onBoardStatus: form.onBoardStatus,

            serviceType: !!form.serviceType ? form.serviceType[0].id : null,
            originBlnumber: !!form.originBlnumber ? form.originBlnumber[0].id : null,
            moveType: !!form.moveType ? form.moveType[0].id : null,
            freightPayment: !!form.freightPayment ? form.freightPayment[0].id : null,
            hbltype: !!form.hbltype ? form.hbltype[0].id : null,

            customerId: form.customer,
            saleManId: form.saleMan,
            shipperId: form.shipper,
            consigneeId: form.consignee,
            notifyPartyId: form.notifyParty,
            originCountryId: form.country,
            pol: form.pol,
            pod: form.pod,
            forwardingAgentId: form.forwardingAgent,
            goodsDeliveryId: form.goodsDelivery,

            // * containers summary
            csMawbcontainers: this.containers,
            commodity: this.goodSummaryComponent.commodities,
            packageContainer: this.goodSummaryComponent.containerDetail,
            desOfGoods: this.goodSummaryComponent.description,
            cbm: this.goodSummaryComponent.totalCBM,
            grossWeight: this.goodSummaryComponent.grossWeight,
            netWeight: this.goodSummaryComponent.netWeight,
        };

        return formData;
    }

    onImport(selectedData: any) {
        this.selectedHbl = selectedData;
        if (!!this.selectedHbl) {
            this.formCreateHBLComponent.onUpdateDataToImport(this.selectedHbl);
        }
    }


    showImportPopup() {
        const dataSearch = { jobId: this.jobId };
        dataSearch.jobId = this.jobId;
        this.importHouseBillPopup.typeFCL = 'Export';
        this.importHouseBillPopup.selected = - 1;
        this.importHouseBillPopup.getHourseBill(dataSearch);
        this.importHouseBillPopup.show();

    }


    checkValidateForm() {
        let valid: boolean = true;

        this.setError(this.formCreateHBLComponent.serviceType);
        this.setError(this.formCreateHBLComponent.moveType);
        this.setError(this.formCreateHBLComponent.originBlnumber);

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
        this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}/hbl`]);
    }
}
