import { createAction, props } from '@ngrx/store';

export enum CustomerActionTypes {
    SEARCH_LIST = '[CustomerAction] Search List',
    LOAD_LIST = '[CustomerAction] Load List',
    LOAD_LIST_SUCCESS = '[CustomerAction] Load List Success]',

    GET_DETAIL = '[CustomerAction] Get Detail',
    GET_DETAIL_SUCCESS = '[CustomerAction] Get Detail Success',
    GET_DETAIL_FAIL = '[CustomerAction] Get Detail Fail',
};

export const SearchList = createAction(CustomerActionTypes.SEARCH_LIST, props<{ payload: any }>());
export const LoadListCustomer = createAction(CustomerActionTypes.LOAD_LIST, props<CommonInterface.IParamPaging>());
export const LoadListCustomerSuccess = createAction(CustomerActionTypes.LOAD_LIST_SUCCESS, props<CommonInterface.IResponsePaging>());
export const getDetailCustomer = createAction(CustomerActionTypes.GET_DETAIL, props<{ payload: any }>());
export const getDetailCustomerSuccess = createAction(CustomerActionTypes.GET_DETAIL_SUCCESS,props<{ payload: any }>());
export const getDetailCustomerFail = createAction(CustomerActionTypes.GET_DETAIL_FAIL, props<{ payload: any }>());
