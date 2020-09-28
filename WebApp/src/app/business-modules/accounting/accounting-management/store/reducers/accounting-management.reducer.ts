import { AccAccountingManagement, AccAccountingManagementCriteria } from "@models";
import { Action, createReducer, on } from "@ngrx/store";
import * as accountingManagementActions from '../actions/accounting-management.action';

export interface IAccountingManagementListState {
    vatInvoices: AccAccountingManagement[];
    vouchers: AccAccountingManagement[];
    isLoading: boolean;
    isLoaded: boolean;
    dataSearchInvoice: AccAccountingManagementCriteria;
    dataSearchVoucher: AccAccountingManagementCriteria;
}

export const initialState: IAccountingManagementListState = {
    vatInvoices: [],
    vouchers: [],
    isLoading: false,
    isLoaded: false,
    dataSearchInvoice: null,
    dataSearchVoucher: null

};


export const accountingManagementListReducer = createReducer(
    initialState,
    on(
        accountingManagementActions.SearchListAccountingMngt, (state: IAccountingManagementListState, payload: AccAccountingManagementCriteria) => (
            { ...initialState }
        )
    ),
);

export function accountingListReducer(state: any | undefined, action: Action) {
    return accountingManagementListReducer(state, action);
}



