import { Surcharge } from '@models';
import { createAction, props } from '@ngrx/store';
import { ISettlementPaymentData } from '../../../detail/detail-settlement-payment.component';
import { ISearchSettlePayment } from './../../form-search-settlement/form-search-settlement.component';

export enum SettlementPaymentActionTypes {
    SEARCH_LIST = '[SettlementPayment] Search List',
    LOAD_LIST = '[SettlementPayment] Load List',
    LOAD_LIST_SUCCESS = '[SettlementPayment] Load List Success',

    GET_DETAIL = '[SettlementPayment] Get Detail',
    GET_DETAIL_SUCCESS = '[SettlementPayment] Get Detail Success',
    GET_DETAIL_FAIL = '[SettlementPayment] Get Detail Fail',
    UPDATE_LIST_NO_GROUP_SURCHARGE = '[SettlementPayment] Update No Group List Surcharge',

};

type searchType = ISearchSettlePayment;

export const SearchListSettlePayment = createAction(SettlementPaymentActionTypes.SEARCH_LIST, props<Partial<searchType>>());

export const LoadListSettlePayment = createAction(SettlementPaymentActionTypes.LOAD_LIST, props<CommonInterface.IParamPaging>());
export const LoadListSettlePaymentSuccess = createAction(SettlementPaymentActionTypes.LOAD_LIST_SUCCESS, props<CommonInterface.IResponsePaging>());

export const LoadDetailSettlePayment = createAction(SettlementPaymentActionTypes.GET_DETAIL, props<{ id: string }>());
export const LoadDetailSettlePaymentSuccess = createAction(SettlementPaymentActionTypes.GET_DETAIL_SUCCESS, props<ISettlementPaymentData>());
export const LoadDetailSettlePaymentFail = createAction(SettlementPaymentActionTypes.GET_DETAIL_FAIL);
export const UpdateListNoGroupSurcharge = createAction(SettlementPaymentActionTypes.UPDATE_LIST_NO_GROUP_SURCHARGE, props<{ data: Surcharge[] }>());
