import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AppList } from 'src/app/app.list';

import * as fromStore from './../../store';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';

@Component({
    selector: 'grant-total-profit',
    templateUrl: './grant-total-profit.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class SeaFCLImportGrantTotalProfitComponent extends AppList {

    shipmentProfits: Observable<fromStore.IProfit> = new Observable<fromStore.IProfit>();

    headers: CommonInterface.IHeaderTable[];

    constructor(
        private _store: Store<fromStore.SeaFCLImportActions>
    ) {
        super();
    }

    ngOnInit() {
        this.shipmentProfits = this._store.select(fromStore.getProfitState);

        this.headers = [
            { title: 'USD', field: 'profitUSD', dataType: 'CURRENCY' },
            { title: 'Local (VND)', field: 'profitLocal', dataType: 'CURRENCY' }
        ];

    }
}
