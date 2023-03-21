import { HttpResponse } from '@angular/common/http';
import { Directive, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmPopupComponent, ReportPreviewComponent, SubHeaderComponent } from '@common';
import { JobConstants, RoutingConstants, SystemConstants } from '@constants';
import { delayTime } from '@decorators';
import { Crystal, CsTransaction } from '@models';
import { Store } from '@ngrx/store';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import moment from 'moment';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { catchError, concatMap, finalize, mergeMap, takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import * as fromShareBussiness from '../../store';
type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL' | 'FILES' | 'ADVANCE-SETTLE';

@Directive()
export abstract class ShareJobDetailComponent extends AppForm {

    @ViewChild(SubHeaderComponent) headerComponent: SubHeaderComponent;

    jobId: string;
    ACTION: CommonType.ACTION_FORM | string = 'UPDATE';
    shipmentDetail: CsTransaction;
    params: any;
    selectedTab: TAB | string = 'SHIPMENT';
    maxDateAta: any = moment();
    maxDateAtd: any = moment();
    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _documenRepo: DocumentationRepo,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _exportRepo: ExportRepo,
        protected _fileMngtRepo: SystemFileManageRepo

    ) {
        super();
    }

    getDetailShipment(jobId: string) {
        this._documenRepo.getDetailTransaction(jobId)
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.shipmentDetail = res;
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailSuccessAction(res));
                    }
                },
            );
    }

    onFinishJob() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Do you want to finish this shipment ?',
            labelConfirm: 'Yes'
        }, () => {
            this.handleChangeStatusJob(JobConstants.FINISH);
        })
    }

    onReopenJob() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Do you want to reopen this shipment ?',
            labelConfirm: 'Yes'
        }, () => {
            this.handleChangeStatusJob(JobConstants.REOPEN);
        })

    }

    handleChangeStatusJob(status: string) {
        let body: any = {
            jobId: this.jobId,
            transactionType: JobConstants.CSTRANSACTION,
            status
        }
        this._documenRepo.updateStatusJob(body).pipe(
            catchError(this.catchError),
            finalize(() => {
                this._progressRef.complete();
            })
        ).subscribe(
            (r: CommonInterface.IResult) => {
                if (r.status) {
                    this.getDetailShipment(this.jobId);
                    this._toastService.success(r.message);
                } else {
                    this._toastService.error(r.message);
                }
            },
        );
    }



    gotoList() {
        this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.shipmentDetail?.transactionType)}`]);
    }

    handleConfirmSaveAttachment(tempalteCode: string) {
        let sub = ((this.componentRef.instance) as ReportPreviewComponent).onConfirmEdoc
            .pipe(
                concatMap(() => this._exportRepo.exportCrystalReportPDF(this.dataReport, 'response', 'text')),
                mergeMap((res: any) => {
                    if ((res as HttpResponse<any>).status == SystemConstants.HTTP_CODE.OK) {
                        const body = {
                            url: (this.dataReport as Crystal).pathReportGenerate || null,
                            module: 'Document',
                            folder: 'Shipment',
                            objectId: this.jobId,
                            hblId: SystemConstants.EMPTY_GUID,
                            templateCode: tempalteCode || 'AT', // * other
                            transactionType: this.shipmentDetail.transactionType
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

        this.handleConfirmSaveAttachment(templateCode);
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    previewPLsheet(currency: string) {
        this._documenRepo.previewSIFPLsheet(this.jobId, SystemConstants.EMPTY_GUID, currency)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        this.renderAndShowReport("PLSheet");
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewShipmentCoverPage() {
        this._documenRepo.previewShipmentCoverPage(this.jobId)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        this.renderAndShowReport("CoverPage");
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

}
