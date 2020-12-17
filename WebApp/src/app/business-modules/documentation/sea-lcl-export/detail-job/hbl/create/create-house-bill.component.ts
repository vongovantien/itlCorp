import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';

import { AppForm } from '@app';
import { InfoPopupComponent, ConfirmPopupComponent } from '@common';
import { DocumentationRepo, CatalogueRepo } from '@repositories';
import { Container, csBookingNote, CsTransactionDetail } from '@models';
import { SystemConstants, RoutingConstants } from '@constants';
import {
    ShareBusinessImportHouseBillDetailComponent,
    ShareBussinessHBLGoodSummaryLCLComponent,
    getTransactionPermission,
    ShareBusinessAttachListHouseBillComponent
} from '@share-bussiness';

import * as fromShareBussiness from './../../../../../share-business/store';
import { ShareSeaServiceFormCreateHouseBillSeaExportComponent } from 'src/app/business-modules/documentation/share-sea/components/form-create-hbl-sea-export/form-create-hbl-sea-export.component';

import { catchError, takeUntil } from 'rxjs/operators';
import isUUID from 'validator/lib/isUUID';
import groupBy from 'lodash/groupBy';

@Component({
    selector: 'app-create-hbl-lcl-export',
    templateUrl: './create-house-bill.component.html'
})

export class SeaLCLExportCreateHBLComponent extends AppForm {

    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(ConfirmPopupComponent) confirmPopup: ConfirmPopupComponent;
    @ViewChild(ShareSeaServiceFormCreateHouseBillSeaExportComponent) formCreateHBLComponent: ShareSeaServiceFormCreateHouseBillSeaExportComponent;
    @ViewChild(ShareBussinessHBLGoodSummaryLCLComponent) goodSummaryComponent: ShareBussinessHBLGoodSummaryLCLComponent;
    @ViewChild(ShareBusinessImportHouseBillDetailComponent) importHouseBillPopup: ShareBusinessImportHouseBillDetailComponent;
    @ViewChild(ShareBusinessAttachListHouseBillComponent) attachListComponent: ShareBusinessAttachListHouseBillComponent;

    jobId: string;

    containers: Container[] = [];
    selectedHbl: CsTransactionDetail;

    constructor(
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _cd: ChangeDetectorRef,
    ) {
        super();

        this._actionStoreSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: fromShareBussiness.ContainerAction) => {
                    if (action.type === fromShareBussiness.ContainerActionTypes.SAVE_CONTAINER) {
                        this.containers = action.payload;

                        if (!!this.containers) {
                            this.containers.forEach(c => {
                                c.id = c.mblid = SystemConstants.EMPTY_GUID;
                            });
                        }

                        if (this.containers.length === 0) {
                            const packageName = this.goodSummaryComponent.packages.find(x => x.id == this.goodSummaryComponent.selectedPackage).code;
                            const data = { 'package': packageName, 'quantity': this.goodSummaryComponent.packageQty };
                            this.formCreateHBLComponent.formCreate.controls["inWord"].setValue(this.handleStringPackage(data));
                        }
                        if (this.containers.length > 0) {
                            // * Update field inword with container data.
                            this.formCreateHBLComponent.formCreate.controls["inWord"].setValue(this.updateInwordField(this.containers));
                        }
                    }
                });
    }

    ngOnInit() {
        this._activedRoute.params
            .subscribe((param: Params) => {
                if (param.jobId && isUUID(param.jobId)) {
                    this.jobId = param.jobId;
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

                    this.permissionShipments = this._store.select(getTransactionPermission);
                } else {
                    this.gotoList();
                }
            });
    }

    selectedPackage($event: any) {
        const data = $event;
        this.formCreateHBLComponent.formCreate.controls["inWord"].setValue(this.handleStringPackage(data));
    }

    ngAfterViewInit() {
        this.goodSummaryComponent.initContainer();
        this.goodSummaryComponent.containerDetail = 'A PART OF CONTAINER S.T.C';

        this._cd.detectChanges();
    }

    getBookingNotes() {
        this._documentationRepo.getBookingNoteSeaLCLExport().subscribe(
            (res: csBookingNote[]) => {
                if (!!this.formCreateHBLComponent) {
                    this.formCreateHBLComponent.csBookingNotes = res || [];
                }
            }
        );
    }

    showCreatepoup() {
        this.confirmPopup.show();
    }

    onSaveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;
        this.goodSummaryComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const modelAdd = this.getDataForm();
        // this._catalogueRepo.getSalemanIdByPartnerId(modelAdd.customerId, this.jobId).subscribe((res: any) => {
        //     if (!!res.salemanId) {
        //         if (res.salemanId !== modelAdd.saleManId) {
        //             this._toastService.error('Not found contract information, please check!');
        //             return;
        //         }
        //     }
        //     if (!!res.officeNameAbbr) {
        //         this._toastService.error('The selected customer not have any agreement for service in office ' + res.officeNameAbbr + '! Please check Again', 'Cannot Create House Bill!');
        //     } else {

        //     }
        // });
        this.createHbl(modelAdd);

    }

    getDataForm() {
        const form: any = this.formCreateHBLComponent.formCreate.getRawValue();
        const formData = {
            id: SystemConstants.EMPTY_GUID,
            jobId: this.jobId,
            sailingDate: !!form.sailingDate && !!form.sailingDate.startDate ? formatDate(form.sailingDate.startDate, 'yyyy-MM-dd', 'en') : null,
            closingDate: !!form.closingDate && !!form.closingDate.startDate ? formatDate(form.closingDate.startDate, 'yyyy-MM-dd', 'en') : null,
            issueHbldate: !!form.issueHbldate && !!form.issueHbldate.startDate ? formatDate(form.issueHbldate.startDate, 'yyyy-MM-dd', 'en') : null,

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
            issueHblplace: form.issueHblplace,
            referenceNo: form.referenceNo,
            exportReferenceNo: form.exportReferenceNo,
            goodsDeliveryDescription: form.goodsDeliveryDescription,
            forwardingAgentDescription: form.forwardingAgentDescription,
            purchaseOrderNo: form.purchaseOrderNo,
            shippingMark: form.shippingMark,
            inWord: form.inWord,
            onBoardStatus: form.onBoardStatus,

            serviceType: form.serviceType,
            originBlnumber: form.originBlnumber,
            moveType: form.moveType,
            freightPayment: form.freightPayment,
            hbltype: form.hbltype,

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
            packageQty: this.goodSummaryComponent.packageQty,
            packageType: +this.goodSummaryComponent.selectedPackage,
            contSealNo: this.goodSummaryComponent.containerDescription,
            chargeWeight: this.goodSummaryComponent.totalChargeWeight,
            attachList: this.attachListComponent.attachList.replace(form.hwbno, '[[HBLNo]]'),
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
        this.importHouseBillPopup.typeTransaction = 8;
        this.importHouseBillPopup.getHourseBill(dataSearch);
        this.importHouseBillPopup.show();
    }

    checkValidateForm() {
        let valid: boolean = true;

        if (!this.formCreateHBLComponent.formCreate.valid) {
            valid = false;
        }

        if (
            this.goodSummaryComponent.grossWeight === null
            || this.goodSummaryComponent.totalCBM === null
            || this.goodSummaryComponent.packageQty === null
            || this.goodSummaryComponent.grossWeight < 0
            || this.goodSummaryComponent.totalCBM < 0
            || this.goodSummaryComponent.packageQty < 0
        ) {
            valid = false;
        }
        return valid;
    }

    createHbl(body: any) {
        this._documentationRepo.createHousebill(body)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');

                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}/hbl/${res.data}`]);
                    }
                }
            );
    }


    gotoList() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}/hbl`]);
    }

    updateInwordField(containers: Container[]) {
        console.log(containers);
        if (!containers.length) {
            return null;
        }
        let containerDetail = '';

        const contObject = (containers || []).map((container: Container) => ({
            package: container.packageTypeName,
            quantity: container.packageQuantity,
        }));

        const contData = [];
        for (const keyName of Object.keys(groupBy(contObject, 'package'))) {
            contData.push({
                package: keyName,
                quantity: groupBy(contObject, 'package')[keyName].map(i => i.quantity).reduce((a: any, b: any) => a += b),
            });
        }
        for (const item of contData) {

            containerDetail += this.handleStringPackage(item);
            if (contData.length > 1) {
                containerDetail += " AND ";
            }
        }

        if (contData.length > 1) {
            containerDetail = containerDetail.substring(0, containerDetail.length - 5);
        }

        containerDetail = containerDetail += " Only.";

        return containerDetail;
    }

    handleStringPackage(contOb: { package: string, quantity: number }) {
        return `${this.utility.convertNumberToWords(contOb.quantity)}${contOb.package}`;
    }


}
