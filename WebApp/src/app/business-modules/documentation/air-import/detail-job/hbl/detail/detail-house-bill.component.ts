import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { ActionsSubject, Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { ConfirmPopupComponent, InfoPopupComponent, ReportPreviewComponent } from '@common';
import { ChargeConstants, RoutingConstants, SystemConstants } from '@constants';
import { delayTime } from '@decorators';
import { ICrystalReport } from '@interfaces';
import { Crystal, CsTransactionDetail, HouseBill } from '@models';
import { CatalogueRepo, DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { DataService } from '@services';

import { AirImportCreateHBLComponent } from '../create/create-house-bill.component';
import * as fromShareBussiness from './../../../../../share-business/store';

import { formatDate } from '@angular/common';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { catchError, concatMap, mergeMap, skip, switchMap, takeUntil } from 'rxjs/operators';
import isUUID from 'validator/lib/isUUID';


enum HBL_TAB {
    DETAIL = 'DETAIL',
    ARRIVAL = 'ARRIVAL',
    AUTHORIZE = 'AUTHORIZE',
    PROOF = 'PROOF'

}
@Component({
    selector: 'app-detail-hbl-air-import',
    templateUrl: './detail-house-bill.component.html',
})
export class AirImportDetailHBLComponent extends AirImportCreateHBLComponent implements OnInit, ICrystalReport {

    hblId: string;
    hblDetail: any;

    selectedTab: string = HBL_TAB.DETAIL;
    isClickSubMenu: boolean = false;
    checkPointPreview;

    constructor(
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _datService: DataService,
        private _exportRepo: ExportRepo,
        private _fileMngtRepo: SystemFileManageRepo

    ) {
        super(
            _activedRoute,
            _store,
            _documentationRepo,
            _catalogueRepo,
            _toastService,
            _actionStoreSubject,
            _router,
            _datService
        );
    }


    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.hblId && isUUID(param.hblId)) {
                this.hblId = param.hblId;
                this.jobId = param.jobId;
                this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                this._store.dispatch(new fromShareBussiness.GetDetailHBLSuccessAction(this.hblId));
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

                this.permissionHblDetail = this._store.select(fromShareBussiness.getDetailHBlPermissionState);

                this.getDetailHbl();

            } else {
                this.gotoList();
            }
        });

        this.isLocked = this._store.select(fromShareBussiness.getTransactionLocked);

        this.listenShortcutSaveHawb();

    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    getDetailHbl() {
        this._store.select(fromShareBussiness.getDetailHBlState)
            .pipe(
                skip(1),
                catchError(this.catchError),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CsTransactionDetail) => {
                    if (!!res) {
                        this.hblDetail = res;
                        this.checkPointPreview = {
                            partnerId: this.hblDetail.customerId,
                            hblId: this.hblId,
                            salesmanId: this.hblDetail.saleManId,
                            transactionType: 'DOC',
                            type: 7
                        }
                    }
                },
            );
    }

    onSaveHBLDetail() {
        switch (this.selectedTab) {
            case HBL_TAB.DETAIL:
                this.saveHBL();
                break;

            // * Update Arrival Note.
            case HBL_TAB.ARRIVAL: {
                this.arrivalNoteComponent.isSubmitted = true;
                if (!this.arrivalNoteComponent.checkValidate()) {
                    return;
                } else if (!!this.arrivalNoteComponent.hblArrivalNote.arrivalNo) {
                    this.arrivalNoteComponent.saveArrivalNote();
                }
                break;
            }
            // * Update Delivery Order.
            case HBL_TAB.AUTHORIZE: {
                this.deliveryComponent.isSubmitted = true;
                if (!!this.deliveryComponent.deliveryOrder.deliveryOrderNo) {
                    this.deliveryComponent.saveDeliveryOrder();
                } else {
                    return;
                }
                break;
            }
            // * Update Proof Of Delivery.
            case HBL_TAB.PROOF: {
                this.proofOfDeliveryComponent.saveProofOfDelivery();
                break;
            }
            default:
                break;
        }
    }

    saveHBL() {
        this.formCreateHBLComponent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: this.invalidFormText,
                title: 'Cannot update HBL'
            });
            return;
        } else {
            this._documentationRepo.checkExistedHawbNo(this.formCreateHBLComponent.hwbno.value, this.jobId, this.hblId)
                .subscribe(
                    (res: any) => {
                        if (res) {
                            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                                body: 'HAWB No has existed, do you want to continue saving?',
                                title: 'HAWB Existed'
                            }, () => { this.confirmUpdateData() });
                        } else {
                            const modelUpdate = this.getDataForm();
                            this.setDataToUpdate(modelUpdate);
                            this.updateHbl(modelUpdate);

                        }
                    }
                );
        }
    }

    confirmUpdateData() {
        const modelUpdate = this.getDataForm();
        this.setDataToUpdate(modelUpdate);
        this.updateHbl(modelUpdate);
    }

    setDataToUpdate(modelUpdate: HouseBill) {
        modelUpdate.id = this.hblId;
        modelUpdate.jobId = this.jobId;
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
        modelUpdate.subAbbr = this.hblDetail.subAbbr;
        modelUpdate.transactionType = ChargeConstants.AI_CODE;
        modelUpdate.userCreated = this.hblDetail.userCreated;

        let arrivalNote = this.arrivalNoteComponent.hblArrivalNote;
        let deliveryOrder = this.deliveryComponent.deliveryOrder;
        let proofOfDelievey = this.proofOfDeliveryComponent.proofOfDelievey;

        modelUpdate.arrivalNo = arrivalNote.arrivalNo;
        modelUpdate.arrivalFirstNotice = arrivalNote.arrivalFirstNotice.startDate ? formatDate(arrivalNote.arrivalFirstNotice.startDate, 'yyyy-MM-dd', 'en') : null;
        modelUpdate.arrivalSecondNotice = arrivalNote.arrivalSecondNotice.startDate ? formatDate(arrivalNote.arrivalSecondNotice.startDate, 'yyyy-MM-dd', 'en') : null;
        modelUpdate.arrivalFooter = arrivalNote.arrivalFooter;
        modelUpdate.arrivalHeader = arrivalNote.arrivalHeader;
        modelUpdate.dofooter = deliveryOrder.dofooter;
        modelUpdate.dosentTo1 = deliveryOrder.doheader1;
        modelUpdate.dosentTo2 = deliveryOrder.doheader2;
        modelUpdate.subAbbr = deliveryOrder.subAbbr;
        modelUpdate.deliveryOrderNo = deliveryOrder.deliveryOrderNo;
        modelUpdate.deliveryOrderPrintedDate = deliveryOrder.deliveryOrderPrintedDate.startDate ? formatDate(deliveryOrder.deliveryOrderPrintedDate.startDate, 'yyyy-MM-dd', 'en') : null;
        modelUpdate.deliveryDate = proofOfDelievey.deliveryDate.startDate ? formatDate(proofOfDelievey.deliveryDate.startDate, 'yyyy-MM-dd', 'en') : null;
        modelUpdate.referenceNoProof = proofOfDelievey.referenceNo;
        modelUpdate.note = proofOfDelievey.note;
        modelUpdate.deliveryPerson = proofOfDelievey.deliveryPerson;
    }

    updateHbl(body: any) {
        const checkPoint = {
            partnerId: body.customerId,
            salesmanId: body.saleManId,
            transactionType: 'DOC',
            type: 8,
            hblId: this.hblId
        };
        this._documentationRepo.validateCheckPointContractPartner(checkPoint)
            .pipe(
                switchMap(
                    (res: CommonInterface.IResult) => {
                        if (!res.status) {
                            this._toastService.warning(res.message);
                            return of(false);
                        }
                        return this._documentationRepo.updateHbl(body);
                    })
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                        this.arrivalNoteComponent.saveArrivalNote();

                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onSelectTab(tabName: HBL_TAB | string) {
        this.selectedTab = tabName;
        // TODO SHOULD DISPATCH HBL DETAIL ?.
        // this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
    }

    previewArrivalNotice(_currency: string) {
        this._documentationRepo.validateCheckPointContractPartner(this.checkPointPreview)
            .pipe(
                switchMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        return this._documentationRepo.previewArrivalNoticeAir({ hblId: this.hblId, currency: _currency });
                    }
                    this._toastService.warning(res.message);
                    return of(false)
                })
            ).subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport("AN");
                        } else {
                            this._toastService.warning('There is no data charge to display preview');
                        }
                    }

                },
            );
    }
    showPreviewSignature(type: string, withSign: boolean) {
        this.isClickSubMenu = false;
        if (type === 'AUTHORIZE_LETTER1') {
            this.previewAuthorizeLetter1(withSign);
        }
        if (type === 'AUTHORIZE_LETTER2') {
            this.previewAuthorizeLetter2(withSign);
        }
    }

    onPreview(type: string) {
        this.isClickSubMenu = false;
        if (type === 'ARRIVAL_ORIGINAL' || type === 'ARRIVAL_VND') {
            const _currency = type === 'ARRIVAL_VND' ? 'VND' : 'ORIGINAL';
            this.previewArrivalNotice(_currency);
        }
        if (type === 'PROOF_OF_DELIVERY') {
            this.previewProofOfDelivery();
        }
        if (type === 'DOCUMENT_RELEASE_FORM') {
            this.previewAirDocumentRelease();
        }
        // if (type === 'AUTHORIZE_LETTER1') {
        //     this.previewAuthorizeLetter1();
        // }
        // if (type === 'AUTHORIZE_LETTER2') {
        //     this.previewAuthorizeLetter2();
        // }
    }

    previewAuthorizeLetter2(withSign: boolean) {
        this._documentationRepo.validateCheckPointContractPartner(this.checkPointPreview)
            .pipe(
                switchMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        return this._documentationRepo.previewAirImportAuthorizeLetter2(this.hblId, withSign);
                    }
                    this._toastService.warning(res.message);
                    return of(false)
                })
            )
            .subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport("AL");
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );
    }
    previewAuthorizeLetter1(withSign: boolean) {
        this._documentationRepo.validateCheckPointContractPartner(this.checkPointPreview)
            .pipe(
                switchMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        return this._documentationRepo.previewAirImportAuthorizeLetter1(this.hblId, withSign);
                    }
                    this._toastService.warning(res.message);
                    return of(false)
                })
            ).subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport("AL");
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );
    }

    previewProofOfDelivery() {
        this._documentationRepo.validateCheckPointContractPartner(this.checkPointPreview)
            .pipe(
                switchMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        return this._documentationRepo.previewAirProofofDelivery(this.hblId);
                    }
                    this._toastService.warning(res.message);
                    return of(false)
                })
            ).subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport("POD");
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );
    }

    previewAirDocumentRelease() {
        this._documentationRepo.validateCheckPointContractPartner(this.checkPointPreview)
            .pipe(
                switchMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        return this._documentationRepo.previewAirDocumentRelease(this.hblId);
                    }
                    this._toastService.warning(res.message);
                    return of(false)
                })
            ).subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport('AirDocumentRelease');
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );
    }

    renderAndShowReport(templateCode: string) {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.viewContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.viewContainerRef.viewContainerRef.clear();
            });
        let sub = ((this.componentRef.instance) as ReportPreviewComponent).onConfirmEdoc
            .pipe(
                concatMap(() => this._exportRepo.exportCrystalReportPDF(this.dataReport, 'response', 'text')),
                mergeMap((res: any) => {
                    if ((res as HttpResponse<any>).status == SystemConstants.HTTP_CODE.OK) {
                        const body = {
                            url: (this.dataReport as Crystal).pathReportGenerate || null,
                            module: 'Document',
                            folder: 'Shipment',
                            objectId: this.hblDetail.jobId,
                            hblId: this.hblDetail.id,
                            templateCode: templateCode,
                            transactionType: this.hblDetail.transactionType
                        };
                        return this._fileMngtRepo.uploadPreviewTemplateEdoc([body]);
                    }
                    return of(false);
                }),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (!res) return;
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.success(res.message || "Upload fail");
                    }
                },
                (errors) => {
                    console.log("error", errors);
                },
                () => {
                    sub.unsubscribe();
                }
            );
    }

    showCreatepoup() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Save HBL',
            body: this.confirmUpdateHblText,
            labelCancel: 'No',
            labelConfirm: 'Yes'
        }, () => { this.saveHBL() });
    }

    sendMail(type: string) {
        this._documentationRepo.validateCheckPointContractPartner(this.checkPointPreview, 'false')
            .pipe(
                catchError((err: HttpErrorResponse) => {
                    if (!!err.error.message) {
                        this._toastService.error("Can not Send mail. " + err.error.message + ". Please recheck again.");
                    }
                    return throwError(err.error.message);
                })
            ).subscribe(
                (res: any) => {
                    if (res.status) {
                        switch (type) {
                            case 'ArrivalNotice':
                                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}/hbl/${this.hblId}/arrivalnotice`]);
                                break;
                            case 'AuthorizeLetter':
                                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}/hbl/${this.hblId}/authorizeletter`]);
                                break;
                            case 'POD':
                                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}/hbl/${this.hblId}/proofofdelivery`]);
                                break;
                            case 'HAWB':
                                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}/hbl/${this.hblId}/houseairwaybill`]);
                                break;
                        }
                    }
                },
            );
    }
}
