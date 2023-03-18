import { Office, User } from "@models";
import { Action, createReducer, on } from "@ngrx/store";
import * as Actions from "./../actions";

export interface ISystemAppState {
    users: User[];
    offices: Office[];
    isLoadingUser: boolean;
    isLoadingOffice: boolean;
}

const initialState: ISystemAppState = {
    isLoadingUser: false,
    isLoadingOffice: false,
    users: [],
    offices: []
};
export const reducer = createReducer(
    initialState,
    on(Actions.GetSystemUser, (state: ISystemAppState, payload: any) => ({
        ...state, isLoadingUser: true
    })),
    on(Actions.GetSystemUserSuccess, (state: ISystemAppState, payload: { data: User[] }) => ({
        ...state, isLoadingUser: false, users: payload.data
    })),
    on(Actions.GetSystemOffice, (state: ISystemAppState, payload: any) => ({
        ...state, isLoadingOffice: true
    })),
    on(Actions.GetSystemOfficeSuccess, (state: ISystemAppState, payload: { data: Office[] }) => ({
        ...state, isLoadingOffice: false, offices: payload.data
    })),
)

export function systemReducer(state: ISystemAppState | undefined, action: Action) {
    return reducer(state, action);
}