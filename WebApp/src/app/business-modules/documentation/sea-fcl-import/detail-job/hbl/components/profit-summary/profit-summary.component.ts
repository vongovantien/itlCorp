import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AppPage } from 'src/app/app.base';

import * as fromStore from './../../../../store';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';
import { map } from 'rxjs/operators';
@Component({
    selector: 'hbl-profit-summary',
    templateUrl: './profit-summary.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class ProfitSummaryHBLComponent extends AppPage {

    hblProfit$: Observable<fromStore.IHBLProfit>;
    totalUSD$: Observable<number>;
    totalVND$: Observable<number>;

    headers: CommonInterface.IHeaderTable[];

    constructor(
        private _store: Store<fromStore.HBlActions>
    ) {
        super();
    }

    ngOnInit() {
        this.hblProfit$ = this._store.select(fromStore.getHBLProfitState);
        this.totalUSD$ = this.hblProfit$.pipe(
            map(data => data.profitUSD)
        );
        this.totalVND$ = this.hblProfit$.pipe(
            map(data => data.profitLocal)
        )
        this.headers = [
            { title: 'USD', field: 'profitUSD', dataType: 'CURRENCY' },
            { title: 'Local (VND)', field: 'profitLocal', dataType: 'CURRENCY' }
        ];

    }
}