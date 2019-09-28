import { Component, Input, ViewChild } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { Currency } from 'src/app/shared/models';
import { catchError, finalize } from 'rxjs/operators';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { NgProgress } from '@ngx-progressbar/core';
import { Crystal } from 'src/app/shared/models/report/crystal.model';
import { ToastrService } from 'ngx-toastr';

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
        private _documentRepo: DocumentationRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService
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
        this._documentRepo.previewPL(this.jobId, this.selectedCurrency.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: Crystal) => {
                    if (res.dataSource.length === 0) {
                        this._toastService.error("This shipment must have to one at least charge to show report", '', { positionClass: 'toast-bottom-right' });
                        return;
                    }
                    this.dataReport = res;
                    setTimeout(() => {
                        this.previewPopup.frm.nativeElement.submit();
                        this.previewPopup.show();
                    }, 1000);

                },
            );
    }
}
