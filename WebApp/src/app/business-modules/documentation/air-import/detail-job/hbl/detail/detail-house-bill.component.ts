import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { AirImportCreateHBLComponent } from '../create/create-house-bill.component';
import { CsTransactionDetail } from '@models';
import { Crystal } from 'src/app/shared/models/report/crystal.model';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { DocumentationRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { skip, catchError, takeUntil, finalize } from 'rxjs/operators';

import * as fromShareBussiness from './../../../../../share-business/store';
import { ReportPreviewComponent } from '@common';
import { ShareBusinessArrivalNoteComponent, ShareBusinessDeliveryOrderComponent } from '@share-bussiness';
enum HBL_TAB {
    DETAIL = 'DETAIL',
    ARRIVAL = 'ARRIVAL',
    AUTHORIZE = 'AUTHORIZE'

}
@Component({
    selector: 'app-detail-hbl-air-import',
    templateUrl: './detail-house-bill.component.html',
})
export class AirImportDetailHBLComponent extends AirImportCreateHBLComponent implements OnInit {
    @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;
    @ViewChild(ShareBusinessArrivalNoteComponent, { static: false }) arrivalNoteComponent: ShareBusinessArrivalNoteComponent;
    @ViewChild(ShareBusinessDeliveryOrderComponent, { static: false }) deliveryComponent: ShareBusinessDeliveryOrderComponent;
    hblId: string;

    hblDetail: any;

    dataReport: Crystal;

    selectedTab: string = HBL_TAB.DETAIL;
    isClickSubMenu: boolean = false;
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
        super(
            _progressService,
            _activedRoute,
            _store,
            _documentationRepo,
            _toastService,
            _actionStoreSubject,
            _router,
            _cd
        );
    }

    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.hblId) {
                this.hblId = param.hblId;
                this.jobId = param.jobId;
                this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));


                this.getDetailHbl();

            } else {
                // TODO handle error. 
            }
        });

        this.isLocked = this._store.select(fromShareBussiness.getTransactionLocked);
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

    onSaveHBLDetail() {
        switch (this.selectedTab) {
            case HBL_TAB.DETAIL:
                this.saveHBL();
                break;

            // * Update Arrival Note.    
            case HBL_TAB.ARRIVAL: {
                this.confirmPopup.hide();
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
            case HBL_TAB.AUTHORIZE: {
                this.confirmPopup.hide();
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


    saveHBL() {
        this.confirmPopup.hide();
        this.formCreateHBLComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const modelUpdate = this.getDataForm();
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
                        this._router.navigate([`/home/documentation/air-import/${this.jobId}/hbl`]);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    preview(reportType: string) {
        this._documentationRepo.previewSeaHBLOfLanding(this.hblId, reportType)
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

    onSelectTab(tabName: HBL_TAB | string) {
        this.selectedTab = tabName;
    }

    previewArrivalNotice(_currency: string) {
        this._documentationRepo.previewArrivalNoticeAir({ hblId: this.hblId, currency: _currency })
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

    onPreview(type: string) {
        this.isClickSubMenu = false;
        // Preview Arrival Notice
        if (type === 'ARRIVAL_ORIGINAL' || type === 'ARRIVAL_VND') {
            const _currency = type === 'ARRIVAL_VND' ? 'VND' : 'ORIGINAL';
            this.previewArrivalNotice(_currency);
        }
    }
}
