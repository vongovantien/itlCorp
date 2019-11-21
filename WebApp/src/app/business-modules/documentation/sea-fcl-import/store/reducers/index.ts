import { createFeatureSelector, createSelector, ActionReducerMap } from '@ngrx/store';
import { IContainerState, ContainerReducer } from './container.reducer';
import { CSTransactionReducer, ICsTransaction } from './sea-fcl-import.reducer';
import { IHBLState, HBLReducer } from './hbl.reducer';


export * from './container.reducer';
export * from './sea-fcl-import.reducer';
export * from './hbl.reducer';

export interface ISeaFCLImportState {
    csMawbcontainers: IContainerState;
    csTransaction: ICsTransaction;
    houseBill: IHBLState;
}


// * Create Selector
export const fclImportstate = createFeatureSelector<ISeaFCLImportState>('seaFClImport');

export const seaFCLImportTransactionState = createSelector(fclImportstate, (state: ISeaFCLImportState) => state.csTransaction.cstransaction);
export const getCSMawbcontainersState = createSelector(fclImportstate, (state: ISeaFCLImportState) => state.csMawbcontainers);
export const getContainerSaveState = createSelector(fclImportstate, (state: ISeaFCLImportState) => state.csMawbcontainers.containers);

export const getHBLState = createSelector(fclImportstate, (state: ISeaFCLImportState) => state.houseBill.hbl);
export const getShipmentProfitState = createSelector(fclImportstate, (state: ISeaFCLImportState) => state.csTransaction.profits);

export const reducers: ActionReducerMap<ISeaFCLImportState> = {
    csMawbcontainers: ContainerReducer,
    csTransaction: CSTransactionReducer,
    houseBill: HBLReducer,
};


