import { SurchargeReducer, ISurcharge } from './surcharge.reducer';
import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';

export * from './surcharge.reducer';


export interface IShareBussinessState {
    surcharge: ISurcharge;
}

export const shareBussinessState = createFeatureSelector<IShareBussinessState>('share-bussiness');

export const getShareBussinessState = createSelector(shareBussinessState, (state: IShareBussinessState) => state);
export const getSurchargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge);
export const getBuyingSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.buyings);
export const getSellingSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.sellings);
export const getOBHSurChargeState = createSelector(shareBussinessState, (state: IShareBussinessState) => state.surcharge.obhs);




export const reducers: ActionReducerMap<IShareBussinessState> = {
    surcharge: SurchargeReducer,
};
