import { createAction, props } from '@ngrx/store';
import { ISearchAdvancePayment } from './../../components/form-search-advance-payment/form-search-advance-payment.component';

export enum AdvancePaymentActionTypes {
    SEARCH_LIST = '[AdvancePayment] Search List',
    LOAD_LIST = '[AdvancePayment] Load List',
    LOAD_LIST_SUCCESS = '[AdvancePayment] Load List Success]',
    LOAD_DETAIL_SUCCESS = '[AdvancePayment] Load Detail Success]'
};

type searchType = ISearchAdvancePayment;

export const SearchListAdvancePayment = createAction(AdvancePaymentActionTypes.SEARCH_LIST, props<Partial<searchType>>());
export const LoadListAdvancePayment = createAction(AdvancePaymentActionTypes.LOAD_LIST, props<CommonInterface.IParamPaging>());
export const LoadListAdvancePaymentSuccess = createAction(AdvancePaymentActionTypes.LOAD_LIST_SUCCESS, props<CommonInterface.IResponsePaging>());
export const LoadDetailSuccess = createAction(AdvancePaymentActionTypes.LOAD_DETAIL_SUCCESS, props<CommonInterface.IResponsePaging>());

