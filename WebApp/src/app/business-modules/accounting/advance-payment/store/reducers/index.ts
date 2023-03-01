import { ActionReducerMap, createFeatureSelector, createSelector } from "@ngrx/store";
import { advancePaymentDetailReducer, AdvancePaymentDetailState } from "./advance-payment-detail-reducer";
import { advancePaymentListReducer, AdvancePaymentListState } from "./advance-payment.reducer";

export * from './advance-payment.reducer';
export interface IAdvancePaymentState {
    list: AdvancePaymentListState;
    advanceDetail: AdvancePaymentDetailState;
}

// * SELECTOR
export const advancePayment = createFeatureSelector<IAdvancePaymentState>('advance-payment');
export const advancePaymentState = createSelector(advancePayment, (state: IAdvancePaymentState) => state.list);
export const getAdvancePaymentSearchParamsState = createSelector(advancePayment, (state: IAdvancePaymentState) => state.list?.dataSearch);
export const getAdvancePaymentListState = createSelector(advancePayment, (state: IAdvancePaymentState) => state.list?.advances);
export const getAdvancePaymentListPagingState = createSelector(advancePayment, (state: IAdvancePaymentState) => state.list.pagingData);
export const getAdvancePaymentListLoadingState = createSelector(advancePayment, (state: IAdvancePaymentState) => state.list.isLoading);
export const getAdvanceDetailState = createSelector(advancePayment, (state: IAdvancePaymentState) => state?.advanceDetail?.data);
export const getAdvanceDetailRequestState = createSelector(advancePayment, (state: IAdvancePaymentState) => state?.advanceDetail?.data.advanceRequests);

export const reducers: ActionReducerMap<IAdvancePaymentState> = {
    list: advancePaymentListReducer,
    advanceDetail: advancePaymentDetailReducer
};
