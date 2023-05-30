import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions';

export interface IAgentState {
    agents: any
    isLoading: boolean;
    isLoaded: boolean;
    dataSearch: any;
    pagingData: any;
}

export const initialState: IAgentState = {
    agents: { data: [], totalItems: 0, },
    isLoading: false,
    isLoaded: false,
    pagingData: { page: 1, pageSize: 15 },
    dataSearch: {}
};

const reducer = createReducer(
    initialState,
    on(Types.SearchListAgent, (state: IAgentState, data: any) => ({
        ...state, dataSearch: { ...data.payload }
    })),
    on(
        Types.LoadListAgent, (state: IAgentState, payload: CommonInterface.IParamPaging) => {
            return { ...state, isLoading: true, pagingData: { page: payload.page, pageSize: payload.size } };
        }
    ),
    on(
        Types.LoadListAgentSuccess, (state: IAgentState, payload: CommonInterface.IResponsePaging) => {
            return { ...state, agents: payload, isLoading: false, isLoaded: true };
        }
    )
);

export function agentReducer(state: any | undefined, action: Action) {
    return reducer(state, action);
};
