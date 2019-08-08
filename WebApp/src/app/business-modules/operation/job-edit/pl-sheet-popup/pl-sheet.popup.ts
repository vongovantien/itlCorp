import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { DataService } from 'src/app/shared/services';
import { Currency } from 'src/app/shared/models';
import { map } from 'rxjs/operators';

@Component({
    selector: 'pl-sheet-popup',
    templateUrl: './pl-sheet.popup.html'
})

export class PlSheetPopupComponent extends PopupBase {

    selectedCurrency: Currency;
    currencyList: Currency[];

    constructor(
        private _dataService: DataService
    ) {
        super();
    }

    ngOnInit() {
        this.getCurrency();
    }

    getCurrency() {
        this._dataService.getDataByKey('lstCurrencies').pipe(
            map((data: any) => {
                if (!!data) {
                    return data.map((item: any) => new Currency(item))
                }
            })
        )
            .subscribe((data: any) => {
                this.currencyList = data || [];
                this.selectedCurrency = this.currencyList.filter( (item: any) => item.id === 'VND')[0];
            });
    }
}
