import { SurchargeReducer, ISurcharge } from './surcharge.reducer';
import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';
import { IContainerState, ContainerReducer } from './container.reducer';
import { ITransactionState, TransactionReducer } from './transaction.reducer';
import { IHBLState, HBLReducer } from './hbl.reducer';

export * from './surcharge.reducer';
export * from './container.reducer';
export * from './transaction.reducer';
export * from './hbl.reducer';


export interface IShareBussinessState {
    surcharge: ISurcharge;
    csMawbcontainers: IContainerState;
    transaction: ITransactionState;
    houseBill: IHBLState;
}

export const shareBussinessState = createFeatureSelector<IShareBussinessState>('share-bussiness');

export const getShareBussinessState = createSelector(shareBussinessState, (state: IShareBussinessState) => state);

export const getSurchargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge);
export const getBuyingSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.buyings);
export const getSellingSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.sellings);
export const getOBHSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.obhs);

export const getProfitState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.houseBill.profit);

export const getCSMawbcontainersState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.csMawbcontainers);
export const getContainerSaveState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.csMawbcontainers.containers);

export const getTransactionState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.transaction);
export const getTransactionProfitState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.transaction && state.transaction.profits);
export const getTransactionProfitLoadingState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.transaction && state.transaction.isLoading);
export const getTransactionDetailCsTransactionState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.transaction && state.transaction.cstransaction);
export const getTransactionListShipment = createSelector(shareBussinessState, (state: IShareBussinessState) => state.transaction.cstransactions);
export const getTransationLoading = createSelector(shareBussinessState, (state: IShareBussinessState) => state.transaction.isLoading);

export const getHBLSState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.houseBill.hbls);
export const getDetailHBlState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.houseBill.hbl);

export const getHBLContainersState = createSelector(shareBussinessState, (state: IShareBussinessState) => state && state.houseBill && state.houseBill.containers);



export const reducers: ActionReducerMap<IShareBussinessState> = {
    surcharge: SurchargeReducer,
    csMawbcontainers: ContainerReducer,
    transaction: TransactionReducer,
    houseBill: HBLReducer
};
