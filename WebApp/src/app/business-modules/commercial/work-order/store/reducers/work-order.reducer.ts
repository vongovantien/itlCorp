import { WorkOrderViewModel } from "@models";
import { Action, createReducer, on } from "@ngrx/store";
import * as WorkOrderActions from "./../actions";

export interface IWorkOrderListState {
    data: WorkOrderViewModel[],
    totalItems: number
    dataSearch: any,
    isLoading: boolean
    pagingData: { page: number, pageSize: number },
}

const initialState: IWorkOrderListState = {
    isLoading: false,
    dataSearch: null,
    pagingData: { page: 1, pageSize: 15 },
    data: [],
    totalItems: 0
};

export const reducer = createReducer(
    initialState,
    on(WorkOrderActions.SearchListWorkOrder, (state: IWorkOrderListState, payload: any) => ({
        ...state, dataSearch: payload, pagingData: { page: 1, pageSize: 15 }
    })),
    on(WorkOrderActions.LoadListWorkOrder, (state: IWorkOrderListState, payload: CommonInterface.IParamPaging) => ({
        ...state, isLoading: true, pagingData: { page: payload.page, pageSize: payload.size }
    })),
    on(WorkOrderActions.LoadListWorkOrderSuccess, (state: IWorkOrderListState, payload: CommonInterface.IResponsePaging) => ({
        ...state, data: payload.data, isLoading: false, isLoaded: true, totalItems: payload.totalItems
    })),
    on(WorkOrderActions.LoadListWorkOrderFail, (state: IWorkOrderListState) => ({
        ...state, isLoading: false, isLoaded: true
    })),

);

export function workOrderListReducer(state: IWorkOrderListState | undefined, action: Action) {
    return reducer(state, action);
}