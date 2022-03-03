import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { DocumentationRepo, ExportRepo, CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';

import { CsTransactionDetail, HouseBill } from '@models';
import { ReportPreviewComponent } from '@common';
import * as fromShareBussiness from '@share-bussiness';
import { SystemConstants, ChargeConstants, RoutingConstants } from '@constants';
import { ICrystalReport } from '@interfaces';
import { delayTime } from '@decorators';

import { InputBookingNotePopupComponent } from '../components/input-booking-note/input-booking-note.popup';
import { AirExportCreateHBLComponent } from '../create/create-house-bill.component';

import { merge } from 'rxjs';
import { catchError, takeUntil, skip, tap } from 'rxjs/operators';
import isUUID from 'validator/lib/isUUID';
import { formatDate } from '@angular/common';
import { HttpResponse } from '@angular/common/http';

@Component({
    selector: 'app-detail-hbl-air-export',
    templateUrl: './detail-house-bill.component.html',
})
export class AirExportDetailHBLComponent extends AirExportCreateHBLComponent implements OnInit, ICrystalReport {
    @ViewChild(ReportPreviewComponent) reportPopup: ReportPreviewComponent;
    @ViewChild(InputBookingNotePopupComponent) inputBookingNotePopupComponent: InputBookingNotePopupComponent;

    hblId: string;
    hblDetail: CsTransactionDetail;

    constructor(
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _exportRepo: ExportRepo,
    ) {
        super(
            _activedRoute,
            _store,
            _documentationRepo,
            _catalogueRepo,
            _toastService,
            _actionStoreSubject,
            _router,
        );
    }

    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (isUUID(param.hblId)) {
                this.hblId = param.hblId;
                this.jobId = param.jobId;

                this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                this._store.dispatch(new fromShareBussiness.GetDimensionHBLAction(this.hblId));
                this._store.dispatch(new fromShareBussiness.GetHBLOtherChargeAction(this.hblId));
                this.permissionHblDetail = this._store.select(fromShareBussiness.getDetailHBlPermissionState);
                this.getDetailHbl();
            } else {
                this.gotoList();
            }
        });
        this.isLocked = this._store.select(fromShareBussiness.getTransactionLocked);


        // * Shortcut
        //#region --- Shortcut ---
        merge(
            this.createShortcut(['ControlLeft', 'KeyI']),
            this.createShortcut(['ControlRight', 'KeyI'])
        ).pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                () => {
                    this.preview('LASTEST_ITL_FRAME');
                }
            );

        this.listenShortcutSaveHawb();

        //#endregion --- Shortcut ---
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
                    }
                },
            );
    }

    saveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        } else {
            this._documentationRepo.checkExistedHawbNoAirExport(this.formCreateHBLComponent.hwbno.value, this.jobId, this.hblId)
                .pipe(
                    catchError(this.catchError),
                )
                .subscribe(
                    (res: any) => {
                        if (!!res && res.length > 0) {
                            this.infoPopupHbl.class = 'bg-danger';
                            let jobNo = '';
                            res.forEach(element => {
                                jobNo += element + '<br>';
                            });
                            this.infoPopupHbl.body = 'Cannot save HB! Hawb no existed in the following job: ' + jobNo;
                            this.infoPopupHbl.show();
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
        this.confirmExistedHbl.hide();
        const modelUpdate = this.getDataForm();

        this.setDataToUpdate(modelUpdate);
        this.updateHbl(modelUpdate);
    }


    setDataToUpdate(modelUpdate: HouseBill) {
        modelUpdate.otherCharges = this.formCreateHBLComponent.otherCharges;

        modelUpdate.otherCharges.forEach(c => {
            c.jobId = this.jobId;
            c.hblId = this.hblId;
        });

        modelUpdate.id = this.hblId;
        modelUpdate.jobId = this.jobId;
        modelUpdate.transactionType = ChargeConstants.AE_CODE;
        modelUpdate.userCreated = this.hblDetail.userCreated;

        for (const dim of modelUpdate.dimensionDetails) {
            dim.hblid = this.hblId;
            // dim.mblId = this.jobId;
        }
    }

    updateHbl(body: any, isSeparate?: boolean) {
        const house = this.setProofOfDelivery(body);
        const deliveryDate = {
            deliveryDate: !!this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate && !!this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate.startDate ? formatDate(this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate.startDate, 'yyyy-MM-dd', 'en') : this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate.startDate == null ? null : this.proofOfDeliveryComponent.proofOfDelievey.deliveryDate,
        };
        house.deliveryDate = deliveryDate;
        this._documentationRepo.updateHbl(Object.assign({}, house, deliveryDate))
            .pipe(
                tap(() => {
                    if (this.proofOfDeliveryComponent.fileList !== null && this.proofOfDeliveryComponent.fileList.length !== 0 && this.proofOfDeliveryComponent.files === null) {
                        this.proofOfDeliveryComponent.uploadFilePOD();
                    }
                }),
                catchError(this.catchError),
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    preview(reportType: string, separateId?: string) {
        const id = !separateId ? this.hblId : separateId;
        this._documentationRepo.previewHouseAirwayBillLastest(id, reportType)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport.dataSource.length > 0) {
                        this.showReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewAttachList() {
        this._documentationRepo.previewAirAttachList(this.hblId)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport.dataSource.length > 0) {
                        this.showReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    exportNeutralHawb() {
        const userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this._exportRepo.exportHawbAirwayBill(this.hblId, userLogged.officeId)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (response: HttpResponse<any>) => {
                    if (response!=null) {
                        this.downLoadFile(response.body, "application/ms-excel", response.headers.get('efms-file-name'));
                    } else {
                        this._toastService.warning('There is no neutral hawb data to print', '');
                    }
                },
            );
    }

    gotoSeparate() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}/hbl/${this.hblId}/separate`]);
    }

    openInputBookingNote(reportType: string) {
        this.inputBookingNotePopupComponent.reportType = reportType;
        this.inputBookingNotePopupComponent.hblId = this.hblId;
        this.inputBookingNotePopupComponent.bindingFormBN(this.hblDetail);
        this.inputBookingNotePopupComponent.show();
    }

    @delayTime(1000)
    showReport(): void {
        this.reportPopup.frm.nativeElement.submit();
        this.reportPopup.show();
    }
}
