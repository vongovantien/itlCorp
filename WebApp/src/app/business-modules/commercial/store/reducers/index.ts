import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { ICommercialSearchParamsState, reducer } from "./commercial.reducer";

export * from './commercial.reducer';
export interface ICommercialState {
    com: ICommercialSearchParamsState;
}


// * SELECTOR
export const commercialState = createFeatureSelector<ICommercialState>('commercial');
export const getCommercialSearchParamsState = createSelector(commercialState, (state: ICommercialState) => state && state.com && state.com.searchParams);

export const reducers: ActionReducerMap<ICommercialState> = {
    com: reducer
};