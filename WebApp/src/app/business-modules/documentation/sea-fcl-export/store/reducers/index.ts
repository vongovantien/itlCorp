import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';
import { SeaFCLExportReducer, ISeaFCLExportShipmentState } from './sea-fcl-export.reducer';

// import * from './sea-fcl-export.reducer';
import * as fromReducer from './sea-fcl-export.reducer';

export interface ISeaFCLExport {
    csTransaction: ISeaFCLExportShipmentState;
}

// * Create Selector
export const seaFCLExportState = createFeatureSelector<ISeaFCLExport>('seaFCLExport');

export const getSeaFCLExportTransactionState = createSelector(seaFCLExportState, (state: ISeaFCLExport) => state.csTransaction);
export const getSeaFCLExportShipment = createSelector(seaFCLExportState, (state: ISeaFCLExport) => state.csTransaction.shipments);
export const getSeaFCLShipmentLoading = createSelector(seaFCLExportState, (state: ISeaFCLExport) => state.csTransaction.isLoading);
export const getSeaFCLShipmentLoaded = createSelector(seaFCLExportState, (state: ISeaFCLExport) => state.csTransaction.isLoaded);

export const reducers: ActionReducerMap<ISeaFCLExport> = {
    csTransaction: SeaFCLExportReducer,
};



