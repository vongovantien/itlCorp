import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions';


export interface IPartnerDataSearchParamsState {
    searchParams: any;

}
export const initialState: IPartnerDataSearchParamsState = {
    searchParams: {}
};

const partnerDataReducer = createReducer(
    initialState,
    on(Types.SearchList, (state: IPartnerDataSearchParamsState, data: any) => ({
        ...state, searchParams: { ...data.payload }
    })),
);

export function reducer(state: any | undefined, action: Action) {
    return partnerDataReducer(state, action);
};