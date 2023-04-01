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
    LOAD_LIST_NO_GROUP_SURCHARGE = '[SettlementPayment] Load list No Group List Surcharge',
    UPDATE_LIST_NO_GROUP_SURCHARGE = '[SettlementPayment] Update No Group List Surcharge',

    UPDATE_LIST_EDOC = '[SettlementPayment] Update List Edoc',
    LOAD_LIST_EDOC = '[SettlementPayment] Load List EDoc',
    LOAD_LIST_EDOC_SUCCESS = '[SettlementPayment] Load List EDoc Success',
};

type searchType = ISearchSettlePayment;

export const SearchListSettlePayment = createAction(SettlementPaymentActionTypes.SEARCH_LIST, props<Partial<searchType>>());

export const LoadListSettlePayment = createAction(SettlementPaymentActionTypes.LOAD_LIST, props<CommonInterface.IParamPaging>());
export const LoadListSettlePaymentSuccess = createAction(SettlementPaymentActionTypes.LOAD_LIST_SUCCESS, props<CommonInterface.IResponsePaging>());

export const LoadDetailSettlePayment = createAction(SettlementPaymentActionTypes.GET_DETAIL, props<{ id: string }>());
export const LoadDetailSettlePaymentSuccess = createAction(SettlementPaymentActionTypes.GET_DETAIL_SUCCESS, props<ISettlementPaymentData>());
export const LoadDetailSettlePaymentFail = createAction(SettlementPaymentActionTypes.GET_DETAIL_FAIL);
export const UpdateListNoGroupSurcharge = createAction(SettlementPaymentActionTypes.UPDATE_LIST_NO_GROUP_SURCHARGE, props<{ data: Surcharge[] }>());
export const LoadListNoGroupSurcharge = createAction(SettlementPaymentActionTypes.LOAD_LIST_NO_GROUP_SURCHARGE);
//export const UpdateListEdocSettle = createAction(SettlementPaymentActionTypes.UPDATE_LIST_EDOC, props<{ data: any[] }>());

export const LoadListEDocSettle = createAction(SettlementPaymentActionTypes.LOAD_LIST_EDOC, props<IEDocSettleSearch>());
export const LoadListEDocSettleSuccess = createAction(SettlementPaymentActionTypes.LOAD_LIST_EDOC_SUCCESS, props<{ data: any[] }>());

export interface IEDocSettleSearch {
    billingId: string;
    transactionType: string
}
