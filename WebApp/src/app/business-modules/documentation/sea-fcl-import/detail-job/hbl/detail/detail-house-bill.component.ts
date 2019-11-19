import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';

import { FormAddHouseBillComponent } from '../components/form-add-house-bill/form-add-house-bill.component';
import { CreateHouseBillComponent } from '../create/create-house-bill.component';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { Container } from 'src/app/shared/models/document/container.model';
import { SeaFCLImportShipmentGoodSummaryComponent } from '../../../components/shipment-good-summary/shipment-good-summary.component';
import { InfoPopupComponent } from 'src/app/shared/common/popup';
import { SeaFClImportArrivalNoteComponent } from '../components/arrival-note/arrival-note.component';

import { catchError, finalize, takeUntil, skip } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import * as fromStore from './../../../store';
import moment from 'moment/moment';
import { SeaFClImportDeliveryOrderComponent } from '../components/delivery-order/delivery-order.component';
import { Crystal } from 'src/app/shared/models/report/crystal.model';
import { ReportPreviewComponent } from 'src/app/shared/common';


enum HBL_TAB {
    DETAIL = 'DETAIL',
    ARRIVAL = 'ARRIVAL',
    DELIVERY = 'DELIVERY'

}

@Component({
    selector: 'app-detail-house-bill',
    templateUrl: './detail-house-bill.component.html',
})
export class DetailHouseBillComponent extends CreateHouseBillComponent {

    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(FormAddHouseBillComponent, { static: false }) formHouseBill: FormAddHouseBillComponent;
    @ViewChild(SeaFCLImportShipmentGoodSummaryComponent, { static: false }) shipmentGoodSummaryComponent: SeaFCLImportShipmentGoodSummaryComponent;
    @ViewChild(SeaFClImportArrivalNoteComponent, { static: false }) arrivalNoteComponent: SeaFClImportArrivalNoteComponent;
    @ViewChild(SeaFClImportDeliveryOrderComponent, { static: false }) deliveryComponent: SeaFClImportDeliveryOrderComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;


    hblId: string;
    containers: Container[] = [];
    hblDetail: any; // TODO model here!!
    dataReport: Crystal;

    selectedTab: string = HBL_TAB.DETAIL;

    constructor(
        protected _progressService: NgProgress,
        protected _documentationRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _activedRoute: ActivatedRoute,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _store: Store<fromStore.ISeaFCLImportState>,
    ) {
        super(_progressService, _documentationRepo, _toastService, _activedRoute, _actionStoreSubject, _router, _store);
    }

    ngAfterViewInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.hblId) {
                this.hblId = param.hblId;
                this.jobId = param.id;

                this._store.dispatch(new fromStore.GetDetailHBLAction(this.hblId));

                this.getDetailHbl();

            } else {
                // TODO handle error. 
            }
        });
    }

    getListContainer() {
        this._store.select<any>(fromStore.getContainerSaveState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (containers: any) => {
                    this.containers = containers || [];
                }
            );
    }

    updateHbl(body: any) {
        switch (this.selectedTab) {
            case HBL_TAB.DETAIL:
                if (this.formHouseBill.formGroup.valid) {
                    this._progressRef.start();
                    this._documentationRepo.updateHbl(body)
                        .pipe(
                            catchError(this.catchError),
                            finalize(() => this._progressRef.complete())
                        )
                        .subscribe(
                            (res: CommonInterface.IResult) => {
                                if (res.status) {
                                    this._toastService.success(res.message);
                                } else {
                                    this._toastService.error(res.message);
                                }
                            }
                        );
                }
                break;

            // * Update Arrival Note.    
            case HBL_TAB.ARRIVAL: {
                this.arrivalNoteComponent.isSubmitted = true;
                if (!this.arrivalNoteComponent.checkValidate()) {
                    return;
                } else if (!!this.arrivalNoteComponent.hblArrivalNote.arrivalNo) {
                    this.arrivalNoteComponent.saveArrivalNote();
                } else {
                    return;
                }
                break;
            }
            // * Update Delivery Order.
            case HBL_TAB.DELIVERY: {
                this.deliveryComponent.isSubmitted = true;
                if (!!this.deliveryComponent.deliveryOrder.deliveryOrderNo) {
                    this.deliveryComponent.saveDeliveryOrder();
                } else {
                    return;
                }
                break;
            }
            default:
                break;
        }
    }

    combackToHBLList() {
        this._router.navigate([`/home/documentation/sea-fcl-import/${this.hblDetail.jobId}/hbl`]);

    }

    onUpdateHblDetail() {
        this.formHouseBill.isSubmited = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        const modelUpdate = this.onsubmitData();
        modelUpdate.jobId = this.hblDetail.jobId;
        modelUpdate.id = this.hblDetail.id;
        modelUpdate.consigneeDescription = !!this.formHouseBill.selectedConsignee.data ? this.formHouseBill.selectedConsignee.data.partnerNameEn + "\n" +
            this.formHouseBill.selectedConsignee.data.addressShippingEn + "\n" +
            "Tel: " + this.formHouseBill.selectedConsignee.data.tel + "\n" +
            "Fax: " + this.formHouseBill.selectedConsignee.data.fax + "\n" : this.hblDetail.consigneeDescription;
        modelUpdate.shipperDescription = !!this.formHouseBill.selectedShipper.data ? this.formHouseBill.selectedShipper.data.partnerNameEn + "\n" +
            this.formHouseBill.selectedShipper.data.addressShippingEn + "\n" +
            "Tel: " + this.formHouseBill.selectedShipper.data.tel + "\n" +
            "Fax: " + this.formHouseBill.selectedShipper.data.fax + "\n" : this.hblDetail.shipperDescription;
        modelUpdate.notifyPartyDescription = !!this.formHouseBill.selectedNotifyParty.data ? this.formHouseBill.selectedNotifyParty.data.partnerNameEn + "\n" +
            this.formHouseBill.selectedNotifyParty.data.addressShippingEn + "\n" +
            "Tel: " + this.formHouseBill.selectedNotifyParty.data.tel + "\n" +
            "Fax: " + this.formHouseBill.selectedNotifyParty.data.fax + "\n" : this.hblDetail.notifyPartyDescription;

        modelUpdate.alsoNotifyPartyDescription = !!this.formHouseBill.selectedAlsoNotifyParty.data ? this.formHouseBill.selectedAlsoNotifyParty.data.partnerNameEn + "\n" +
            this.formHouseBill.selectedAlsoNotifyParty.data.addressShippingEn + "\n" +
            "Tel: " + this.formHouseBill.selectedAlsoNotifyParty.data.tel + "\n" +
            "Fax: " + this.formHouseBill.selectedAlsoNotifyParty.data.fax + "\n" : this.hblDetail.alsoNotifyPartyDescription;

        this.updateHbl(modelUpdate);
    }

    getDetailHbl() {
        this.formHouseBill.isDetail = true;
        this._progressRef.start();
        this._store.select(fromStore.getHBLState)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    this._progressRef.complete();
                    if (res.status) {
                        this.hblDetail = res.data;
                        this.shipmentGoodSummaryComponent.containerDetail = this.hblDetail.packageContainer;
                        this.shipmentGoodSummaryComponent.commodities = this.hblDetail.commodity;
                        this.shipmentGoodSummaryComponent.description = this.hblDetail.desOfGoods;
                        this.shipmentGoodSummaryComponent.grossWeight = this.hblDetail.grossWeight;
                        this.shipmentGoodSummaryComponent.netWeight = this.hblDetail.netWeight;
                        this.shipmentGoodSummaryComponent.totalChargeWeight = this.hblDetail.chargeWeight;
                        this.shipmentGoodSummaryComponent.totalCBM = this.hblDetail.cbm;
                        this.formHouseBill.etd.setValue(res.data.etd);
                        !!this.hblDetail.etd ? this.formHouseBill.etd.setValue({ startDate: new Date(this.hblDetail.etd), endDate: new Date(this.hblDetail.etd) }) : this.formHouseBill.etd.setValue(null), // * Date;
                            this.formHouseBill.getListSaleman();
                        this.formHouseBill.mtBill.setValue(res.data.mawb);
                        this.formHouseBill.shipperDescription.setValue(res.data.shipperDescription);
                        this.formHouseBill.consigneeDescription.setValue(res.data.consigneeDescription);
                        this.formHouseBill.notifyPartyDescription.setValue(res.data.notifyPartyDescription);
                        this.formHouseBill.alsonotifyPartyDescription.setValue(res.data.alsoNotifyPartyDescription);

                        this.formHouseBill.hwbno.setValue(res.data.hwbno);
                        this.formHouseBill.pickupPlace.setValue(res.data.pickupPlace);
                        // this.formHouseBill.eta.setValue(res.data.eta);
                        !!this.hblDetail.eta ? this.formHouseBill.eta.setValue({ startDate: new Date(this.hblDetail.eta), endDate: new Date(this.hblDetail.eta) }) : this.formHouseBill.eta.setValue(null), // * Date;
                            this.formHouseBill.finalDestinationPlace.setValue(res.data.finalDestinationPlace);

                        this.formHouseBill.selectedShipper = { field: 'shortName', value: res.data.shipperId };
                        this.formHouseBill.hbltype.setValue(this.formHouseBill.hbOfladingTypes.filter(i => i.value === res.data.hbltype)[0]);
                        this.formHouseBill.servicetype.setValue(this.formHouseBill.serviceTypes.filter(i => i.value === res.data.serviceType)[0]);
                        this.formHouseBill.localVessel.setValue(res.data.localVessel);
                        this.formHouseBill.localVoyNo.setValue(res.data.localVoyNo);
                        this.formHouseBill.oceanVessel.setValue(res.data.oceanVessel);
                        this.formHouseBill.oceanVoyNo.setValue(res.data.oceanVoyNo);
                        !!this.hblDetail.documentDate ? this.formHouseBill.documentDate.setValue({ startDate: new Date(this.hblDetail.documentDate), endDate: new Date(this.hblDetail.documentDate) }) : this.formHouseBill.documentDate.setValue(null), // * Date;

                            this.formHouseBill.documentNo.setValue(res.data.documentNo);
                        !!this.hblDetail.etawarehouse ? this.formHouseBill.etawarehouse.setValue({ startDate: new Date(this.hblDetail.etawarehouse), endDate: new Date(this.hblDetail.etawarehouse) }) : this.formHouseBill.etawarehouse.setValue(null), // * Date;
                            this.formHouseBill.warehouseNotice.setValue(res.data.warehouseNotice);
                        this.formHouseBill.shippingMark.setValue(res.data.shippingMark);
                        this.formHouseBill.remark.setValue(res.data.remark);
                        !!this.hblDetail.issueHbldate ? this.formHouseBill.issueHBLDate.setValue({ startDate: new Date(this.hblDetail.issueHbldate), endDate: new Date(this.hblDetail.issueHbldate) }) : this.formHouseBill.issueHBLDate.setValue(null), // * Date;

                            this.formHouseBill.referenceNo.setValue(res.data.referenceNo);
                        this.formHouseBill.originBLNumber.setValue(this.formHouseBill.numberOfOrigins.filter(i => i.value === res.data.originBlnumber)[0]);
                        this.formHouseBill.mindateEta = !!this.formHouseBill.mindateEta ? moment(this.hblDetail.etd) : null;
                        this.formHouseBill.mindateEtaWareHouse = !!this.hblDetail.eta ? moment(this.hblDetail.eta) : null;

                        setTimeout(() => {
                            this.formHouseBill.saleMans.forEach(item => {
                                if (item.id === this.hblDetail.saleManId) {
                                    this.formHouseBill.selectedSaleman = item;
                                    return;
                                }
                            });
                            this.formHouseBill.selectedCustomer = { field: 'id', value: res.data.customerId };
                            this.formHouseBill.selectedShipper = { field: 'id', value: res.data.shipperId };
                            this.formHouseBill.selectedConsignee = { field: 'id', value: res.data.consigneeId };
                            this.formHouseBill.selectedNotifyParty = { field: 'id', value: res.data.notifyPartyId };
                            this.formHouseBill.selectedAlsoNotifyParty = { field: 'id', value: res.data.alsoNotifyPartyId };
                            this.formHouseBill.selectedPortOfLoading = { field: 'id', value: res.data.pol };
                            this.formHouseBill.selectedPortOfDischarge = { field: 'id', value: res.data.pod };
                            this.formHouseBill.selectedSupplier = { field: 'id', value: res.data.coloaderId };
                            this.formHouseBill.selectedPlaceOfIssued = { field: 'id', value: res.data.issueHblplace };
                        }, 500);
                    }

                    // * Dispatch to save containers.
                    this._store.dispatch(new fromStore.SaveContainerAction(res.data.csMawbcontainers || []));

                    // * Dispatch to get container's shipment.
                    this._store.dispatch(new fromStore.GetContainerAction({ mblid: this.jobId }));

                    // * Get container to update model
                    this.getListContainer();

                },
            );
    }

    onSelectTab(tabName: HBL_TAB | string) {
        this.selectedTab = tabName;
    }
    onPreview() {
        if (this.selectedTab === HBL_TAB.DELIVERY) {
            this._documentationRepo.previewDeliveryOrder(this.hblId)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { })
                )
                .subscribe(
                    (res: any) => {
                        this.dataReport = res;
                        setTimeout(() => {
                            this.reportPopup.show();
                        }, 1000);

                    },
                );
        }
        if (this.selectedTab === HBL_TAB.DETAIL) {
            this._documentationRepo.previewProofofDelivery(this.hblDetail.id)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { })
                )
                .subscribe(
                    (res: any) => {
                        this.dataReport = res;
                        setTimeout(() => {
                            this.reportPopup.show();
                        }, 1000);

                    },
                );
        }

    }
}
