import { IReceiptState, receiptReducer } from './customer-payment.reducer';
import { createFeatureSelector, ActionReducerMap, createSelector } from '@ngrx/store';

export interface ICustomerPaymentState {
    receipt: IReceiptState;
}


// * SELECTOR
export const customerPaymentState = createFeatureSelector<ICustomerPaymentState>('customer-payment');

export const customerPaymentReceiptState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt);
export const customerPaymentReceipListState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.list);
export const customerPaymentReceipInvoiceListState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.invoices);
export const customerPaymentReceipLoadingState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.isLoading);

export const ReceiptDebitListState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.debitList);
export const ReceiptCreditListState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.creditList);
export const ReceiptTypeState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.type);
export const reducers: ActionReducerMap<ICustomerPaymentState> = {
    receipt: receiptReducer,
};
