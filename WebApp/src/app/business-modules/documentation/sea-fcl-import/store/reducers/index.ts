import { createFeatureSelector, createSelector, ActionReducerMap } from '@ngrx/store';
import { IContainerState, ContainerReducer } from './container.reducer';
import { CSTransactionReducer, ICsTransaction } from './sea-fcl-import.reducer';

export * from './container.reducer';
export * from './sea-fcl-import.reducer';

export interface ISeaFCLImportState {
    csMawbcontainers: IContainerState;
    csTransaction: ICsTransaction;
}


// * Create Selector
export const transactionState = createFeatureSelector<ISeaFCLImportState>('seaFClImport');

export const seaFCLImportTransactionState = createSelector(transactionState, (state: ISeaFCLImportState) => state.csTransaction.cstransaction);
export const getCSMawbcontainersState = createSelector(transactionState, (state: ISeaFCLImportState) => state.csMawbcontainers);
export const getContainerSaveState = createSelector(transactionState, (state: ISeaFCLImportState) => state.csMawbcontainers.containers);

export const reducers: ActionReducerMap<ISeaFCLImportState> = {
    csMawbcontainers: ContainerReducer,
    csTransaction: CSTransactionReducer
};
