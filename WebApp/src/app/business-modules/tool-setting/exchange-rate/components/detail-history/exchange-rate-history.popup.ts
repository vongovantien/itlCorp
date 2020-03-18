import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'exchange-rate-history-popup',
    templateUrl: './exchange-rate-history.popup.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExchangeRateHistoryPopupComponent extends PopupBase implements OnInit {
    exchangeRatesOfDay: any;

    constructor() {
        super();
    }

    ngOnInit(): void { }
}
