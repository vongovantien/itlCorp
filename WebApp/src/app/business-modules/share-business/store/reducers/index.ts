import { SurchargeReducer, ISurcharge } from './surcharge.reducer';
import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';
import { IContainerState, ContainerReducer } from './container.reducer';
import { ITransactionState, TransactionReducer } from './transaction.reducer';
import { IHBLState, HBLReducer } from './hbl.reducer';
import { IDimensionState, DimensionReducer } from './dimension.reduder';


export * from './surcharge.reducer';
export * from './container.reducer';
export * from './transaction.reducer';
export * from './hbl.reducer';
export * from './dimension.reduder';

export interface IShareBussinessState {
    surcharge: ISurcharge;
    csMawbcontainers: IContainerState;
    transaction: ITransactionState;
    houseBill: IHBLState;
    dimensions: IDimensionState;
}

export const shareBussinessState = createFeatureSelector<IShareBussinessState>('share-bussiness');

export const getShareBussinessState = createSelector(shareBussinessState, (state: IShareBussinessState) => state);

export const getSurchargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge);
export const getBuyingSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.buyings);
export const getSellingSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.sellings);
export const getOBHSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.obhs);
export const getSurchargeLoadingState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.isLoading);

export const getProfitState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.houseBill.profit);

export const getCSMawbcontainersState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.csMawbcontainers);
export const getContainerSaveState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.csMawbcontainers.containers);

export const getTransactionState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.transaction);
export const getTransactionProfitState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.transaction && state.transaction.profits);
export const getTransactionProfitLoadingState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.transaction && state.transaction.isLoading);
export const getTransactionDetailCsTransactionState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.transaction && state.transaction.cstransaction);
export const getTransactionListShipment = createSelector(shareBussinessState, (state: IShareBussinessState) => state.transaction.cstransactions);
export const getTransationLoading = createSelector(shareBussinessState, (state: IShareBussinessState) => state.transaction.isLoading);
export const getTransactionLocked = createSelector(shareBussinessState, (state: IShareBussinessState) => state.transaction.cstransaction.isLocked);
export const getTransactionPermission = createSelector(shareBussinessState, (state: IShareBussinessState) => state.transaction.cstransaction.permission);

export const getHBLSState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.houseBill.hbls);
export const getDetailHBlState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.houseBill && state.houseBill.hbl);
export const getHBLLoadingState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.houseBill && state.houseBill.isLoading);

export const getHBLContainersState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.houseBill && state.houseBill.containers);

export const getDimensionState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.dimensions);
export const getDimensionVolumesState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.dimensions.dims);



export const reducers: ActionReducerMap<IShareBussinessState> = {
    surcharge: SurchargeReducer,
    csMawbcontainers: ContainerReducer,
    transaction: TransactionReducer,
    houseBill: HBLReducer,
    dimensions: DimensionReducer
};
