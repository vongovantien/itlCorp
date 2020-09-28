
import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { IAccountingManagementPartnerState, reducer } from "./accounting-management-partner.reducer";
import { accountingListReducer, IAccountingManagementListState } from "./accounting-management.reducer";

export * from './accounting-management-partner.reducer';
export interface IAccountingManagementState {
    partner: IAccountingManagementPartnerState;
    list: IAccountingManagementListState;
}


// * SELECTOR
export const accountingManagementState = createFeatureSelector<IAccountingManagementState>('accounting-management');

export const accountingManagementListVatInvoiceState = createSelector(accountingManagementState, (state: IAccountingManagementState) => state && state.list.vatInvoices);
export const accountingManagementListVatVoucherState = createSelector(accountingManagementState, (state: IAccountingManagementState) => state && state.list.vouchers);
export const accountingManagementVatInvoiceDataSearchState = createSelector(accountingManagementState, (state: IAccountingManagementState) => state && state.list.dataSearchInvoice);
export const accountingManagementVoucherDataSearchState = createSelector(accountingManagementState, (state: IAccountingManagementState) => state && state.list.dataSearchVoucher);
export const accountingManagementListLoadingState = createSelector(accountingManagementState, (state: IAccountingManagementState) => state && state.list.isLoading);

export const getAccoutingManagementPartnerState = createSelector(accountingManagementState, (state: IAccountingManagementState) => state && state.partner);
export const getAccoutingManagementPaymentTermState = createSelector(accountingManagementState, (state: IAccountingManagementState) => state && state.partner.paymentTerm);
export const getAccountingManagementPartnerChargeState = createSelector(accountingManagementState, (state: IAccountingManagementState) => state.partner.charges);

export const reducers: ActionReducerMap<IAccountingManagementState> = {
    partner: reducer,
    list: accountingListReducer
};

