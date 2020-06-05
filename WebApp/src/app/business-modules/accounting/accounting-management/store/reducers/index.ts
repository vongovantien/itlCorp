
import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { IAccountingManagementPartnerState, accountingManagementPartnerReducer } from "./accounting-management-partner.reducer";

export * from './accounting-management-partner.reducer';
export interface IAccountingManagementState {
    partner: IAccountingManagementPartnerState;
}


// * SELECTOR
export const accountingManagementState = createFeatureSelector<IAccountingManagementState>('accounting-management');
export const getAccoutingManagementPartnerState = createSelector(accountingManagementState, (state: IAccountingManagementState) => state && state.partner);


export const reducers: ActionReducerMap<IAccountingManagementState> = {
    partner: accountingManagementPartnerReducer
};

