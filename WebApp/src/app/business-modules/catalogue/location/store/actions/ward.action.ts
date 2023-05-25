import { createAction, props } from '@ngrx/store';

export enum WardActionTypes {
    SEARCH_LIST = '[WardAction] Search List',
    LOAD_LIST_WARD = '[WardAction] Load List',
    LOAD_LIST_WARD_SUCCESS = '[WardAction] Load List Success]'
};

export const SearchListWard = createAction(WardActionTypes.SEARCH_LIST, props<{ payload: any }>());
export const LoadListWardLocation = createAction(WardActionTypes.LOAD_LIST_WARD, props<CommonInterface.IParamPaging>());
export const LoadListWardSuccess = createAction(WardActionTypes.LOAD_LIST_WARD_SUCCESS, props<CommonInterface.IResponsePaging>());