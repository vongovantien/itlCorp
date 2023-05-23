import { createAction, props } from '@ngrx/store';

export enum PartnerActionTypes {
    GET_DETAIL = '[Commercial] Get Detail Partner',
    GET_DETAIL_SUCCESS = '[Commercial] Get Detail Partner Success',
    GET_DETAIL_FAIL = '[Commercial] Get Detail Partner Fail',
};

export const getDetailPartner = createAction(PartnerActionTypes.GET_DETAIL, props<{ payload: any }>());
export const getDetailPartnerSuccess = createAction(PartnerActionTypes.GET_DETAIL_SUCCESS,props<{ payload: any }>());
export const getDetailPartnerFail = createAction(PartnerActionTypes.GET_DETAIL_FAIL, props<{ payload: any }>());

