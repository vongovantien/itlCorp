import { SOA } from "@models";
import { createAction, props } from "@ngrx/store";

export enum SOAActionTypes {
    SEARCH_LIST = '[SOA] Search List',
    LOAD_LIST = '[SOA] Load List',
    LOAD_LIST_SUCCESS = '[SOA] Load List Success',
    LOAD_SOA_DETAIL_SUCCESS = '[SOA] Load Detail SOA Success'
}

export const SearchListSOA = createAction(SOAActionTypes.SEARCH_LIST, props<Partial<any>>());
export const LoadListSOA = createAction(SOAActionTypes.LOAD_LIST, props<CommonInterface.IParamPaging>());
export const LoadListSOASuccess = createAction(SOAActionTypes.LOAD_LIST_SUCCESS, props<CommonInterface.IResponsePaging>());
export const LoadSOADetailSuccess = createAction(SOAActionTypes.LOAD_SOA_DETAIL_SUCCESS, props<{ detail: SOA }>());
