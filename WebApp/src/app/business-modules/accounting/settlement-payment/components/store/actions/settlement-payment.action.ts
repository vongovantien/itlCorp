import { ISearchSettlePayment } from './../../form-search-settlement/form-search-settlement.component';
import { createAction, props } from '@ngrx/store';

export enum SettlementPaymentActionTypes {
    SEARCH_LIST = '[SettlementPayment] Search List',
    LOAD_LIST = '[SettlementPayment] Load List',
    LOAD_LIST_SUCCESS = '[SettlementPayment] Load List Success',
};

type searchType = ISearchSettlePayment;

export const SearchListSettlePayment = createAction(SettlementPaymentActionTypes.SEARCH_LIST, props<Partial<searchType>>());
export const LoadListSettlePayment = createAction(SettlementPaymentActionTypes.LOAD_LIST, props<CommonInterface.IParamPaging>());
export const LoadListSettlePaymentSuccess = createAction(SettlementPaymentActionTypes.LOAD_LIST_SUCCESS, props<CommonInterface.IResponsePaging>());