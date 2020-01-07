import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Action, Store } from '@ngrx/store';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { map, catchError, withLatestFrom, switchMap } from 'rxjs/operators';
import { CatalogueRepo } from '@repositories';
import {
    CatalogueActions, CatalogueActionTypes, GetCataloguePortSuccessAction, GetCataloguePortFailAction, GetCatalogueCarrierSuccessAction, GetCatalogueCarrierFailAction, GetCatalogueAgentSuccessAction, GetCatalogueAgentFailAction, GetCatalogueUnitSuccessAction, GetCatalogueUnitFailAction, GetCatalogueCommoditySuccessAction, GetCatalogueCommodityFailAction, GetCatalogueCustomerAction, GetCatalogueCustomerSuccessAction, GetCatalogueCustomerFailAction, GetCatalogueCountrySuccessAction, GetCatalogueCountryFailAction
} from '../actions';
import { getCataloguePortState, getCatalogueCarrierState, getCatalogueAgentState, getCatalogueUnitState, getCatalogueCommodityState, getCatalogueCustomerState, getCatalogueCountryState } from '../reducers';
import { Commodity, Unit, Customer, PortIndex, CountryModel } from '@models';

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
                (action: CatalogueActions, ports: PortIndex[]) => ({ data: ports, action: action })),

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
                (action: CatalogueActions, carriers: Customer[]) => ({ data: carriers, action: action })),

            switchMap((data: { data: Customer[], action: CatalogueActions }) => {
                // * Check carriers in redux store.
                if (!!data && data.data && data.data.length) {
                    return of(new GetCatalogueCarrierSuccessAction(data.data));
                }
                return this._catalogueRepo.getPartnersByType(data.action.payload).pipe(
                    map((response: Customer[]) => new GetCatalogueCarrierSuccessAction(response)),
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
                (action: CatalogueActions, agents: Customer[]) => ({ data: agents, action: action })),

            switchMap((data: { data: Customer[], action: CatalogueActions }) => {
                // * Check carriers in redux store.
                if (!!data && data.data && data.data.length) {
                    return of(new GetCatalogueAgentSuccessAction(data.data));
                }
                return this._catalogueRepo.getPartnersByType(data.action.payload).pipe(
                    map((response: Customer[]) => new GetCatalogueAgentSuccessAction(response)),
                    catchError(err => of(new GetCatalogueAgentFailAction(err)))
                );
            })
        );

    @Effect()
    getUnits$: Observable<Action> = this.actions$
        .pipe(
            ofType<CatalogueActions>(CatalogueActionTypes.GET_UNIT),
            withLatestFrom(
                this._store.select(getCatalogueUnitState),
                (action: CatalogueActions, units: Unit[]) => ({ data: units, action: action })),

            switchMap((data: { data: Unit[], action: CatalogueActions }) => {
                // * Check carriers in redux store.
                if (!!data && data.data && data.data.length) {
                    return of(new GetCatalogueUnitSuccessAction(data.data));
                }
                return this._catalogueRepo.getUnit(data.action.payload).pipe(
                    map((response: Unit[]) => new GetCatalogueUnitSuccessAction(response)),
                    catchError(err => of(new GetCatalogueUnitFailAction(err)))
                );
            })
        );

    @Effect()
    getCommodity$: Observable<Action> = this.actions$
        .pipe(
            ofType<CatalogueActions>(CatalogueActionTypes.GET_COMMODITY),
            withLatestFrom(
                this._store.select(getCatalogueCommodityState),
                (action: CatalogueActions, commodities: Commodity[]) => ({ data: commodities, action: action })),

            switchMap((data: { data: Commodity[], action: CatalogueActions }) => {
                // * Check carriers in redux store.
                if (!!data && data.data && data.data.length) {
                    return of(new GetCatalogueCommoditySuccessAction(data.data));
                }
                return this._catalogueRepo.getCommondity(data.action.payload).pipe(
                    map((response: Commodity[]) => new GetCatalogueCommoditySuccessAction(response)),
                    catchError(err => of(new GetCatalogueCommodityFailAction(err)))
                );
            })
        );

    @Effect()
    getCutomer$: Observable<Action> = this.actions$
        .pipe(
            ofType<CatalogueActions>(CatalogueActionTypes.GET_CUSTOMER),
            withLatestFrom(
                this._store.select(getCatalogueCustomerState),
                (action: CatalogueActions, customers: Customer[]) => ({ data: customers, action: action })),
            switchMap((data: { data: Customer[], action: CatalogueActions }) => {
                // * Check carriers in redux store.
                if (!!data && data.data && data.data.length) {
                    return of(new GetCatalogueCustomerSuccessAction(data.data));
                }
                return this._catalogueRepo.getPartnersByType(data.action.payload).pipe(
                    map((response: Customer[]) => new GetCatalogueCustomerSuccessAction(response)),
                    catchError(err => of(new GetCatalogueCustomerFailAction(err)))
                );
            })
        );

    @Effect()
    getCountries$: Observable<Action> = this.actions$
        .pipe(
            ofType<CatalogueActions>(CatalogueActionTypes.GET_COUNTRY),
            withLatestFrom(
                this._store.select(getCatalogueCountryState),
                (action: CatalogueActions, countries: CountryModel[]) => ({ data: countries, action: action })),
            switchMap((data: { data: CountryModel[], action: CatalogueActions }) => {
                // * Check carriers in redux store.
                if (!!data && data.data && data.data.length) {
                    return of(new GetCatalogueCountrySuccessAction(data.data));
                }
                return this._catalogueRepo.getCountry().pipe(
                    map((response: CountryModel[]) => new GetCatalogueCountrySuccessAction(response)),
                    catchError(err => of(new GetCatalogueCountryFailAction(err)))
                );
            })
        );
}
