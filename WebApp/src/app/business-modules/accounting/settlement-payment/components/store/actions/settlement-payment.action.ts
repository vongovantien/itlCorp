import { Action, createAction, props } from '@ngrx/store';

export enum SettlementPaymentActionTypes {
    SEARCH_LIST = '[SettlementPayment] Search List',
};

export const SearchList = createAction(SettlementPaymentActionTypes.SEARCH_LIST, props<{ payload: any }>());