import { createFeatureSelector, createSelector, ActionReducerMap } from "@ngrx/store";
import { opsReducer, IOPSTransactionState } from "./operation.reducer";

export interface IOperationState {
    transaction: IOPSTransactionState;
}

export const OperationState = createFeatureSelector<IOperationState>('operations');

export const getOperationState = createSelector(OperationState, (state: IOperationState) => state);
export const getOperationTransationState = createSelector(OperationState, (state: IOperationState) => state && state.transaction);
export const getOperationTransationListShipment = createSelector(OperationState, (state: IOperationState) => state && state.transaction.opsTransations);
export const getOperationTransationDataSearch = createSelector(OperationState, (state: IOperationState) => state && state.transaction.dataSearch);
export const getOperationTransationLoadingState = createSelector(OperationState, (state: IOperationState) => state && state.transaction.isLoading);

export const reducers: ActionReducerMap<IOperationState> = {
    transaction: opsReducer,
}

