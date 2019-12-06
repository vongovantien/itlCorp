import * as fromRouter from '@ngrx/router-store';
import { Params, RouterStateSnapshot, Data } from '@angular/router';
import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';
import { spinnerReducer, ISpinnerState } from './spinner.reducer';

export interface IRouterStateUrl {
    url: string;
    queryParams: Params;
    params: Params;
    data: Data;
}

export interface IAppState {
    routerReducer: fromRouter.RouterReducerState<IRouterStateUrl>;
    spinnerReducer: ISpinnerState;
}

export const reducers: ActionReducerMap<IAppState> = {
    routerReducer: fromRouter.routerReducer,
    spinnerReducer: spinnerReducer
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

export const isSpinnerShowing = createSelector(spinnerReducer, (state: ISpinnerState) => state.show);
