import { ActionReducerMap, createFeatureSelector, createSelector } from "@ngrx/store";
import { IAgentState, agentReducer } from "./agent.reducer";
import { ICustomerState, customerReducer } from "./customer.reducer";
import { IPartnerState, partnerReducer } from "./partner.reducer";

export interface ICommercialState {
    agent: IAgentState;
    customer: ICustomerState
    partner: IPartnerState
}

// * SELECTOR
export const commercialState = createFeatureSelector<ICommercialState>('commercial');
export const agentState = createSelector(commercialState, (state: ICommercialState) => state.agent);
export const customerState = createSelector(commercialState, (state: ICommercialState) => state.customer);

export const getAgentSearchParamsState = createSelector(agentState, (state: IAgentState) => state && state.dataSearch);
export const getAgentDataListState = createSelector(agentState, (state: IAgentState) => state.agents);
export const getAgentPagingState = createSelector(agentState, (state: IAgentState) => state?.pagingData);
export const getAgentLoadingState = createSelector(agentState, (state: IAgentState) => state?.isLoading);

export const getCustomerSearchParamsState = createSelector(customerState, (state: ICustomerState) => state && state.dataSearch);
export const getCustomerListState = createSelector(customerState, (state: ICustomerState) => state?.customers);
export const getCustomerLoadingState = createSelector(customerState, (state: ICustomerState) => state?.isLoading);

export const reducers: ActionReducerMap<ICommercialState> = {
    agent: agentReducer,
    customer: customerReducer,
    partner: partnerReducer
};
