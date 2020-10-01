import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { IAdvancePaymentSearchParamsState, reducer } from "./advance-payment.reducer";

export * from './advance-payment.reducer';
export interface IAdvancePaymentState {
    adv: IAdvancePaymentSearchParamsState;
}


// * SELECTOR
export const advancePaymentState = createFeatureSelector<IAdvancePaymentState>('advance-payment');
export const getAdvancePaymentSearchParamsState = createSelector(advancePaymentState, (state: IAdvancePaymentState) => state && state.adv && state.adv.searchParams);

export const reducers: ActionReducerMap<IAdvancePaymentState> = {
    adv: reducer
};