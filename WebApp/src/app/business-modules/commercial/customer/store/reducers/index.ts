import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { ICustomerSearchParamsState, reducer } from "./customer.reducer";

export * from './customer.reducer';
export interface ICustomerState {
    com: ICustomerSearchParamsState;
}


// * SELECTOR
export const commercialState = createFeatureSelector<ICustomerState>('customer');
export const getCustomerSearchParamsState = createSelector(commercialState, (state: ICustomerState) => state && state.com && state.com.searchParams);

export const reducers: ActionReducerMap<ICustomerState> = {
    com: reducer
};