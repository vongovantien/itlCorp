import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { IAgentSearchParamsState, reducer } from "./agent.reducer";

export * from './agent.reducer';
export interface IAgentState {
    com: IAgentSearchParamsState;
}


// * SELECTOR
export const commercialState = createFeatureSelector<IAgentState>('agent');
export const getAgentSearchParamsState = createSelector(commercialState, (state: IAgentState) => state && state.com && state.com.searchParams);

export const reducers: ActionReducerMap<IAgentState> = {
    com: reducer
};