import { AppList } from "src/app/app.list";
import { Component, ViewChild } from "@angular/core";
import { NgProgress } from "@ngx-progressbar/core";
import { ToastrService } from "ngx-toastr";
import { DocumentationRepo } from "@repositories";
import { catchError, finalize } from "rxjs/operators";
import { Crystal } from "@models";
import { ReportPreviewComponent } from "@common";
import { CommonEnum } from "@enums";

@Component({
    selector: 'app-sale-report',
    templateUrl: './sale-report.component.html',
})
export class SaleReportComponent extends AppList {
    @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;
    dataReport: Crystal;

    constructor(
        private _progressService: NgProgress,
        private _toastService: ToastrService,
        private _documentationRepo: DocumentationRepo
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
    }

    onSearchSaleReport(data: ReportInterface.ISaleReportCriteria) {
        console.log(data);
        switch (data.typeReport) {
            case CommonEnum.SALE_REPORT_TYPE.SR_MONTHLY:
                this.previewMonthlyReport(data);
                break;
            case CommonEnum.SALE_REPORT_TYPE.SR_DEPARTMENT:
                this.previewDepartmentReport(data);
                break;
            case CommonEnum.SALE_REPORT_TYPE.SR_QUARTER:
                this.previewQuaterReport(data);
                break;
            case CommonEnum.SALE_REPORT_TYPE.SR_SUMMARY:
                this.previewSummaryReport(data);
                break;
            case CommonEnum.SALE_REPORT_TYPE.SR_COMBINATION:
                this.previewCombinationStatictisReport(data);
                break;
        }
    }
    previewMonthlyReport(data: ReportInterface.ISaleReportCriteria) {

        this._documentationRepo.previewSaleMonthlyReport(data)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res != null && res.dataSource.length > 0) {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            setTimeout(() => {
                                this.reportPopup.frm.nativeElement.submit();
                                this.reportPopup.show();
                            }, 1000);
                        }
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewQuaterReport(data: ReportInterface.ISaleReportCriteria) {
        this._documentationRepo.previewSaleQuaterReport(data)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res != null && res.dataSource.length > 0) {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            setTimeout(() => {
                                this.reportPopup.frm.nativeElement.submit();
                                this.reportPopup.show();
                            }, 1000);
                        }
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewDepartmentReport(data: ReportInterface.ISaleReportCriteria) {
        this._documentationRepo.previewSaleDepartmentReport(data)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res != null && res.dataSource.length > 0) {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            setTimeout(() => {
                                this.reportPopup.frm.nativeElement.submit();
                                this.reportPopup.show();
                            }, 1000);
                        }
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewSummaryReport(data: ReportInterface.ISaleReportCriteria) {
        this._documentationRepo.previewSaleSummaryReport(data)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res != null && res.dataSource.length > 0) {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            setTimeout(() => {
                                this.reportPopup.frm.nativeElement.submit();
                                this.reportPopup.show();
                            }, 1000);
                        }
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewCombinationStatictisReport(data: ReportInterface.ISaleReportCriteria) {
        this._documentationRepo.previewSaleSummaryReport(data)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res != null && res.dataSource.length > 0) {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            setTimeout(() => {
                                this.reportPopup.frm.nativeElement.submit();
                                this.reportPopup.show();
                            }, 1000);
                        }
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }


}