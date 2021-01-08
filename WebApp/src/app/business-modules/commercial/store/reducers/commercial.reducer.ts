import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions';


export interface ICommercialSearchParamsState {
    searchParams: any;

}
export const initialState: ICommercialSearchParamsState = {
    searchParams: {}
};

const commercialReducer = createReducer(
    initialState,
    on(Types.SearchList, (state: ICommercialSearchParamsState, data: any) => ({
        ...state, searchParams: { ...data.payload }
    })),
);

export function reducer(state: any | undefined, action: Action) {
    return commercialReducer(state, action);
};