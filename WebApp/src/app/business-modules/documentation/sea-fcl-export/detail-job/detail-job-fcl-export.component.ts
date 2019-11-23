import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';

import { AppForm } from 'src/app/app.form';
import { getParamsRouterState, getQueryParamsRouterState } from 'src/app/store';

import * as fromShareBussiness from './../../../share-business/store';
import { combineLatest, of } from 'rxjs';
import { map } from 'rxjs/internal/operators/map';
import { tap } from 'rxjs/internal/operators/tap';
import { switchMap } from 'rxjs/internal/operators/switchMap';
import { take } from 'rxjs/operators';

@Component({
    selector: 'app-detail-job-fcl-export',
    templateUrl: './detail-job-fcl-export.component.html'
})

export class SeaFCLExportDetailJobComponent extends AppForm implements OnInit {

    jobId: string;

    constructor(
        private _store: Store<fromShareBussiness.TransactionActions>,
    ) {
        super();
    }

    ngOnInit() { }

    ngAfterViewInit() {
        combineLatest([
            this._store.select(getParamsRouterState),
            this._store.select(getQueryParamsRouterState),
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams })),
            tap((param: any) => {
                // this.selectedTab = !!param.tab ? param.tab.toUpperCase() : 'SHIPMENT';
                this.jobId = !!param.jobId ? param.jobId : '';
                // if (param.action) {
                //     this.ACTION = param.action.toUpperCase();
                // }

                // this.cdr.detectChanges();
            }),
            switchMap(() => of(this.jobId)),
            take(1)
        ).subscribe(
            (jobId: string) => {
                if (!!jobId) {
                    this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));

                }
            }
        );
    }
}
