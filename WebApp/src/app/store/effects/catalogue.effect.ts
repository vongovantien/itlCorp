import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Action, Store } from '@ngrx/store';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { map, catchError, withLatestFrom, switchMap } from 'rxjs/operators';
import { CatalogueRepo } from '@repositories';
import { CatalogueActions, CatalogueActionTypes, GetCataloguePortSuccessAction, GetCataloguePortFailAction, GetCatalogueCarrierSuccessAction, GetCatalogueCarrierFailAction, GetCatalogueAgentSuccessAction, GetCatalogueAgentFailAction } from '../actions';
import { getCataloguePortState, getCatalogueCarrierState, getCatalogueAgentState } from '../reducers';

@Injectable()
export class CatalogueEffect {
    constructor(
        private actions$: Actions,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<any>
    ) { }

    @Effect()
    getPorts$: Observable<Action> = this.actions$
        .pipe(
            ofType<CatalogueActions>(CatalogueActionTypes.GET_PORT),
            withLatestFrom(
                this._store.select(getCataloguePortState),
                (action: CatalogueActions, ports: any[]) => ({ data: ports, action: action })),

            switchMap((data: { data: any[], action: CatalogueActions }) => {
                // * Check ports in redux store.
                if (!!data && data.data && data.data.length) {
                    return of(new GetCataloguePortSuccessAction(data.data));
                }
                return this._catalogueRepo.getListPort(data.action.payload).pipe(
                    map((response: any) => new GetCataloguePortSuccessAction(response)),
                    catchError(err => of(new GetCataloguePortFailAction(err)))
                );
            })
        );

    @Effect()
    getCarriers$: Observable<Action> = this.actions$
        .pipe(
            ofType<CatalogueActions>(CatalogueActionTypes.GET_CARRIER),
            withLatestFrom(
                this._store.select(getCatalogueCarrierState),
                (action: CatalogueActions, carriers: any[]) => ({ data: carriers, action: action })),

            switchMap((data: { data: any[], action: CatalogueActions }) => {
                // * Check carriers in redux store.
                if (!!data && data.data && data.data.length) {
                    return of(new GetCatalogueCarrierSuccessAction(data.data));
                }
                return this._catalogueRepo.getPartnersByType(data.action.payload).pipe(
                    map((response: any) => new GetCatalogueCarrierSuccessAction(response)),
                    catchError(err => of(new GetCatalogueCarrierFailAction(err)))
                );
            })
        );


    @Effect()
    getAgents$: Observable<Action> = this.actions$
        .pipe(
            ofType<CatalogueActions>(CatalogueActionTypes.GET_AGENT),
            withLatestFrom(
                this._store.select(getCatalogueAgentState),
                (action: CatalogueActions, agents: any[]) => ({ data: agents, action: action })),

            switchMap((data: { data: any[], action: CatalogueActions }) => {
                // * Check carriers in redux store.
                if (!!data && data.data && data.data.length) {
                    return of(new GetCatalogueAgentSuccessAction(data.data));
                }
                return this._catalogueRepo.getPartnersByType(data.action.payload).pipe(
                    map((response: any) => new GetCatalogueAgentSuccessAction(response)),
                    catchError(err => of(new GetCatalogueAgentFailAction(err)))
                );
            })
        );
}
