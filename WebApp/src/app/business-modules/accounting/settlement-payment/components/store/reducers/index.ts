import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { ISettlementPaymentSearchParamsState, reducer } from "./settlement-payment.reducer";

export * from './settlement-payment.reducer';
export interface ISettlementPaymentState {
    settle: ISettlementPaymentSearchParamsState;
}


// * SELECTOR
export const settlementPaymentState = createFeatureSelector<ISettlementPaymentState>('settlement-payment');
export const getSettlementPaymentSearchParamsState = createSelector(settlementPaymentState, (state: ISettlementPaymentState) => state && state.settle && state.settle.searchParams);

export const reducers: ActionReducerMap<ISettlementPaymentState> = {
    settle: reducer
};