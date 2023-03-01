import { ActionReducerMap, createFeatureSelector, createSelector } from "@ngrx/store";
import { SettlePaymentDetailState, settlePaymentDetailreducer } from "./settlement-payment-detail-reducer";
import { SettlePaymentListState, settlePaymentListreducer } from "./settlement-payment-list.reducer";

export * from './settlement-payment-list.reducer';
export interface ISettlementPaymentState {
    list: SettlePaymentListState;
    detail: SettlePaymentDetailState
}


// * SELECTOR
export const settlementPayment = createFeatureSelector<ISettlementPaymentState>('settlement-payment');

export const getSettlePaymentState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list);

export const getSettlementPaymentSearchParamsState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list?.dataSearch);
export const getSettlementPaymentListState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list?.settlements);
export const getSettlementPaymentListPagingState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list?.pagingData);
export const getSettlementPaymentListLoadingState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.list?.isLoading);

export const getSettlementPaymentDetailState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.detail.settlement);
export const getGrpChargeSettlementPaymentDetailState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.detail?.settlement?.chargeNoGrpSettlement);
export const getSettlementPaymentDetailLoadingState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.detail?.isLoading);
export const getUpdateListEDoc = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.detail?.IsUpdateListEDoc);
export const getEdocLoadingState = createSelector(settlementPayment, (state: ISettlementPaymentState) => state.detail?.isLoadEdoc);


export const reducers: ActionReducerMap<ISettlementPaymentState> = {
    list: settlePaymentListreducer,
    detail: settlePaymentDetailreducer
};
