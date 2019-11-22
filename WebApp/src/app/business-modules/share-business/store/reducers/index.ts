import { SurchargeReducer, ISurcharge } from './surcharge.reducer';
import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';
import { IContainerState, ContainerReducer } from './container.reducer';

export * from './surcharge.reducer';
export * from './container.reducer';


export interface IShareBussinessState {
    surcharge: ISurcharge;
    csMawbcontainers: IContainerState;

}

export const shareBussinessState = createFeatureSelector<IShareBussinessState>('share-bussiness');

export const getShareBussinessState = createSelector(shareBussinessState, (state: IShareBussinessState) => state);
export const getSurchargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge);
export const getBuyingSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.buyings);
export const getSellingSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.sellings);
export const getOBHSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.obhs);
export const getProfitState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.profit);

export const getCSMawbcontainersState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.csMawbcontainers);
export const getContainerSaveState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.csMawbcontainers.containers);


export const reducers: ActionReducerMap<IShareBussinessState> = {
    surcharge: SurchargeReducer,
    csMawbcontainers: ContainerReducer
};
