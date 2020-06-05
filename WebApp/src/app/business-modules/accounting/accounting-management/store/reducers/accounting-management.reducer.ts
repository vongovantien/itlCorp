import { createReducer, on } from '@ngrx/store';
import * as accountingManagementActions from '../actions/accounting-management.action';
import { ChargeOfAccountingManagementModel } from '@models';

export interface IAccountingManagementState {
    charges: ChargeOfAccountingManagementModel[];
}

export const initialState: IAccountingManagementState = { charges: [] };

export const accountingManagementReducer = createReducer(initialState, on(accountingManagementActions.SelectPartner, state => ({
    ...state, charges: state.charges
})));


