import { Office, User } from "@models";
import { createAction, props } from "@ngrx/store";

export enum SystemAppActionTypes {
    GET_USER = '[System] Get User',
    GET_USER_SUCCESS = '[System] Get User Success',
    GET_USER_FAIL = '[System] Get User Fail',


    GET_OFFICES = '[System] Get Office',
    GET_OFFICES_SUCCESS = '[System] Get Office Success',
    GET_OFFICES_FAIL = '[System] Get Office Fail',

}


export const GetSystemUser = createAction(SystemAppActionTypes.GET_USER, props<{ [key: string]: any }>());
export const GetSystemUserSuccess = createAction(SystemAppActionTypes.GET_USER_SUCCESS, props<{ data: User[] }>());
export const GetSystemUserFail = createAction(SystemAppActionTypes.GET_USER_FAIL);

export const GetSystemOffice = createAction(SystemAppActionTypes.GET_OFFICES);
export const GetSystemOfficeSuccess = createAction(SystemAppActionTypes.GET_OFFICES_SUCCESS, props<{ data: Office[] }>());
export const GetSystemOfficeFail = createAction(SystemAppActionTypes.GET_OFFICES_FAIL);