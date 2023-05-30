import { createAction, props } from '@ngrx/store';

export enum PartnerDataActionTypes {
    GET_DETAIL = '[PartnerDataAction] Get Detail Partner',
    GET_DETAIL_SUCCESS = '[PartnerDataAction] Get Detail Partner Success',
    GET_DETAIL_FAIL = '[PartnerDataAction] Get Detail Partner Fail',

    SEARCH_LIST = '[PartnerDataAction] Search List',
    LOAD_LIST = '[PartnerDataAction] Load List',
    LOAD_LIST_SUCCESS = '[PartnerDataAction] Load List Success]'
};

export const getDetailPartner = createAction(PartnerDataActionTypes.GET_DETAIL, props<{ payload: any }>());
export const getDetailPartnerSuccess = createAction(PartnerDataActionTypes.GET_DETAIL_SUCCESS,props<{ payload: any }>());
export const getDetailPartnerFail = createAction(PartnerDataActionTypes.GET_DETAIL_FAIL, props<{ payload: any }>());

export const SearchListPartner = createAction(PartnerDataActionTypes.SEARCH_LIST, props<{ payload: any }>());
export const LoadListPartner = createAction(PartnerDataActionTypes.LOAD_LIST, props<CommonInterface.IParamPaging>());
export const LoadListPartnerSuccess = createAction(PartnerDataActionTypes.LOAD_LIST_SUCCESS, props<CommonInterface.IResponsePaging>());
