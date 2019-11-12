import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AppList } from 'src/app/app.list';

import * as fromStore from './../../store';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
    selector: 'grant-total-profit',
    templateUrl: './grant-total-profit.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class SeaFCLImportGrantTotalProfitComponent extends AppList {

    shipmentProfits$: Observable<fromStore.IProfit[]> = new Observable<fromStore.IProfit[]>();
    totalUSD$: Observable<number>;
    totalVND$: Observable<number>;

    headers: CommonInterface.IHeaderTable[];

    constructor(
        private _store: Store<fromStore.SeaFCLImportActions>
    ) {
        super();
    }

    ngOnInit() {
        this.shipmentProfits$ = this._store.select(fromStore.getProfitState);

        this.totalUSD$ = this.shipmentProfits$.pipe(
            map((profits: any[]) => profits.reduce((acc: number, curr: any) => acc += curr.profitUSD, 0))
        );
        this.totalVND$ = this.shipmentProfits$.pipe(
            map((profits: fromStore.IProfit[]) => profits.reduce((acc: number, curr: any) => acc += curr.profitLocal, 0))
        );

        this.headers = [
            { title: 'USD', field: 'profitUSD', dataType: 'CURRENCY' },
            { title: 'Local (VND)', field: 'profitLocal', dataType: 'CURRENCY' }
        ];

    }
}
