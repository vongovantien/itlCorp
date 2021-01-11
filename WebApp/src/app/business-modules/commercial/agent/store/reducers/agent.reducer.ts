import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions';


export interface IAgentSearchParamsState {
    searchParams: any;

}
export const initialState: IAgentSearchParamsState = {
    searchParams: {}
};

const agentReducer = createReducer(
    initialState,
    on(Types.SearchList, (state: IAgentSearchParamsState, data: any) => ({
        ...state, searchParams: { ...data.payload }
    })),
);

export function reducer(state: any | undefined, action: Action) {
    return agentReducer(state, action);
};