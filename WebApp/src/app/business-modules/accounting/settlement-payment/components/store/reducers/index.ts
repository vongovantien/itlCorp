import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { reducer, SettlePaymentListState } from "./settlement-payment.reducer";

export * from './settlement-payment.reducer';
export interface ISettlementPaymentState {
    list: SettlePaymentListState;
}


// * SELECTOR
export const settlementPayment = createFeatureSelector<ISettlementPaymentState>('settlement-payment');

export const getSettlePaymentState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list);
export const getSettlementPaymentSearchParamsState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list?.dataSearch);
export const getSettlementPaymentListState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list?.settlements);
export const getSettlementPaymentListPagingState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list?.pagingData);
export const getSettlementPaymentListLoadingState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list?.isLoading);

export const reducers: ActionReducerMap<ISettlementPaymentState> = {
    list: reducer
};