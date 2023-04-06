import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';
import { IReceiptState, receiptReducer } from './customer-payment.reducer';
import { IReceiptCombineState, receiptCombineReducer } from './receipt-combine.reducer';

export interface ICustomerPaymentState {
    receipt: IReceiptState;
}


// * SELECTOR
export const customerPaymentState = createFeatureSelector<ICustomerPaymentState>('customer-payment');

export const customerPaymentReceiptState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt);
export const customerPaymentReceipListState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.list);
export const customerPaymentReceipLoadingState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.isLoading);
export const customerPaymentReceipSearchState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt?.dataSearch);
export const customerPaymentReceipPagingState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt?.pagingData);

export const ReceiptDebitListState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.debitList);
export const ReceiptCreditListState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.creditList);
export const ReceiptTypeState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.type);
export const ReceiptPartnerCurrentState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt?.partnerId);
export const ReceiptDateState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt?.date);
export const ReceiptAgreementCreditCurrencyState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt.agreement?.creditCurrency);
export const ReceiptAgreementCusAdvanceVNDState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt.agreement?.customerAdvanceAmountVnd);
export const ReceiptAgreementCusAdvanceUSDState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt.agreement?.customerAdvanceAmountUsd);
export const ReceiptAgreementState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt.agreement);
export const ReceiptIsAutoConvertPaidState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt?.isAutoConvertPaid);
export const ReceiptClassState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.class);
export const ReceiptExchangeRate = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state.receipt?.exchangeRate);
export const ReceiptPaymentMethodState = createSelector(customerPaymentState, (state: ICustomerPaymentState) => state?.receipt?.paymentMethod);

export const reducers: ActionReducerMap<ICustomerPaymentState> = {
    receipt: receiptReducer,
};
