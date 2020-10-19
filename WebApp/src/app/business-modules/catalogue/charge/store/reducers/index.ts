import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { IChargeSearchParamsState, reducer } from "./charge.reducer";

export * from './charge.reducer';
export interface IChargeState {
    chg: IChargeSearchParamsState;
}


// * SELECTOR
export const chargeState = createFeatureSelector<IChargeState>('charge');
export const getChargeSearchParamsState = createSelector(chargeState, (state: IChargeState) => state && state.chg && state.chg.searchParams);

export const reducers: ActionReducerMap<IChargeState> = {
    chg: reducer
};