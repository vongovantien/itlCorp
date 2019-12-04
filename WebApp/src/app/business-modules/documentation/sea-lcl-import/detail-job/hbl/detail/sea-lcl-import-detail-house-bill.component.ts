import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';

import { SeaLCLImportCreateHouseBillComponent } from '../create/sea-lcl-import-create-house-bill.component';
import { DocumentationRepo, ExportRepo } from 'src/app/shared/repositories';
import { Container } from 'src/app/shared/models/document/container.model';
import { InfoPopupComponent } from 'src/app/shared/common/popup';
import { Crystal } from 'src/app/shared/models/report/crystal.model';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { ShareBussinessShipmentGoodSummaryComponent } from 'src/app/business-modules/share-business/components/shipment-good-summary/shipment-good-summary.component';

import { catchError, finalize, takeUntil, skip } from 'rxjs/operators';

import * as fromShareBussiness from '../../../../../share-business/store';
import { ShareBusinessFormCreateHouseBillImportComponent, ShareBusinessArrivalNoteComponent, ShareBusinessDeliveryOrderComponent } from 'src/app/business-modules/share-business';

enum HBL_TAB {
    DETAIL = 'DETAIL',
    ARRIVAL = 'ARRIVAL',
    DELIVERY = 'DELIVERY'

}

@Component({
    selector: 'sea-lcl-import-detail-house-bill',
    templateUrl: './sea-lcl-import-detail-house-bill.component.html',
})
export class SeaLCLImportDetailHouseBillComponent extends SeaLCLImportCreateHouseBillComponent {

    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ShareBusinessFormCreateHouseBillImportComponent, { static: false }) formHouseBill: ShareBusinessFormCreateHouseBillImportComponent;
    @ViewChild(ShareBussinessShipmentGoodSummaryComponent, { static: false }) shipmentGoodSummaryComponent: ShareBussinessShipmentGoodSummaryComponent;
    @ViewChild(ShareBusinessArrivalNoteComponent, { static: false }) arrivalNoteComponent: ShareBusinessArrivalNoteComponent;
    @ViewChild(ShareBusinessDeliveryOrderComponent, { static: false }) deliveryComponent: ShareBusinessDeliveryOrderComponent;
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
        protected _store: Store<fromShareBussiness.ITransactionState>,
        private _exportRepository: ExportRepo
    ) {
        super(_progressService, _documentationRepo, _toastService, _activedRoute, _actionStoreSubject, _router, _store);
    }

    ngOnInit() {
    }

    ngAfterViewInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.hblId && param.jobId) {
                this.hblId = param.hblId;
                this.jobId = param.jobId;
                this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));

                this.getDetailHbl();

            } else {
                // TODO handle error. 
            }
        });
    }

    getListContainer() {
        this._store.select<any>(fromShareBussiness.getContainerSaveState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (containers: any) => {
                    this.containers = containers || [];
                }
            );
    }

    onSaveHBLDetail() {
        switch (this.selectedTab) {
            case HBL_TAB.DETAIL:
                this.onUpdateHblDetail();
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
        this._router.navigate([`/home/documentation/sea-lcl-import/${this.hblDetail.jobId}/hbl`]);

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
        modelUpdate.consigneeDescription = this.formHouseBill.consigneeDescription.value;
        modelUpdate.shipperDescription = this.formHouseBill.shipperDescription.value;
        modelUpdate.notifyPartyDescription = this.formHouseBill.notifyPartyDescription.value;
        modelUpdate.alsoNotifyPartyDescription = this.formHouseBill.alsonotifyPartyDescription.value;
        modelUpdate.arrivalFirstNotice = this.hblDetail.arrivalFirstNotice;
        modelUpdate.arrivalFooter = this.hblDetail.arrivalFooter;
        modelUpdate.arrivalHeader = this.hblDetail.arrivalHeader;
        modelUpdate.arrivalNo = this.hblDetail.arrivalNo;
        modelUpdate.arrivalSecondNotice = this.hblDetail.arrivalSecondNotice;
        modelUpdate.deliveryOrderNo = this.hblDetail.deliveryOrderNo;
        modelUpdate.deliveryOrderPrintedDate = this.hblDetail.deliveryOrderPrintedDate;
        modelUpdate.dofooter = this.hblDetail.dofooter;
        modelUpdate.dosentTo1 = this.hblDetail.dosentTo1;
        modelUpdate.dosentTo2 = this.hblDetail.dosentTo2;

        this.updateHbl(modelUpdate);
    }

    updateHbl(body: any) {
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

    getDetailHbl() {
        this.formHouseBill.isDetail = true;
        this._progressRef.start();
        this._store.select(fromShareBussiness.getDetailHBlState)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    this._progressRef.complete();
                    if (!!res) {
                        this.hblDetail = res;

                        this.formHouseBill.etd.setValue(this.hblDetail.etd);
                        !!this.hblDetail.etd ? this.formHouseBill.etd.setValue({ startDate: new Date(this.hblDetail.etd), endDate: new Date(this.hblDetail.etd) }) : this.formHouseBill.etd.setValue(null), // * Date;
                            this.formHouseBill.getListSaleman();
                        this.formHouseBill.mtBill.setValue(this.hblDetail.mawb);
                        this.formHouseBill.shipperDescription.setValue(this.hblDetail.shipperDescription);
                        this.formHouseBill.consigneeDescription.setValue(this.hblDetail.consigneeDescription);
                        this.formHouseBill.notifyPartyDescription.setValue(this.hblDetail.notifyPartyDescription);
                        this.formHouseBill.alsonotifyPartyDescription.setValue(this.hblDetail.alsoNotifyPartyDescription);
                        this.formHouseBill.hwbno.setValue(this.hblDetail.hwbno);
                        this.formHouseBill.pickupPlace.setValue(this.hblDetail.pickupPlace);
                        !!this.hblDetail.eta ? this.formHouseBill.eta.setValue({ startDate: new Date(this.hblDetail.eta), endDate: new Date(this.hblDetail.eta) }) : this.formHouseBill.eta.setValue(null), // * Date;
                            this.formHouseBill.finalDestinationPlace.setValue(this.hblDetail.finalDestinationPlace);
                        this.formHouseBill.shipper.setValue(this.hblDetail.shipperId);

                        this.formHouseBill.localVessel.setValue(this.hblDetail.localVessel);
                        this.formHouseBill.localVoyNo.setValue(this.hblDetail.localVoyNo);
                        this.formHouseBill.oceanVessel.setValue(this.hblDetail.oceanVessel);
                        this.formHouseBill.oceanVoyNo.setValue(this.hblDetail.oceanVoyNo);
                        !!this.hblDetail.documentDate ? this.formHouseBill.documentDate.setValue({ startDate: new Date(this.hblDetail.documentDate), endDate: new Date(this.hblDetail.documentDate) }) : this.formHouseBill.documentDate.setValue(null), // * Date;
                            this.formHouseBill.documentNo.setValue(this.hblDetail.documentNo);
                        !!this.hblDetail.etawarehouse ? this.formHouseBill.etawarehouse.setValue({ startDate: new Date(this.hblDetail.etawarehouse), endDate: new Date(this.hblDetail.etawarehouse) }) : this.formHouseBill.etawarehouse.setValue(null), // * Date;
                            this.formHouseBill.warehouseNotice.setValue(this.hblDetail.warehouseNotice);
                        this.formHouseBill.shippingMark.setValue(this.hblDetail.shippingMark);
                        this.formHouseBill.remark.setValue(this.hblDetail.remark);
                        !!this.hblDetail.issueHbldate ? this.formHouseBill.issueHBLDate.setValue({ startDate: new Date(this.hblDetail.issueHbldate), endDate: new Date(this.hblDetail.issueHbldate) }) : this.formHouseBill.issueHBLDate.setValue(null), // * Date;
                            this.formHouseBill.referenceNo.setValue(this.hblDetail.referenceNo);
                        this.formHouseBill.originBLNumber.setValue(this.formHouseBill.numberOfOrigins.filter(i => i.value === this.hblDetail.originBlnumber)[0]);
                        this.formHouseBill.mindateEta = !!this.formHouseBill.mindateEta ? this.createMoment(this.hblDetail.etd) : null;
                        this.formHouseBill.mindateEtaWareHouse = !!this.hblDetail.eta ? this.createMoment(this.hblDetail.eta) : null;
                        this.formHouseBill.saleMan.setValue(this.hblDetail.saleManId);
                        this.formHouseBill.customer.setValue(this.hblDetail.customerId);
                        this.formHouseBill.shipper.setValue(this.hblDetail.shipperId);
                        this.formHouseBill.consignee.setValue(this.hblDetail.consigneeId);
                        this.formHouseBill.notifyParty.setValue(this.hblDetail.notifyPartyId);
                        this.formHouseBill.alsoNotifyParty.setValue(this.hblDetail.alsoNotifyPartyId);
                        this.formHouseBill.pol.setValue(this.hblDetail.pol);
                        this.formHouseBill.pod.setValue(this.hblDetail.pod);
                        this.formHouseBill.supplier.setValue(this.hblDetail.coloaderId);
                        this.formHouseBill.placeOfIssues.setValue(this.hblDetail.issueHblplace);
                        this.formHouseBill.servicetype.setValue([<CommonInterface.INg2Select>{ id: this.hblDetail.serviceType, text: this.hblDetail.serviceType }]);
                        this.formHouseBill.hbltype.setValue([<CommonInterface.INg2Select>{ id: this.hblDetail.hbltype, text: this.hblDetail.hbltype }]);
                    }

                    // * Dispatch to save containers.
                    this._store.dispatch(new fromShareBussiness.SaveContainerAction(this.hblDetail.csMawbcontainers || []));

                    // * Get container to update model
                    this.getListContainer();

                },
            );
    }

    onSelectTab(tabName: HBL_TAB | string) {
        this.selectedTab = tabName;
    }

    onPreview(type: string) {
        // Preview Delivery Order
        if (type === 'DELIVERY_ORDER') {
            this.previewDeliveryOrder();
        }

        // Preview Arrival Notice
        if (type === 'ARRIVAL_ORIGINAL' || type === 'ARRIVAL_VND') {
            const _currency = type === 'ARRIVAL_VND' ? 'VND' : 'ORIGINAL';
            this.previewArrivalNotice(_currency);
        }

        // PREVIEW PROOF OF DELIVERY
        if (type === 'PROOF_OF_DELIVERY') {
            this.previewProofOfDelivery();
        }
        if (type === 'E_MANIFEST') {
            this.exportEManifest();
        }
        if (type === 'GOODS_DECLARE') {
            this.exportGoodsDeclare();
        }
        if (type === 'DANGEROUS_GOODS') {
            this.exportDangerousGoods();
        }
    }
    previewProofOfDelivery() {
        this._documentationRepo.previewProofofDelivery(this.hblId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    setTimeout(() => {
                        this.reportPopup.frm.nativeElement.submit();
                        this.reportPopup.show();
                    }, 1000);

                },
            );
    }
    previewArrivalNotice(_currency: string) {
        this._documentationRepo.previewArrivalNotice({ hblId: this.hblId, currency: _currency })
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }
    previewDeliveryOrder() {
        this._documentationRepo.previewDeliveryOrder(this.hblId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no container data to display preview');
                    }
                },
            );
    }
    exportDangerousGoods() {
        this._exportRepository.exportDangerousGoods(this.hblId)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "Goods Declare.xlsx");
                },
            );
    }

    exportGoodsDeclare() {
        this._exportRepository.exportGoodDeclare(this.hblId)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "Goods Declare.xlsx");
                },
            );
    }

    exportEManifest() {
        this._exportRepository.exportEManifest(this.hblId)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "E-Manifest.xlsx");
                },
            );
    }
}
