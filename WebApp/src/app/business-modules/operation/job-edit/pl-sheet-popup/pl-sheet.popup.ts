import { Component, Input, ViewChild } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { Currency } from 'src/app/shared/models';
import { map, takeUntil, catchError, finalize } from 'rxjs/operators';
import { CatalogueRepo, JobRepo } from 'src/app/shared/repositories';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'pl-sheet-popup',
    templateUrl: './pl-sheet.popup.html'
})

export class PlSheetPopupComponent extends PopupBase {
    @Input() jobId: string;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;
    selectedCurrency: Currency;
    currencyList: Currency[];
    dataReport: any = null;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _jobRepo: JobRepo,
        private _progressService: NgProgress
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.getCurrency();
    }

    getCurrency() {
        this._catalogueRepo.getCurrency()
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.currencyList = res || [];
                    this.selectedCurrency = this.currencyList.filter((item: any) => item.id === 'VND')[0];
                },
            );
    }
    previewPL() {
        this._progressRef.start();
        this._jobRepo.previewPL(this.jobId, this.selectedCurrency.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    setTimeout(() => {
                        this.previewPopup.frm.nativeElement.submit();
                        this.previewPopup.show();
                    }, 1000);

                },
            );
    }
}
