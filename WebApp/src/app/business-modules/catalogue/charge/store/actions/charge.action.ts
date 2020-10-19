import { createAction, props } from '@ngrx/store';

export enum ChargeActionTypes {
    SEARCH_LIST = '[ChargeAction] Search List',
};

export const SearchList = createAction(ChargeActionTypes.SEARCH_LIST, props<{ payload: any }>());