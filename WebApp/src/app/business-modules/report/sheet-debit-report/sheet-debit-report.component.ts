import { Component } from "@angular/core";
import { AppList } from "src/app/app.list";
import { NgProgress } from "@ngx-progressbar/core";
import { ToastrService } from "ngx-toastr";
import { ExportRepo } from "@repositories";
import { CommonEnum } from "@enums";
import { catchError, finalize } from "rxjs/operators";

@Component({
    selector: 'app-sheet-debit-report',
    templateUrl: './sheet-debit-report.component.html',
})
export class SheetDebitReportComponent extends AppList {
    constructor(
        private _progressService: NgProgress,
        private _toastService: ToastrService,
        private _exportRepo: ExportRepo
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
    }

    onSearchSheetDebitReport(data: ReportInterface.ISaleReportCriteria) {
        switch (data.typeReport) {
            case CommonEnum.SHEET_DEBIT_REPORT_TYPE.ACCNT_PL_SHEET:
                this.exportAccountingPLSheet(data);
                break;
            case CommonEnum.JOB_PROFIT_ANALYSIS_TYPE.JOB_PROFIT_ANALYSIS:
                this.exportJobProfitAnalysis(data);
                break;
            case CommonEnum.SHEET_DEBIT_REPORT_TYPE.SUMMARY_OF_COST:
                this.exportSummaryOfCostsIncurred(data);
                break;
            case CommonEnum.SHEET_DEBIT_REPORT_TYPE.SUMMARY_OF_REVENUE:
                this.exportSummaryOfRevenueIncurred(data);
                break;
        }
    }

    exportAccountingPLSheet(data) {
        this._progressRef.start();
        this._exportRepo.exportAccountingPLSheet(data)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'Accounting PL Sheet (' + data.currency + ').xlsx');
                    } else {
                        this._toastService.warning('There is no mawb data to print', '');
                    }
                },
            );
    }

    exportJobProfitAnalysis(data) {
        this._progressRef.start();
        this._exportRepo.exportJobProfitAnalysis(data)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'Job Profit Analysis.xlsx');
                    } else {
                        this._toastService.warning('There is no mawb data to print', '');
                    }
                },
            );
    }

    exportSummaryOfCostsIncurred(data) {
        this._progressRef.start();
        this._exportRepo.exportSummaryOfCostsIncurred(data)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'Summary of costs incurred.xlsx');
                    } else {
                        this._toastService.warning('There is no mawb data to print', '');
                    }
                },
            );
    }

    exportSummaryOfRevenueIncurred(data) {
        this._progressRef.start();
        this._exportRepo.exportSummaryOfRevenueIncurred(data)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'Summary of revenue incurred.xlsx');
                    } else {
                        this._toastService.warning('There is no mawb data to print', '');
                    }
                },
            );
    }
}