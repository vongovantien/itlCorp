import { Action, createAction, props } from '@ngrx/store';

export enum AdvancePaymentActionTypes {
    SEARCH_LIST = '[AdvancePayment] Search List',
};

export const SearchList = createAction(AdvancePaymentActionTypes.SEARCH_LIST, props<{ payload: any }>());