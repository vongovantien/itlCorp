import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { DataService } from 'src/app/shared/services';
import { Currency } from 'src/app/shared/models';
import { map, takeUntil, catchError, finalize } from 'rxjs/operators';
import { CatalogueRepo } from 'src/app/shared/repositories';

@Component({
    selector: 'pl-sheet-popup',
    templateUrl: './pl-sheet.popup.html'
})

export class PlSheetPopupComponent extends PopupBase {

    selectedCurrency: Currency;
    currencyList: Currency[];
    dataReport: any = null;

    constructor(
        private _dataService: DataService,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
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
    previewAdvPayment() {
        this._progressRef.start();
        // this._accoutingRepo.previewAdvancePayment(this.advId)
        //     .pipe(
        //         catchError(this.catchError),
        //         finalize(() => this._progressRef.complete())
        //     )
        //     .subscribe(
        //         (res: any) => {
        //             this.dataReport = res;
        //             setTimeout(() => {
        //                 this.previewPopup.frm.nativeElement.submit();
        //                 this.previewPopup.show();
        //             }, 1000);

        //         },
        //     );
    }
}
