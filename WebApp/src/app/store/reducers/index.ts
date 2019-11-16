import * as fromRouter from '@ngrx/router-store';
import { Params, RouterStateSnapshot } from '@angular/router';
import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';

export interface IRouterStateUrl {
    url: string;
    queryParams: Params;
    params: Params;
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
        const { params } = state;

        return { url, params, queryParams };
    }
}

// * Selector

export const routerState = createFeatureSelector<fromRouter.RouterReducerState<IRouterStateUrl>>('routerReducer');

export const getRouterState = createSelector(routerState, (state: fromRouter.RouterReducerState<IRouterStateUrl>) => state.state);
export const getQueryParamsRouterState = createSelector(routerState, (state: fromRouter.RouterReducerState<IRouterStateUrl>) => state.state.queryParams);
export const getParamsRouterState = createSelector(routerState, (state: fromRouter.RouterReducerState<IRouterStateUrl>) => state.state.params);
export const getUrlRouterState = createSelector(routerState, (state: fromRouter.RouterReducerState<IRouterStateUrl>) => state.state.url);
export const selectRouterParamByKey = createSelector(
    routerState,
    (state: fromRouter.RouterReducerState<IRouterStateUrl>, { field }: { field: string }) => !!state.state.params[field] ? state.state.params[field] : null
);

