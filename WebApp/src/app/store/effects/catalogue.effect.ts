import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Action, Store } from '@ngrx/store';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { map, catchError, withLatestFrom, switchMap } from 'rxjs/operators';
import { CatalogueRepo } from '@repositories';
import { CatalogueActions, CatalogueActionTypes, GetCataloguePortSuccessAction, GetCataloguePortFailAction } from '../actions';
import { getCataloguePortState } from '../reducers';

@Injectable()
export class CatalogueEffect {
    constructor(
        private actions$: Actions,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<any>
    ) { }
    @Effect() getPorts$: Observable<Action> = this.actions$
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

}
