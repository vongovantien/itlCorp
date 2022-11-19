import { ActionReducerMap, createFeatureSelector, createSelector } from '@ngrx/store';
import { soaDetailReducer, SOADetailState } from './soa-detail-reducer';
import { ISOAReducerState, SOAReducer } from './soa.reducer';

export interface ISOAState {
    soa: ISOAReducerState;
    soaDetail: SOADetailState;
}
// * SELECTOR
export const SOAState = createFeatureSelector<ISOAState>('soa');

export const SOADataState = createSelector(SOAState, (state: ISOAState) => state?.soa);
export const getDataSearchSOAState = createSelector(SOAState, (state: ISOAState) => state?.soa?.dataSearch);
export const getSOAPagingState = createSelector(SOAState, (state: ISOAState) => state?.soa?.pagingData);
export const getSOAListState = createSelector(SOAState, (state: ISOAState) => state?.soa?.list);
export const getSOALoadingListState = createSelector(SOAState, (state: ISOAState) => state?.soa?.isLoading);
export const getSOADetailState = createSelector(SOAState, (state: ISOAState) => state?.soaDetail?.data);

export const reducers: ActionReducerMap<ISOAState> = {
    soa: SOAReducer,
    soaDetail: soaDetailReducer
};
