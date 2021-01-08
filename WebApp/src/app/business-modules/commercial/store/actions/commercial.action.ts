import { createAction, props } from '@ngrx/store';

export enum CommercialActionTypes {
    SEARCH_LIST = '[CommercialAction] Search List',
};

export const SearchList = createAction(CommercialActionTypes.SEARCH_LIST, props<{ payload: any }>());