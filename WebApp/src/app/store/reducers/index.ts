import * as fromRouter from '@ngrx/router-store';
import { Params, RouterStateSnapshot, Data } from '@angular/router';
import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';

export interface IRouterStateUrl {
    url: string;
    queryParams: Params;
    params: Params;
    data: Data;
}

export interface IState {
    routerReducer: fromRouter.RouterReducerState<IRouterStateUrl>;
}

export const reducers: ActionReducerMap<IState> = {
    routerReducer: fromRouter.routerReducer
};

// * Custom Serializer

export class CustomSerializer implements fromRouter.RouterStateSerializer<IRouterStateUrl> {
    serialize(routerStateSnapshot: RouterStateSnapshot): IRouterStateUrl {

        let state = routerStateSnapshot.root;

        while (state.firstChild) {
            state = state.firstChild;
        }

        const { url, root: { queryParams }, } = routerStateSnapshot;
        const { params, data } = state;

        return { url, params, queryParams, data };
    }
}

// * Selector

export const routerState = createFeatureSelector<fromRouter.RouterReducerState<IRouterStateUrl>>('routerReducer');

export const getRouterState = createSelector(routerState, (state: fromRouter.RouterReducerState<IRouterStateUrl>) => state.state && state.state);
export const getQueryParamsRouterState = createSelector(routerState, (state: fromRouter.RouterReducerState<IRouterStateUrl>) => state.state && state.state.queryParams);
export const getParamsRouterState = createSelector(routerState, (state: fromRouter.RouterReducerState<IRouterStateUrl>) => state.state && state.state.params);
export const getUrlRouterState = createSelector(routerState, (state: fromRouter.RouterReducerState<IRouterStateUrl>) => state.state && state.state.url);
export const getDataRouterState = createSelector(routerState, (state: fromRouter.RouterReducerState<IRouterStateUrl>) => state.state && state.state.data);

export const selectRouterParamByKey = createSelector(
    routerState,
    (state: fromRouter.RouterReducerState<IRouterStateUrl>, { field }: { field: string }) => !!state.state.params[field] ? state.state.params[field] : null
);

