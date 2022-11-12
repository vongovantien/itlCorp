import { HttpResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnInit, ViewChild } from '@angular/core';
import { PopupBase } from '@app';
import { ReportPreviewComponent } from '@common';
import { SystemConstants } from '@constants';
import { delayTime } from '@decorators';
import { InjectViewContainerRefDirective } from '@directives';
import { ICrystalReport } from '@interfaces';
import { Crystal, CsTransactionDetail } from '@models';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { concatMap, mapTo, mergeMap, startWith, switchMap, takeUntil } from 'rxjs/operators';

@Component({
    selector: 'app-menu-preview-hbl-sea-export',
    templateUrl: './menu-preview-hbl-sea-export.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShareSeaServiceMenuPreviewHBLSeaExportComponent extends PopupBase implements OnInit, ICrystalReport {
    @Input() hblDetail: CsTransactionDetail;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;
    templateCode: string;

    constructor(
        private readonly _documentationRepo: DocumentationRepo,
        private readonly _cd: ChangeDetectorRef,
        private readonly _export: ExportRepo,
        private readonly _toastService: ToastrService,
        private readonly _fileMngtRepo: SystemFileManageRepo) {
        super();
    }

    ngOnInit(): void { }

    preview(reportType: string) {
        const checkPoint = {
            partnerId: this.hblDetail.customerId,
            hblId: this.hblDetail.id,
            salesmanId: this.hblDetail.saleManId,
            transactionType: 'DOC',
            type: 7
        };
        this._documentationRepo.validateCheckPointContractPartner(checkPoint)
            .pipe(
                switchMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        return this._documentationRepo.previewSeaHBLOfLanding(this.hblDetail.id, reportType);
                    }
                    this._toastService.warning(res.message);
                    return of(false);
                })
            )
            .subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.templateCode = 'HBL';
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }

                },
            );
    }

    renderAndShowReport() {
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
                concatMap(() => this._export.exportCrystalReportPDF(this.dataReport, 'response', 'text')),
                mergeMap((res: any) => {
                    if ((res as HttpResponse<any>).status == SystemConstants.HTTP_CODE.OK) {
                        const body = {
                            url: (this.dataReport as Crystal).pathReportGenerate || null,
                            module: 'Document',
                            folder: 'Shipment',
                            objectId: this.hblDetail.jobId,
                            hblId: this.hblDetail.id,
                            templateCode: this.templateCode,
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

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.ShowWithDelay();
        this._cd.detectChanges(); // * Mark cd detect to refresh instance Popup.
    }

    previewAttachList() {
        const checkPoint = {
            partnerId: this.hblDetail.customerId,
            hblId: this.hblDetail.id,
            salesmanId: this.hblDetail.saleManId,
            settlementCode: null,
            transactionType: 'DOC',
            type: 7
        };
        this._documentationRepo.validateCheckPointContractPartner(checkPoint)
            .pipe(
                switchMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        return this._documentationRepo.previewAirAttachList(this.hblDetail.id);
                    }
                    this._toastService.warning(res.message);
                    return of(false);
                })
            ).subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.templateCode = 'AT';
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }

                },
            );
    }
}
