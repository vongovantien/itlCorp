import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';

import { DocumentationRepo, ExportRepo, CatalogueRepo } from '@repositories';
import { Container } from '@models';
import { ReportPreviewComponent } from '@common';
import { ShareBussinessShipmentGoodSummaryComponent } from '@share-bussiness';
import { ChargeConstants, RoutingConstants } from '@constants';
import { ICrystalReport } from '@interfaces';
import { delayTime } from '@decorators';
import { DataService } from '@services';

import { SeaConsolImportCreateHBLComponent } from '../create/create-hbl-consol-import.component';
import * as fromShareBussiness from './../../../../../share-business/store';

import { catchError, finalize, takeUntil, skip } from 'rxjs/operators';
import isUUID from 'validator/lib/isUUID';

enum HBL_TAB {
    DETAIL = 'DETAIL',
    ARRIVAL = 'ARRIVAL',
    DELIVERY = 'DELIVERY'

}

@Component({
    selector: 'app-detail-hbl-consol-import',
    templateUrl: './detail-hbl-consol-import.component.html',
})
export class SeaConsolImportDetailHBLComponent extends SeaConsolImportCreateHBLComponent implements ICrystalReport {

    @ViewChild(ShareBussinessShipmentGoodSummaryComponent) shipmentGoodSummaryComponent: ShareBussinessShipmentGoodSummaryComponent;
    @ViewChild(ReportPreviewComponent) reportPopup: ReportPreviewComponent;

    hblId: string;
    containers: Container[] = [];
    hblDetail: any;

    selectedTab: string = HBL_TAB.DETAIL;
    isClickSubMenu: boolean = false;

    constructor(
        protected _progressService: NgProgress,
        protected _documentationRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _activedRoute: ActivatedRoute,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _store: Store<fromShareBussiness.ITransactionState>,
        private _exportRepository: ExportRepo,
        protected _cd: ChangeDetectorRef,
        protected _dataService: DataService

    ) {
        super(_progressService, _documentationRepo, _catalogueRepo, _toastService, _activedRoute, _actionStoreSubject, _router, _store, _cd, _dataService);
    }

    ngOnInit() {
        this.isLocked = this._store.select(fromShareBussiness.getTransactionLocked);
    }

    ngAfterViewInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.hblId && param.jobId && isUUID(param.hblId)) {
                this.hblId = param.hblId;
                this.jobId = param.jobId;
                this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                this.permissionHblDetail = this._store.select(fromShareBussiness.getDetailHBlPermissionState);
                this.getDetailHbl();
            } else {
                this.combackToHBLList();
            }
        });
    }

    getListContainer() {
        this._store.select<any>(fromShareBussiness.getHBLContainersState)
            .pipe(takeUntil(this.ngUnsubscribe))
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
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_CONSOL_IMPORT}/${this.hblDetail.jobId}/hbl`]);

    }

    onUpdateHblDetail() {
        this.formHouseBill.isSubmited = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        const modelUpdate: any = this.onsubmitData();

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
        modelUpdate.userCreated = this.hblDetail.userCreated;
        this._catalogueRepo.getSalemanIdByPartnerId(modelUpdate.customerId, this.jobId).subscribe((res: any) => {
            if (!!res.salemanId) {
                if (res.salemanId !== modelUpdate.saleManId) {
                    this._toastService.error('Not found contract information, please check!');
                    return;
                }
            }
            if (!!res.officeNameAbbr) {
                this._toastService.error('The selected customer not have any agreement for service in office ' + res.officeNameAbbr + '! Please check Again', 'Cannot Update House Bill!');
            } else {
                this.updateHbl(modelUpdate);
            }
        });
    }

    updateHbl(body: any) {
        this._progressRef.start();
        body.transactionType = ChargeConstants.SCI_CODE;
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
                        // this.formHouseBill.getListSaleman();
                        this.formHouseBill.updateDataToForm(this.hblDetail);

                        // * Dispatch to save containers.
                        // this._store.dispatch(new fromShareBussiness.SaveContainerAction(this.hblDetail.csMawbcontainers || []));
                        this._store.dispatch(new fromShareBussiness.GetContainersHBLSuccessAction(this.hblDetail.csMawbcontainers || []));


                        // * Get container to update model
                        this.getListContainer();
                    }
                },
            );
    }

    onSelectTab(tabName: HBL_TAB | string) {
        this.selectedTab = tabName;
    }

    onPreview(type: string) {
        this.isClickSubMenu = false;

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
                    if (this.dataReport.dataSource.length > 0) {
                        this.showReport();
                    }
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
                        this.showReport();
                    } else {
                        this._toastService.warning('There is no data charge to display preview');
                    }
                },
            );
    }
    previewDeliveryOrder() {
        if (this.hblDetail.deliveryOrderNo === null) {
            this._toastService.warning('There is no delivery order information. You must save delivery order information');
            return;
        }
        this._documentationRepo.previewDeliveryOrder(this.hblId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport.dataSource.length > 0) {
                        this.showReport();
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
                    this.downLoadFile(res, "application/ms-excel", "Dangerous Goods.xlsx");
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

    @delayTime(1000)
    showReport(): void {
        this.reportPopup.frm.nativeElement.submit();
        this.reportPopup.show();
    }
}
