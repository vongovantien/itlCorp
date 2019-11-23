import { createFeatureSelector, createSelector, ActionReducerMap } from '@ngrx/store';
import { CSTransactionReducer, ICsTransaction } from './sea-fcl-import.reducer';
import { IHBLState, HBLReducer } from './hbl.reducer';

export * from './sea-fcl-import.reducer';
export * from './hbl.reducer';

export interface ISeaFCLImportState {
    csTransaction: ICsTransaction;
    houseBill: IHBLState;
}


// * Create Selector
export const fclImportstate = createFeatureSelector<ISeaFCLImportState>('seaFClImport');

export const seaFCLImportTransactionState = createSelector(fclImportstate, (state: ISeaFCLImportState) => state.csTransaction.cstransaction);

export const getHBLState = createSelector(fclImportstate, (state: ISeaFCLImportState) => state.houseBill.hbl);

export const reducers: ActionReducerMap<ISeaFCLImportState> = {
    csTransaction: CSTransactionReducer,
    houseBill: HBLReducer,
};


