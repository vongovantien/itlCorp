import { Component, Input, ViewChild, ElementRef } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { Currency } from 'src/app/shared/models';
import { catchError, finalize } from 'rxjs/operators';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { NgProgress } from '@ngx-progressbar/core';
import { Crystal } from 'src/app/shared/models/report/crystal.model';
import { ToastrService } from 'ngx-toastr';
import { DomSanitizer } from '@angular/platform-browser';
import { API_MENU } from 'src/constants/api-menu.const';
import { ModalDirective } from 'ngx-bootstrap';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { environment } from 'src/environments/environment';

@Component({
    selector: 'pl-sheet-popup',
    templateUrl: './pl-sheet.popup.html'
})

export class PlSheetPopupComponent extends PopupBase {
    @Input() jobId: string;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;
    @ViewChild('formPL', { static: false }) formPL: ElementRef;
    @ViewChild("popupReport", { static: false }) popupReport: ModalDirective;

    selectedCurrency: Currency;
    currencyList: Currency[];
    dataReport: any = null;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _documentRepo: DocumentationRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService,
        private sanitizer: DomSanitizer,
        private api_menu: API_MENU,
        private _dataService: DataService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY)) {
            this.currencyList = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY) || [];
            this.selectedCurrency = this.currencyList.filter((item: any) => item.id === 'VND')[0];
        } else {
            this.getCurrency();
        }
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
                    this.dataReport = JSON.stringify(res);

                    setTimeout(() => {
                        if (!this.popupReport.isShown) {
                            this.popupReport.config = this.options;
                            this.popupReport.show();
                        }
                        this.submitFormPreview();
                    }, 1000);
                },
            );
    }

    get scr() {
        // http://localhost:51830/Default.aspx       
        // return this.sanitizer.bypassSecurityTrustResourceUrl(this.api_menu.Report);
        return this.sanitizer.bypassSecurityTrustResourceUrl(`${environment.HOST.REPORT}`);
    }

    ngAfterViewInit() {
        if (!!this.dataReport) {
            this.formPL.nativeElement.submit();
        }
    }

    submitFormPreview() {
        this.formPL.nativeElement.submit();
    }

    onSubmitForm(f) {
        return true;
    }

    hidePreview() {
        this.popupReport.hide();
    }

}
