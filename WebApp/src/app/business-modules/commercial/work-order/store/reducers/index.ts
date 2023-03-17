import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';
import { IWorkOrderDetailState, workOrderDetailReducer } from './work-order-detail.reducer';
import { IWorkOrderListState, workOrderListReducer } from './work-order.reducer';

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
export const workOrderDetailCodeState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.code);
export const workOrderDetailTransactionTypeNameState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.transactionTypeName);
export const workOrderDetailPermissionState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.permission);
export const workOrderDetailPermissionUpdateState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.permission.allowUpdate);
export const workOrderDetailActiveState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.active);
export const workOrderDetailStatusState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.status);
export const workOrderDetailTransationTypeState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.transactionType);
export const workOrderDetailLoadingState = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.isLoading);
export const WorkOrderListPricestate = createSelector(workOrderState, (state: IWorkOrderMngtState) => state.detail?.listPrice);
export const WorkOrderPriceItemUpdateModeState = createSelector(workOrderState, (state: IWorkOrderMngtState) => (state.detail?.listPrice || []).find(x => x.mode === 'UPDATE'));


export const workOrderDetailIsReadOnlyState = createSelector(
    workOrderDetailActiveState,
    workOrderDetailPermissionUpdateState,
    (active, allowUpdate) => {
        return active === true || allowUpdate === false;
    }
);

export const reducers: ActionReducerMap<IWorkOrderMngtState> = {
    list: workOrderListReducer,
    detail: workOrderDetailReducer
};
