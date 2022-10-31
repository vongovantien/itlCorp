import { HttpResponse } from "@angular/common/http";
import { PopupBase } from "@app";
import { ReportPreviewComponent } from "@common";
import { SystemConstants } from "@constants";
import { delayTime } from "@decorators";
import { TransactionTypeEnum } from "@enums";
import { ICrystalReport } from "@interfaces";
import { Crystal } from "@models";
import { AccountingRepo, DocumentationRepo, ExportRepo, SystemFileManageRepo } from "@repositories";
import { SortService } from "@services";
import { ToastrService } from "ngx-toastr";
import { of } from "rxjs";
import { concatMap, mergeMap, takeUntil } from "rxjs/operators";

export abstract class DetailCDNoteBase extends PopupBase implements ICrystalReport {
    jobId: string = null;
    transactionType: TransactionTypeEnum = 0;

    constructor(
        protected _documentRepo: DocumentationRepo,
        protected _sortService: SortService,
        protected _toastService: ToastrService,
        protected _accountantRepo: AccountingRepo,
        protected _fileMngtRepo: SystemFileManageRepo,
        protected _exportRepo: ExportRepo
    ) {
        super();
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
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
            })


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
                            templateCode: templateCode,
                            transactionType: this.utility.getTransationTypeByEnum(this.transactionType)
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
}