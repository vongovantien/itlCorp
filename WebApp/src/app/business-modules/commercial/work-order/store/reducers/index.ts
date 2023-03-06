import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';
import { IWorkOrderDetailState, workOrderDetailReducer } from './work-order-detail.reducer';
import { IWorkOrderListState, workOrderListReducer } from './work-order.reducer';

export * from './work-order.reducer';
export * from './work-order.reducer';


export interface IWorkOrderMngtState {
    list: IWorkOrderListState,
    detail: IWorkOrderDetailState
}


export const workOrderState = createFeatureSelector<IWorkOrderMngtState>('work-order');

export const workOrderListState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.list);
export const workOrderLoadingState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state?.list?.isLoading);
export const workOrderSearchState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.list?.dataSearch);
export const workOrderPagingState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.list?.pagingData);

export const workOrderDetailState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail);
export const WorkOrderListPricestate = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.listPrice);

export const reducers: ActionReducerMap<IWorkOrderMngtState> = {
    list: workOrderListReducer,
    detail: workOrderDetailReducer
};
