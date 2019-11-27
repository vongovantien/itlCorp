import { createFeatureSelector, createSelector, ActionReducerMap } from '@ngrx/store';
import { CSTransactionReducer, ICsTransaction } from './sea-fcl-import.reducer';

export * from './sea-fcl-import.reducer';

export interface ISeaFCLImportState {
    csTransaction: ICsTransaction;
}

// * Create Selector
export const fclImportstate = createFeatureSelector<ISeaFCLImportState>('seaFClImport');

export const seaFCLImportTransactionState = createSelector(fclImportstate, (state: ISeaFCLImportState) => state.csTransaction.cstransaction);


export const reducers: ActionReducerMap<ISeaFCLImportState> = {
    csTransaction: CSTransactionReducer,
};


