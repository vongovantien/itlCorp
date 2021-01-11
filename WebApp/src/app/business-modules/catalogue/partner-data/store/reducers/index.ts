import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { IPartnerDataSearchParamsState, reducer } from "./partnerData.reducer";

export * from './partnerData.reducer';
export interface IPartnerDataState {
    com: IPartnerDataSearchParamsState;
}


// * SELECTOR
export const partnerState = createFeatureSelector<IPartnerDataState>('partnerData');
export const getPartnerDataSearchParamsState = createSelector(partnerState, (state: IPartnerDataState) => state && state.com && state.com.searchParams);

export const reducers: ActionReducerMap<IPartnerDataState> = {
    com: reducer
};